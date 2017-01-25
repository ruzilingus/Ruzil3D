using System;
using static Ruzil3D.Utility.CStatic;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет многомерный вектор над полем вещественных чисел.
	/// </summary>
	public struct Vector : ICloneable
	{
		#region Fields & Properties

		//Коээффциенты линейной комбинации
		private readonly double[] _a;

		/// <summary>
		/// Представляет нулевой вектор.
		/// </summary>
		public static readonly Vector Empty = new Vector(new double[0]);

		/// <summary>
		/// Получает или задает коэффициент первого члена вектора.
		/// </summary>
		public double X => _a[0];

		/// <summary>
		/// Получает или задает коэффициент второго члена вектора.
		/// </summary>
		public double Y => _a.Length > 1 ? _a[1] : 0;

		/// <summary>
		/// Получает или задает коэффициент третьего члена вектора.
		/// </summary>
		public double Z => _a.Length > 2 ? _a[2] : 0;

		/// <summary>
		/// Получает или задает коэффициент вектора соответствующий индексу.
		/// </summary>
		/// <param name="index">Индекс члена.</param>
		/// <returns>Коэффициент вектора соответствующий индексу заданного параметром <paramref name="index"/>.</returns>
		public double this[int index]
		{
			get { return _a.Length > index ? _a[index] : 0; }
			set
			{
				/*
				if (A.Length <= index)
				{
				    Array.Resize(ref A, index + 1);
				    //double[] aCopy = A;
				    //A = new double[index+1];
				    //aCopy.CopyTo(A, 0);
				}
				 */
				_a[index] = value;

			}
		}

		/// <summary>
		/// Возвращает количество элементов вектора.
		/// </summary>
		public int Length => _a.Length;

		/// <summary>
		/// Возвращает значение, показывающее, являтся ли данный вектор нулевым.
		/// </summary>
		/// <param name="x">Вектор.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="x"/> равняется <see cref="Empty"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsEmpty(Vector x) => x == Empty;

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Vector"/> из массива структур <see cref="double"/> представляющий коэффициенты членов.
		/// </summary>
		/// <param name="a">Массив коэффициентов членов.</param>
		public Vector(double[] a)
		{
			_a = a;
		}

		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Vector"/> из одного члена.
		/// </summary>
		/// <param name="x">Значение коэффициента соответствующий первому члену.</param>
		public Vector(double x)
		{
			_a = new[] {x};
		}

		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Vector"/> из двух членов.
		/// </summary>
		/// <param name="x">Значение коэффициента соответствующий первому члену.</param>
		/// <param name="y">Значение коэффициента соответствующий второму члену.</param>
		public Vector(double x, double y)
		{
			_a = new[] {x, y};
		}

		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Vector"/> из структуры <see cref="PointD"/>.
		/// </summary>
		/// <param name="point">Двумерный вектор координаты которого используются для создания многомерного вектора <see cref="Vector"/>.</param>
		public Vector(PointD point)
		{
			_a = new[] {point.X, point.Y};
		}

		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Vector"/> из трех членов.
		/// </summary>
		/// <param name="x">Значение коэффициента соответствующий первому члену.</param>
		/// <param name="y">Значение коэффициента соответствующий второму члену.</param>
		/// <param name="z">Значение коэффициента соответствующий третьему члену.</param>
		public Vector(double x, double y, double z)
		{
			_a = new[] {x, y, z};
		}

		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Vector"/> из структуры <see cref="Point3D"/>.
		/// </summary>
		/// <param name="point">Трехмерный вектор координаты которого используются для создания многомерного вектора <see cref="Vector"/>.</param>
		public Vector(Point3D point)
		{
			_a = new[] {point.X, point.Y, point.Z};
		}

		#endregion

		#region Overloads

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух векторов.
		/// </summary>
		/// <param name="x">Первый вектор для сравнения.</param>
		/// <param name="y">Второй вектор сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Vector x, Vector y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух векторов.
		/// </summary>
		/// <param name="x">Первый вектор для сравнения.</param>
		/// <param name="y">Второй вектор сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Vector x, Vector y)
		{
			double[] xArray, yArray;
			if (x._a.Length < y._a.Length)
			{
				xArray = x._a;
				yArray = y._a;
			}
			else
			{
				xArray = y._a;
				yArray = x._a;
			}

			for (var i = xArray.Length; i < yArray.Length; i++)
			{
				if (!yArray[i].Equals(0D))
				{
					return false;
				}
			}


			for (var i = 0; i < xArray.Length; i++)
			{
				if (!xArray[i].Equals(yArray[i]))
				{
					return false;
				}
			}

			return true;
		}
		
		/// <summary>
		/// Определяет неявное преобразование двухмерной структуры <see cref="PointD"/> в <see cref="Vector"/>.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в <see cref="Vector"/>.</param>
		/// <returns>Преобразованный объект <see cref="Vector"/>.</returns>
		public static implicit operator Vector(PointD value)
		{
			return new Vector(value);
		}

		/// <summary>
		/// Определяет неявное преобразование двухмерной структуры <see cref="Point3D"/> в <see cref="Vector"/>.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в <see cref="Vector"/>.</param>
		/// <returns>Преобразованный объект <see cref="Vector"/>.</returns>
		public static implicit operator Vector(Point3D value)
		{
			return new Vector(value);
		}

		/// <summary>
		/// Возвращает аддитивную инверсию вектора.
		/// </summary>
		/// <param name="x">Инвертируемое значение.</param>
		/// <returns>Результат умножения исходного вектора на -1.</returns>
		public static Vector operator -(Vector x)
		{
			var a = new double[x._a.Length];

			for (var i = 0; i < x._a.Length; i++)
			{
				a[i] = -x._a[i];
			}

			return new Vector(a);
		}

		/// <summary>
		/// Возвращает исходный вектор.
		/// </summary>
		/// <param name="x">Исходный вектор.</param>
		/// <returns>Исходный вектор.</returns>
		public static Vector operator +(Vector x)
		{
			return x;
		}


		/// <summary>
		/// Возвращает произведение исходного вектора на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Исходный вектор.</param>
		/// <param name="y">Множитель с плавающей запятой двойной точности.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Vector operator *(Vector x, double y)
		{
			var a = new double[x._a.Length];

			for (var i = 0; i < x._a.Length; i++)
			{
				a[i] = x._a[i]*y;
			}

			return new Vector(a);
		}

		/// <summary>
		/// Возвращает произведение исходного вектора на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <param name="y">Исходный вектор.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Vector operator *(double x, Vector y)
		{
			return y*x;
		}

		/// <summary>
		/// Делит исходный вектор на число с плавающей запятой двойной точности и возвращает результат.
		/// </summary>
		/// <param name="x">Исходный вектор.</param>
		/// <param name="y">Знаменатель с плавающей запятой двойной точности.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Vector operator /(Vector x, double y)
		{
			return x*(1/y);
		}

		/// <summary>
		/// Складывает два вектора.
		/// </summary>
		/// <param name="x">Первое из складываемых векторов.</param>
		/// <param name="y">Второе из складываемых векторов.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Vector operator +(Vector x, Vector y)
		{
			double[] xArray, yArray;
			if (x._a.Length < y._a.Length)
			{
				xArray = x._a;
				yArray = y._a;
			}
			else
			{
				xArray = y._a;
				yArray = x._a;
			}

			var result = new double[yArray.Length];

			for (var i = 0; i < xArray.Length; i++)
			{
				result[i] = xArray[i] + yArray[i];
			}

			for (var i = xArray.Length; i < yArray.Length; i++)
			{
				result[i] = yArray[i];
			}

			return new Vector(result);
		}

		/// <summary>
		/// Вычитает два вектора.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Vector operator -(Vector x, Vector y)
		{
			return x + -y;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Возвращает скалярное произведение двух векторов.
		/// </summary>
		/// <param name="x">Первый вектор для скалярное перемножения.</param>
		/// <param name="y">Второй вектор для скалярное перемножения.</param>
		/// <returns>Скалярное произведение двух векторов.</returns>
		public static double DotProduct(Vector x, Vector y)
		{
			double result = 0;

			var len = Math.Min(x.Length, y.Length);
			for (var i = 0; i < len; i++)
			{
				result += x[i]*y[i];
				//result += 1000000000000*x[i] * y[i];
			}

			return result;
			//return result/1000000000000;
		}
		
		/// <summary>
		/// Возвращает скалярное произведение текущего вектора на другой вектор.
		/// </summary>
		/// <param name="vector">Вектор, на который скалярно умножается текущий вектор.</param>
		/// <returns>Скалярное произведение текущего вектора на вектор заданный параметром <paramref name="vector"/>.</returns>
		public double DotProduct(Vector vector)
		{
			return DotProduct(this, vector);
		}

		/// <summary>
		/// Возвращает строковое представлеине данного вектора.
		/// </summary>
		/// <returns>Строковое представлеине данного вектора.</returns>
		public override string ToString()
		{
			var result = "";

			//Черточка вектора
			var vector = "x" + Macron;

			for (var i = 0; i < _a.Length; i++)
			{
				var val = vector + GetIndex(i + 1);
				AddLinearItem(ref result, _a[i], val);
			}

			return result == "" ? "Ø" : result;

		}

		/// <summary>
		/// Создает новый объект, который является копией текущего экземпляра.
		/// </summary>
		/// <returns>
		/// Новый объект, являющийся копией этого экземпляра.
		/// </returns>
		public object Clone()
		{
			return new Vector(_a.Clone() as double[]);
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другой вектор.</param>
		/// <returns>Значение <b>true</b>, если два вектора совпадают; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Vector other)
		{
			return this == other;
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Vector"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Vector && Equals((Vector)obj);
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