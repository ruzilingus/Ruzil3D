using Ruzil3D.Utility;
using static Ruzil3D.Math;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет элемент кольца кватернионов.
	/// </summary>
	public struct Quaternion
	{
		/// <summary>
		/// Получает вещественную часть кватерниона представленную числом с плавающей запятой двойной точности.
		/// </summary>
		public readonly double W;

		/// <summary>
		/// Получает мнимую часть кватерниона представленную структурой <see cref="Point3D"/>.
		/// </summary>
		public readonly Point3D U;

		/// <summary>
		/// Представляет новый экземпляр структуры <see cref="Quaternion"/> с неинициализированными данными членов.
		/// </summary>
		public static readonly Quaternion Empty = new Quaternion(0, Point3D.Empty);

		/// <summary>
		/// Представляет единичный кватернион.
		/// </summary>
		public static readonly Quaternion Identity = new Quaternion(1, Point3D.Empty);

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Quaternion"/> с указанными параметрами.
		/// </summary>
		/// <param name="w">Вещественная часть кватерниона представленную числом с плавающей запятой двойной точности.</param>
		/// <param name="u">Мнимая часть кватерниона представленную структурой <see cref="Point3D"/>.</param>
		public Quaternion(double w, Point3D u)
		{
			W = w;
			U = u;
		}

		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Quaternion"/> с указанными координатами.
		/// </summary>
		/// <param name="w">Задает компонент W кватерниона.</param>
		/// <param name="x">Задает компонент X кватерниона.</param>
		/// <param name="y">Задает компонент Y кватерниона.</param>
		/// <param name="z">Задает компонент Z кватерниона.</param>
		public Quaternion(double w, double x, double y, double z)
		{
			W = w;
			U = new Point3D(x, y, z);
		}

		#endregion

		#region Overloads

		/// <summary>
		/// Определяет неявное преобразование числа с плавающей запятой двойной точности в кватернион.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в кватернион.</param>
		/// <returns>Преобразованный объект <see cref="Quaternion"/> со значением <see cref="Point3D.Empty"/> в качестве параметра <see cref="U"/></returns>
		public static implicit operator Quaternion(double value)
		{
			return new Quaternion(value, Point3D.Empty);
		}


		/// <summary>
		/// Определяет неявное преобразование структуры <see cref="Point3D"/> в кватернион.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в кватернион.</param>
		/// <returns>Преобразованный объект <see cref="Quaternion"/> со значением 0 в качестве параметра <see cref="W"/></returns>
		public static implicit operator Quaternion(Point3D value)
		{
			return new Quaternion(0, value);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух кватернионов.
		/// </summary>
		/// <param name="x">Первый кватернион для сравнения.</param>
		/// <param name="y">Второй кватернион для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Quaternion x, Quaternion y)
		{
			return x.Equals(y);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух кватернионов.
		/// </summary>
		/// <param name="x">Первый кватернион для сравнения.</param>
		/// <param name="y">Второй кватернион для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Quaternion x, Quaternion y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Складывает два кватерниона.
		/// </summary>
		/// <param name="x">Первое из складываемых кватернионов.</param>
		/// <param name="y">Второе из складываемых кватернионов.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Quaternion operator +(Quaternion x, Quaternion y)
		{
			return new Quaternion(x.W + y.W, x.U + y.U);
		}

		/// <summary>
		/// Вычитает два кватерниона.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Quaternion operator -(Quaternion x, Quaternion y)
		{
			return new Quaternion(x.W - y.W, x.U - y.U);
		}

		/// <summary>
		/// Возвращает исходный кватернион.
		/// </summary>
		/// <param name="x">Исходный кватернион.</param>
		/// <returns>Исходный кватернион.</returns>
		public static Quaternion operator +(Quaternion x)
		{
			return x;
		}

		/// <summary>
		/// Возвращает аддитивную инверсию кватерниона.
		/// </summary>
		/// <param name="x">Инвертируемое значение.</param>
		/// <returns>Результат умножения исходного кватерниона на -1.</returns>
		public static Quaternion operator -(Quaternion x)
		{
			return new Quaternion(-x.W, -x.U);
		}

		/// <summary>
		/// Возвращает произведение двух кватернионов.
		/// </summary>
		/// <param name="x">Первый кватернион для перемножения.</param>
		/// <param name="y">Второй кватернион для перемножения.</param>
		/// <returns>Произведение двух кватернионов.</returns>
		public static Quaternion operator *(Quaternion x, Quaternion y)
		{
			return new Quaternion(x.W*y.W - x.U.DotProduct(y.U), x.W*y.U + y.W*x.U + x.U*y.U);
		}

		/// <summary>
		/// Возвращает произведение исходного кватерниона на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Исходный кватернион.</param>
		/// <param name="y">Множитель с плавающей запятой двойной точности.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Quaternion operator *(Quaternion x, double y)
		{
			return new Quaternion(x.W*y, x.U*y);
		}

		/// <summary>
		/// Возвращает произведение исходного кватерниона на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <param name="y">Исходный кватернион.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Quaternion operator *(double x, Quaternion y)
		{
			return y*x;
		}

		/// <summary>
		/// Делит исходный кватернион на число с плавающей запятой двойной точности и возвращает результат.
		/// </summary>
		/// <param name="x">Исходный кватернион.</param>
		/// <param name="y">Знаменатель с плавающей запятой двойной точности.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Quaternion operator /(Quaternion x, double y)
		{
			return new Quaternion(x.W/y, x.U/y);
		}

		/// <summary>
		/// Делит число с плавающей запятой двойной точности на исходнуый кватернион и возвращает результат.
		/// </summary>
		/// <param name="x">Числитель с плавающей запятой двойной точности.</param>
		/// <param name="y">Исходный кватернион.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Quaternion operator /(double x, Quaternion y)
		{
			var p = x/y.GetAbs2();
			return new Quaternion(p*y.W, -p*y.U);
		}


		/// <summary>
		/// Делит один кватернион на другой и возвращает результат.
		/// </summary>
		/// <param name="x">Кватернион-числитель.</param>
		/// <param name="y">Кватернион - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Quaternion operator /(Quaternion x, Quaternion y)
		{
			return x*y.GetInverse();
		}

		/// <summary>
		/// Возвращает произведение исходного кватерниона на вектор справа, преставленный структурой <see cref="Point3D"/>.
		/// </summary>
		/// <param name="x">Исходный кватернион.</param>
		/// <param name="y">Вектор для перемножения.</param>
		/// <returns>Произведение исходной матрицы на вектор <paramref name="y"/>.</returns>
		public static Quaternion operator *(Quaternion x, Point3D y)
		{
			return new Quaternion(-x.U.DotProduct(y), x.W*y + x.U*y);
		}

		/// <summary>
		/// Возвращает произведение исходного кватерниона на вектор справа, преставленный структурой <see cref="PointD"/>.
		/// </summary>
		/// <param name="x">Исходный кватернион.</param>
		/// <param name="y">Вектор для перемножения.</param>
		/// <returns>Произведение исходной матрицы на вектор <paramref name="y"/>.</returns>
		public static Quaternion operator *(Quaternion x, PointD y)
		{
			return new Quaternion(-x.U.DotProduct(y), x.W*y + x.U*y);
		}

		/// <summary>
		/// Возвращает произведение исходного кватерниона на вектор слева, преставленный структурой <see cref="Point3D"/>.
		/// </summary>
		/// <param name="x">Вектор для перемножения.</param>
		/// <param name="y">Исходный кватернион.</param>
		/// <returns>Произведение исходной матрицы на вектор <paramref name="y"/>.</returns>
		public static Quaternion operator *(Point3D x, Quaternion y)
		{
			return new Quaternion(-x.DotProduct(y.U), y.W*x + x*y.U);
		}

		/// <summary>
		/// Возвращает произведение исходного кватерниона на вектор слева, преставленный структурой <see cref="PointD"/>.
		/// </summary>
		/// <param name="x">Вектор для перемножения.</param>
		/// <param name="y">Исходный кватернион.</param>
		/// <returns>Произведение исходной матрицы на вектор <paramref name="y"/>.</returns>
		public static Quaternion operator *(PointD x, Quaternion y)
		{
			return new Quaternion(-x.DotProduct(y.U), y.W*x + x*y.U);
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Возвращает кватернион вращения на заданный угол по произвольной оси.
		/// </summary>
		/// <param name="angle">Угол вращения в радианах заданный числом с плавающей запятой двойной точности.</param>
		/// <param name="axis">Ось вращения заданная структурой <see cref="Point3D"/>.</param>
		/// <returns>Кватернион вращения на угол <paramref name="angle"/> по оси <paramref name="axis"/>.</returns>
		public static Quaternion GetRotation(double angle, Point3D axis)
		{
			axis /= axis.Length;
			var cos = Cos(angle/2D);
			var sin = Sin(angle/2D);
			return new Quaternion(cos, axis*sin);
		}

		/// <summary>
		/// Возвращает кватернион вращения на заданный угол по оси X.
		/// </summary>
		/// <param name="angle">Угол вращения по оси X в радианах, заданный числом с плавающей запятой двойной точности.</param>
		/// <returns>Кватернион вращения на угол <paramref name="angle"/> по оси X.</returns>
		public static Quaternion GetRotationX(double angle)
		{
			var cos = Cos(angle/2D);
			var sin = Sin(angle/2D);
			return new Quaternion(cos, new Point3D(sin, 0, 0));
		}

		/// <summary>
		/// Возвращает кватернион вращения на заданный угол по оси Y.
		/// </summary>
		/// <param name="angle">Угол вращения по оси Y в радианах, заданный числом с плавающей запятой двойной точности.</param>
		/// <returns>Кватернион вращения на угол <paramref name="angle"/> по оси Y.</returns>
		public static Quaternion GetRotationY(double angle)
		{
			var cos = Cos(angle/2D);
			var sin = Sin(angle/2D);
			return new Quaternion(cos, new Point3D(0, sin, 0));
		}

		/// <summary>
		/// Возвращает кватернион вращения на заданный угол по оси Z.
		/// </summary>
		/// <param name="angle">Угол вращения по оси Z в радианах, заданный числом с плавающей запятой двойной точности.</param>
		/// <returns>Кватернион вращения на угол <paramref name="angle"/> по оси Z.</returns>
		public static Quaternion GetRotationZ(double angle)
		{
			var cos = Cos(angle/2);
			var sin = Sin(angle/2);
			return new Quaternion(cos, new Point3D(0, 0, sin));
		}

		#endregion

		#region Methods

		/// <summary>
		/// Возвращает квадрат модуля кватерниона.
		/// </summary>
		/// <returns>Квадрат модуля кватерниона.</returns>
		private double GetAbs2() => W*W + U.X*U.X + U.Y*U.Y + U.Z*U.Z;

		/// <summary>
		/// Получает абсолютное значение (или величину) кватерниона.
		/// </summary>
		/// <value>Абсолютное значение (или величина) кватерниона.</value>
		public double Abs => Sqrt(GetAbs2());

		/// <summary>
		/// Получает сопряженный кватернион.
		/// </summary>
		public Quaternion Conjugate => new Quaternion(W, -U);

		/// <summary>
		/// Возвращает кватернион обратный к текущему.
		/// </summary>
		/// <returns>Кватернион обратный к текущему.</returns>
		public Quaternion GetInverse()
		{
			return Conjugate/GetAbs2();
		}

		/// <summary>
		/// Поворачивает производльный вектор ось и угол вращения которого заданы текущим кватернионом.
		/// </summary>
		/// <param name="vector">Вектор для вращения.</param>
		/// <returns>Вектор полученный поворотом вектора <paramref name="vector"/></returns>
		public Point3D Rotate(Point3D vector)
		{
			//var x = new Quaternion(-U.DotProduct(point), W * point + U * point);
			//return (-x.W*U + W*x.U  - x.U*U) / abs2;

			var v = W*vector + U*vector;
			return (U.DotProduct(vector)*U + W*v - v*U)/GetAbs2();
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
			return Round(order*value)/order;
		}

		/// <summary>
		/// Возвращает строковое представлеине данного кватерниона.
		/// </summary>
		/// <returns>Строковое представлеине данного кватерниона.</returns>
		/// <remarks>
		/// <code>
		/// var q = Quaternion.GetRotation(Math.PI, new Point3D(1, 1, 1));
		/// Console.Write(q); //Результат: √3/3 i + √3/3 j + √3/3 k
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			if (0D.Equals(W) && 0D.Equals(U.X) && 0D.Equals(U.Y) && 0D.Equals(U.Z))
			{
				return "0";
			}

			if (double.IsNaN(W) || double.IsNaN(U.X) || double.IsNaN(U.Y) || double.IsNaN(U.Z))
			{
				return "NaN";
			}

			var result = "";


			var order = Max(0, 13 - (int)GetOrder(Abs));
			CStatic.AddLinearItem(ref result, System.Math.Round(W, order), null);
			CStatic.AddLinearItem(ref result, System.Math.Round(U.X, order), "i");
			CStatic.AddLinearItem(ref result, System.Math.Round(U.Y, order), "j");
			CStatic.AddLinearItem(ref result, System.Math.Round(U.Z, order), "k");

			/*
			var order = 1E13*Abs;
			CStatic.AddLinearItem(ref result, Round15(W, order), null);
			CStatic.AddLinearItem(ref result, Round15(U.X, order), "i");
			CStatic.AddLinearItem(ref result, Round15(U.Y, order), "j");
			CStatic.AddLinearItem(ref result, Round15(U.Z, order), "k");
			*/


			return result;
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другой кватернион.</param>
		/// <returns>Значение <b>true</b>, если две точки совпадают; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Quaternion other)
		{
			return W.Equals(other.W) && U.Equals(other.U);
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Quaternion"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Quaternion && Equals((Quaternion) obj);
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
				return (W.GetHashCode()*397) ^ U.GetHashCode();
			}
		}

		#endregion
	}
}