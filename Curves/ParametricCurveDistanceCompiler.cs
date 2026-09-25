using System;
using System.Threading;
using Ruzil3D.Approximation;
using Ruzil3D.Algebra;

namespace Ruzil3D.Curves
{
	/// <summary>
	/// Универсальный класс компилированной кривой для обращения к точкам через дистанцию.
	/// </summary>
	/// <remarks>При компиляции кривая делится на 2ᴬᶜᶜᵘʳᵃᶜʸ равных по параметру участков, их длины последовательно
	/// суммируются, а параметр как функция расстояния от начала кривой восстанавливается монотонной интерполяцией
	/// (см. <see cref="Type"/>). Поэтому <see cref="GetParameter"/> не убывает с ростом дистанции при любой точности.</remarks>
	public class ParametricCurveDistanceCompiler<T> where T : ParametricCurve
	{
		#region Types

		/// <summary>
		/// Монотонная интерполяция параметра кривой по расстоянию на сетке узлов.
		/// </summary>
		/// <remarks>Значения вычисляются в локальной координате клетки сетки и не выходят за пределы значений в её узлах.</remarks>
		private abstract class GridInterpolator : IMonotonicFunctionApproximation
		{
			/// <summary>
			/// Расстояния в узлах сетки, строго возрастают.
			/// </summary>
			protected readonly double[] X;

			/// <summary>
			/// Параметры кривой в узлах сетки, строго возрастают.
			/// </summary>
			protected readonly double[] Y;

			protected GridInterpolator(double[] x, double[] y)
			{
				X = x;
				Y = y;
			}

			public double LArgument => X[0];

			public double RArgument => X[X.Length - 1];

			public double LValue => Y[0];

			public double RValue => Y[Y.Length - 1];

			public double MinValue => LValue;

			public double MaxValue => RValue;

			public double GetValue(double x)
			{
				if (double.IsNaN(x))
				{
					return double.NaN;
				}

				if (x <= LArgument)
				{
					return LValue;
				}

				if (x >= RArgument)
				{
					return RValue;
				}

				var index = FindCell(X, x);
				var result = GetCellValue(index, (x - X[index])/(X[index + 1] - X[index]));

				//Значение не выходит за пределы клетки даже с учётом погрешности округления, поэтому функция монотонна.
				return Clamp(result, Y[index], Y[index + 1]);
			}

			public double GetArgument(double y)
			{
				if (double.IsNaN(y))
				{
					return double.NaN;
				}

				if (y <= LValue)
				{
					return LArgument;
				}

				if (y >= RValue)
				{
					return RArgument;
				}

				var index = FindCell(Y, y);
				var result = X[index] + GetCellArgument(index, y)*(X[index + 1] - X[index]);

				return Clamp(result, X[index], X[index + 1]);
			}

			/// <summary>
			/// Возвращает значение интерполяции в клетке сетки.
			/// </summary>
			/// <param name="index">Индекс клетки.</param>
			/// <param name="u">Локальная координата в клетке от 0 до 1.</param>
			protected abstract double GetCellValue(int index, double u);

			/// <summary>
			/// Возвращает локальную координату в клетке сетки, в которой интерполяция принимает заданное значение.
			/// </summary>
			/// <param name="index">Индекс клетки.</param>
			/// <param name="y">Значение от Y[index] до Y[index + 1].</param>
			protected abstract double GetCellArgument(int index, double y);

			/// <summary>
			/// Возвращает индекс клетки i, для которой values[i] ≤ value &lt; values[i + 1].
			/// </summary>
			private static int FindCell(double[] values, double value)
			{
				var min = 0;
				var max = values.Length - 1;

				while (max - min > 1)
				{
					var mid = (min + max)/2;

					if (value < values[mid])
					{
						max = mid;
					}
					else
					{
						min = mid;
					}
				}

				return min;
			}

			private static double Clamp(double value, double min, double max)
			{
				return value < min ? min : value > max ? max : value;
			}
		}

		/// <summary>
		/// Кусочно-линейная интерполяция.
		/// </summary>
		private sealed class LinearInterpolator : GridInterpolator
		{
			public LinearInterpolator(double[] x, double[] y) : base(x, y)
			{
			}

			protected override double GetCellValue(int index, double u)
			{
				return Y[index] + (Y[index + 1] - Y[index])*u;
			}

