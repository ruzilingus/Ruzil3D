using System;
using System.Collections.Generic;
using System.Linq;
using Ruzil3D.Algebra;

namespace Ruzil3D.Curves
{
	/// <summary>
	/// Кривые Безье произвольной степени, заданные узловыми точками и многочленами Бернштейна.
	/// </summary>
	/// <remarks>Названы в честь Сергея Натановича Бернштейна. Степень кривой на единицу меньше количества узловых точек,
	/// параметр кривой меняется от 0 до 1.</remarks>
	public class BernsteinCurve : ParametricCurve
	{
		#region Fields

		//Значение
		private Polynomial _polynomX0;
		private Polynomial _polynomY0;
		private Polynomial _polynomZ0;

		//Первая производная
		private Polynomial _polynomX1;
		private Polynomial _polynomY1;
		private Polynomial _polynomZ1;

		//Вторая производная
		private Polynomial _polynomX2;
		private Polynomial _polynomY2;
		private Polynomial _polynomZ2;

		//Третья производная
		private Polynomial _polynomX3;
		private Polynomial _polynomY3;
		private Polynomial _polynomZ3;

		#endregion

		/// <summary>
		/// Массив узловых точек из структур <see cref="Point3D"/>.
		/// </summary>
		protected readonly Point3D[] Points;

