using Ruzil3D.Algebra;
using static Ruzil3D.Utility.CStatic;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет прямую на плоскости.
	/// </summary>
	public struct Line
	{
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
		/// Инициализирует новую прямую в плоскости проходящую через заданные точки.
		/// </summary>
		/// <param name="point0">Первая заданная точка.</param>
		/// <param name="point1">Вторая заданная точка.</param>
		public Line(PointD point0, PointD point1)
		{
			A = point1.Y - point0.Y;
			B = point0.X - point1.X;
			C = -A*point0.X - B*point0.Y;

			Normalize();
		}

		/// <summary>
		/// Инициализирует новую прямую через коэффициенты общего уравнения прямой.
		/// </summary>
		/// <param name="a">Задает коэффициент A.</param>
		/// <param name="b">Задает коэффициент B.</param>
		/// <param name="c">Задает коэффициент C.</param>
		public Line(double a, double b, double c)
		{
			A = a;
			B = b;
			C = c;
		}

		#region Overloads

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух прямых.
		/// </summary>
		/// <param name="x">Первая прямая для сравнения.</param>
		/// <param name="y">Вторая прямая для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> задают одну и ту же прямую или имеют одинаковые коэффициенты; в противном случае — значение <b>false</b>.</returns>
		/// <remarks>
		/// Сравниваются прямые, а не коэффициенты: уравнения, отличающиеся ненулевым множителем (в том числе отрицательным), задают одну прямую.
		/// Нормали сравниваются с точностью 10⁻¹⁴, а расстояния до начала координат — с относительной точностью 10⁻¹⁴ (но не меньше 10⁻¹⁴ по абсолютной величине),
		/// чтобы погрешность приведения к нормальному виду не делала различными уравнения одной и той же прямой.
		/// Коэффициенты, которые не задают прямую (например, все равные нулю), равны только таким же коэффициентам.
		/// Если среди коэффициентов есть <see cref="double.NaN"/> (например, прямая построена по совпадающим точкам), оператор возвращает <b>false</b>, как и для <see cref="double.NaN"/>.
		/// </remarks>
		public static bool operator ==(Line x, Line y)
		{
			//Одинаковые коэффициенты задают одно и то же уравнение, даже если оно не задает прямую (например, все
			//коэффициенты равны нулю): прежде new Line(0, 0, 0) не была равна самой себе.
			if (x.A == y.A && x.B == y.B && x.C == y.C)
			{
				return true;
			}

			//Прежде сравнивались коэффициенты, умноженные на нормы, без допуска, а знак приводился только при C ≠ 0:
			//прямая, построенная по тем же точкам в обратном порядке, и прямые, проходящие через начало координат,
			//оказывались неравными. Теперь сравниваются нормальные уравнения с обоими знаками и с допуском.
			double xa, xb, xc, ya, yb, yc;
			if (!x.GetUnit(out xa, out xb, out xc) || !y.GetUnit(out ya, out yb, out yc))
			{
				return false;
			}

			var scale = Math.Max(1D, Math.Max(Math.Abs(xc), Math.Abs(yc)));

			return
				IsNear(xa, ya, xb, yb, xc, yc, scale) ||
				IsNear(xa, -ya, xb, -yb, xc, -yc, scale);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух прямых.
		/// </summary>
		/// <param name="x">Первая прямая для сравнения.</param>
		/// <param name="y">Вторая прямая для сравнения.</param>
		/// <returns>Значение <b>true</b>, если оператор <see cref="operator ==(Line, Line)"/> для тех же параметров возвращает <b>false</b>; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Line x, Line y)
		{
			return !(x == y);
		}

		#endregion

		#region Methods

		/*
		public double Resolve(PointD point)
		{
			return A * point.X + B * point.Y + C;
		}
		*/

		/// <summary>
		/// Приводит уравнение прямой к нормальному виду: нормаль (<see cref="A"/>, <see cref="B"/>) получает единичную длину, а <see cref="C"/> ≤ 0.
		/// </summary>
		public void Normalize()
		{
			//Сумма квадратов вычисляется один раз, а не трижды (в IsNormalized, в GetNorm и в Length).
			var norm2 = A*A + B*B;
			if (IsUnit(norm2, C))
			{
				return;
			}

			var norm = Point3D.GetLength(norm2, A, B, 0, 0);

			if (C > 0)
			{
				norm *= -1;
			}

			A /= norm;
			B /= norm;
			C /= norm;
		}

		//Допуск, с которым сравниваются прямые: разность единичных нормалей и относительная разность расстояний
		//до начала координат. Он покрывает погрешность приведения к нормальному виду (несколько ulp), но не больше:
		//с допуском 1e-10 равными оказывались прямые на расстоянии 1e12 от начала координат, отстоящие друг от друга на 70.
		private const double EqualityTolerance = 1E-14;

		//Допуск для признака нормального вида: после Normalize квадрат длины нормали отличается от 1 на несколько ulp.
		private const double NormalizedTolerance = 1E-14;

		//Уравнение приведено к нормальному виду (с точностью до погрешности округления после Normalize).
		//Признак вычисляется по текущим коэффициентам: поля A, B и C открыты для записи, и прежде сохраненный флаг
		//устаревал — после изменения коэффициентов GetNorm возвращал 1, а Normalize ничего не делал.
		private bool IsNormalized => IsUnit(A*A + B*B, C);

		//Признак нормального вида по квадрату длины нормали и коэффициенту C.
		private static bool IsUnit(double norm2, double c) => Math.Abs(norm2 - 1) <= NormalizedTolerance && c <= 0;

		private double GetNorm()
		{
			return IsNormalized ? 1 : new PointD(A, B).Length;
		}

		//Коэффициенты уравнения, деленные на длину нормали. Возвращает false, если коэффициенты не задают прямую.
		private bool GetUnit(out double a, out double b, out double c)
		{
			var norm = new PointD(A, B).Length;
			a = A/norm;
			b = B/norm;
			c = C/norm;

			return norm > 0 && !double.IsInfinity(norm) && !double.IsNaN(c) && !double.IsInfinity(c);
		}

		private static bool IsNear(double a1, double a2, double b1, double b2, double c1, double c2, double scale)
		{
			return Math.Abs(a1 - a2) <= EqualityTolerance && Math.Abs(b1 - b2) <= EqualityTolerance && Math.Abs(c1 - c2) <= EqualityTolerance*scale;
		}

		//Хотя бы один коэффициент не является числом (например, прямая построена по двум совпадающим точкам).
		//Проверка нужна, потому что Math.Sign(NaN) выбрасывает исключение, и прежде падал даже ToString.
		private bool HasNaN => double.IsNaN(A) || double.IsNaN(B) || double.IsNaN(C);

		/// <summary>
		/// Возвращает значение указывающее на пересечение данной прямой с указанным отрезком.
		/// </summary>
		/// <param name="point0">Первая точка отрезка</param>
		/// <param name="point1">Вторая точка отрезка</param>
		/// <returns>Значение <b>true</b>, если отрезок пересекает прямую или касается ее; в противном случае — значение <b>false</b>.</returns>
		public bool IntersectsWith(PointD point0, PointD point1)
		{
			//Сравниваются знаки значений уравнения на концах отрезка. Прежде значения перемножались, и произведение
			//маленьких чисел одного знака обращалось в ноль: отрезок на высоте 1e-200 над прямой y = 0 «пересекал» ее.
			var value0 = A*point0.X + B*point0.Y + C;
			var value1 = A*point1.X + B*point1.Y + C;

			return value0 <= 0 && value1 >= 0 || value0 >= 0 && value1 <= 0;
		}

		/// <summary>
		/// Получает угол нормали.
		/// </summary>
		public double Theta
		{
			get
			{
				var norm = GetNorm();

				double theta;

				if (norm.Equals(0D) || double.IsNaN(norm) || HasNaN)
				{
					theta = double.NaN;
				}
				else
				{
					var sign = double.IsNaN(C) ? 0 : Math.Sign(-C);

					if (sign == 0)
					{
						sign = A.Equals(0D) ? Math.Sign(B) : Math.Sign(A);
					}

					theta = B.Equals(0D) ? Math.Atan2(0D, sign*A) : Math.Atan2(sign*B, sign*A);
				}

				return theta;
			}
		}

		/// <summary>
		/// Получает расстояние от начала координат до прямой.
		/// </summary>
		public double P
		{
			get
			{
				var norm = GetNorm();

				double p;

				if (norm.Equals(0D) || double.IsNaN(norm) || HasNaN)
				{
					p = double.NaN;
				}
				else
				{
					var sign = double.IsNaN(C) ? 0 : Math.Sign(-C);

					if (sign == 0)
					{
						sign = A.Equals(0D) ? Math.Sign(B) : Math.Sign(A);
					}

					var theta = Math.Atan2(sign*B, sign*A);
					p = double.IsNaN(theta) ? double.NaN : -sign*C/norm;
				}

				return p;
			}
		}

		/// <summary>
		/// Возвращает строковое представление данной прямой.
		/// </summary>
		/// <returns>Строковое представление данной прямой.</returns>
		/// <remarks>
		/// <code>
		/// var line = new Line(1,1,3);
		/// Console.Write(line); //Результат: θ = -3π/4 p = 3/√2
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			/*
			var norm = GetNorm();

			double theta,  p;

			if (norm.Equals(0D) || double.IsNaN(norm))
			{
				theta = p = double.NaN;
			}
			else
			{
				var sign = double.IsNaN(C) ? 0 : Math.Sign(-C);
				
				if (sign == 0)
				{
					sign = A.Equals(0D) ? Math.Sign(B) : Math.Sign(A);
				}

				theta = Math.Atan2(sign * B, sign * A);
				p = double.IsNaN(theta) ? double.NaN : -sign * C / norm;
			}
            */

			return "θ = " + AngleToString(Theta) + EmSp + "p = " + DoubleToString(P);
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другая прямая.</param>
		/// <returns>Значение <b>true</b>, если две прямые совпадают (см. <see cref="operator ==(Line, Line)"/>) или имеют одинаковые коэффициенты; в противном случае — значение <b>false</b>.</returns>
		/// <remarks>В отличие от оператора ==, метод, как и <see cref="double.Equals(double)"/>, считает равными и одинаковые коэффициенты <see cref="double.NaN"/>, поэтому любая прямая равна самой себе.</remarks>
		public bool Equals(Line other)
		{
			//Метод рефлексивен и для коэффициентов NaN: прежде такая прямая не находилась, например, в List.Contains.
			return this == other || A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Line"/> и равен исходной прямой в смысле метода <see cref="Equals(Line)"/>; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Line && Equals((Line) obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		/// <remarks>Хэш-код одинаков для всех прямых, так как равные прямые могут иметь разные коэффициенты.</remarks>
		public override int GetHashCode()
		{
			return 0;
		}

		#endregion
	}
}