			protected override double GetCellArgument(int index, double y)
			{
				return (y - Y[index])/(Y[index + 1] - Y[index]);
			}
		}

		/// <summary>
		/// Монотонная кусочно-кубическая интерполяция Эрмита.
		/// </summary>
		/// <remarks>Производные в узлах берутся от натурального кубического сплайна и при необходимости уменьшаются по
		/// Фричу — Карлсону так, чтобы интерполяция оставалась монотонной. Для гладких данных ограничение не срабатывает,
		/// и результат совпадает с натуральным кубическим сплайном.</remarks>
		private sealed class MonotoneCubicInterpolator : GridInterpolator
		{
			/// <summary>
			/// Производные интерполяции в узлах сетки.
			/// </summary>
			private readonly double[] _slopes;

			public MonotoneCubicInterpolator(double[] x, double[] y) : base(x, y)
			{
				_slopes = GetSlopes(x, y);
			}

			protected override double GetCellValue(int index, double u)
			{
				//Многочлен Эрмита в локальной координате клетки. Прежде использовались многочлены сплайна в глобальной
				//координате: около резкого поворота кривой их коэффициенты достигали 1e15, и значения теряли всякую точность.
				var h = X[index + 1] - X[index];
				var v = 1 - u;

				return Y[index] + (Y[index + 1] - Y[index])*u*u*(3 - 2*u) + h*u*v*(_slopes[index]*v - _slopes[index + 1]*u);
			}

			protected override double GetCellArgument(int index, double y)
			{
				//Интерполяция в клетке монотонна, поэтому корень находится делением отрезка пополам.
				var min = 0D;
				var max = 1D;

				for (var i = 0; i < 64; i++)
				{
					var mid = (min + max)/2;
					if (mid <= min || mid >= max)
					{
						break;
					}

					if (GetCellValue(index, mid) < y)
					{
						min = mid;
					}
					else
					{
						max = mid;
					}
				}

				return (min + max)/2;
			}

			/// <summary>
			/// Возвращает производные монотонной интерполяции в узлах.
			/// </summary>
			private static double[] GetSlopes(double[] x, double[] y)
			{
				var cells = x.Length - 1;
				var secants = new double[cells];

				for (var i = 0; i < cells; i++)
				{
					secants[i] = (y[i + 1] - y[i])/(x[i + 1] - x[i]);
				}

				//Производные натурального кубического сплайна: трёхдиагональная система
				//2m₀ + m₁ = 3δ₀, hᵢmᵢ₋₁ + 2(hᵢ₋₁ + hᵢ)mᵢ + hᵢ₋₁mᵢ₊₁ = 3(hᵢδᵢ₋₁ + hᵢ₋₁δᵢ), mₙ₋₁ + 2mₙ = 3δₙ₋₁,
				//где hᵢ — шаг сетки, δᵢ — наклон хорды, решается методом прогонки.
				var slopes = new double[cells + 1];
				var alpha = new double[cells + 1];

				alpha[0] = 0.5;
				slopes[0] = 1.5*secants[0];

				for (var i = 1; i <= cells; i++)
				{
					double lower, diagonal, upper, right;

					if (i < cells)
					{
						var h0 = x[i] - x[i - 1];
						var h1 = x[i + 1] - x[i];

						lower = h1;
						diagonal = 2*(h0 + h1);
						upper = h0;
						right = 3*(h1*secants[i - 1] + h0*secants[i]);
					}
					else
					{
						lower = 1;
						diagonal = 2;
						upper = 0;
						right = 3*secants[i - 1];
					}

					var denominator = diagonal - lower*alpha[i - 1];

					alpha[i] = upper/denominator;
					slopes[i] = (right - lower*slopes[i - 1])/denominator;
				}

				for (var i = cells - 1; i >= 0; i--)
				{
					slopes[i] -= alpha[i]*slopes[i + 1];
				}

				//Ограничение Фрича — Карлсона. Прежде натуральный сплайн использовался без него: на ступенчатых данных
				//(точка возврата или резкий поворот кривой) он выходил за пределы значений в узлах, и параметр мог
				//оказаться на другом конце кривой.
				for (var i = 0; i <= cells; i++)
				{
					if (!(slopes[i] > 0))
					{
						slopes[i] = 0;
					}
				}

				for (var i = 0; i < cells; i++)
				{
					var a = slopes[i]/secants[i];
					var b = slopes[i + 1]/secants[i];
					var norm = a*a + b*b;

					if (norm > 9)
					{
						var scale = 3/System.Math.Sqrt(norm);

						slopes[i] = scale*a*secants[i];
						slopes[i + 1] = scale*b*secants[i];
					}
				}

				return slopes;
			}
		}

