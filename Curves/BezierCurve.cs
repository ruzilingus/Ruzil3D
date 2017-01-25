using System;
using System.Collections.Generic;
using Ruzil3D.Approximation;
using Ruzil3D.Algebra;
using Ruzil3D.Calculus;
using Ruzil3D.Geometry;

// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace Ruzil3D.Curves
{
	/// <summary>
	/// Кривая Безье в трехмерном евклидовом пространстве.
	/// </summary>
	public class BezierCurve : BernsteinCurve
	{
		//Для преведения кривых через заданные точки
		private static readonly double[] Line1 = {1D};
		private static readonly double[] Line2 = {-3D, 3D};
		private static readonly double[] Line3 = {3D, -6D, 3D};
		private static readonly double[] Line4 = {-1D, 3D, -3D, 1D};

		#region Main

		/// <summary>
		///  Получает координату P0 кривой <see cref="BezierCurve"/> представленную структурой <see cref="Point3D"/>.
		/// </summary>
		/// <value>Координата P0 кривой <see cref="BezierCurve"/> представленную структурой <see cref="Point3D"/>.</value>
		public Point3D P0 => Points[0];

		/// <summary>
		///  Получает координату P1 кривой <see cref="BezierCurve"/> представленную структурой <see cref="Point3D"/>.
		/// </summary>
		/// <value>Координата P1 кривой <see cref="BezierCurve"/> представленную структурой <see cref="Point3D"/>.</value>
		public Point3D P1 => Points[1];

		/// <summary>
		///  Получает координату P2 кривой <see cref="BezierCurve"/> представленную структурой <see cref="Point3D"/>.
		/// </summary>
		/// <value>Координата P2 кривой <see cref="BezierCurve"/> представленную структурой <see cref="Point3D"/>.</value>
		public Point3D P2 => Points[2];

		/// <summary>
		///  Получает координату P3 кривой <see cref="BezierCurve"/> представленную структурой <see cref="Point3D"/>.
		/// </summary>
		/// <value>Координата P3 кривой <see cref="BezierCurve"/> представленную структурой <see cref="Point3D"/>.</value>
		public Point3D P3 => Points[3];

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="BezierCurve"/> с указанными узловыми точками.
		/// </summary>
		/// <param name="p0">Первая узловая точка.</param>
		/// <param name="p1">Вторая узловая точка.</param>
		/// <param name="p2">Третья узловая точка.</param>
		/// <param name="p3">Четвертая узловая точка.</param>
		public BezierCurve(Point3D p0, Point3D p1, Point3D p2, Point3D p3) : base(new[] {p0, p1, p2, p3})
		{
			//Временно
			//var tmp = RectificationDerivativeFunction;
		}

		private BezierCurve(Point3D[] points) : base(points)
		{
		}
        
        #endregion

        #region Overloads

        /// <summary>
        /// Преобразует исходную кривую в соответствии с заданным преобразованием.
        /// </summary>
        /// <param name="transform">Преобразование трехмерного евклидово пространства.</param>
        /// <param name="curve">Исходная кривая.</param>
        /// <returns>Преобразованная кривая.</returns>
        public static BezierCurve operator *(Affinity transform, BezierCurve curve)
        {
            return new BezierCurve(transform * curve.Points);
        }

        /// <summary>
        /// Возвращает произведение исходной матрицы на кривую, преставленный структурой <see cref="BezierCurve"/>.
        /// </summary>
        /// <param name="matrix">Исходная кривая.</param>
        /// <param name="curve">Плоскость для перемножения.</param>
        /// <returns>Произведение исходной матрицы на кривую <paramref name="curve"/>.</returns>
        public static BezierCurve operator *(Matrix3D matrix, BezierCurve curve)
        {
            return new BezierCurve(matrix*curve.Points);
        }

        /// <summary>
		/// Перемещает исходную кривую в направлении и расстоянии указанное вектором.
		/// </summary>
		/// <param name="curve">Исходная кривая.</param>
		/// <param name="vector">Вектор указывающий перемещение.</param>
		/// <returns>Перемещенная кривая.</returns>
        public static BezierCurve operator +(BezierCurve curve, Point3D vector)
        {
            return new BezierCurve(curve.Points + vector);
        }

        /// <summary>
        /// Перемещает исходную кривую в направлении и расстоянии указанное вектором.
        /// </summary>
        /// <param name="vector">Вектор указывающий перемещение.</param>
        /// <param name="curve">Исходная линия.</param>
        /// <returns>Перемещенная кривая.</returns>
        public static BezierCurve operator +(Point3D vector, BezierCurve curve)
        {
            return curve + vector;
        }

        /// <summary>
        /// Перемещает исходную кривую в обратном направлении и расстоянии указанное вектором.
        /// </summary>
        /// <param name="curve">Исходная кривая.</param>
        /// <param name="vector">Вектор указывающий перемещение.</param>
        /// <returns>Перемещенная кривая.</returns>
        public static BezierCurve operator -(BezierCurve curve, Point3D vector)
        {
            return curve + -vector;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Разбивиет кривую на две в заданной произвольным параметром точке и возвращает массив кривых <see cref="BezierCurve"/>.
        /// </summary>
        /// <param name="parameter">Параметр точки разбиения кривой.</param>
        /// <returns>Массив кривых.</returns>
        public BezierCurve[] Split(double parameter)
		{
			var points1 = new Point3D[Points.Length];
			var points2 = new Point3D[Points.Length];
			SplitPoints(parameter, points1, points2);
			return new[] {new BezierCurve(points1), new BezierCurve(points2)};
		}

		/// <summary>
		/// Разбивиет кривую на две в точке соответствующей значению параметра 0.5 и возвращает массив кривых <see cref="BezierCurve"/>.
		/// </summary>
		/// <returns>Массив кривых.</returns>
		public BezierCurve[] Split()
		{
			return Split(0.5);
		}

		/// <summary>
		/// Возвращает участок кривой между двумя точками заданных параметрически.
		/// </summary>
		/// <param name="t0">Параметр начала участка кривой.</param>
		/// <param name="t1">Параметр конца участка кривой.</param>
		/// <returns>Участок кривой.</returns>
		public BezierCurve GetPart(double t0, double t1)
		{
			return new BezierCurve(GetPartPoints(t0, t1));
		}

		/// <summary>
		/// Подбирает точки Безье для указанного полинома.
		/// </summary>
		/// <param name="polynomial"></param>
		/// <returns></returns>
		private static double[] GetBezierPoints(Polynomial polynomial)
		{
			return LinearSystem.Resolve(new[]
			{
				new Linear(Line1, polynomial[0]),
				new Linear(Line2, polynomial[1]),
				new Linear(Line3, polynomial[2]),
				new Linear(Line4, polynomial[3]),
			});
		}

		/// <summary>
		/// Возвращает дважды-гладкую последовательность кривых проходящих через заданные точки представленных набором структур <see cref="Point3D"/>.
		/// </summary>
		/// <param name="points">Набор структур <see cref="Point3D"/>.</param>
		/// <returns>Массив кривых.</returns>
		public static BezierCurve[] FromPoints(IList<Point3D> points)
		{
			if (points.Count < 2)
			{
				throw new ArgumentException("Количество точек должно быть больше 1.", nameof(points));
			}

			var pointsX = new PointD[points.Count];
			var pointsY = new PointD[points.Count];
			var pointsZ = new PointD[points.Count];

			for (var i = 0; i < points.Count; i++)
			{
				var point = points[i];
				pointsX[i] = new PointD(i, point.X);
				pointsY[i] = new PointD(i, point.Y);
				pointsZ[i] = new PointD(i, point.Z);
			}

			//Строем кубические сплайны X(t), Y(t), Z(t)
			var funcX = new CubicInterpolation(pointsX);
			var funcY = new CubicInterpolation(pointsY);
			var funcZ = new CubicInterpolation(pointsZ);

			var result = new BezierCurve[points.Count - 1];

			for (var i = 0; i < result.Length; i++)
			{
				var polynomX = funcX.GetPolynom(i);
				var polynomY = funcY.GetPolynom(i);
				var polynomZ = funcZ.GetPolynom(i);

				//Замена переменной
				var sub = new Polynomial(i, 1);

				var resolveX = GetBezierPoints(polynomX.Substitution(sub));
				var resolveY = GetBezierPoints(polynomY.Substitution(sub));
				var resolveZ = GetBezierPoints(polynomZ.Substitution(sub));

				result[i] = new BezierCurve(
					new Point3D(resolveX[0], resolveY[0], resolveZ[0]),
					new Point3D(resolveX[1], resolveY[1], resolveZ[1]),
					new Point3D(resolveX[2], resolveY[2], resolveZ[2]),
					new Point3D(resolveX[3], resolveY[3], resolveZ[3]));
			}

			return result;
		}


		private static double GetLength_OBSOLETE(Point3D p0, Point3D p1, Point3D p2, Point3D p3, double accuracy,
			double level = 0)
		{

			const int minLevel = 2;
			const int maxLevel = 15;

			var len = level >= minLevel ? p0.Distance(p3) : -1;

			if (level > maxLevel || len >= 0 && (p0 + p3).Distance(p1 + p2) <= accuracy*len)
				//if (len < 0.00001D)
				//if (level > 128)
			{
				//Аппроксимируем кругом
				var cos = (p0 - p1).Cos(p2 - p3);

				if (cos >= 1)
				{
					return len;
				}

				var angle = Math.Acos(cos);
				var sin = Math.Sqrt((1 - cos)/2);
				var diam = len/sin;

				return diam*angle/2;
			}
			var q0 = (p0 + p1)/2;
			var q1 = (p1 + p2)/2;
			var q2 = (p2 + p3)/2;
			var r0 = (q0 + q1)/2;
			var r1 = (q1 + q2)/2;
			var s0 = (r0 + r1)/2;

			level++;

			return GetLength_OBSOLETE(p0, q0, r0, s0, accuracy, level) +
			       GetLength_OBSOLETE(s0, r1, q2, p3, accuracy, level);
		}

		/// <summary>
		/// Находит кривую Безье по заданным точкам и касательным в конечных точках. Метод устарел.
		/// </summary>
		/// <param name="point0"></param>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="point3"></param>
		/// <param name="vector0">Начальный касательный вектор.</param>
		/// <param name="vector3">Конечный касательный вектор.</param>
		/// <returns></returns>
		[Obsolete("Метод устарел.")]
		// ReSharper disable once InconsistentNaming
		public static BezierCurve ResolveXY(PointD point0, PointD point1, PointD point2, PointD point3, PointD vector0,
			PointD vector3)
		{
			var B0 = Polynomial.GetBernstein(0, 3);
			var B1 = Polynomial.GetBernstein(1, 3);
			var B2 = Polynomial.GetBernstein(2, 3);
			var B3 = Polynomial.GetBernstein(3, 3);

			const double eps = 0.000001D;
			const double step = 0.001D;
			// ReSharper disable once InconsistentNaming
			var uuu_tuu3 = B0 + B1;
			// ReSharper disable once InconsistentNaming
			var ttt_ttu3 = B3 + B2;


			var z1 = ((point1 - point0)*(point2 - point1)).Z;
			var z2 = ((point2 - point1)*(point3 - point2)).Z;

			//Точки находятся почти на одной линии
			if (Math.Abs(z1) < eps && Math.Abs(z2) < eps)
			{
				return new BezierCurve(point0, point0, point3, point3);
			}

			var method1 = new PointD(vector3.Y, -vector3.X);
			var method2 = new PointD(vector0.Y, -vector0.X);
			var dot1 = vector0.DotProduct(method1);
			var dot2 = vector3.DotProduct(method2);

			var distance = (point0 - point3).Length;
			//double kMax1 = 10000D * distance / vector0.Length;
			//double kMax2 = 10000D * distance / vector3.Length;

			var kMax1 = 1D*distance/vector0.Length;
			var kMax2 = 1D*distance/vector3.Length;


			#region Полезные комменты

			//Идеальный случай
			//double kMin1 = 0;
			//double kMin2 = 0;

			//Классический случай
			//double kMin1 = 0.001D * distance / vector0.Length;
			//double kMin2 = 0.001D * distance / vector3.Length;

			//Point3D s1 = new Point3D(Math.Cos(angle0), Math.Sin(angle0), 0);
			//Point3D s2 = new Point3D(Math.Cos(angle3), Math.Sin(angle3), 0);

			//point1 = u1 * u1 * u1 * point0 + 3 * t1 * u1 * u1 * (point0 + k1 * s1) + 3 * t1 * t1 * u1 * (point3 + k2 * s2) + t1 * t1 * t1 * point3;
			//point2 = u2 * u2 * u2 * point0 + 3 * t2 * u2 * u2 * (point0 + k1 * s1) + 3 * t2 * t2 * u2 * (point3 + k2 * s2) + t2 * t2 * t2 * point3;

			//3 * t1 * u1 * u1 * k1 * s1 = point1 - u1 * u1 * u1 * point0 - 3 * t1 * u1 * u1 * point0 - 3 * t1 * t1 * u1 * point3 - 3 * t1 * t1 * u1 * k2 * s2 - t1 * t1 * t1 * point3
			//3 * t2 * u2 * u2 * k1 * s1 = point2 - u2 * u2 * u2 * point0 - 3 * t2 * u2 * u2 * point0 - 3 * t2 * t2 * u2 * point3 - 3 * t2 * t2 * u2 * k2 * s2 + t2 * t2 * t2 * point3

			#endregion

			//Иначе Corel Draw не распознает слишком близкие числа
			var kMin1 = 0.01D/vector0.Length;
			var kMin2 = 0.01D/vector3.Length;

			var min = double.PositiveInfinity;
			double k1Min = kMin1, k2Min = -kMin2;

			var px1 = uuu_tuu3*point0.X + ttt_ttu3*point3.X - point2.X;
			var px2 = B1*vector0.X;
			var px3 = B2*vector3.X;

			var py1 = uuu_tuu3*point0.Y + ttt_ttu3*point3.Y - point2.Y;
			var py2 = B1*vector0.Y;
			var py3 = B2*vector3.Y;

			var polyDenomX = point1.X - uuu_tuu3*point0.X - ttt_ttu3*point3.X;
			var polyDenomY = point1.Y - uuu_tuu3*point0.Y - ttt_ttu3*point3.Y;

			for (var t1 = step; t1 < 1D; t1 += step)
			{
				#region Находим k1 и k2 для текущего t1

				var u1 = 1D - t1;

				//PointD denominator = point1 - u1 * u1 * u1 * point0 - 3 * t1 * u1 * u1 * point0 - 3 * t1 * t1 * u1 * point3 - t1 * t1 * t1 * point3;
				var denominator = new PointD(polyDenomX.GetValue(t1), polyDenomY.GetValue(t1));

				var k1 = denominator.DotProduct(method1)/(3*t1*u1*u1*dot1);
				//double k1 = denominator.DotProduct(method1) / (B1.GetValue(t1) * dot1);
				if (k1 > kMax1 || k1 < kMin1)
				{
					continue;
				}

				var k2 = denominator.DotProduct(method2)/(3*t1*t1*u1*dot2);
				//double k2 = denominator.DotProduct(method2) / (B0.GetValue(t1) * dot2);
				if (k2 < -kMax2 || k2 > -kMin2)
				{
					continue;
				}

				#endregion

				#region Находим время

				//Polynomial polynomX = B0 * point0.X + b3 * point3.X - point2.X + B1 * (point0.X + k1 * vector0.X) + B0 * (point3.X + k2 * vector3.X);
				//Polynomial polynomY = B0 * point0.Y + b3 * point3.Y - point2.Y + B1 * (point0.Y + k1 * vector0.Y) + B0 * (point3.Y + k2 * vector3.Y);

				var polynomX = px1 + k1*px2 + k2*px3;
				var polynomY = py1 + k1*py2 + k2*py3;

				//Находим корни кубического уравнения
				var resolve = polynomX.Resolve(false);

				foreach (var time in resolve)
				{
					if (time < 0 || time > 1)
					{
						continue;
					}

					var value = Math.Abs(polynomY.GetValue(time));
					if (value < min)
					{
						min = value;
						k1Min = k1;
						k2Min = k2;
					}
				}

				#endregion

				#region Второй метод

				/*

				for (double t2 = t1 + step; t2 < 1D; t2 += step)
				{
				    double u2 = 1D - t2;

				    PointD denominator2 = point2 - u2 * u2 * u2 * point0 - 3 * t2 * u2 * u2 * point0 - 3 * t2 * t2 * u2 * point3 - t2 * t2 * t2 * point3;
				    double k12 = denominator2.DotProduct(method1) / (3 * t2 * u2 * u2 * dot1);
				    double k22 = denominator2.DotProduct(method2) / (3 * t2 * t2 * u2 * dot2);

				    if (k12 < 0 || k22 > 0)
				    {
					continue;
				    }

				    double d1 = k12 - k1;
				    double d2 = k22 - k2;
				    value = d1 * d1 + d2 * d2;

				    if (value < min)
				    {
					min = value;
					k1min = k1;
					k2min = k2;
				    }
				}
				 */

				#endregion
			}

			//k1min = 0.001D;
			//k2min = -0.001D;

			return new BezierCurve(point0, point0 + k1Min*vector0, point3 + k2Min*vector3, point3);
		}
		

		#endregion
	}
}