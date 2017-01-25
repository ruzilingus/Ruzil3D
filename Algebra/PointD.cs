using System.Drawing;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет точку/вектор на плоскости.
	/// </summary>
	public struct PointD
	{
		/// <summary>
		/// Получает или задает координату X точки <see cref="PointD"/>.
		/// </summary>
		public double X;

		/// <summary>
		/// Получает или задает координату Y точки <see cref="PointD"/>.
		/// </summary>
		public double Y;

		/// <summary>
		/// Представляет новый экземпляр класса <see cref="PointD"/> с неинициализированными данными членов.
		/// </summary>
		public static readonly PointD Empty = PointF.Empty;

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="PointD"/> с указанными координатами.
		/// </summary>
		/// <param name="x">Координата по оси <paramref name="x"/>.</param>
		/// <param name="y">Координата по оси <paramref name="y"/>.</param>
		public PointD(double x, double y)
		{
			X = x;
			Y = y;
		}
		
		#endregion

		#region Overloads

		/// <summary>
		/// Складывает два вектора представленные структурой <see cref="PointD"/>.
		/// </summary>
		/// <param name="point0">Первое из складываемых векторов.</param>
		/// <param name="point1">Второе из складываемых векторов.</param>
		/// <returns>Сумма <paramref name="point0"/> и <paramref name="point1"/>.</returns>
		public static PointD operator +(PointD point0, PointD point1)
		{
			return new PointD(point0.X + point1.X, point0.Y + point1.Y);
		}

		/// <summary>
		/// Добавляет ко всем элементам массива исходный вектор.
		/// </summary>
		/// <param name="points">Массив структур <see cref="PointD"/> к которым добавляется вектор <paramref name="offset"/>.</param>
		/// <param name="offset">Исходный добавляемый вектор.</param>
		/// <returns>Массив структур к элементам которым добавлен <paramref name="offset"/>.</returns>
		public static PointD[] operator +(PointD[] points, PointD offset)
		{
			var result = new PointD[points.Length];
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
		/// <param name="points">Вычитаемый массив структур <see cref="PointD"/> которые вычитаются от исходного вектора <paramref name="offset"/>.</param>
		/// <returns>Массив структур к элементы которого вычтены из <paramref name="offset"/>.</returns>
		public static PointD[] operator +(PointD offset, PointD[] points)
		{
			return points + offset;
		}

		/// <summary>
		/// Вычитает два вектора представленные структурой <see cref="PointD"/>.
		/// </summary>
		/// <param name="point0">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="point1">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="point0"/> и <paramref name="point1"/>.</returns>
		public static PointD operator -(PointD point0, PointD point1)
		{
			return new PointD(point0.X - point1.X, point0.Y - point1.Y);
		}

		/// <summary>
		/// Возвращает аддитивную инверсию вектора заданного параметром <paramref name="point"/>.
		/// </summary>
		/// <param name="point">Инвертируемое значение.</param>
		/// <returns>Результат умножения исходного вектора на -1.</returns>
		public static PointD operator -(PointD point)
		{
			return new PointD(-point.X, -point.Y);
		}

		/// <summary>
		/// Возвращает исходный вектор.
		/// </summary>
		/// <param name="point">Исходный вектор.</param>
		/// <returns>Исходный вектор.</returns>
		public static PointD operator +(PointD point)
		{
			return point;
		}

		/// <summary>
		/// Возвращает произведение исходного вектора на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <param name="point">Исходный вектор.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="point"/>.</returns>
		public static PointD operator *(double x, PointD point)
		{
			return new PointD(x*point.X, x*point.Y);
		}

		/// <summary>
		/// Возвращает произведение исходного вектора на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="point">Исходный вектор.</param>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="point"/>.</returns>
		public static PointD operator *(PointD point, double x)
		{
			return new PointD(x*point.X, x*point.Y);
		}

		/// <summary>
		/// Возвращает векторное произведение двух векторов.
		/// </summary>
		/// <param name="point0">Первый вектор для векторного перемножения.</param>
		/// <param name="point1">Второй вектор для векторного перемножения.</param>
		/// <returns>Произведение <paramref name="point0"/> и <paramref name="point1"/>.</returns>
		public static Point3D operator *(PointD point0, PointD point1)
		{
			return new Point3D(0, 0, point0.X*point1.Y - point0.Y*point1.X);
		}

		/// <summary>
		/// Делит исходный вектор на число с плавающей запятой двойной точности и возвращает результат.
		/// </summary>
		/// <param name="point">Исходный вектор.</param>
		/// <param name="x">Знаменатель с плавающей запятой двойной точности.</param>
		/// <returns>Частное от деления <paramref name="point"/> на <paramref name="x"/>.</returns>
		public static PointD operator /(PointD point, double x)
		{
			return new PointD(point.X/x, point.Y/x);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух точек.
		/// </summary>
		/// <param name="point1">Первая точка для сравнения.</param>
		/// <param name="point2">Вторая точка для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="point1"/> и <paramref name="point2"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(PointD point1, PointD point2)
		{
			return !(point1 == point2);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух точек.
		/// </summary>
		/// <param name="point1">Первая точка для сравнения.</param>
		/// <param name="point2">Вторая точка для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="point1"/> и <paramref name="point2"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(PointD point1, PointD point2)
		{
			return point1.Equals(point2);
		}

		/// <summary>
		/// Определяет неявное преобразование структуры <see cref="PointD"/> в <see cref="PointF"/>.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в <see cref="PointF"/>.</param>
		/// <returns>Преобразованный объект <see cref="PointF"/>.</returns>
		public static implicit operator PointF(PointD value)
		{
			return new PointF((float) value.X, (float) value.Y);
		}

		/// <summary>
		/// Определяет неявное преобразование структуры <see cref="PointF"/> в <see cref="PointD"/>.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в <see cref="PointD"/>.</param>
		/// <returns>Преобразованный объект <see cref="PointD"/>.</returns>
		public static implicit operator PointD(PointF value)
		{
			return new PointD(value.X, value.Y);
		}

		/// <summary>
		/// Определяет неявное преобразование структуры <see cref="Point"/> в <see cref="PointD"/>.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в <see cref="PointD"/>.</param>
		/// <returns>Преобразованный объект <see cref="PointD"/>.</returns>
		public static implicit operator PointD(Point value)
		{
			return new PointD(value.X, value.Y);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Возвращает значение, показывающее, являтся ли данный вектор нулевым.
		/// </summary>
		/// <param name="x">Вектор представленный структурой <see cref="PointD"/>.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="x"/> равняется <see cref="Empty"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsEmpty(PointD x) => x == Empty;

		/// <summary>
		/// Возращает длину исходного вектора.
		/// </summary>
		public double Length => Math.Sqrt(X*X + Y*Y);

		#endregion

		#region Required Methods


		/// <summary>
		/// Возвращает вектор нормальный к исходному вектору.
		/// </summary>
		/// <returns>Вектор нормали, направленный перпендикулярно налево от исходного и имеющий единичную длину.</returns>
		public PointD GetNormal()
		{
			var len = Length;
			return new PointD(-Y/len, X/len);
		}

		/// <summary>
		/// Возвращает скалярное произведение текущего вектора на вектор заданный структурой <see cref="PointD"/>.
		/// </summary>
		/// <param name="point">Вектор, на который скалярно умножается текущий вектор.</param>
		/// <returns>Скалярное произведение текущего вектора на вектор заданный параметром <paramref name="point"/>.</returns>
		public double DotProduct(PointD point)
		{
			return X*point.X + Y*point.Y;
		}

		/// <summary>
		/// Возвращает скалярное произведение текущего вектора на вектор заданный структурой <see cref="Point3D"/>.
		/// </summary>
		/// <param name="point">Вектор, на который скалярно умножается текущий вектор.</param>
		/// <returns>Скалярное произведение текущего вектора на вектор заданный параметром <paramref name="point"/>.</returns>
		public double DotProduct(Point3D point)
		{
			return X * point.X + Y * point.Y;
		}

		/// <summary>
		/// Возвращает расстояние от текущей точки до точки заданной параметром <paramref name="point"/>.
		/// </summary>
		/// <param name="point">Точка, до которой нужно вернуть расстояние.</param>
		/// <returns>Расстояние от текущей точки до точки заданной параметром <paramref name="point"/>.</returns>
		public double Distance(PointD point)
		{
			var dx = X - point.X;
			var dy = Y - point.Y;
			return Math.Sqrt(dx*dx + dy*dy);
		}

		/// <summary>
		/// Возвращает строковое представлеине данной точки.
		/// </summary>
		/// <returns>Строковое представлеине данной точки.</returns>
		public override string ToString()
		{
			return nameof(X) + " = " + X + " " + nameof(Y) + " = " + Y;
		}

		#endregion

		#region Equality members
		
		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другая точка.</param>
		/// <returns>Значение <b>true</b>, если две точки совпадают; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(PointD other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="PointD"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is PointD && Equals((PointD) obj);
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
				return (X.GetHashCode()*397) ^ Y.GetHashCode();
			}
		}

		#endregion

	}
}