		/// <summary>
		/// Интерполяция через функцию, обратную функции выпрямления кривой.
		/// </summary>
		private sealed class CustomInterpolator : IMonotonicFunctionApproximation
		{
			private readonly Func<double, double> _invertRectification;
			private readonly Func<double, double> _rectification;

			public CustomInterpolator(Func<double, double> invertRectification, Func<double, double> rectification, double length)
			{
				_invertRectification = invertRectification;
				_rectification = rectification;
				RArgument = length;
			}

			public double LArgument => 0;

			public double RArgument { get; }

			public double LValue => 0;

			public double RValue => 1;

			public double MinValue => 0;

			public double MaxValue => 1;

			public double GetValue(double x)
			{
				var result = _invertRectification(x);

				//Погрешность округления не выводит параметр за пределы кривой.
				return result < 0 ? 0 : result > 1 ? 1 : result;
			}

			public double GetArgument(double y)
			{
				return _rectification(y < 0 ? 0 : y > 1 ? 1 : y);
			}
		}

		/// <summary>
		/// Интерполяция для кривой нулевой или неопределённой длины.
		/// </summary>
		private sealed class ConstantInterpolator : IMonotonicFunctionApproximation
		{
			private readonly double _value;

			public ConstantInterpolator(double value, double length)
			{
				_value = value;
				RArgument = length;
			}

			public double LArgument => 0;

			public double RArgument { get; }

			public double LValue => _value;

			public double RValue => _value;

			public double MinValue => _value;

			public double MaxValue => _value;

			public double GetValue(double x)
			{
				return double.IsNaN(x) ? double.NaN : _value;
			}

			public double GetArgument(double y)
			{
				return double.IsNaN(y) ? double.NaN : 0;
			}
		}

		#endregion

		//Количество замен кривых во всех компиляторах с данным типом кривых. По нему последовательности кривых
		//(ParametricCurves) узнают, что сохранённые длины могли устареть, не проверяя все кривые при каждом обращении.
		private static int _curveReplacements;

		/// <summary>
		/// Получает количество замен кривой (свойство <see cref="Curve"/>) во всех компиляторах с данным типом кривых.
		/// </summary>
		internal static int CurveReplacements => Interlocked.CompareExchange(ref _curveReplacements, 0, 0);

		private T _curve;

		/// <summary>
		/// Получает и задает исходную кривую.
		/// </summary>
		/// <remarks>Задается конструктором. Изменение этого параметра сбрасывает результаты компиляции и как следствие
		/// приводит к повторной компиляции при следующем обращении к функциям <see cref="GetParameter"/> и <see cref="GetValue"/>.
		/// Последовательности кривых (<see cref="ParametricCurves{T}"/>), содержащие этот компилятор, учитывают новую длину кривой.</remarks>
		public T Curve
		{
			get { return _curve; }
			set
			{
				if (_curve != value)
				{
					var replaced = _curve != null;

					_curve = value;
					Reset();

					//Прежде последовательности кривых продолжали использовать длину прежней кривой.
					if (replaced)
					{
						Interlocked.Increment(ref _curveReplacements);
					}
				}
			}
		}

		private int _accuracy;

		/// <summary>
		/// Получает и задает точность аппроксимации.
		/// </summary>
		/// <remarks>Кривая делится на 2ᵛᵃˡᵘᵉ равных по параметру участков, поэтому время компиляции и занимаемая память
		/// растут вдвое с каждой единицей точности. Изменение этого параметра сбрасывает результаты компиляции и как следствие приводит к повторной компиляции при следующем обращении к функциям <see cref="GetParameter"/> и <see cref="GetValue"/>.</remarks>
		/// <exception cref="ArgumentOutOfRangeException">Значение меньше 1 или больше 30.</exception>
		public int Accuracy
		{
			get { return _accuracy; }
			set
			{
				//Проверка стоит до сравнения с текущим значением: прежде значение 0 проходило через конструктор без проверки.
				//Кривая делится на 2^value участков, поэтому при value > 30 сдвиг 1 << value переполнялся.
				if (value < 1 || value > MaxAccuracy)
				{
					throw new ArgumentOutOfRangeException(nameof(Accuracy), value,
						"Точность аппроксимации должна быть от 1 до " + MaxAccuracy + ".");
				}

				if (_accuracy != value)
				{
					_accuracy = value;
					Reset();
				}
			}
		}

