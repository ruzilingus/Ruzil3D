using static Ruzil3D.Math;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет точку/вектор в трехмерном евклидовом пространстве.
	/// </summary>
	public struct Point3D
	{
		/// <summary>
		/// Получает или задает координату X точки <see cref="Point3D"/>.
		/// </summary>
		public double X;

		/// <summary>
		/// Получает или задает координату Y точки <see cref="Point3D"/>.
		/// </summary>
		public double Y;

		/// <summary>
		/// Получает или задает координату Z точки <see cref="Point3D"/>.
		/// </summary>
		public double Z;

		/// <summary>
		/// Представляет новый экземпляр класса <see cref="Point3D"/> с неинициализированными данными членов.
		/// </summary>
		public static readonly Point3D Empty = new Point3D(0, 0, 0);

		/// <summary>
		/// Представляет вектор (1, 0, 0).
		/// </summary>
		public static readonly Point3D UnitX = new Point3D(1, 0, 0);

		/// <summary>
		/// Представляет вектор (0, 1, 0).
		/// </summary>
		public static readonly Point3D UnitY = new Point3D(0, 1, 0);

		/// <summary>
		/// Представляет вектор (0, 0, 1).
		/// </summary>
		public static readonly Point3D UnitZ = new Point3D(0, 0, 1);

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Point3D"/> с указанными координатами.
		/// </summary>
		/// <param name="x">Координата по оси <paramref name="x"/>.</param>
		/// <param name="y">Координата по оси <paramref name="y"/>.</param>
		/// <param name="z">Координата по оси <paramref name="z"/>.</param>
		public Point3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Возвращает значение, показывающее, являтся ли данный вектор нулевым.
		/// </summary>
		/// <param name="x">Вектор представленный структурой <see cref="Point3D"/>.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="x"/> равняется <see cref="Empty"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsEmpty(Point3D x) => x == Empty;

		/// <summary>
		/// Возращает длину исходного вектора.
		/// </summary>
		public double Length => Sqrt(X * X + Y * Y + Z * Z);

		/// <summary>
		/// Возвращает условие показывающее, что хотя бы один из компонентов X, Y или Z не является числом.
		/// </summary>
		public bool IsNaN => double.IsNaN(X) || double.IsNaN(Y) || double.IsNaN(Y);

		#region Overloads

		/// <summary>
		/// Определяет неявное преобразование двухмерной структуры <see cref="PointD"/> в трехмерный <see cref="Point3D"/>.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в <see cref="Point3D"/>.</param>
		/// <returns>Преобразованный объект <see cref="Point3D"/> со значением 0 в качестве параметра <see cref="Z"/></returns>
		public static implicit operator Point3D(PointD value)
		{
			return new Point3D(value.X, value.Y, 0);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух точек.
		/// </summary>
		/// <param name="point1">Первая точка для сравнения.</param>
		/// <param name="point2">Вторая точка для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="point1"/> и <paramref name="point2"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Point3D point1, Point3D point2)
		{
			return point1.Equals(point2);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух точек.
		/// </summary>
		/// <param name="point1">Первая точка для сравнения.</param>
		/// <param name="point2">Вторая точка для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="point1"/> и <paramref name="point2"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Point3D point1, Point3D point2)
		{
			return !(point1 == point2);
		}

		/// <summary>
		/// Складывает два вектора представленные структурой <see cref="Point3D"/>.
		/// </summary>
		/// <param name="point1">Первое из складываемых векторов.</param>
		/// <param name="point2">Второе из складываемых векторов.</param>
		/// <returns>Сумма <paramref name="point1"/> и <paramref name="point2"/>.</returns>
		public static Point3D operator +(Point3D point1, Point3D point2)
		{
			return new Point3D(point1.X + point2.X, point1.Y + point2.Y, point1.Z + point2.Z);
		}

		/// <summary>
		/// Добавляет исходный вектор ко всем элементам массива.
		/// </summary>
		/// <param name="offset">Исходный добавляемый вектор.</param>
		/// <param name="points">Массив структур <see cref="Point3D"/> к которому добавляется вектор <paramref name="offset"/>.</param>
		/// <returns>Массив структур к элементам которым добавлен <paramref name="offset"/>.</returns>
		public static Point3D[] operator +(Point3D offset, Point3D[] points)
		{
			var result = new Point3D[points.Length];
			for (var i = 0; i < points.Length; i++)
			{
				result[i] = points[i] + offset;
			}

			return result;

		}

		/// <summary>
		/// Вычитает элементы массива от исходного вектора.
		/// </summary>
		/// <param name="offset">Исходный вектор.</param>
		/// <param name="points">Вычитаемый массив структур <see cref="Point3D"/> которые вычитаются от исходного вектора <paramref name="offset"/>.</param>
		/// <returns>Массив структур к элементы которого вычтены из <paramref name="offset"/>.</returns>
		public static Point3D[] operator -(Point3D offset, Point3D[] points)
		{
			var result = new Point3D[points.Length];
			for (var i = 0; i < points.Length; i++)
			{
				result[i] = offset - points[i];
			}

			return result;
		}

		/// <summary>
		/// Добавляет ко всем элементам массива исходный вектор.
		/// </summary>
		/// <param name="points">Массив структур <see cref="Point3D"/> к которым добавляется вектор <paramref name="offset"/>.</param>
		/// <param name="offset">Исходный добавляемый вектор.</param>
		/// <returns>Массив структур к элементам которым добавлен <paramref name="offset"/>.</returns>
		public static Point3D[] operator +(Point3D[] points, Point3D offset)
		{
			return offset + points;
		}

		/// <summary>
		/// Добавляет со всех элементов массива исходный вектор.
		/// </summary>
		/// <param name="points">Массив структур <see cref="Point3D"/> из которых вычитается вектор <paramref name="offset"/>.</param>
		/// <param name="offset">Исходный вычитаемый вектор.</param>
		/// <returns>Массив структур из элементов которого вычтен <paramref name="offset"/>.</returns>
		public static Point3D[] operator -(Point3D[] points, Point3D offset)
		{

			return -offset + points;
		}

		/// <summary>
		/// Вычитает два вектора представленные структурой <see cref="Point3D"/>.
		/// </summary>
		/// <param name="point1">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="point2">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="point1"/> и <paramref name="point2"/>.</returns>
		public static Point3D operator -(Point3D point1, Point3D point2)
		{
			return new Point3D(point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z);
		}

		/// <summary>
		/// Возвращает аддитивную инверсию вектора заданного параметром <paramref name="point"/>.
		/// </summary>
		/// <param name="point">Инвертируемое значение.</param>
		/// <returns>Результат умножения исходного вектора на -1.</returns>
		public static Point3D operator -(Point3D point)
		{
			return new Point3D(-point.X, -point.Y, -point.Z);
		}

		/// <summary>
		/// Возвращает исходный вектор.
		/// </summary>
		/// <param name="point">Исходный вектор.</param>
		/// <returns>Исходный вектор.</returns>s
		public static Point3D operator +(Point3D point)
		{
			return point;
		}

		/// <summary>
		/// Возвращает произведение исходного вектора на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <param name="point">Исходный вектор.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="point"/>.</returns>
		public static Point3D operator *(double x, Point3D point)
		{
			return new Point3D(x * point.X, x * point.Y, x * point.Z);
		}

		/// <summary>
		/// Возвращает произведение исходного вектора на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="point">Исходный вектор.</param>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="point"/>.</returns>
		public static Point3D operator *(Point3D point, double x)
		{
			return x * point;
		}

		/// <summary>
		/// Возвращает векторное произведение двух векторов.
		/// </summary>
		/// <param name="point1">Первый вектор для векторного перемножения.</param>
		/// <param name="point2">Второй вектор для векторного перемножения.</param>
		/// <returns>Произведение <paramref name="point1"/> и <paramref name="point2"/>.</returns>
		public static Point3D operator *(Point3D point1, Point3D point2)
		{
			return new Point3D(
				point1.Y * point2.Z - point1.Z * point2.Y,
				point1.Z * point2.X - point1.X * point2.Z,
				point1.X * point2.Y - point1.Y * point2.X
				);
		}

		/// <summary>
		/// Делит исходный вектор на число с плавающей запятой двойной точности и возвращает результат.
		/// </summary>
		/// <param name="point">Исходный вектор.</param>
		/// <param name="x">Знаменатель с плавающей запятой двойной точности.</param>
		/// <returns>Частное от деления <paramref name="point"/> на <paramref name="x"/>.</returns>
		public static Point3D operator /(Point3D point, double x)
		{
			return new Point3D(point.X / x, point.Y / x, point.Z / x);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Возвращает скалярное произведение текущего вектора на вектор заданный структурой <see cref="Point3D"/>.
		/// </summary>
		/// <param name="point">Вектор, на который скалярно умножается текущий вектор.</param>
		/// <returns>Скалярное произведение текущего вектора на вектор заданный параметром <paramref name="point"/>.</returns>
		public double DotProduct(Point3D point)
		{
			return X * point.X + Y * point.Y + Z * point.Z;
		}

		/// <summary>
		/// Возвращает косинус угла между текущим вектором и вектором заданным параметром <paramref name="point"/>.
		/// </summary>
		/// <param name="point">Вектор, между которым возвращается косинус угла.</param>
		/// <returns>Косинус угла между текущим вектором и вектором заданным параметром <paramref name="point"/>.</returns>
		public double Cos(Point3D point)
		{
			return DotProduct(point) / (Length * point.Length);
		}

		/// <summary>
		/// Возвращает угол между текущим вектором и вектором заданным параметром <paramref name="point"/>.
		/// </summary>
		/// <param name="point">Вектор, между которым возвращается угол.</param>
		/// <returns>Угол между текущим вектором и вектором заданным параметром <paramref name="point"/>.</returns>
		public double Angle(Point3D point)
		{
			return Acos(Cos(point));
		}

		/// <summary>
		/// Прибавляет текущему вектору вектор заданный параметром <paramref name="point"/>.
		/// </summary>
		/// <param name="point">Прибавляемый вектор.</param>
		public void Add(Point3D point)
		{
			X += point.X;
			Y += point.Y;
			Z += point.Z;
		}

		/// <summary>
		/// Возвращает расстояние от текущей точки до точки заданной параметром <paramref name="point"/>.
		/// </summary>
		/// <param name="point">Точка, до которой нужно вернуть расстояние.</param>
		/// <returns>Расстояние от текущей точки до точки заданной параметром <paramref name="point"/>.</returns>
		public double Distance(Point3D point)
		{
			var dx = X - point.X;
			var dy = Y - point.Y;
			var dz = Z - point.Z;
			return Sqrt(dx * dx + dy * dy + dz * dz);
		}

		/// <summary>
		/// Возвращает проекцию текущей точки на плоскость xOy.
		/// </summary>
		/// <returns>Проекция текущей точки на плоскость xOy.</returns>
		public PointD ToPointD()
		{
			return new PointD(X, Y);
		}

		/// <summary>
		/// Возвращает скалярное произведение текущего вектора на вектор заданный структурой <see cref="PointD"/>.
		/// </summary>
		/// <param name="point">Вектор, на который скалярно умножается текущий вектор.</param>
		/// <returns>Скалярное произведение текущего на вектор заданный структурой <see cref="PointD"/>.</returns>
		public double DotProduct(PointD point)
		{
			return X * point.X + Y * point.Y;
		}

		/// <summary>
		/// Округление числа, до указанного количества значимых десятичных символов.
		/// </summary>
		/// <param name="value">Округляемое число.</param>
		/// <param name="order">Порядок, указывающий на количество значимых десятичных символов.</param>
		/// <returns>Число <paramref name="value"/> округленное до определенного количества значимых десятичных символов заданный параметром <paramref name="order"/></returns>
		private static double Round15(double value, double order)
		{
			if (order.Equals(0D))
			{
				return value;
			}
			return Round(order * value) / order;
		}

		/// <summary>
		/// Возвращает строковое представлеине данной точки.
		/// </summary>
		/// <returns>Строковое представлеине данной точки.</returns>
		public override string ToString()
		{
			/*
			var order = 1E13 * Length;
			var x = Round15(X, order);
			var y = Round15(Y, order);
			var z = Round15(Z, order);
			*/

			var order = Max(0, 13 - (int)GetOrder(Length));
			var x = System.Math.Round(X, order);
			var y = System.Math.Round(Y, order);
			var z = System.Math.Round(Z, order);

			return nameof(X) + " = " + x + " " + nameof(Y) + " = " + y + " " + nameof(Z) + " = " + z;
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другая точка.</param>
		/// <returns>Значение <b>true</b>, если две точки совпадают; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Point3D other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Point3D"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Point3D && Equals((Point3D)obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = X.GetHashCode();
				hashCode = (hashCode * 397) ^ Y.GetHashCode();
				hashCode = (hashCode * 397) ^ Z.GetHashCode();
				return hashCode;
			}
		}

		#endregion
	}
}