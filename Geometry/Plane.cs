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
		/// Получает значение указывающее, что уравнение плоскости является каноническим: нормаль (<see cref="A"/>, <see cref="B"/>, <see cref="C"/>) имеет единичную длину (с точностью до погрешности округления), а <see cref="D"/> ≤ 0.
		/// </summary>
		/// <remarks>Значение вычисляется по текущим коэффициентам, поэтому учитывает и их изменение после вызова <see cref="Normalize"/>.</remarks>
		public bool IsNormalized => Math.Abs(A*A + B*B + C*C - 1) <= NormalizedTolerance && D <= 0;

		/// <summary>
		/// Получает или задает коэффициент <see cref="A"/> из общего уравнения плоскости.
		/// </summary>
		public double A;

		/// <summary>
		/// Получает или задает коэффициент <see cref="B"/> из общего уравнения плоскости.
		/// </summary>
		public double B;

		/// <summary>
		/// Получает или задает коэффициент <see cref="C"/> из общего уравнения плоскости.
		/// </summary>
		public double C;

		/// <summary>
		/// Получает или задает коэффициент <see cref="D"/> из общего уравнения плоскости.
		/// </summary>
		public double D;

		//Допуск, с которым сравниваются плоскости и проверяется параллельность: синус угла между нормалями
		//(или между нормалью и прямой) и относительное расстояние. Погрешность округления при построении
		//плоскостей и прямых по точкам на несколько порядков меньше.
		private const double Tolerance = 1E-10;

		//Допуск для признака нормального вида: после Normalize квадрат длины нормали отличается от 1 на несколько ulp.
		private const double NormalizedTolerance = 1E-14;


		/// <summary>
		/// Инициализирует новую плоскость в трехмерном евклидовом пространстве проходящую через заданные точки.
		/// </summary>
		/// <param name="point0">Первая заданная точка.</param>
		/// <param name="point1">Вторая заданная точка.</param>
		/// <param name="point2">Третья заданная точка.</param>
		public Plane(Point3D point0, Point3D point1, Point3D point2)
		{
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
			A = normal.X;
			B = normal.Y;
			C = normal.Z;
			D = -A*point.X - B*point.Y - C*point.Z;

			Normalize();
		}

		/// <summary>
		/// Инициализирует новую плоскость в трехмерном евклидовом пространстве через коэффициенты общего уравнения плоскости.
		/// </summary>
		/// <param name="a">Задает коэффициент A.</param>
		/// <param name="b">Задает коэффициент B.</param>
		/// <param name="c">Задает коэффициент C.</param>
		/// <param name="d">Задает коэффициент D.</param>
		public Plane(double a, double b, double c, double d)
		{
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
		/// В канонической форме <see cref="D"/> ≤ 0, поэтому отрицательное значение означает, что точка <paramref name="point"/> и начало координат находятся по одну сторону от плоскости,
		/// а положительное — что по разные стороны. Например, для плоскости z = 1 значение в точке (0, 0, 0.5) равно -0.5, а в точке (0, 0, 2) равно 1.
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
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> задают одну и ту же плоскость или имеют одинаковые коэффициенты; в противном случае — значение <b>false</b>.</returns>
		/// <remarks>
		/// Сравниваются плоскости, а не коэффициенты: уравнения, отличающиеся ненулевым множителем (в том числе отрицательным), задают одну плоскость.
		/// Нормали сравниваются с точностью 10⁻¹⁰, а расстояния до начала координат — с относительной точностью 10⁻¹⁰ (но не меньше 10⁻¹⁰ по абсолютной величине),
		/// чтобы погрешность округления не делала различными одну и ту же плоскость, построенную, например, по точкам в разном порядке.
		/// Коэффициенты, которые не задают плоскость (например, все равные нулю), равны только таким же коэффициентам.
		/// Если среди коэффициентов есть <see cref="double.NaN"/> (например, плоскость построена по точкам на одной прямой), оператор возвращает <b>false</b>, как и для <see cref="double.NaN"/>.
		/// </remarks>
		public static bool operator ==(Plane x, Plane y)
		{
			//Одинаковые коэффициенты задают одно и то же уравнение, даже если оно не задает плоскость (например, все
			//коэффициенты равны нулю): прежде new Plane(0, 0, 0, 0) не была равна самой себе.
			if (x.A == y.A && x.B == y.B && x.C == y.C && x.D == y.D)
			{
				return true;
			}

			//Прежде сравнивались коэффициенты, умноженные на нормы, без допуска, а знак приводился только при D ≠ 0:
			//плоскость, построенная по тем же точкам в другом порядке, и плоскости, проходящие через начало координат,
			//оказывались неравными. Теперь сравниваются нормальные уравнения с обоими знаками и с допуском.
			Point3D xNormal, yNormal;
			double xd, yd;
			if (!x.GetUnit(out xNormal, out xd) || !y.GetUnit(out yNormal, out yd))
			{
				return false;
			}

			var scale = Math.Max(1D, Math.Max(Math.Abs(xd), Math.Abs(yd)));

			return
				IsNear(xNormal, yNormal, xd, yd, scale) ||
				IsNear(xNormal, -yNormal, xd, -yd, scale);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух плоскостей.
		/// </summary>
		/// <param name="x">Первая плоскость для сравнения.</param>
		/// <param name="y">Вторая плоскость для сравнения.</param>
		/// <returns>Значение <b>true</b>, если оператор <see cref="operator ==(Plane, Plane)"/> для тех же параметров возвращает <b>false</b>; в противном случае — значение <b>false</b>.</returns>
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
		/// <exception cref="ArgumentException">Если прямая и плоскость параллельны: синус угла между ними не больше 10⁻¹⁰, в том числе если прямая лежит в плоскости.</exception>
		public static Point3D operator *(Plane plane, Line3D line)
		{
			var normal = plane.GetNormal();
			var denominator = normal.DotProduct(line.S);

			//Параллельность проверяется с допуском относительно длин нормали и направляющей: прежде проверялось точное
			//равенство нулю, и погрешность округления для параллельной прямой давала точку на расстоянии ~1e15.
			if (Math.Abs(denominator) <= Tolerance*normal.Length*line.S.Length)
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
		/// <returns>Линия пересечения друх плоскостей. Ее точка <see cref="Line3D.M"/> — ближайшая к началу координат, а направляющая сонаправлена векторному произведению нормалей.</returns>
		/// <exception cref="ArgumentException">
		/// Один из параметров не является плоскостью (коэффициенты не являются конечными числами или нормаль нулевая)
		/// либо плоскости параллельны (синус угла между ними не больше 10⁻¹⁰), в том числе совпадают.
		/// </exception>
		public static Line3D operator *(Plane plane1, Plane plane2)
		{
			Point3D normal1, normal2;
			double d1, d2;

			if (!plane1.GetUnit(out normal1, out d1))
			{
				throw new ArgumentException("Параметр " + nameof(plane1) + " не является плоскостью!");
			}

			if (!plane2.GetUnit(out normal2, out d2))
			{
				throw new ArgumentException("Параметр " + nameof(plane2) + " не является плоскостью!");
			}

			//Направляющая перпендикулярна обеим нормалям, а для единичных нормалей ее длина равна синусу угла между
			//плоскостями. Прежде параллельность проверялась точным сравнением с нулем: для параллельных плоскостей
			//получалась прямая на расстоянии ~1e15 или, в зависимости от порядка плоскостей, исключение о совпадающих точках.
			var s = normal1*normal2;
			var sin2 = s.DotProduct(s);

			if (!(sin2 > Tolerance*Tolerance))
			{
				throw new ArgumentException("Плоскости параллельны!");
			}

			//Ближайшая к началу координат точка прямой (m·n₁ = -d₁, m·n₂ = -d₂, m·s = 0). Прежде точка искалась через
			//первый ненулевой коэффициент первой плоскости, и при малом ненулевом коэффициенте (например, -5.55e-17 из-за
			//округления) погрешность достигала расстояния между точками плоскостей, а при 1e-310 выбрасывалось исключение.
			var m = (-d1*(normal2*s) - d2*(s*normal1))/sin2;

			//Прямая строится от начала координат и затем переносится в точку m, чтобы направляющая не теряла точность.
			return new Line3D(Point3D.Empty, s) + m;
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
		/// <returns>Преобразованная плоскость: образ плоскости <paramref name="plane"/> при преобразовании <paramref name="transform"/>.</returns>
		/// <remarks>Если матрица преобразования вырождена и отображает плоскость в прямую или точку, коэффициенты результата не являются числами (<see cref="double.NaN"/>).</remarks>
		public static Plane operator *(Affinity transform, Plane plane)
		{
			return transform.Matrix*plane + transform.Center;
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на плоскость в пространстве, преставленный структурой <see cref="Plane"/>.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="plane">Плоскость для перемножения.</param>
		/// <returns>Произведение исходной матрицы на плоскость <paramref name="plane"/>: образ плоскости при линейном преобразовании с матрицей <paramref name="matrix"/>, приведенный к каноническому виду.</returns>
		/// <remarks>Если матрица вырождена и отображает плоскость в прямую или точку, коэффициенты результата не являются числами (<see cref="double.NaN"/>).</remarks>
		public static Plane operator *(Matrix3D matrix, Plane plane)
		{
			//Точка x лежит на образе плоскости, если n·M⁻¹x + D = 0, то есть (M⁻ᵀn)·x + D = 0. Уравнение умножается на
			//определитель: нормаль преобразуется матрицей алгебраических дополнений det(M)·M⁻ᵀ, а D умножается на det(M),
			//поэтому результат верен и для вырожденной матрицы, отображающей плоскость на плоскость.
			//Прежде нормаль умножалась на саму матрицу (неверно для любого преобразования, кроме поворота и равномерного
			//масштабирования), а точка плоскости бралась на расстоянии -D/|n| вместо -D/|n|² от начала координат.
			var normal = matrix.GetAdjugate().GetTranspose()*plane.GetNormal();
			var result = new Plane(normal.X, normal.Y, normal.Z, matrix.GetDeterminant()*plane.D);
			result.Normalize();

			return result;
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
		/// Приводит уравнение плоскости к нормальному виду: нормаль (<see cref="A"/>, <see cref="B"/>, <see cref="C"/>) получает единичную длину, а <see cref="D"/> ≤ 0.
		/// </summary>
		public void Normalize()
		{
			//Признак нормального вида вычисляется по текущим коэффициентам: прежде сохраненный флаг устаревал после
			//изменения полей A, B, C или D, и метод ничего не делал.
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
		}

		private double GetNorm()
		{
			return IsNormalized ? 1 : GetNormal().Length;
		}

		private Point3D GetNormal()
		{
			return new Point3D(A, B, C);
		}

		//Нормаль единичной длины и коэффициент D, деленный на длину нормали. Возвращает false, если коэффициенты не задают плоскость.
		private bool GetUnit(out Point3D normal, out double d)
		{
			var norm = GetNormal().Length;
			normal = GetNormal()/norm;
			d = D/norm;

			return norm > 0 && !double.IsInfinity(norm) && !double.IsNaN(d) && !double.IsInfinity(d);
		}

		private static bool IsNear(Point3D normal1, Point3D normal2, double d1, double d2, double scale)
		{
			return
				Math.Abs(normal1.X - normal2.X) <= Tolerance &&
				Math.Abs(normal1.Y - normal2.Y) <= Tolerance &&
				Math.Abs(normal1.Z - normal2.Z) <= Tolerance &&
				Math.Abs(d1 - d2) <= Tolerance*scale;
		}

		/// <summary>
		/// Находит точку Z = f(x,y).
		/// </summary>
		/// <param name="x">Параметр x.</param>
		/// <param name="y">Параметр y.</param>
		/// <returns>Параметр z.</returns>
		[Obsolete("Метод устарел. Используйте пересечение плоскости с прямой, параллельной оси Z: plane * new Line3D(new Point3D(x, y, 0), new Point3D(x, y, 1)).")]
		public double GetZValue(double x, double y)
		{
			return -(A*x + B*y + D)/C;
			//return x*A + y*B + D;
		}





		/// <summary>
		/// Возвращает строковое представление данной плоскости.
		/// </summary>
		/// <returns>Строковое представление данной плоскости.</returns>
		/// <remarks>
		/// <code>
		/// var plane = new Plane(1,1,1,1);
		/// Console.Write(plane); //Результат: n̅ = (-1/√3; -1/√3; -1/√3) p = 1/√3
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			var norm = GetNorm();

			double cos1, cos2, cos3, p;

			//Проверяем и сами коэффициенты: Math.Sign(NaN) выбрасывает исключение, и прежде ToString падал,
			//если плоскость построена по коллинеарным точкам или коэффициент D не является числом.
			if (norm.Equals(0D) || double.IsNaN(norm) ||
			    double.IsNaN(A) || double.IsNaN(B) || double.IsNaN(C) || double.IsNaN(D))
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
		/// <returns>Значение <b>true</b>, если две плоскости совпадают (см. <see cref="operator ==(Plane, Plane)"/>) или имеют одинаковые коэффициенты; в противном случае — значение <b>false</b>.</returns>
		/// <remarks>В отличие от оператора ==, метод, как и <see cref="double.Equals(double)"/>, считает равными и одинаковые коэффициенты <see cref="double.NaN"/>, поэтому любая плоскость равна самой себе.</remarks>
		public bool Equals(Plane other)
		{
			//Метод рефлексивен и для коэффициентов NaN: прежде такая плоскость не находилась, например, в List.Contains.
			return this == other || A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Plane"/> и равен исходной плоскости в смысле метода <see cref="Equals(Plane)"/>; в противном случае — значение <b>false</b>.
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
		/// <remarks>Хэш-код одинаков для всех плоскостей, так как равные плоскости могут иметь разные коэффициенты.</remarks>
		public override int GetHashCode()
		{
			return 0;
		}

		#endregion
	}
}