		private const int MaxAccuracy = 30;

		private EApproximationType _type;

		/// <summary>
		/// Получает и задает способ аппроксимации.
		/// </summary>
		/// <remarks>
		/// <see cref="EApproximationType.Linear"/> и <see cref="EApproximationType.HighSpeed"/> означают кусочно-линейную
		/// интерполяцию параметра по расстоянию, а <see cref="EApproximationType.Default"/>, <see cref="EApproximationType.Cubic"/>
		/// и <see cref="EApproximationType.HighQuality"/> — монотонную кусочно-кубическую интерполяцию: натуральный кубический
		/// сплайн, производные которого при необходимости ограничиваются так, чтобы параметр не убывал. Таким образом,
		/// <see cref="EApproximationType.HighQuality"/> совпадает с <see cref="EApproximationType.Default"/>, а
		/// <see cref="EApproximationType.HighSpeed"/> — с <see cref="EApproximationType.Linear"/>; точность обоих способов
		/// повышается свойством <see cref="Accuracy"/>. Для кривых с известной обратной функцией выпрямления (например,
		/// натуральных) способ аппроксимации не используется.
		/// Изменение этого параметра сбрасывает результаты компиляции и как следствие приводит к повторной компиляции при следующем обращении к функциям <see cref="GetParameter"/> и <see cref="GetValue"/>.</remarks>
		public EApproximationType Type
		{
			get { return _type; }
			set
			{
				if (_type != value)
				{
					_type = value;
					Reset();
				}
			}
		}

		#region Approx

		private void Reset()
		{
			_approx = null;
		}

		private IMonotonicFunctionApproximation _approx;

		private IMonotonicFunctionApproximation Approx
		{
			get
			{
				//Поле читается один раз, поэтому одновременный сброс из другого потока не приводит к null.
				var approx = _approx;

				if (approx == null)
				{
					_approx = approx = Compile();
				}

				return approx;
			}
		}

		#endregion

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ParametricCurveDistanceCompiler{T}"/> для обращения к точкам на кривой через дистанцию.
		/// </summary>
		/// <param name="curve">Исходная кривая.</param>
		/// <param name="acc">Показатель разбиения. Кривая делится на 2ᵃᶜᶜ участков.</param>
		/// <param name="type">Способ аппроксимации.</param>
		/// <exception cref="ArgumentOutOfRangeException">Значение <paramref name="acc"/> меньше 1 или больше 30.</exception>
		public ParametricCurveDistanceCompiler(T curve, int acc = 6, EApproximationType type = EApproximationType.Default)
		{
			Curve = curve;
			Accuracy = acc;
			Type = type;
		}

