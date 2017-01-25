using System;
using System.Collections.Generic;
using System.Linq;
using Ruzil3D.Algebra;

namespace Ruzil3D.Curves
{
	/// <summary>
	/// NURBS кривые заданных полиномами Бернштейна.
	/// </summary>
	/// <remarks>Назван в честь Сергеея Натановича Бернштейна.</remarks>
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
		/// <param name="points1">Массив узловых точек из струтур <see cref="Point3D"/> для первой кривой.</param>
		/// <param name="points2">Массив узловых точек из струтур <see cref="Point3D"/> для второй кривой.</param>
		public void SplitPoints(double parameter, Point3D[] points1, Point3D[] points2)
		{
			Points.CopyTo(points1, 0);

			var u = 1 - parameter;
			var max = points1.Length - 1;
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
		public Point3D[] GetPartPoints(double t0, double t1)
		{
			var points = new Point3D[Points.Length];
			Points.CopyTo(points, 0);

			var max = points.Length - 1;

			if (!t0.Equals(0D))
			{
				var u = t0;
				for (var i = 0; i < max; i++)
				{
					for (var j = 0; j < max - i; j++)
					{
						points[j] += (points[j + 1] - points[j])*u;
					}
				}
			}

			if (!t1.Equals(1D))
			{
				var u = 1 - (t1 - t0)/(1D - t0);
				for (var i = 0; i < max; i++)
				{
					for (var j = max; j > i; j--)
					{
						points[j] += (points[j - 1] - points[j])*u;
					}
				}
			}
			return points;
		}

		#endregion

		#region Virtual

		/// <summary>
		/// Возвращает точку на кривой соответствующую параметру.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Координаты точки на кривой представленой структурой <see cref="Point3D"/>.</returns>
		public override Point3D GetValue(double parameter)
		{
			if (parameter < 0 || parameter > 1)
			{
				throw new ArgumentException("Атрибут \"" + nameof(parameter) + "\" должен быть от 0 до 1.");
			}

			return new Point3D(PolynomX0.GetValue(parameter), PolynomY0.GetValue(parameter), PolynomZ0.GetValue(parameter));
		}

		/// <summary>
		/// Возвращает вектор касательной к кривой в заданной произвольным параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение вектора касательной представленный структурой <see cref="Point3D"/>.</returns>
		public override Point3D GetTangent(double parameter)
		{
			var result = new Point3D(PolynomX1.GetValue(parameter), PolynomY1.GetValue(parameter), PolynomZ1.GetValue(parameter));

			//Точка сингулярности.
			if (result == Point3D.Empty)
			{
				var order = 2;
				while (result == Point3D.Empty && order < Points.Length)
				{
					result = new Point3D(
						GetXPolynom(order).GetValue(parameter),
						GetYPolynom(order).GetValue(parameter),
						GetZPolynom(order).GetValue(parameter));

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
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение вектора кривизны представленный структурой <see cref="Point3D"/>.</returns>
		/// <remarks>Псевдовектор кривизны направлен перпендикулярно к плоскости образованной векторами нормали и касательной.</remarks>
		public override Point3D GetCurvature(double parameter)
		{
			//Первая производная
			var v1 = new Point3D(PolynomX1.GetValue(parameter), PolynomY1.GetValue(parameter), PolynomZ1.GetValue(parameter));

			//Вторая производная
			var v2 = new Point3D(PolynomX2.GetValue(parameter), PolynomY2.GetValue(parameter), PolynomZ2.GetValue(parameter));

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
			var polynom = PolynomX1.Pow(2) + PolynomY1.Pow(2) + PolynomZ1.Pow(2);

			var a0 = polynom[0];
			var a1 = polynom[1];
			var a2 = polynom[2];
			var a3 = polynom[3];
			var a4 = polynom[4];

			return (x) => System.Math.Sqrt(a0 + x*(a1 + x*(a2 + x*(a3 + x*a4))));
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