		/// <summary>
		/// Получает значение указывающее, что эквивалентная репараметризация <i>p = <see cref="ParametricCurve.Length"/>*t</i> данной кривой является натуральной.
		/// </summary>
		/// <remarks>Натуральность означает, что длина любого участка от <i>t₀</i> до <i>t₁</i> (0 ≤ <i>t₀</i> ≤ <i>t₁</i> ≤ 1) кривой равняется <i><see cref="ParametricCurve.Length"/>*(t₁ - t₀)</i>.</remarks>
		public sealed override bool IsNatural => Points.Length == 2;

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="BernsteinCurve"/> из перечислителя структур <see cref="Point3D"/>.
		/// </summary>
		/// <param name="points">Перечислитель структур <see cref="Point3D"/>.</param>
		/// <exception cref="ArgumentNullException">Параметр  <paramref name="points"/> имеет значение <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Количество точек меньше двух.</exception>
		/// <remarks>Количество точек должно быть не меньше двух.</remarks>
		public BernsteinCurve(IEnumerable<Point3D> points)
		{
			if (points == null)
			{
				throw new ArgumentNullException(nameof(points));
			}

			var pointsArray = points.ToArray();

			if (pointsArray.Length < 2)
			{
				throw new ArgumentException("Количество точек должно быть не менее двух.", nameof(points));
			}

			Points = pointsArray;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="BernsteinCurve"/> из массива структур <see cref="Point3D"/>.
		/// </summary>
		/// <param name="points">Массив структур <see cref="Point3D"/>.</param>
		/// <exception cref="ArgumentNullException">Параметр  <paramref name="points"/> имеет значение <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Количество точек меньше двух.</exception>
		/// <remarks>Количество точек должно быть не меньше двух.</remarks>
		protected BernsteinCurve(Point3D[] points)
		{
			if (points == null)
			{
				throw new ArgumentNullException(nameof(points));
			}

			if (points.Length < 2)
			{
				throw new ArgumentException("Количество точек должно быть не менее двух.", nameof(points));
			}

			Points = points;
		}

		#region Private

		private Polynomial GetXPolynom()
		{
			var deg = Points.Length - 1;
			var result = Polynomial.GetBernstein(0, deg)*Points[0].X;

			for (var i = 1; i <= deg; i++)
			{
				result += Polynomial.GetBernstein(i, deg)*Points[i].X;
			}

			return result;
		}

		private Polynomial GetYPolynom()
		{
			var deg = Points.Length - 1;
			var result = Polynomial.GetBernstein(0, deg)*Points[0].Y;

			for (var i = 1; i <= deg; i++)
			{
				result += Polynomial.GetBernstein(i, deg)*Points[i].Y;
			}

			return result;
		}

		private Polynomial GetZPolynom()
		{
			var deg = Points.Length - 1;
			var result = Polynomial.GetBernstein(0, deg)*Points[0].Z;

			for (var i = 1; i <= deg; i++)
			{
				result += Polynomial.GetBernstein(i, deg)*Points[i].Z;
			}

			return result;
		}

		/// <summary>
		/// Проверяет, что параметр кривой принадлежит отрезку [0, 1].
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <exception cref="ArgumentOutOfRangeException">Параметр меньше 0 или больше 1.</exception>
		private static void CheckParameter(double parameter)
		{
			//Прежде GetValue выбрасывал ArgumentException без имени параметра. Теперь выбрасывается ArgumentOutOfRangeException
			//(наследник ArgumentException) с именем параметра.
			if (parameter < 0 || parameter > 1)
			{
				throw new ArgumentOutOfRangeException(nameof(parameter), parameter,
					"Параметр \"" + nameof(parameter) + "\" должен быть от 0 до 1.");
			}
		}

		//Узловые точки производных кривой (годографов), умноженные на биномиальные коэффициенты; индекс — порядок производной.
		//Массив заполняется целиком и только затем публикуется (volatile — чтобы другой поток увидел его заполненным).
		private volatile Point3D[][] _derivativePoints;

		/// <summary>
		/// Возвращает узловые точки производной заданного порядка, умноженные на биномиальные коэффициенты.
		/// </summary>
		/// <param name="order">Порядок производной от 1 до степени кривой.</param>
		/// <returns>Точки C(m, i)·Dᵢ, где Dᵢ — узловые точки производной как кривой Безье степени m.</returns>
		private Point3D[] GetDerivativePoints(int order)
		{
			var result = _derivativePoints;

			if (result == null)
			{
				var degree = Points.Length - 1;
				var points = new Point3D[Points.Length];
				Points.CopyTo(points, 0);

				result = new Point3D[Points.Length][];

				for (var k = 1; k <= degree; k++)
				{
					//Узловые точки производной порядка k: n·(n − 1)·…·(n − k + 1)·Δᵏ Pᵢ.
					var count = degree - k + 1;
					for (var i = 0; i < count; i++)
					{
						points[i] = (points[i + 1] - points[i])*count;
					}

					var scaled = new Point3D[count];
					var binomial = 1D;
					for (var i = 0; i < count; i++)
					{
						scaled[i] = points[i]*binomial;
						binomial = binomial*(count - 1 - i)/(i + 1);
					}

					result[k] = scaled;
				}

				_derivativePoints = result;
			}

			return result[order];
		}

		/// <summary>
		/// Возвращает производную кривой заданного порядка, вычисленную по разностям узловых точек.
		/// </summary>
		/// <param name="order">Порядок производной, не меньше 1.</param>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Вектор производной, представленный структурой <see cref="Point3D"/>.</returns>
		/// <remarks>Производная вычисляется в базисе Бернштейна, поэтому при <i>t</i> = 0 и <i>t</i> = 1 результат точно равен
		/// соответствующей разности крайних узловых точек, например <i>n</i>·(<i>P</i>ₙ − <i>P</i>ₙ₋₁) для первой производной
		/// в конце кривой.</remarks>
		private Point3D GetDerivative(int order, double parameter)
		{
			if (order >= Points.Length)
			{
				return Point3D.Empty;
			}

			var points = GetDerivativePoints(order);
			var degree = points.Length - 1;
			var u = 1 - parameter;

			//Схема Горнера для многочлена в базисе Бернштейна: Σ C(m, i)·Dᵢ·tⁱ·(1 − t)ᵐ⁻ⁱ равна (1 − t)ᵐ·Σ C(m, i)·Dᵢ·sⁱ
			//при s = t/(1 − t) и tᵐ·Σ C(m, i)·Dᵢ·sᵐ⁻ⁱ при s = (1 − t)/t; выбирается вариант с s ≤ 1.
			Point3D result;
			double scale;
			double power = 1;

			if (parameter < 0.5)
			{
				var s = parameter/u;
				result = points[degree];
				for (var i = degree - 1; i >= 0; i--)
				{
					result = result*s + points[i];
				}

				scale = u;
			}
			else
			{
				var s = u/parameter;
				result = points[0];
				for (var i = 1; i <= degree; i++)
				{
					result = result*s + points[i];
				}

				scale = parameter;
			}

			for (var i = 0; i < degree; i++)
			{
				power *= scale;
			}

			return result*power;
		}

		#endregion

		#region Properties

		private Polynomial[] _polynomsX;



		/// <summary>
		/// Получает полином для вычисления X координаты производной степени <paramref name="order"/> кривой.
		/// </summary>
		/// <param name="order">Степень производной.</param>
		/// <returns>Полином для вычисления X координаты производной степени <paramref name="order"/> кривой.</returns>
		public Polynomial GetXPolynom(int order)
		{
			if (_polynomsX == null)
			{
				_polynomsX = new Polynomial[Points.Length];
			}

			if (order < 0)
			{
				throw new ArgumentException("Параметр \"" + nameof(order) + "\" не должен быть меньше 0.", nameof(order));
			}
			
			if (order >= Points.Length)
			{
				return Polynomial.Empty;
			}

			var result = _polynomsX[order];

			if (result == null)
			{
				_polynomsX[order] = result = order == 0 ? GetXPolynom() : GetXPolynom(order - 1).GetDerivative();
			}

			return result;
		}
		

		private Polynomial[] _polynomsY;

		/// <summary>
		/// Получает полином для вычисления Y координаты производной степени <paramref name="order"/> кривой.
		/// </summary>
		/// <param name="order">Степень производной.</param>
		/// <returns>Полином для вычисления Y координаты производной степени <paramref name="order"/> кривой.</returns>
		public Polynomial GetYPolynom(int order)
		{
			if (_polynomsY == null)
			{
				_polynomsY = new Polynomial[Points.Length];
			}

			if (order < 0)
			{
				throw new ArgumentException("Параметр \"" + nameof(order) + "\" не должен быть меньше 0.", nameof(order));
			}

			if (order >= Points.Length)
			{
				return Polynomial.Empty;
			}

			var result = _polynomsY[order];

			if (result == null)
			{
				_polynomsY[order] = result = order == 0 ? GetYPolynom() : GetYPolynom(order - 1).GetDerivative();
			}

			return result;
		}

		private Polynomial[] _polynomsZ;

		/// <summary>
		/// Получает полином для вычисления Z координаты производной степени <paramref name="order"/> кривой.
		/// </summary>
		/// <param name="order">Степень производной.</param>
		/// <returns>Полином для вычисления Z координаты производной степени <paramref name="order"/> кривой.</returns>
		public Polynomial GetZPolynom(int order)
		{
			if (_polynomsZ == null)
			{
				_polynomsZ = new Polynomial[Points.Length];
			}

			if (order < 0)
			{
				throw new ArgumentException("Параметр \"" + nameof(order) + "\" не должен быть меньше 0.", nameof(order));
			}

			if (order >= Points.Length)
			{
				return Polynomial.Empty;
			}

			var result = _polynomsZ[order];

			if (result == null)
			{
				_polynomsZ[order] = result = order == 0 ? GetZPolynom() : GetZPolynom(order - 1).GetDerivative();
			}

			return result;
		}


		/// <summary>
		/// Получает полином для вычисления X координаты кривой.
		/// </summary>
		/// <value>Полином для вычисления X координаты кривой.</value>
		public Polynomial PolynomX0 => _polynomX0 ?? (_polynomX0 = GetXPolynom());

		/// <summary>
		/// Получает полином для вычисления Y координаты кривой.
		/// </summary>
		/// <value>Полином для вычисления Y координаты кривой.</value>
		public Polynomial PolynomY0 => _polynomY0 ?? (_polynomY0 = GetYPolynom());

		/// <summary>
		/// Получает полином для вычисления Z координаты кривой.
		/// </summary>
		/// <value>Полином для вычисления Z координаты кривой.</value>
		public Polynomial PolynomZ0 => _polynomZ0 ?? (_polynomZ0 = GetZPolynom());

		/// <summary>
		/// Получает полином для вычисления X координаты первой производной кривой.
		/// </summary>
		/// <value>Полином для вычисления X координаты первой производной кривой.</value>
		public Polynomial PolynomX1 => _polynomX1 ?? (_polynomX1 = PolynomX0.GetDerivative());

		/// <summary>
		/// Получает полином для вычисления Y координаты первой производной кривой.
		/// </summary>
		/// <value>Полином для вычисления Y координаты первой производной кривой.</value>
		public Polynomial PolynomY1 => _polynomY1 ?? (_polynomY1 = PolynomY0.GetDerivative());

		/// <summary>
		/// Получает полином для вычисления Z координаты первой производной кривой.
		/// </summary>
		/// <value>Полином для вычисления Z координаты первой производной кривой.</value>
		public Polynomial PolynomZ1 => _polynomZ1 ?? (_polynomZ1 = PolynomZ0.GetDerivative());

		/// <summary>
		/// Получает полином для вычисления X координаты второй производной кривой.
		/// </summary>
		/// <value>Полином для вычисления X координаты второй производной кривой.</value>
		public Polynomial PolynomX2 => _polynomX2 ?? (_polynomX2 = PolynomX1.GetDerivative());

		/// <summary>
		/// Получает полином для вычисления Y координаты второй производной кривой.
		/// </summary>
		/// <value>Полином для вычисления Y координаты второй производной кривой.</value>
		public Polynomial PolynomY2 => _polynomY2 ?? (_polynomY2 = PolynomY1.GetDerivative());

		/// <summary>
		/// Получает полином для вычисления Z координаты второй производной кривой.
		/// </summary>
		/// <value>Полином для вычисления Z координаты второй производной кривой.</value>
		public Polynomial PolynomZ2 => _polynomZ2 ?? (_polynomZ2 = PolynomZ1.GetDerivative());


		/// <summary>
		/// Получает полином для вычисления X координаты третьей производной кривой.
		/// </summary>
		/// <value>Полином для вычисления X координаты третьей производной кривой.</value>
		public Polynomial PolynomX3 => _polynomX3 ?? (_polynomX3 = PolynomX2.GetDerivative());

		/// <summary>
		/// Получает полином для вычисления Y координаты третьей производной кривой.
		/// </summary>
		/// <value>Полином для вычисления Y координаты третьей производной кривой.</value>
		public Polynomial PolynomY3 => _polynomY3 ?? (_polynomY3 = PolynomY2.GetDerivative());

		/// <summary>
		/// Получает полином для вычисления Z координаты третьей производной кривой.
		/// </summary>
		/// <value>Полином для вычисления Z координаты третьей производной кривой.</value>
		public Polynomial PolynomZ3 => _polynomZ3 ?? (_polynomZ3 = PolynomZ2.GetDerivative());

		#endregion

		#region Methods

		/// <summary>
		/// Разбивиет кривую на два в заданной произвольным параметром точке и возвращает набор точек.
		/// </summary>
		/// <param name="parameter">Параметр точки разбиения кривой.</param>
		/// <returns>Набор из массивов узловых точек.</returns>
		public Point3D[][] SplitPoints(double parameter)
		{
			var points1 = new Point3D[Points.Length];
			var points2 = new Point3D[Points.Length];
			SplitPoints(parameter, points1, points2);
			return new[] {points1, points2};
		}

		/// <summary>
		/// Разбивиет кривую на два в заданной произвольным параметром точке и заполняет массивы струтур <see cref="Point3D"/>.
		/// </summary>
		/// <param name="parameter">Параметр точки разбиения кривой.</param>
		/// <param name="points1">Массив узловых точек из струтур <see cref="Point3D"/> для первой кривой. Длина массива должна быть не меньше количества узловых точек кривой; заполняются первые элементы.</param>
		/// <param name="points2">Массив узловых точек из струтур <see cref="Point3D"/> для второй кривой. Длина массива должна быть не меньше количества узловых точек кривой; заполняются первые элементы.</param>
		/// <exception cref="ArgumentNullException">Параметр <paramref name="points1"/> или <paramref name="points2"/> имеет значение <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Длина массива <paramref name="points1"/> или <paramref name="points2"/> меньше количества узловых точек кривой, либо это один и тот же массив.</exception>
		public void SplitPoints(double parameter, Point3D[] points1, Point3D[] points2)
		{
			if (points1 == null)
			{
				throw new ArgumentNullException(nameof(points1));
			}

			if (points2 == null)
			{
				throw new ArgumentNullException(nameof(points2));
			}

			//Прежде степень кривой бралась из длины массива points1: для более длинных массивов точки разбиения получались
			//неверными (например, для массивов из пяти точек кубическая кривая делилась не в той точке). Теперь заполняются
			//первые элементы массивов, а более короткие массивы, как и прежде, отклоняются.
			if (points1.Length < Points.Length)
			{
				throw new ArgumentException("Длина массива должна быть не меньше количества узловых точек кривой (" + Points.Length + ").",
					nameof(points1));
			}

			if (points2.Length < Points.Length)
			{
				throw new ArgumentException("Длина массива должна быть не меньше количества узловых точек кривой (" + Points.Length + ").",
					nameof(points2));
			}

			//Массивы заполняются одновременно, поэтому общий массив дал бы бессмысленный результат.
			if (ReferenceEquals(points1, points2))
			{
				throw new ArgumentException("Для двух частей кривой нужны разные массивы.", nameof(points2));
			}

			Points.CopyTo(points1, 0);

			var u = 1 - parameter;
			var max = Points.Length - 1;
			for (var i = 0; i <= max; i++)
			{
				points2[max - i] = points1[max];
				for (var j = max; j > i; j--)
				{
					points1[j] += (points1[j - 1] - points1[j])*u;
				}
			}
		}

		/// <summary>
		/// Возвращает массив узловых точек из структур <see cref="Point3D"/> для участка кривой.
		/// </summary>
		/// <param name="t0">Параметр начала участка кривой.</param>
		/// <param name="t1">Параметр конца участка кривой.</param>
		/// <returns>Массив узловых точек из струтур <see cref="Point3D"/> для участка кривой.</returns>
		/// <remarks>Если <paramref name="t0"/> больше <paramref name="t1"/>, участок проходится в обратном направлении.</remarks>
		public Point3D[] GetPartPoints(double t0, double t1)
		{
			//Узловые точки участка [t0, t1] — значения полярной формы (блоссома) кривой: Qₖ = B(t0, …, t0, t1, …, t1),
			//где t1 повторяется k раз. Прежде участок вычислялся двумя делениями, второе — в точке (t1 − t0)/(1 − t0):
			//при t0 = 1 получалось деление на ноль и точки NaN, а при t0, близком к 1, — большая потеря точности.
			var max = Points.Length - 1;
			var result = new Point3D[Points.Length];
			var points = new Point3D[Points.Length];

			for (var k = 0; k <= max; k++)
			{
				Points.CopyTo(points, 0);

				for (var i = 0; i < max; i++)
				{
					var u = i < k ? t1 : t0;
					var v = 1 - u;
					for (var j = 0; j < max - i; j++)
					{
						points[j] = points[j]*v + points[j + 1]*u;
					}
				}

				result[k] = points[0];
			}

			return result;
		}

		#endregion

		#region Virtual

		/// <summary>
		/// Возвращает точку на кривой соответствующую параметру.
		/// </summary>
		/// <param name="parameter">Параметр кривой от 0 до 1.</param>
		/// <returns>Координаты точки на кривой представленой структурой <see cref="Point3D"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Параметр <paramref name="parameter"/> меньше 0 или больше 1.</exception>
		public override Point3D GetValue(double parameter)
		{
			CheckParameter(parameter);

			return new Point3D(PolynomX0.GetValue(parameter), PolynomY0.GetValue(parameter), PolynomZ0.GetValue(parameter));
		}

		/// <summary>
		/// Возвращает вектор касательной к кривой в заданной произвольным параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой от 0 до 1.</param>
		/// <returns>Единичный вектор касательной, направленный в сторону возрастания параметра, представленный структурой <see cref="Point3D"/>.</returns>
		/// <remarks>В особой точке, где первая производная равна нулю, направление касательной задаёт первая ненулевая
		/// производная старшего порядка; в конечной точке кривой (<paramref name="parameter"/> = 1) возвращается левая касательная.
		/// Для параметра вне отрезка [0, 1] касательная вычисляется для продолжения кривой (как и прежде).</remarks>
		public override Point3D GetTangent(double parameter)
		{
			//Производные вычисляются по разностям узловых точек, а не по многочленам в степенном базисе: прежде в конце
			//кривой с совпадающими последними узлами (P2 = P3) первая производная получалась не нулевой, а порядка 1e-15,
			//особая точка не распознавалась, и касательная имела случайное направление.
			var result = GetDerivative(1, parameter);

			//Точка сингулярности.
			if (result == Point3D.Empty)
			{
				var order = 2;
				while (result == Point3D.Empty && order < Points.Length)
				{
					result = GetDerivative(order, parameter);

					order++;
				}

				//Если параметр = 1, возвращаем левую касательную
				if (parameter.Equals(1D) && order % 2 == 1)
				{
					result *= -1;
				}
			}



			/*
			//Точка сингулярности. Правило Бернулли — Лопиталя.
			for (var order = 2; result == Point3D.Empty && order < Points.Length; order++)
			{
				result = new Point3D(
					GetXPolynom(order).GetValue(parameter),
					GetYPolynom(order).GetValue(parameter),
					GetZPolynom(order).GetValue(parameter));
			}
			*/
			
			return result / result.Length;
		}

		/*
		public void GetTangent(double parameter, ref Point3D result)
		{
			var x = PolynomX1.GetValue(parameter);
			var y = PolynomY1.GetValue(parameter);
			var z = PolynomZ1.GetValue(parameter);

			var len = System.Math.Sqrt(x*x + y*y + z*z);
			result.X = x/len;
			result.Y = y/len;
			result.Z = z/len;
		}
		*/

		/// <summary>
		/// Возвращает вектор кривизны к кривой в заданной произвольным параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой от 0 до 1.</param>
		/// <returns>Значение вектора кривизны представленный структурой <see cref="Point3D"/>.</returns>
		/// <remarks>Псевдовектор кривизны направлен перпендикулярно к плоскости образованной векторами нормали и касательной.
		/// Для параметра вне отрезка [0, 1] кривизна вычисляется для продолжения кривой (как и прежде).</remarks>
		public override Point3D GetCurvature(double parameter)
		{
			//Первая производная
			var v1 = GetDerivative(1, parameter);

			//Вторая производная
			var v2 = GetDerivative(2, parameter);

			var len = v1.Length;

			//Кривизна
			return v1*v2/(len*len*len);
		}

		/// <summary>
		/// Возвращает узловые точки <see cref="Point3D"/> по индексу.
		/// </summary>
		/// <param name="index">Индекс точки.</param>
		/// <returns>Узловая точка.</returns>
		/// <exception cref="IndexOutOfRangeException">Индекс находится вне массива.</exception>
		public Point3D this[int index] => Points[index];

		/// <summary>
		/// Возвращает производную от функции выпрямления кривой.
		/// </summary>
		/// <returns>Производная от функции выпрямления.</returns>
		protected override Func<double, double> GetRectificationDerivative()
		{
			//Прежде использовались только коэффициенты a0…a4 многочлена |P′(t)|², которых хватает лишь для кривых не выше
			//третьей степени: длина кривой x(t) = t⁴ получалась равной 0, а длины и расстояния для кривых из пяти и более
			//точек — неверными.
			if (Points.Length != 4)
			{
				//Производная вычисляется в базисе Бернштейна: многочлен |P′(t)|² в степенном базисе при высокой степени
				//плохо обусловлен (для кривой из 21 точки погрешность длины превышала 40 %).
				return (x) => GetDerivative(1, x).Length;
			}

			//Кубическая кривая — самый частый случай, поэтому для неё схема Горнера для |P′(t)|² развёрнута. Производная
			//P′(t) = c₀ + c₁t + c₂t² вычисляется по разностям узловых точек Dᵢ = 3(Pᵢ₊₁ − Pᵢ).
			var d0 = (Points[1] - Points[0])*3;
			var d1 = (Points[2] - Points[1])*3;
			var d2 = (Points[3] - Points[2])*3;

			var c0 = d0;
			var c1 = (d1 - d0)*2;
			var c2 = (d2 - d1) - (d1 - d0);

			var a0 = c0.DotProduct(c0);
			var a1 = 2*c0.DotProduct(c1);
			var a2 = c1.DotProduct(c1) + 2*c0.DotProduct(c2);
			var a3 = 2*c1.DotProduct(c2);
			var a4 = c2.DotProduct(c2);

			return (x) =>
			{
				//Около точки возврата (P′ = 0) погрешность округления может сделать значение многочлена отрицательным:
				//прежде корень из него давал NaN, например в GetDistance(0.5 − 1e-9, 0.5 + 1e-9).
				var value = a0 + x*(a1 + x*(a2 + x*(a3 + x*a4)));
				return value < 0 ? 0 : System.Math.Sqrt(value);
			};
		}

		/// <summary>
		/// Приводит кривую к кубической кривой Безье.
		/// </summary>
		/// <returns>Кубическая кривая Безье, совпадающая с исходной кривой, в том числе по параметризации.</returns>
		/// <exception cref="NotImplementedException">Степень кривой больше 3: такая кривая в общем случае не представима одной кубической кривой Безье.</exception>
		/// <remarks>Кривые первой и второй степени приводятся к кубической повышением степени, кубическая кривая копируется.</remarks>
		public override BezierCurve RecastToBezierCurve()
		{
			//Прежде метод не был реализован даже для кубических (в том числе для самой BezierCurve) и квадратичных кривых.
			switch (Points.Length)
			{
				case 2:
					//Отрезок: промежуточные точки делят его на три равные части, поэтому параметризация сохраняется.
					return new BezierCurve(Points[0], Points[0] + (Points[1] - Points[0])/3, Points[0] + (Points[1] - Points[0])*2/3, Points[1]);

				case 3:
					//Повышение степени квадратичной кривой.
					return new BezierCurve(Points[0], Points[0] + (Points[1] - Points[0])*2/3, Points[2] + (Points[1] - Points[2])*2/3, Points[2]);

				case 4:
					return new BezierCurve(Points[0], Points[1], Points[2], Points[3]);

				default:
					throw new NotImplementedException("Кривая степени выше 3 не приводится к одной кубической кривой Безье.");
			}
		}

		#endregion

		/*
        internal void CloneFieldsTo(BernsteinCurve curve)
        {
            base.CloneFieldsTo(curve);

            curve._polynomX0 = _polynomX0;
            curve._polynomY0 = _polynomY0;
            curve._polynomZ0 = _polynomZ0;

            curve._polynomX1 = _polynomX1;
            curve._polynomY1 = _polynomY1;
            curve._polynomZ1 = _polynomZ1;

            curve._polynomX2 = _polynomX2;
            curve._polynomY2 = _polynomY2;
            curve._polynomZ2 = _polynomZ2;
        }
        */
	}
}