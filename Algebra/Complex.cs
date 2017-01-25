using System;
using Ruzil3D.Utility;
using static Ruzil3D.Math;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет комплексное число.
	/// </summary>
	public struct Complex : IEquatable<Complex>, IFormattable
	{
		/// <summary>
		/// Получает вещественную часть текущего объекта <see cref="Complex"/>.
		/// </summary>
		public readonly double R;

		/// <summary>
		/// Получает мнимую часть текущего объекта <see cref="Complex"/>.
		/// </summary>
		public readonly double I;
		
		/// <summary>
		/// Представляет комплексное число равное 0.
		/// </summary>
		public static readonly Complex Empty = new Complex(0D, 0D);
		
		/// <summary>
		/// Представляет комплексное число равное 1.
		/// </summary>
		public static readonly Complex Identity = new Complex(1D, 0D);
		
		/// <summary>
		/// Представляет комплексное число равное мнимой единице.
		/// </summary>
		public static readonly Complex Imaginary = new Complex(0D, 1D);
		
		/// <summary>
		/// Инициализирует новый экземпляр структуры <see cref="Complex"/> с использованием заданных значений действительного и мнимого чисел.
		/// </summary>
		/// <param name="r">Действительная часть комплексного числа.</param>
		/// <param name="i">Мнимая часть комплексного числа.</param>
		public Complex(double r, double i)
		{
			R = r;
			I = i;
		}

		#region Overloads

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух комплексных числа.
		/// </summary>
		/// <param name="x">Первое комплексное число для сравнения.</param>
		/// <param name="y">Второе комплексное число для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Complex x, Complex y)
		{
			return x.R.Equals(y.R) && x.I.Equals(y.I);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух комплексных числа.
		/// </summary>
		/// <param name="x">Первое комплексное число для сравнения.</param>
		/// <param name="y">Второе комплексное число для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Complex x, Complex y)
		{
			return !(x == y);
		}
		
		/// <summary>
		/// Возвращает исходное комплексное число.
		/// </summary>
		/// <param name="x">Исходное комплексное число.</param>
		/// <returns>Исходное комплексное число.</returns>
		public static Complex operator +(Complex x)
		{
			return x;
		}

		/// <summary>
		/// Возвращает аддитивную инверсию комплексного числа заданного параметром <paramref name="x"/>.
		/// </summary>
		/// <param name="x">Инвертируемое значение.</param>
		/// <returns>Результат умножения частей <see cref="R"/> и <see cref="I"/> параметра <paramref name="x"/> на -1.</returns>
		public static Complex operator -(Complex x)
		{
			return new Complex(-x.R, -x.I);
		}

		/// <summary>
		/// Складывает два комплексных числа.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator +(Complex x, Complex y)
		{
			return new Complex(x.R + y.R, x.I + y.I);
		}

		/// <summary>
		/// Вычитает комплексное число из другого комплексного числа.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator -(Complex x, Complex y)
		{
			return new Complex(x.R - y.R, x.I - y.I);
		}
		
		/// <summary>
		/// Возвращает произведение двух комплексных чисел.
		/// </summary>
		/// <param name="x">Первое комплексное число для перемножения.</param>
		/// <param name="y">Второе комплексное число для перемножения.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator *(Complex x, Complex y)
		{
			return new Complex(x.R*y.R - x.I*y.I, x.R*y.I + x.I*y.R);
		}

		/// <summary>
		/// Делит одно комплексное число на другое и возвращает результат.
		/// </summary>
		/// <param name="x">Комплексное число-числитель.</param>
		/// <param name="y">Комплексное число - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Complex operator /(Complex x, Complex y)
		{
			var divider = y.Abs2;
			return new Complex((x.R*y.R + x.I*y.I)/divider, (x.I*y.R - x.R*y.I)/divider);
		}

		/// <summary>
		/// Возвращает заданное комплексное число, возведенное в степень, заданную числом с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Комплексное число для возведения в степень.</param>
		/// <param name="y">Число двойной точности с плавающей запятой, задающее степень.</param>
		/// <returns>Комплексное число <paramref name="x"/>, возведенное в степень <paramref name="y"/>.</returns>
		/// <remarks>Метод устарел. Используйте метод <see cref="Pow"/> вместо него.</remarks>
		[Obsolete("Метод устарел. Используйте метод " + nameof(Pow) + " вместо него.")]
		public static Complex operator ^(Complex x, double y)
		{
			return x.Pow(y);
		}


		/// <summary>
		/// Определяет неявное преобразование числа с плавающей запятой двойной точности в комплексное число.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в комплексное число.</param>
		/// <returns>Объект, содержащий значение параметра <paramref name="value"/> как действительную часть и ноль как мнимую часть.</returns>
		public static implicit operator Complex(double value)
		{
			return new Complex(value, 0D);
		}
		
		#region Speed-up Необязательные. Для ускорения работы

		/// <summary>
		/// Возвращает произведение комплексныого числа на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель.</param>
		/// <param name="y">Комплексное число.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator *(double x, Complex y)
		{
			return new Complex(x * y.R, x * y.I);
		}

		/// <summary>
		/// Возвращает произведение комплексныого числа на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Комплексное число.</param>
		/// <param name="y">Множитель.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator *(Complex x, double y)
		{
			return y * x;
		}

		/// <summary>
		/// Делит число с плавающей запятой двойной точности на комплексное число и возвращает результат.
		/// </summary>
		/// <param name="x">Числитель с плавающей запятой двойной точности.</param>
		/// <param name="y">Комплексное число - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Complex operator /(double x, Complex y)
		{
			x /= y.Abs2;
			return new Complex(x * y.R, - x * y.I);
		}

		/// <summary>
		/// Делит комплексное число на число с плавающей запятой двойной точности и возвращает результат.
		/// </summary>
		/// <param name="x">Комплексное число-числитель.</param>
		/// <param name="y">Знаменатель с плавающей запятой двойной точности.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Complex operator /(Complex x, double y)
		{
			return new Complex(x.R/y, x.I/y);
		}

		/// <summary>
		/// Складывает число с плавающей запятой двойной точности и комплексное число.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator +(double x, Complex y)
		{
			return new Complex(x + y.R, y.I);
		}

		/// <summary>
		/// Складывает комплексное число и число с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator +(Complex x, double y)
		{
			return y + x;
		}
		
		/// <summary>
		/// Вычитает комплексное число из числа с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator -(double x, Complex y)
		{
			return new Complex(x - y.R, -y.I);
		}

		/// <summary>
		/// Вычитает число с плавающей запятой двойной точности из комплексного числа.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Complex operator -(Complex x, double y)
		{
			return new Complex(x.R - y, x.I);
		}

		#endregion

		#endregion

		/// <summary>
		/// Возвращает все корни комплексного числа целой степени <paramref name="deg"/>.
		/// </summary>
		/// <param name="deg"></param>
		/// <returns>Массив корней комплексного числа целой степени <paramref name="deg"/>.</returns>
		public Complex[] GetRoots(int deg)
		{
			if (deg < 1)
			{
				throw new ArgumentException("Аргумент \"" + nameof(deg) + "\" должен быть больше 0", nameof(deg));
			}

			double r;

			if (I.Equals(0D) && R > 0)
			{
				r = System.Math.Pow(R, 1D/deg);

				switch (deg)
				{
					case 1:
						return new[]
						{
							new Complex(r, 0)
						};
					case 2:
						return new[]
						{
							new Complex(r, 0),
							new Complex(-r, 0)
						};
					case 3:
						var a = -r/2D;
						var b = Sqrt3*a;
						return new[]
						{
							new Complex(r, 0),
							new Complex(a, -b),
							new Complex(a, b)
						};
					case 4:
						return new[]
						{
							new Complex(r, 0),
							new Complex(0, r),
							new Complex(-r, 0),
							new Complex(0, -r)
						};
				}
			}
			else
			{
				r = System.Math.Pow(Abs2, 0.5D / deg);
			}
			
			var result = new Complex[deg];

			var angle = Angle;

			for (var i = 0; i < deg; i++)
			{
				var fi = (angle + Tau * i) / deg;
				var cos = Cos(fi);
				var sin = Sin(fi);
				//var sin = System.Math.Sqrt(1 - cos*cos);

				result[i] = new Complex(r * cos, r * sin);
			}

			return result;
		}

		private double Abs2 => R*R + I*I;

		/// <summary>
		/// Получает абсолютное значение (или величину) комплексного числа.
		/// </summary>
		/// <value>Абсолютное значение (или величина) комплексного числа.</value>
		public double Abs => System.Math.Sqrt(Abs2);

		/// <summary>
		/// Получает аргумент комплексного числа.
		/// </summary>
		/// <value>Аргумент комплексного числа.</value>
		public double Angle => System.Math.Atan2(I, R);
		
		/// <summary>
		/// Возвращает исходное комплексное число, возведенное в степень, заданную числом с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="y">Число двойной точности с плавающей запятой, задающее степень.</param>
		/// <returns>Комплексное число, возведенное в степень <paramref name="y"/>.</returns>
		public Complex Pow(double y)
		{
			var r = System.Math.Pow(Abs2, y/2D);
			var fi = Angle * y;

			//Первый корень
			return new Complex(r * Cos(fi), r * Sin(fi));
		}
		
		/// <summary>
		/// Возвращает значение, показывающее, равняется ли данное комплексное число 0.
		/// </summary>
		/// <param name="x">Комплексное число.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="x"/> равняется <see cref="Empty"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsEmpty(Complex x) => x == Empty;

		/// <summary>
		/// Возвращает значение, показывающее, равняется ли данное комплексное число 1.
		/// </summary>
		/// <param name="x">Комплексное число.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="x"/> равняется <see cref="Identity"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsIdentity(Complex x) => x == Identity;

		/// <summary>
		/// Возвращает значение, показывающее, является ли данное комплексное число нечисловым значением.
		/// </summary>
		/// <param name="x">Комплексное число.</param>
		/// <returns>Значение <b>true</b>, если поле <see cref="R"/> или поле <see cref="I"/> параметра <paramref name="x"/> равны значению <see cref="double.NaN"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsNaN(Complex x) => double.IsNaN(x.R) || double.IsNaN(x.I);

		/// <summary>
		/// Возвращает значение, показывающее, является ли данное комплексное число вещественным.
		/// </summary>
		/// <param name="x">Комплексное число.</param>
		/// <returns>Значение <b>true</b>, если поле <see cref="I"/> параметра <paramref name="x"/> равняется <b>0</b>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsReal (Complex x) => x.I.Equals(0D);

		/// <summary>
		/// Возвращает строковое представлеине данного комплексного числа.
		/// </summary>
		/// <returns>Строковое представление данного комплексного числа.</returns>
		/// <remarks>
		/// <code>
		/// var num = new Complex(2, -1);
		/// Console.Write(num); //Результат: 2 - i
		/// Console.Write(-2*num); //Результат: -4 + 2i
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			if (0D.Equals(R) && 0D.Equals(I))
			{
				return "0";
			}

			if (double.IsNaN(R) || double.IsNaN(I))
			{
				return "NaN";
			}

			var result = "";
			CStatic.AddLinearItem(ref result, R, null);
			CStatic.AddLinearItem(ref result, I, "i");
			return result;
		}
		
		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другое комплексное число.</param>
		/// <returns>Значение <b>true</b>, если два числа равны; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Complex other)
		{
			return R.Equals(other.R) && I.Equals(other.I);
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Complex"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Complex && Equals((Complex) obj);
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
				return (R.GetHashCode()*397) ^ I.GetHashCode();
			}
		}

		#endregion

		/// <summary>
		/// Форматирует значение текущего экземпляра с использованием заданного формата.
		/// </summary>
		/// <param name="format">Объект <see cref="T:System.String"/>, задающий используемый формат.</param>
		/// <param name="formatProvider">Объект <see cref="T:System.IFormatProvider"/>, используемый для форматирования значения.</param><filterpriority>2</filterpriority>
		/// <returns>
		/// Объект <see cref="T:System.String"/> содержит значение текущего экземпляра в заданном формате.
		/// </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return ToString();
		}
	}
}