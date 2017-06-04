using System;
using Ruzil3D.Algebra;
using static Ruzil3D.Utility.CStatic;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет плоскость в трехмерном евклидовом пространстве.
	/// </summary>
	public struct Plane
	{
		/// <summary>
		/// Получает значение указывающее, что уравнение плоскости является канонической.
		/// </summary>
		public bool IsNormalized { get; private set; }

		/// <summary>
		/// Получает или задает коэффициент <see cref="A"/> из общего уравнения прямой.
		/// </summary>
		public double A;

		/// <summary>
		/// Получает или задает коэффициент <see cref="B"/> из общего уравнения прямой.
		/// </summary>
		public double B;

		/// <summary>
		/// Получает или задает коэффициент <see cref="C"/> из общего уравнения прямой.
		/// </summary>
		public double C;

		/// <summary>
		/// Получает или задает коэффициент <see cref="D"/> из общего уравнения прямой.
		/// </summary>
		public double D;


		/// <summary>
		/// Инициализирует новую плоскость в трехмерном евклидовом пространстве проходящую через заданные точки.
		/// </summary>
		/// <param name="point0">Первая заданная точка.</param>
		/// <param name="point1">Вторая заданная точка.</param>
		/// <param name="point2">Третья заданная точка.</param>
		public Plane(Point3D point0, Point3D point1, Point3D point2)
		{
			IsNormalized = false;

			//Проводим плоскость через 3 точки

			var dx1 = point1.X - point0.X;
			var dy1 = point1.Y - point0.Y;
			var dz1 = point1.Z - point0.Z;

			var dx2 = point2.X - point0.X;
			var dy2 = point2.Y - point0.Y;
			var dz2 = point2.Z - point0.Z;

			A = dy1*dz2 - dz1*dy2;
			B = dz1*dx2 - dx1*dz2;
			C = dx1*dy2 - dy1*dx2;
			D = -point0.X*A - point0.Y*B - point0.Z*C;

			Normalize();
		}

		/// <summary>
		/// Проводит плоскость через заданную точку, перпендикуярную к заданной нормали.
		/// </summary>
		/// <param name="point">Заданная точка на плоскости.</param>
		/// <param name="normal">Нормаль к плоскости.</param>
		public Plane(Point3D point, Point3D normal)
		{
			IsNormalized = false;
			A = normal.X;
			B = normal.Y;
			C = normal.Z;
			D = -A*point.X - B*point.Y - C*point.Z;

			Normalize();
		}

		/// <summary>
		/// Инициализирует новую плоскость в трехмерном евклидовом пространстве через коэффициенты общего уравнения прямой.
		/// </summary>
		/// <param name="a">Задает коэффициент A.</param>
		/// <param name="b">Задает коэффициент B.</param>
		/// <param name="c">Задает коэффициент C.</param>
		/// <param name="d">Задает коэффициент D.</param>
		public Plane(double a, double b, double c, double d)
		{
			IsNormalized = false;

			A = a;
			B = b;
			C = c;
			D = d;
		}

		/// <summary>
		/// Получает значение уравнения плоскости в указанной точке.
		/// </summary>
		/// <param name="point">Точка в которой считается значение уравнения.</param>
		/// <returns>Значение уравнения плоскости в точе <paramref name="point"/>.</returns>
		/// <remarks>
		/// Если уравнение плоскости задано в канонической форме, то абсолютное значение функции <see cref="GetValue"/> равно кратчайшему расстоянию от точки <paramref name="point"/> до плоскости.
		/// Если при вычислении получилось отрицательное число, то это означает, что точка <paramref name="point"/> и начало координат находятся по разные стороны от плоскости.
		/// Для приведения плоскости к каноническому виду нужно предварительно вызвать метод <see cref="Normalize"/>.
		/// Чтобы проверить, является ли плоскость нормализованной, нужно обратиться к свойству <see cref="IsNormalized"/>.
		/// </remarks>
		public double GetValue(Point3D point)
		{
			return A*point.X + B*point.Y + C*point.Z + D;
		}



		#region Overloads

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух плоскостей.
		/// </summary>
		/// <param name="x">Первая плоскость для сравнения.</param>
		/// <param name="y">Вторая плоскость для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Plane x, Plane y)
		{
			var xNorm = x.GetNorm();
			if (double.IsNaN(xNorm) || double.IsInfinity(xNorm) || 0D.Equals(xNorm))
			{
				return false;
			}

			var yNorm = y.GetNorm();
			if (double.IsNaN(yNorm) || double.IsInfinity(yNorm) || 0D.Equals(yNorm))
			{
				return false;
			}

			if (x.D > 0)
			{
				xNorm *= -1;
			}

			if (y.D > 0)
			{
				yNorm *= -1;
			}

			return
				(x.A*yNorm).Equals(y.A*xNorm) &&
				(x.B*yNorm).Equals(y.B*xNorm) &&
				(x.C*yNorm).Equals(y.C*xNorm) &&
				(x.D*yNorm).Equals(y.D*xNorm);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух плоскостей.
		/// </summary>
		/// <param name="x">Первая плоскость для сравнения.</param>
		/// <param name="y">Вторая плоскость для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Plane x, Plane y)
		{
			return !(x == y);
		}
		
		/// <summary>
		/// Возвращает точку пересечения плоскости и прямой.
		/// </summary>
		/// <param name="plane">Плоскость.</param>
		/// <param name="line">Прямая.</param>
		/// <returns>Точка пересечения плоскости и прямой.</returns>
		/// <exception cref="ArgumentException">Если прямая и плоскость параллельны.</exception>
		public static Point3D operator *(Plane plane, Line3D line)
		{
			var denominator = plane.A*line.S.X + plane.B*line.S.Y + plane.C*line.S.Z;

			if (denominator.Equals(0D))
			{
				throw new ArgumentException("Плоскость и прямая параллельны.");
			}

			var t = -(plane.A*line.M.X + plane.B*line.M.Y + plane.C*line.M.Z + plane.D)/denominator;

			return line.GetValue(t);
		}

		/// <summary>
		/// Возвращает линию пересечения друх плоскостей.
		/// </summary>
		/// <param name="plane1">Первая плоскость.</param>
		/// <param name="plane2">Вторая плоскость.</param>
		/// <returns>Линия пересечения друх плоскостей.</returns>
		/// <exception cref="ArgumentException">Плоскости параллельны.</exception>
		public static Line3D operator *(Plane plane1, Plane plane2)
		{

			//return new Line3D(Point3D.Empty, Point3D.UnitX);


			double x0, y0, z0, x1, y1, z1, x2, y2, z2;
			
			if (!0D.Equals(plane1.A))
			{
				y0 = z0 = y1 = z2 = 0;
				z1 = y2 = 1;

				x0 = -plane1.D / plane1.A;
				x1 = (-plane1.D - plane1.C * z1) / plane1.A;
				x2 = (-plane1.D - plane1.B * y2) / plane1.A;
			}
			else if (!0D.Equals(plane1.B))
			{
				x0 = z0 = x1 = z2 = 0;
				z1 = x2 = 1;

				y0 = -plane1.D / plane1.B;
				y1 = (-plane1.D - plane1.C * z1) / plane1.B;
				y2 = (-plane1.D - plane1.A * x2) / plane1.B;
			}
			else if (!0D.Equals(plane1.C))
			{
				x0 = y0 = x1 = y2 = 0;
				y1 = x2 = 1;

				z0 = -plane1.D/plane1.C;
				z1 = (-plane1.D - plane1.B*y1)/plane1.C;
				z2 = (-plane1.D - plane1.A*x2)/plane1.C;
			}
			else
			{
				throw new ArgumentException("Параметр " + nameof(plane1) + " не является плоскостью!");
			}
			
			//Координаты направляющей
			double sx, sy, sz;

			var ux = x1 - x0;
			var uy = y1 - y0;
			var uz = z1 - z0;
			var n = plane2.A * ux + plane2.B * uy + plane2.C * uz;

			if (0D.Equals(n))
			{
				sx = ux;
				sy = uy;
				sz = uz;

				ux = x2 - x0;
				uy = y2 - y0;
				uz = z2 - z0;
				n = plane2.A * ux + plane2.B * uy + plane2.C * uz;
			}
			else
			{
				sx = plane1.B * plane2.C - plane1.C * plane2.B;
				sy = plane1.C * plane2.A - plane1.A * plane2.C;
				sz = plane1.A * plane2.B - plane1.B * plane2.A;			
			}

			if (n.Equals(0D))
			{
				throw new ArgumentException("Плоскости параллельны!");
			}
			
			//Точка лежащая на обеих плоскостях
			var t = (-plane2.D - plane2.A * x0 - plane2.B * y0 - plane2.C * z0) / n;
			var m = new Point3D(x0 + ux * t, y0 + uy * t, z0 + uz * t);

			return new Line3D(m, new Point3D(sx + m.X, sy + m.Y, sz + m.Z));
		}

		/*
		public static Line3D operator *(Plane plane1, Plane plane2)
		{
			//Опускаем перпендикуляры из начала координат
			var t1 = -plane1.D/plane1.GetNorm2();
			var t2 = -plane2.D/plane2.GetNorm2();
			var point1 = new Point3D(plane1.A*t1, plane1.B*t1, plane1.C*t1);
			var point2 = new Point3D(plane2.A*t2, plane2.B*t2, plane2.C*t2);

			//Строим плоскость перпендикулярную обеим
			var plane3 = new Plane(Point3D.Empty, point1, point2);

			//Решая систему уравнений находим точку пересечения трех плоскостей
			var matrix = new Matrix3D(plane1.A, plane1.B, plane1.C, plane2.A, plane2.B, plane2.C, plane3.A, plane3.B, plane3.C);
			var p0 = matrix.Resolve(new Point3D(-plane1.D, -plane2.D, -plane3.D));

			//var matrix = new Matrix3(plane1.A, plane1.B, plane1.C, plane2.A, plane2.B, plane2.C, plane3.A, plane3.B, plane3.C);
			//var p0 = matrix.Resolve(new Point3D(-plane1.D, -plane2.D, -plane3.D));

			//var matrix = new Matrix3(plane1.A, plane1.B, plane1.C, plane2.A, plane2.B, plane2.C, plane3.A, plane3.B, plane3.C);
			//var p0 = matrix.Resolve(-plane1.D, -plane2.D, -plane3.D);
			
			var p1 = new Point3D(
				plane1.B*plane2.C - plane1.C*plane2.B + p0.X,
				plane1.C*plane2.A - plane1.A*plane2.C + p0.Y,
				plane1.A*plane2.B - plane1.B*plane2.A + p0.Z
				);

			return new Line3D(p0, p1);
		}
		*/

		/// <summary>
		/// Преобразует исходную плоскость в соответствии с заданным преобразованием.
		/// </summary>
		/// <param name="transform">Преобразование трехмерного евклидово пространства.</param>
		/// <param name="plane">Исходная плоскость.</param>
		/// <returns>Преобразованная плоскость.</returns>
		public static Plane operator *(Affinity transform, Plane plane)
		{
			return transform.Matrix*plane + transform.Center;

			/*
            var normal = new Point3D(plane.A, plane.B, plane.C);
            var point = -plane.D / plane.GetNorm() * normal;
            return new Plane(transform.Matrix * point + transform.Center, transform.Matrix * normal);
            */
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на плоскость в пространстве, преставленный структурой <see cref="Plane"/>.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="plane">Плоскость для перемножения.</param>
		/// <returns>Произведение исходной матрицы на плоскость <paramref name="plane"/>.</returns>
		public static Plane operator *(Matrix3D matrix, Plane plane)
		{
			var normal = new Point3D(plane.A, plane.B, plane.C);
			var point = -plane.D/plane.GetNorm()*normal;

			return new Plane(matrix*point, matrix*normal);
		}

		/// <summary>
		/// Перемещает исходную плоскость в направлении и расстоянии указанное вектором.
		/// </summary>
		/// <param name="plane">Исходная плоскость.</param>
		/// <param name="vector">Вектор указывающий перемещение.</param>
		/// <returns>Перемещенная плоскость.</returns>
		public static Plane operator +(Plane plane, Point3D vector)
		{
			return new Plane(plane.A, plane.B, plane.C, plane.D - plane.A*vector.X - plane.B*vector.Y - plane.C*vector.Z);
		}

		/// <summary>
		/// Перемещает исходную плоскость в направлении и расстоянии указанное вектором.
		/// </summary>
		/// <param name="vector">Вектор указывающий перемещение.</param>
		/// <param name="plane">Исходная плоскость.</param>
		/// <returns>Перемещенная плоскость.</returns>
		public static Plane operator +(Point3D vector, Plane plane)
		{
			return plane + vector;
		}

		/// <summary>
		/// Перемещает исходную плоскость в обратном направлении и расстоянии указанное вектором.
		/// </summary>
		/// <param name="plane">Исходная плоскость.</param>
		/// <param name="vector">Вектор указывающий перемещение.</param>
		/// <returns>Перемещенная плоскость.</returns>
		public static Plane operator -(Plane plane, Point3D vector)
		{
			return plane + -vector;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Приводит уравнение прямой к нормальному виду.
		/// </summary>
		public void Normalize()
		{
			if (IsNormalized)
			{
				return;
			}

			var norm = GetNorm();

			if (D > 0)
			{
				norm *= -1;
			}

			A /= norm;
			B /= norm;
			C /= norm;
			D /= norm;

			IsNormalized = true;

		}

		private double GetNorm2()
		{
			return IsNormalized ? 1 : A*A + B*B + C*C;
		}

		private double GetNorm()
		{
			return IsNormalized ? 1 : Math.Sqrt(A*A + B*B + C*C);
		}

		/// <summary>
		/// Находит точку Z = f(x,y).
		/// </summary>
		/// <param name="x">Параметр x.</param>
		/// <param name="y">Параметр y.</param>
		/// <returns>Параметр z.</returns>
		[Obsolete]
		public double GetZValue(double x, double y)
		{
			return -(A*x + B*y + D)/C;
			//return x*A + y*B + D;
		}





		/// <summary>
		/// Возвращает строковое представлеине данной плоскости.
		/// </summary>
		/// <returns>Строковое представлеине данной плоскости.</returns>
		/// <remarks>
		/// <code>
		/// var plane = new new Plane(1,1,1,1);
		/// Console.Write(plane); //Результат: n̅ = (-1/√3; -1/√3; -1/√3) p = 1/√3
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			var norm = GetNorm();

			double cos1, cos2, cos3, p;

			if (norm.Equals(0D) || double.IsNaN(norm))
			{
				cos1 = cos2 = cos3 = p = double.NaN;
			}
			else
			{
				var sign = Math.Sign(-D);
				if (sign == 0) sign = Math.Sign(A);
				if (sign == 0) sign = Math.Sign(B);
				if (sign == 0) sign = Math.Sign(C);

				norm *= sign;

				cos1 = A/norm;
				cos2 = B/norm;
				cos3 = C/norm;
				p = -D/norm;
			}

			/*
			return "cos" + CStatic.NarrowNbSp + "α = " + CStatic.DoubleToString(cos1) + CStatic.EmSp + "cos" + CStatic.NarrowNbSp +
			       "β = " + CStatic.DoubleToString(cos2) + CStatic.EmSp + "cos" + CStatic.NarrowNbSp + "γ = " +
			       CStatic.DoubleToString(cos3) + CStatic.EmSp + "p = " + CStatic.DoubleToString(p);
			*/

			return "n" + Macron + " = (" + DoubleToString(cos1) + "; " + DoubleToString(cos2) + "; " + DoubleToString(cos3) + ")" + EmSp + "p = " + DoubleToString(p);



			/*
			var result = "";

			CStatic.AddLinearItem(ref result, A, "x");
			CStatic.AddLinearItem(ref result, B, "y");
			CStatic.AddLinearItem(ref result, C, "z");
			CStatic.AddLinearItem(ref result, D, null);

			return result + " = 0";
			*/
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другая плоскость.</param>
		/// <returns>Значение <b>true</b>, если две прямые совпадают; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Plane other)
		{
			return this == other;
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Plane"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Plane && Equals((Plane) obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		public override int GetHashCode()
		{
			return 0;
		}

		#endregion
	}
}