		private IMonotonicFunctionApproximation Compile()
		{
			var curve = Curve;
			var length = curve.Length;

			if (double.IsNaN(length) || double.IsInfinity(length))
			{
				return new ConstantInterpolator(double.NaN, length);
			}

			//Кривая нулевой длины (например, отрезок с совпадающими концами): прежде расстояние делилось на нулевую
			//длину, и вместо начальной точки кривой получались точки NaN.
			if (!(length > 0))
			{
				return new ConstantInterpolator(0, 0);
			}

			var rectification = curve.GetInvertRectificationFunction();
			if (rectification != null)
			{
				return new CustomInterpolator(rectification, t => curve.GetDistance(0, t), length);
			}

			var count = curve.IsNatural ? 1 : 1 << Accuracy;
			var step = 1D/count;

			//Расстояния до узлов — суммы длин участков между соседними узлами, поэтому они не убывают. Прежде расстояние
			//до каждого узла было отдельным интегралом от 0: при большой точности погрешность интегрирования превышала шаг
			//сетки, узлы переставали возрастать, и около точки возврата параметр оказывался на другом конце кривой.
			var sums = new double[count + 1];
			var sum = 0D;

			for (var i = 1; i <= count; i++)
			{
				var distance = curve.GetDistance((i - 1)*step, i*step);
				if (distance > 0)
				{
					sum += distance;
				}

				sums[i] = sum;
			}

			//Суммы приводятся к длине кривой Length, чтобы расстоянию Length соответствовал параметр 1. Узлы с
			//совпадающими расстояниями (участки нулевой длины) пропускаются.
			var scale = sum > 0 ? length/sum : 0;

			var x = new double[count + 1];
			var y = new double[count + 1];
			var last = 0;

			for (var i = 1; i < count; i++)
			{
				var distance = sums[i]*scale;

				if (distance > x[last] && distance < length)
				{
					last++;
					x[last] = distance;
					y[last] = i*step;
				}
			}

			last++;
			x[last] = length;
			y[last] = 1;

			Array.Resize(ref x, last + 1);
			Array.Resize(ref y, last + 1);

			//Аппроксимация (интерполяция) t от расстояния
			switch (Type)
			{
				case EApproximationType.HighSpeed:
				case EApproximationType.Linear:
					return new LinearInterpolator(x, y);

				case EApproximationType.Default:
				case EApproximationType.HighQuality:
				case EApproximationType.Cubic:
				default:
					return new MonotoneCubicInterpolator(x, y);
			}
		}

		/// <summary>
		/// Получает параметр кривой соответствующий указанной дистанции.
		/// </summary>
		/// <param name="distance">Исходная дистанция: расстояние вдоль кривой от её начальной точки, от 0 до <see cref="ParametricCurve.Length"/>.</param>
		/// <returns>Параметр кривой соответствующий указанной дистанции. Для кривой нулевой длины возвращается 0.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Дистанция меньше 0 или больше длины кривой больше чем на погрешность округления.</exception>
		/// <remarks>Параметр не убывает с ростом дистанции; дистанциям 0 и <see cref="ParametricCurve.Length"/> соответствуют параметры 0 и 1.
		/// Дистанция, выходящая за отрезок [0, <see cref="ParametricCurve.Length"/>] не больше чем на 16 ulp длины (например, i·L/N при i = N),
		/// считается равной ближайшему концу.</remarks>
		public double GetParameter(double distance)
		{
			var approx = Approx;

			//Прежде выход за пределы кривой приводил, в зависимости от типа кривой, к ArgumentException без имени
			//параметра (из GetValue кривой) или к ArgumentOutOfRangeException для аргумента x (из интерполяции).
			//Выход на погрешность округления допускается: для дуг окружности прежде возвращался конец кривой, а i·L/N при i = N
			//бывает больше L (у дуги с L = 2.1·0.3 уже при N = 7).
			var tolerance = 16*MachineEpsilon*Math.Max(Math.Abs(approx.LArgument), Math.Abs(approx.RArgument));
			if (distance < approx.LArgument)
			{
				if (!(distance >= approx.LArgument - tolerance))
				{
					throw new ArgumentOutOfRangeException(nameof(distance), distance,
						"Дистанция должна быть от 0 до длины кривой (" + approx.RArgument + ").");
				}

				distance = approx.LArgument;
			}
			else if (distance > approx.RArgument)
			{
				if (!(distance <= approx.RArgument + tolerance))
				{
					throw new ArgumentOutOfRangeException(nameof(distance), distance,
						"Дистанция должна быть от 0 до длины кривой (" + approx.RArgument + ").");
				}

				distance = approx.RArgument;
			}

			return approx.GetValue(distance);
		}

		/// <summary>
		/// Машинный эпсилон: расстояние от 1 до следующего числа двойной точности.
		/// </summary>
		private const double MachineEpsilon = 2.220446049250313E-16;

		/// <summary>
		/// Получает точку на кривой соответствующую указанной дистанции.
		/// </summary>
		/// <param name="distance">Исходная дистанция: расстояние вдоль кривой от её начальной точки, от 0 до <see cref="ParametricCurve.Length"/>.</param>
		/// <returns>Точка на кривой соответствующую указанной дистанции. Для кривой нулевой длины возвращается её начальная точка.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Дистанция меньше 0 или больше длины кривой больше чем на погрешность округления.</exception>
		public Point3D GetValue(double distance)
		{
			return Curve.GetValue(GetParameter(distance));
		}

	}
}