using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
    /// <summary>
	/// Компилированный объект сглаживающего фильтра: дискретизированное симметричное ядро для сглаживания (свёртки)
	/// функций одной переменной.
	/// </summary>
	/// <remarks>
	/// <para>При создании ядро <see cref="BlurFunctionHandler"/> интегрируется по секторам: центральный сектор
	/// [−0.09128·accuracy, 0.09128·accuracy], каждый следующий сектор в 1.16 раза шире предыдущего. Каждый сектор заменяется одним узлом
	/// в его середине с весом, равным интегралу ядра по сектору, а веса нормируются так, чтобы их сумма была равна единице.
	/// Сглаженное значение функции f в точке x равно w₀·f(x) + Σ wᵢ·(f(x − rᵢ) + f(x + rᵢ)).</para>
	/// <para>Секторы перебираются, пока ядро не станет пренебрежимо малым (модуль веса сектора меньше 1e-50 наибольшего) на участке,
	/// где радиус вырастает в 10 раз; нулевые и отрицательные участки внутри ядра не прерывают перебор. Радиус ядра ограничен
	/// значением в 10⁶ раз больше внешнего радиуса сектора с наибольшим весом, поэтому у ядер с тяжёлыми хвостами хвост обрезается.</para>
	/// <para>Скомпилированный объект неизменяем, и его можно использовать из нескольких потоков.</para>
	/// </remarks>
	public class BlurCompiler
	{
		private class CompileItem
		{
			public readonly double[] ArrayRadius;
			public readonly double[] ArrayVolumes;
			public readonly BlurFunctionHandler Filter;

			public CompileItem(double[] arrayRadius, double[] arrayVolumes, BlurFunctionHandler filter)
			{
				ArrayRadius = arrayRadius;
				ArrayVolumes = arrayVolumes;
				Filter = filter;
			}
		}

		//private static readonly CompileItem[] CompileItemsCache = new CompileItem[9];

		//private static volatile CompileItem _defaultCompileItem = null;

		private readonly CompileItem _compileItem;

		#region Static

        private const double GausDivider = 1/Math.SqrtPi;

        private static double _GausFunction(double r)
		{
			const double sign = 2;
			return sign * GausDivider * Math.Exp(-sign * sign * r * r);
		}

		/// <summary>
		/// Получает ядро по умолчанию — гауссово ядро f(r) = 2/√π·exp(−4r²) со среднеквадратичным отклонением
		/// σ = 1/(2√2) ≈ 0.354 и интегралом по всей прямой, равным единице.
		/// </summary>
		public static BlurFunctionHandler DefaultFilter => _GausFunction;

	    #endregion

		#region Truncation

		//Сектор считается пренебрежимо малым, если модуль его объёма меньше этой доли наибольшего модуля объёма сектора
		private const double RelativeLimit = 1E-50D;

		//Ядро считается закончившимся, если после последнего значимого сектора радиус вырос во столько раз,
		//а значимых секторов больше не встретилось (так перекрываются нулевые участки внутри ядра)
		private const double SupportGap = 10D;

		//Радиус ядра ограничен этим множителем относительно внешнего радиуса сектора с наибольшим объёмом (тяжёлые хвосты)
		private const double MaxSpread = 1E6D;

		//Предельное число секторов
		private const int MaxSectors = 1000;

		//Сумма весов считается нулевой, если она по модулю меньше этой доли суммы модулей весов
		private const double ZeroWeightTolerance = 1E-6D;

		/// <summary>
		/// Проверяет, что объём сектора — конечное число.
		/// </summary>
		private static void CheckVolume(double volume)
		{
			if (double.IsNaN(volume) || double.IsInfinity(volume))
			{
				throw new ArgumentException("Функция ядра должна возвращать конечные значения при radius ≥ 0.", "filter");
			}
		}

		#endregion

		#region Compile

		/// <summary>
		/// Вычисляет площадь кривой вокруг точки center.
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="center"></param>
		/// <param name="dr"></param>
		/// <returns></returns>
		private static double GetArea(BlurFunctionHandler filter, double center, double dr)
		{
			//Степень многочлена которым будем аппроксимировать исходную функцию
			const int deg = 8;

			//Метод многочленов deg-степени
			var points = new PointD[deg + 1];

			//Левая граница
			var rm = center - dr;

			for (var i = 0; i <= deg; i++)
			{
				var x = i * 2D * dr / deg;

				//Ядро симметрично, а делегат определён для radius ≥ 0: прежде центральный сектор [−dr, dr] передавал
				//ядру отрицательный радиус, и, например, exp(−√r) давал NaN во всех весах.
				points[i] = new PointD(i, filter(Math.Abs(rm + x)));
			}

			//Строим многочлен deg-степени проходящий через заданные точки
			var polynom = Polynomial.GetPolynomial(points);

			//Вычисляем площадь фигуры ограниченной полиномом
			return polynom.GetArea(0, deg) * 2D * dr / deg;




			/*
			const int deg = 8;

			//Метод многочленов deg-степени
			PointD[] points = new Point2D[deg + 1];

			double rm = center - dr / 2D;

			for (int i = 0; i <= deg; i++)
			{
			    double x = i * dr / deg;
			    points[i] = new PointD(x, filter(rm + x));
			}

			return Polynomial.Resolve(points).GetArea(0, dr);
			*/

		}

	    /// <summary>
	    /// Заполняет вспомогательные данные методом многочленов deg-степени.
	    /// </summary>
	    /// <param name="filter">Фильтр-функция</param>
	    /// <param name="accuracy">Относительный шаг разбиения ядра на секторы.</param>
	    private static CompileItem CompileImplementation(BlurFunctionHandler filter, double accuracy = 1D)
		{
			#region Начальные значения
			
			var drStart = 0.18256D * accuracy;
			const double drMultiplier = 1.16D;

			var listRadius = new List<double>();
			var listVolumes = new List<double>();

			//Начальные значение радиуса
			double r = 0;

			//Начальное значение ширины сектора
			var dr = drStart / 2D;

			#endregion

			#region Вычисляем и заполняем площади

			var volume = GetArea(filter, r, dr);
			CheckVolume(volume);

			//Сохраняем параметры текущей итерации
			listRadius.Add(0);
			listVolumes.Add(volume);

			//Прежде перебор останавливался на первом секторе с объёмом меньше 1e-50·f(0). Поэтому ядро с f(0) = 0
			//не останавливалось вовсе (System.Exception после 1000 секторов), ядро с отрицательными лепестками обрезалось
			//на первом отрицательном секторе, ядро с нулевым промежутком — в начале промежутка, а ядро с тяжёлым хвостом
			//доходило до радиуса 1e49. Теперь малость сектора оценивается по модулю относительно наибольшего сектора,
			//ядро считается закончившимся только после длинного участка пренебрежимо малых секторов, а радиус ограничен.

			//Наибольший модуль объёма сектора и внешний радиус этого сектора
			var maxVolume = Math.Abs(volume);
			var peakEdge = dr;

			//Внешний радиус последнего значимого сектора
			var lastEdge = dr;

			do
			{
				//Предыдущее значение
				var saveDr = dr;

				//Текущее значение
				dr *= drMultiplier;

				r += saveDr + dr;

				//Метод многочленов
				volume = GetArea(filter, r, dr);
				CheckVolume(volume);

				//Сохраняем параметры текущей итерации
				listRadius.Add(r);
				listVolumes.Add(volume);

				var edge = r + dr;
				var absVolume = Math.Abs(volume);

				if (absVolume > maxVolume)
				{
					maxVolume = absVolume;
					peakEdge = edge;
				}

				if (absVolume > RelativeLimit * maxVolume)
				{
					lastEdge = edge;
				}

				//Ядро закончилось: после последнего значимого сектора радиус вырос в SupportGap раз
				if (maxVolume > 0 && edge >= SupportGap * lastEdge)
				{
					break;
				}

				//Тяжёлый хвост либо нулевое ядро
				if (edge >= MaxSpread * peakEdge)
				{
					break;
				}

			    if (listRadius.Count > MaxSectors)
			    {
			        throw new ArgumentException("Не удалось построить ядро: функция не убывает к нулю с ростом радиуса либо шаг accuracy слишком мал.", nameof(filter));
			    }
			} while (true);

			#endregion

			#region Отбрасываем пренебрежимо малые секторы в конце

			//Как и прежде, сохраняется первый пренебрежимо малый сектор после последнего значимого,
			//поэтому для ядра по умолчанию секторы и веса не изменились.
			var count = 1;
			for (var i = listVolumes.Count - 1; i > 0; i--)
			{
				if (Math.Abs(listVolumes[i]) > RelativeLimit * maxVolume)
				{
					count = i + 1;
					break;
				}
			}

			count = Math.Min(count + 1, listVolumes.Count);
			listRadius.RemoveRange(count, listRadius.Count - count);
			listVolumes.RemoveRange(count, listVolumes.Count - count);

			#endregion

			#region Проверяем сумму весов

			var vSum = listVolumes[0];
			var absSum = Math.Abs(listVolumes[0]);
			for (var i = 1; i < listVolumes.Count; i++)
			{
				vSum += 2 * listVolumes[i];
				absSum += 2 * Math.Abs(listVolumes[i]);
			}

			//Прежде ядро с нулевой суммой (например, вейвлет Рикера) молча нормировалось делением на погрешность,
			//а нулевое ядро давало NaN во всех весах.
			if (!(Math.Abs(vSum) > ZeroWeightTolerance * absSum))
			{
				throw new ArgumentException("Сумма весов ядра равна нулю или пренебрежимо мала по сравнению с суммой их модулей, поэтому ядро нельзя нормировать.", nameof(filter));
			}

			#endregion

			#region Нормируем объемы

			if (!vSum.Equals(1D))
			{
				for (var i = 0; i < listVolumes.Count; i++)
				{
					listVolumes[i] /= vSum;
				}
			}

			#endregion

			return new CompileItem(listRadius.ToArray(), listVolumes.ToArray(), filter);
		}

	    /// <summary>
	    /// Заполняет вспомогательные данные методом многочленов deg-степени.
	    /// </summary>
	    /// <param name="filter">Фильтр-функция</param>
	    /// <param name="accuracy">Относительный шаг разбиения ядра на секторы.</param>
	    private static CompileItem Compile(BlurFunctionHandler filter, double accuracy = 1D)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			//Прежде значение не проверялось: при 0 все веса были 0/0 = NaN, при отрицательном получались два бессмысленных
			//сектора, а при NaN перебор шёл 1000 итераций и заканчивался System.Exception.
			if (!(accuracy > 0) || double.IsInfinity(accuracy))
			{
				throw new ArgumentOutOfRangeException(nameof(accuracy), accuracy, "Шаг разбиения ядра accuracy должен быть положительным конечным числом.");
			}

			return CompileImplementation(filter, accuracy);



			/*
			CompileItem result = CompileItemsCache[deg];

			if (result == null)
			{
			    if (deg >= 0 && deg <= 8)
			    {
				CompileItemsCache[deg] = result = CompileImplementation(filter, deg);
			    }
			    else
			    {
				throw new ArgumentException("Значение deg должно быть от 0 до 8");
			    }

			}

			return result;
			 * */
		}

		#endregion

		#region Public Items

		/// <summary>
		/// Получает функцию ядра, по которой скомпилирован фильтр.
		/// </summary>
		public BlurFunctionHandler Filter => _compileItem.Filter;

	    /// <summary>
	    /// Возвращает сглаженное значение векторной функции <paramref name="function"/> в точке <paramref name="x"/>:
	    /// w₀·f(x) + Σ wᵢ·(f(x − rᵢ) + f(x + rᵢ)), где rᵢ и wᵢ — узлы и нормированные веса скомпилированного ядра.
	    /// </summary>
	    /// <param name="function">Сглаживаемая функция. Вызывается в точках, удалённых от <paramref name="x"/> на радиус ядра,
	    /// поэтому должна быть определена на всей числовой прямой.</param>
	    /// <param name="x">Точка, в которой вычисляется сглаженное значение.</param>
	    /// <returns>Сглаженное значение функции.</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="function"/> равна null.</exception>
	    public Point3D GetValue3D(Func<double, Point3D> function, double x)
		{
			if (function == null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			var result = function(x) * _compileItem.ArrayVolumes[0];
			for (var i = 1; i < _compileItem.ArrayRadius.Length; i++)
			{
				var r = _compileItem.ArrayRadius[i];
				result.Add((function(x - r) + function(x + r)) * _compileItem.ArrayVolumes[i]);
			}

			return result;
		}




		/// <summary>
		/// Возвращает сглаженное значение векторной функции <paramref name="function"/> в точке <paramref name="x"/> с ядром,
		/// растянутым в <paramref name="scale"/> раз: w₀·f(x) + Σ wᵢ·(f(x − scale·rᵢ) + f(x + scale·rᵢ)).
		/// </summary>
		/// <param name="function">Сглаживаемая функция, определённая на всей числовой прямой.</param>
		/// <param name="x">Точка, в которой вычисляется сглаженное значение.</param>
		/// <param name="scale">Масштаб ядра, не меньше нуля. При 0 возвращается само значение функции, при 1 результат совпадает с
		/// <see cref="GetValue3D(Func{double, Point3D}, double)"/>.</param>
		/// <returns>Сглаженное значение функции.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="function"/> равна null.</exception>
		/// <exception cref="ArgumentException"><paramref name="scale"/> меньше нуля.</exception>
		/// <exception cref="NotImplementedException"><paramref name="scale"/> равен положительной бесконечности.</exception>
		public Point3D GetValue3D(Func<double, Point3D> function, double x, double scale = 1)
		{
			//При scale = Infinity, получается средняя значение кривой не зависящее от x
			//При scale = 0, получается сама кривая
			//При scale = 1, значение по умолчанию

			if (function == null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			if (scale < 0)
			{
				throw new ArgumentException("Значение scale не должно быть меньше нуля", nameof(scale));
			}
		    if (double.IsPositiveInfinity(scale))
		    {
		        throw new NotImplementedException("Пока не реализовано");
		    }
		    if (scale.Equals(0D))
		    {
		        return function(x);
		    }

		    var result = function(x) * _compileItem.ArrayVolumes[0];
			for (var i = 1; i < _compileItem.ArrayRadius.Length; i++)
			{
				var r = _compileItem.ArrayRadius[i];
				result.Add((function(x - scale * r) + function(x + scale * r)) * _compileItem.ArrayVolumes[i]);
			}

			return result;
		}

	    /// <summary>
	    /// Возвращает приращение сглаженной функции на отрезке [x − delta, x + delta]:
	    /// G(x + delta) − G(x − delta), где G — функция <paramref name="function"/>, сглаженная ядром масштаба <paramref name="scale"/>.
	    /// </summary>
	    /// <param name="function">Сглаживаемая функция, определённая на всей числовой прямой.</param>
	    /// <param name="x">Середина отрезка.</param>
	    /// <param name="delta">Половина длины отрезка.</param>
	    /// <param name="scale">Масштаб ядра, не меньше нуля (см. <see cref="GetValue3D(Func{double, Point3D}, double, double)"/>).</param>
	    /// <returns>Приращение сглаженной функции.</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="function"/> равна null.</exception>
	    /// <exception cref="ArgumentException"><paramref name="scale"/> меньше нуля.</exception>
	    public Point3D GetDifferential(Func<double, Point3D> function, double x, double delta, double scale = 1)
		{
			return GetValue3D(function, x + delta, scale) - GetValue3D(function, x - delta, scale);
		}

		/// <summary>
		/// Возвращает производную сглаженной функции в точке <paramref name="x"/>, оценённую центральной разностью:
		/// (G(x + delta) − G(x − delta)) / (2·delta).
		/// </summary>
		/// <param name="function">Сглаживаемая функция, определённая на всей числовой прямой.</param>
		/// <param name="x">Точка, в которой вычисляется производная.</param>
		/// <param name="delta">Шаг центральной разности, больше нуля.</param>
		/// <returns>Производная сглаженной функции.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="function"/> равна null.</exception>
		public Point3D GetDerivative(Func<double, Point3D> function, double x, double delta)
		{

			return GetDifferential(function, x, delta) / (2 * delta);
		}

		/// <summary>
		/// Возвращает сглаженное значение функции <paramref name="function"/> в точке <paramref name="x"/>:
		/// w₀·f(x) + Σ wᵢ·(f(x − rᵢ) + f(x + rᵢ)), где rᵢ и wᵢ — узлы и нормированные веса скомпилированного ядра.
		/// </summary>
		/// <param name="function">Сглаживаемая функция, определённая на всей числовой прямой.</param>
		/// <param name="x">Точка, в которой вычисляется сглаженное значение.</param>
		/// <returns>Сглаженное значение функции.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="function"/> равна null.</exception>
		public double GetValue(Func<double, double> function, double x)
		{
			if (function == null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			var result = function(x) * _compileItem.ArrayVolumes[0];
			for (var i = 1; i < _compileItem.ArrayRadius.Length; i++)
			{
				var r = _compileItem.ArrayRadius[i];
				result += (function(x - r) + function(x + r)) * _compileItem.ArrayVolumes[i];
			}

			return result;
		}

		/// <summary>
		/// Возвращает сглаженное значение функции <paramref name="function"/> в точке <paramref name="x"/> с ядром,
		/// растянутым в <paramref name="scale"/> раз: w₀·f(x) + Σ wᵢ·(f(x − scale·rᵢ) + f(x + scale·rᵢ)).
		/// </summary>
		/// <param name="function">Сглаживаемая функция, определённая на всей числовой прямой.</param>
		/// <param name="x">Точка, в которой вычисляется сглаженное значение.</param>
		/// <param name="scale">Масштаб ядра, не меньше нуля. При 0 возвращается само значение функции, при 1 результат совпадает с
		/// <see cref="GetValue(Func{double, double}, double)"/>.</param>
		/// <returns>Сглаженное значение функции.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="function"/> равна null.</exception>
		/// <exception cref="ArgumentException"><paramref name="scale"/> меньше нуля.</exception>
		/// <exception cref="NotImplementedException"><paramref name="scale"/> равен положительной бесконечности.</exception>
		public double GetValue(Func<double, double> function, double x, double scale = 1)
		{
			//При scale = Infinity, получается средняя значение кривой не зависящее от x
			//При scale = 0, получается сама кривая
			//При scale = 1, значение по умолчанию

			if (function == null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			if (scale < 0)
			{
				throw new ArgumentException("Значение scale не должно быть меньше нуля", nameof(scale));
			}
		    if (double.IsPositiveInfinity(scale))
		    {
		        throw new NotImplementedException("Пока не реализовано");
		    }
		    if (scale.Equals(0))
		    {
		        return function(x);
		    }

		    var result = function(x) * _compileItem.ArrayVolumes[0];
			for (var i = 1; i < _compileItem.ArrayRadius.Length; i++)
			{
				var r = _compileItem.ArrayRadius[i];
				result += (function(x - scale * r) + function(x + scale * r)) * _compileItem.ArrayVolumes[i];
			}

			return result;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="BlurCompiler"/> с гауссовым ядром <see cref="DefaultFilter"/>.
		/// </summary>
		/// <param name="accuracy">Относительный шаг дискретизации ядра (а не точность): центральный сектор имеет ширину
		/// 0.18256·accuracy, каждый следующий сектор в 1.16 раза шире предыдущего. Чем меньше значение, тем точнее и медленнее
		/// дискретизация; при больших значениях ядро почти целиком попадает в центральный сектор, и сглаживание пропадает
		/// (при accuracy = 10 вес центрального узла ядра по умолчанию равен 0.99). По умолчанию 1.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="accuracy"/> не является положительным конечным числом.</exception>
		public BlurCompiler(double accuracy = 1D)
		{
			_compileItem = Compile(DefaultFilter, accuracy);
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="BlurCompiler"/> с заданным ядром.
		/// </summary>
		/// <param name="filter">Ядро фильтра: вес в зависимости от расстояния radius ≥ 0 до центра. Нормировать ядро не нужно.
		/// Отрицательные значения и нулевые участки допустимы, но значения должны быть конечными, а сумма весов — отличной от нуля.</param>
		/// <param name="accuracy">Относительный шаг дискретизации ядра (а не точность): центральный сектор имеет ширину
		/// 0.18256·accuracy, каждый следующий сектор в 1.16 раза шире предыдущего. Чем меньше значение, тем точнее и медленнее
		/// дискретизация; при больших значениях ядро почти целиком попадает в центральный сектор, и сглаживание пропадает. По умолчанию 1.</param>
		/// <exception cref="ArgumentNullException"><paramref name="filter"/> равен null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="accuracy"/> не является положительным конечным числом.</exception>
		/// <exception cref="ArgumentException">Ядро возвращает NaN или бесконечность, не убывает к нулю с ростом радиуса
		/// либо сумма его весов равна нулю (например, у вейвлета Рикера).</exception>
		public BlurCompiler(BlurFunctionHandler filter, double accuracy = 1D)
		{
			_compileItem = Compile(filter, accuracy);
		}

		#endregion
	}
}
