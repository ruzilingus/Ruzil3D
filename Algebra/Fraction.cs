using System;
using System.Globalization;
using Ruzil3D.Utility;
using static Ruzil3D.Math;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет дробное число.
	/// </summary>
	/// <remarks>
	/// Значение дроби равно <see cref="Numerator"/>/<see cref="Denominator"/>·10^<see cref="Order"/>.
	/// Арифметические операции и сравнения выполняются точно, если результат представим в таком виде.
	/// Если точный результат не помещается в 64-разрядные числитель и знаменатель, возвращается ближайшее
	/// приближение, а бесконечности и <see cref="NaN"/> ведут себя так же, как у чисел двойной точности.
	/// </remarks>
	public struct Fraction : IComparable, IFormattable, IConvertible, IComparable<Fraction>, IEquatable<Fraction>
	{
		#region Properties

		/// <summary>
		/// Получает числитель дроби представленный 64-разрядным целым числом со знаком.
		/// </summary>
		public long Numerator { get; private set; }

		/// <summary>
		/// Получает знаменатель дроби представленный 64-разрядным целым числом со знаком.
		/// </summary>
		public long Denominator { get; private set; }

		/// <summary>
		/// Получает порядок дроби представленный 32-разрядным целым числом со знаком.
		/// </summary>
		public int Order { get; private set; }

		/// <summary>
		/// Получает характеристику дроби представленное числом двойной точности с плавающей запятой.
		/// </summary>
		public double Exponent => Exp10(Order);

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Fraction"/> из числителя, знаменателя.
		/// </summary>
		/// <param name="numerator">Числитель дроби.</param>
		/// <param name="denominator">Знаменатель дроби.</param>
		/// <param name="simplify">Параметр указывающий на необходимость упрощадь дробь.</param>
		public Fraction(long numerator, long denominator, bool simplify = false)
		{
			this = Create(numerator, denominator, 0, simplify);
		}

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Fraction"/> из числителя, знаменателя и порядка.
		/// </summary>
		/// <param name="numerator">Числитель дроби.</param>
		/// <param name="denominator">Знаменатель дроби.</param>
		/// <param name="order">Порядок дроби.</param>
		/// <param name="simplify">Параметр указывающий на необходимость упрощадь дробь.</param>
		public Fraction(long numerator, long denominator, int order, bool simplify = false)
		{
			this = Create(numerator, denominator, order, simplify);
		}

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Fraction"/> из числителя.
		/// </summary>
		/// <param name="value">Числитель дроби.</param>
		public Fraction(long value)
		{
			Numerator = value;
			Denominator = 1;
			Order = 0;
		}

		private static Fraction Create(long numerator, long denominator, int order, bool simplify)
		{
			if (denominator == 0L)
			{
				return numerator == 0L ? NaN : (numerator > 0L ? PositiveInfinity : NegativeInfinity);
			}

			if (numerator == 0L)
			{
				return Empty;
			}

			//Знак переносится в числитель через модули: прежде смена знака long.MinValue переполнялась,
			//и new Fraction(1, long.MinValue) получала положительное значение.
			var negative = (numerator < 0L) != (denominator < 0L);
			var num = Magnitude(numerator);
			var den = Magnitude(denominator);
			var fits = den <= long.MaxValue && num <= (negative ? SignBit : long.MaxValue);

			if (simplify)
			{
				Fraction result;
				if (TryExact(negative, new U128(0UL, num), new U128(0UL, den), order, out result) == CanonicalOk)
				{
					return result;
				}
			}

			if (fits)
			{
				return Raw(negative ? unchecked((long) (0UL - num)) : (long) num, (long) den, order);
			}

			//Модуль знаменателя (или положительного числителя) равен 2^63 и не помещается в long.
			return FromParts(negative, new U128(0UL, num), new U128(0UL, den), order);
		}

		private static Fraction Raw(long numerator, long denominator, int order)
		{
			return new Fraction {Numerator = numerator, Denominator = denominator, Order = order};
		}

		#endregion

		#region Statics

		/// <summary>
		/// Представляет значение не являющееся дробью.
		/// </summary>
		public static readonly Fraction NaN = new Fraction {Numerator = 0L, Denominator = 0L, Order = 0};

		/// <summary>
		/// Представляет плюс бесконечность.
		/// </summary>
		public static readonly Fraction PositiveInfinity = new Fraction {Numerator = 1L, Denominator = 0L, Order = 0};

		/// <summary>
		/// Представляет минус бесконечность.
		/// </summary>
		public static readonly Fraction NegativeInfinity = new Fraction {Numerator = -1, Denominator = 0, Order = 0};

		/// <summary>
		/// Представляет дробь равное 0.
		/// </summary>
		public static readonly Fraction Empty = new Fraction {Numerator = 0L, Denominator = 1L, Order = 0};

		/// <summary>
		/// Возвращает значение, показывающее, является ли данная дробь нечисловым значением <see cref="NaN"/>.
		/// </summary>
		/// <param name="f">Дробное число.</param>
		/// <returns>Значение <b>true</b>, если значение параметра <paramref name="f"/> равно значению <see cref="NaN"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsNaN(Fraction f)
		{
			return f.Numerator == 0L && f.Denominator == 0L;
		}

		/// <summary>
		///  Возвращает значение, показывающее, равна ли данная дробь нулю.
		/// </summary>
		/// <param name="f">Дробное число.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="f"/> равняется <see cref="Empty"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsEmpty(Fraction f)
		{
			return f.Numerator == 0L && f.Denominator != 0L;
		}

		/// <summary>
		/// Возвращает значение, показывающее, является ли данная дробь плюс или минус бесконечностью.
		/// </summary>
		/// <param name="f">Дробное число.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="f"/> равен значению <see cref="PositiveInfinity"/> или <see cref="NegativeInfinity"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsInfinity(Fraction f)
		{
			return f.Numerator != 0L && f.Denominator == 0L;
		}

		/// <summary>
		/// Возвращает значение, показывающее, равна ли данная дробь плюс бесконечности.
		/// </summary>
		/// <param name="f">Дробное число.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="f"/> равен значению <see cref="PositiveInfinity"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsPositiveInfinity(Fraction f)
		{
			return f.Numerator > 0L && f.Denominator == 0L;
		}

		/// <summary>
		/// Возвращает значение, показывающее, равна ли данная дробь минус бесконечности.
		/// </summary>
		/// <param name="f">Дробное число.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="f"/> равен значению <see cref="NegativeInfinity"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsNegativeInfinity(Fraction f)
		{
			return f.Numerator < 0L && f.Denominator == 0L;
		}

		#endregion

		#region Overloads

		/// <summary>
		/// Определяет неявное преобразование 8-битового целого числа со знаком в дробное число.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в дробное число.</param>
		/// <returns>Неявное преобразование 8-битового целого числа со знаком в дробное число.</returns>
		public static implicit operator Fraction(sbyte value)
		{
			return new Fraction(value);
		}

		/// <summary>
		/// Определяет неявное преобразование 32-битового целого числа со знаком в дробное число.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в дробное число.</param>
		/// <returns>Неявное преобразование 32-битового целого числа со знаком в дробное число.</returns>
		public static implicit operator Fraction(int value)
		{
			return new Fraction(value);
		}

		/// <summary>
		/// Определяет неявное преобразование 64-битового целого числа со знаком в дробное число.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в дробное число.</param>
		/// <returns>Неявное преобразование 64-битового целого числа со знаком в дробное число.</returns>
		public static implicit operator Fraction(long value)
		{
			return new Fraction(value);
		}

		/// <summary>
		/// Определяет неявное преобразование числа с плавающей запятой одиночной точности в дробное число.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в дробное число.</param>
		/// <returns>Неявное преобразование числа с плавающей запятой одиночной точности в дробное число.</returns>
		public static implicit operator Fraction(float value)
		{
			return Parse(value);
		}

		/// <summary>
		/// Определяет неявное преобразование числа с плавающей запятой двойной точности в дробное число.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в дробное число.</param>
		/// <returns>Неявное преобразование числа с плавающей запятой двойной точности в дробное число.</returns>
		/// <remarks>Преобразование выполняется методом <see cref="Parse"/> с 15 значащими цифрами.</remarks>
		public static implicit operator Fraction(double value)
		{
			return Parse(value);
		}

		/// <summary>
		/// Определяет неявное преобразование дробного числа в число с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в число с плавающей запятой двойной точности.</param>
		/// <returns>Неявное преобразование дробного числа в число с плавающей запятой двойной точности.</returns>
		public static implicit operator double(Fraction value)
		{
			//return value.Numerator*value.Exponent/value.Denominator;

			var result = value.Order >= 0
				? value.Numerator*value.Exponent/value.Denominator
				: value.Numerator/Exp10(-value.Order)/value.Denominator;

			if ((double.IsInfinity(result) || result.Equals(0D)) && value.Numerator != 0L && value.Denominator != 0L)
			{
				//Промежуточное произведение переполнилось или обнулилось, хотя само значение может быть конечным
				//(например, 5/10·10³⁰⁸): масштабируем мантиссу по частям.
				return ScaleByPowerOfTen((double) value.Numerator/value.Denominator, value.Order);
			}

			return result;
		}

		/// <summary>
		/// Складывает два дробных числа.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator +(Fraction x, Fraction y)
		{
			//Прежде порядок переносился в числитель и знаменатель через double с усечением
			//((Fraction)1e-5 + (Fraction)1e-5 = 2/99999), а перекрёстные произведения молча переполнялись.
			bool exact;
			return Add(x, y, false, out exact);
		}

		/// <summary>
		/// Складывает дробное число с 64-разрядным целым числом со знаком.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator +(Fraction x, long y)
		{
			bool exact;
			return Add(x, new Fraction(y), false, out exact);
		}

		/// <summary>
		/// Складывает 64-разрядное целое число со знаком с дробным числом.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator +(long x, Fraction y)
		{
			bool exact;
			return Add(new Fraction(x), y, false, out exact);
		}

		/// <summary>
		/// Вычитает дробное число из другого дробного числа.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator -(Fraction x, Fraction y)
		{
			bool exact;
			return Add(x, y, true, out exact);
		}

		/// <summary>
		/// Вычитает 64-разрядное целое число со знаком из дробного числа.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator -(Fraction x, long y)
		{
			bool exact;
			return Add(x, new Fraction(y), true, out exact);
		}

		/// <summary>
		/// Вычитает дробное число из 64-разрядного целого числа со знаком.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator -(long x, Fraction y)
		{
			bool exact;
			return Add(new Fraction(x), y, true, out exact);
		}

		/// <summary>
		/// Возвращает исходное дробное число.
		/// </summary>
		/// <param name="x">Исходное дробное число.</param>
		/// <returns>Исходное дробное число.</returns>
		public static Fraction operator +(Fraction x)
		{
			return x;
		}

		/// <summary>
		/// Возвращает аддитивную инверсию дробного числа заданного параметром <paramref name="x"/>.
		/// </summary>
		/// <param name="x">Инвертируемое значение.</param>
		/// <returns>Результат умножения дробного числа на -1.</returns>
		public static Fraction operator -(Fraction x)
		{
			if (x.Numerator == long.MinValue)
			{
				//-long.MinValue не помещается в long: строим равную дробь другого вида.
				return FromParts(false, new U128(0UL, SignBit), new U128(0UL, (ulong) x.Denominator), x.Order);
			}

			return new Fraction(-x.Numerator, x.Denominator, x.Order);
		}

		/// <summary>
		/// Возвращает произведение двух дробных чисел.
		/// </summary>
		/// <param name="x">Первое дробное число для перемножения.</param>
		/// <param name="y">Второе дробное число для перемножения.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator *(Fraction x, Fraction y)
		{
			//Прежде произведения числителей и знаменателей молча переполнялись:
			//(Fraction)Math.PI * (Fraction)Math.E давало -1.94e-07.
			bool exact;
			return Multiply(x, y, false, out exact);
		}


		/// <summary>
		/// Возвращает произведение дробного числа на число с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Дробное число для перемножения.</param>
		/// <param name="y">Число с плавающей запятой двойной точности для перемножения.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static double operator *(Fraction x, double y)
		{
			return (double) x*y;
		}

		/// <summary>
		/// Возвращает произведение числа с плавающей запятой двойной точности на дробное число.
		/// </summary>
		/// <param name="x">Число с плавающей запятой двойной точности для перемножения.</param>
		/// <param name="y">Дробное число для перемножения.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static double operator *(double x, Fraction y)
		{
			return x*(double) y;
		}

		/// <summary>
		/// Возвращает произведение дробного числа на 64-разрядное целое число со знаком.
		/// </summary>
		/// <param name="x">Дробное число для перемножения.</param>
		/// <param name="y">64-разрядное целое число со знаком для перемножения.</param>
		/// <returns>Произведение дробного числа на 64-разрядное целое число со знаком.</returns>
		public static Fraction operator *(Fraction x, long y)
		{
			bool exact;
			return Multiply(x, new Fraction(y), false, out exact);
		}

		/// <summary>
		/// Возвращает произведение 64-разрядного целого числа со знаком на дробное число.
		/// </summary>
		/// <param name="x">64-разрядное целое число со знаком для перемножения.</param>
		/// <param name="y">Дробное число для перемножения.</param>
		/// <returns>Произведение 64-разрядного целого числа со знаком на дробное число.</returns>
		public static Fraction operator *(long x, Fraction y)
		{
			bool exact;
			return Multiply(new Fraction(x), y, false, out exact);
		}

		/// <summary>
		/// Делит одно дробное число на другое и возвращает результат.
		/// </summary>
		/// <param name="x">Дробное число - числитель.</param>
		/// <param name="y">Дробное число - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator /(Fraction x, Fraction y)
		{
			bool exact;
			return Multiply(x, y, true, out exact);
		}

		/// <summary>
		/// Делит дробное число на 64-разрядное целое число со знаком.
		/// </summary>
		/// <param name="x">Дробное число-числитель.</param>
		/// <param name="y">64-разрядное целое число со знаком - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator /(Fraction x, long y)
		{
			bool exact;
			return Multiply(x, new Fraction(y), true, out exact);
		}

		/// <summary>
		/// Делит 64-разрядное целое число со знаком на дробное число.
		/// </summary>
		/// <param name="x">64-разрядное целое число со знаком - числитель.</param>
		/// <param name="y">Дробное число - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator /(long x, Fraction y)
		{
			//Прежде вычислялось x·N/D вместо x·D/N, и 1 / new Fraction(1, 2) давало 1/2.
			bool exact;
			return Multiply(new Fraction(x), y, true, out exact);
		}

		/// <summary>
		/// Делит дробное число на число с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Дробное число - числитель.</param>
		/// <param name="y">Число с плавающей запятой двойной точности - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static double operator /(Fraction x, double y)
		{
			return (double) x/y;
		}

		/// <summary>
		/// Делит число с плавающей запятой двойной точности на дробное число.
		/// </summary>
		/// <param name="x">Число с плавающей запятой двойной точности - числитель.</param>
		/// <param name="y">Дробное число - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static double operator /(double x, Fraction y)
		{
			return x/(double) y;
		}

		/// <summary>
		/// Вычисляет остаток после деления первого дробного числа на второй.
		/// </summary>
		/// <param name="x">Дробное число - числитель.</param>
		/// <param name="y">Дробное число - знаменатель.</param>
		/// <returns>Остаток от деления <paramref name="x"/> на <paramref name="y"/>. Частное округляется к нулю, поэтому знак остатка совпадает со знаком <paramref name="x"/>. Если <paramref name="y"/> равно нулю или <paramref name="x"/> бесконечно, возвращается <see cref="NaN"/>.</returns>
		public static Fraction operator %(Fraction x, Fraction y)
		{
			//Прежде частное вычислялось в double: (Fraction)0.3 % (Fraction)0.1 давало 1/10, а 5 % 0 — 5.
			bool exact;
			return Remainder(x, y, out exact);
		}

		/// <summary>
		/// Вычисляет остаток после деления дробного числа на 64-разрядное целое число со знаком.
		/// </summary>
		/// <param name="x">Дробное число - числитель.</param>
		/// <param name="y">64-разрядное целое число со знаком - знаменатель.</param>
		/// <returns>Остаток от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator %(Fraction x, long y)
		{
			bool exact;
			return Remainder(x, new Fraction(y), out exact);
		}

		/// <summary>
		/// Вычисляет остаток после деления 64-разрядного целого числа со знаком на дробное число.
		/// </summary>
		/// <param name="x">64-разрядное целое число со знаком - числитель.</param>
		/// <param name="y">Дробное число - знаменатель.</param>
		/// <returns>Остаток от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator %(long x, Fraction y)
		{
			bool exact;
			return Remainder(new Fraction(x), y, out exact);
		}

		/// <summary>
		/// Возвращает значение указывающее что первое дробное число больше или равно второму.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд больше или равен второму, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator >=(Fraction x, Fraction y)
		{
			//Сравнение точное: прежде дроби сравнивались через double с двойным округлением.
			return !IsNaN(x) && !IsNaN(y) && Compare(x, y) >= 0;
		}

		/// <summary>
		/// Возвращает значение указывающее что первое дробное число меньше или равно второму.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд меньше или равен второму, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator <=(Fraction x, Fraction y)
		{
			return !IsNaN(x) && !IsNaN(y) && Compare(x, y) <= 0;
		}

		/// <summary>
		/// Возвращает значение указывающее что первое дробное число больше второго.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд больше второго, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator >(Fraction x, Fraction y)
		{
			return !IsNaN(x) && !IsNaN(y) && Compare(x, y) > 0;
		}

		/// <summary>
		/// Возвращает значение указывающее что первое дробное число меньше второго.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд меньше второго, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator <(Fraction x, Fraction y)
		{
			return !IsNaN(x) && !IsNaN(y) && Compare(x, y) < 0;
		}

		/// <summary>
		/// Возвращает значение указывающее на равенство дробных чисел.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд равен второму, в противном случае возвращается значение <b>false</b>. Как и для чисел двойной точности, <see cref="NaN"/> не равно никакому значению, в том числе самому себе.</returns>
		public static bool operator ==(Fraction x, Fraction y)
		{
			//Прежде дроби сравнивались через double: 2^53 + 1 == 2^53, а new Fraction(1, 300000) не равнялась
			//new Fraction(1, 3) / 100000L из-за двойного округления.
			return !IsNaN(x) && !IsNaN(y) && Compare(x, y) == 0;
		}

		/// <summary>
		/// Возвращает значение указывающее на неравенство дробных чисел.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд не равен второму, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator !=(Fraction x, Fraction y)
		{
			return !(x == y);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Превращает вещественное число в дробь.
		/// </summary>
		/// <param name="value">Исходное число двойной точности с плавающей запятой.</param>
		/// <param name="order">Число значащих десятичных цифр, которые учитываются при поиске периода и округлении (от 1 до 18).</param>
		/// <returns>
		/// Дробное число численно близкое к параметру <paramref name="value"/>: периодическая десятичная дробь, если её период
		/// повторяется в первых <paramref name="order"/> значащих цифрах, иначе число, округлённое до <paramref name="order"/>
		/// значащих цифр. Результат никогда не бывает дальше от <paramref name="value"/>, чем <paramref name="value"/>,
		/// усечённое до <paramref name="order"/> значащих цифр.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">Значение параметра <paramref name="order"/> меньше 1 или больше 18.</exception>
		public static Fraction Parse(double value, int order = 15)
		{
			//Прежде order не проверялся: 0, отрицательные значения и значения больше 18 приводили
			//к IndexOutOfRangeException, OverflowException или ArgumentException.
			if (order < 1 || order > MaxDigits)
			{
				throw new ArgumentOutOfRangeException(nameof(order), order, "Число значащих цифр должно быть от 1 до " + MaxDigits + ".");
			}

			if (double.IsNegativeInfinity(value))
			{
				return NegativeInfinity;
			}

			if (double.IsPositiveInfinity(value))
			{
				return PositiveInfinity;
			}

			if (double.IsNaN(value))
			{
				return NaN;
			}

			if (value.Equals(0D))
			{
				return Empty;
			}

			var negative = value < 0D;
			if (negative)
			{
				value = -value;
			}

			ulong mantissa;
			int exponent;
			Decompose(value, out mantissa, out exponent);

			//Значащие цифры digits = floor(value·10^(order - orderValue)) и остаток rest/scale вычисляются точно.
			//Прежде произведение считалось в double: для малых чисел множитель 10^n переполнялся,
			//и (Fraction)1.5e-300 выбрасывал исключение.
			var orderValue = (int) Floor(GetOrder(value)) + 1;
			var digits = 0UL;
			Big rest = null, scale = null;
			var found = false;
			for (var attempt = 0; attempt < 4 && !found; attempt++)
			{
				//Десятичный логарифм может ошибиться на единицу рядом со степенями десяти.
				if (!ScaleFloor(mantissa, exponent, order - orderValue, out digits, out rest, out scale) || digits >= Pow10[order])
				{
					orderValue++;
				}
				else if (digits < Pow10[order - 1])
				{
					orderValue--;
				}
				else
				{
					found = true;
				}
			}

			if (found)
			{
				var period = FindPeriod(negative, digits, order, orderValue, rest, scale);
				if (!IsNaN(period))
				{
					return period;
				}
			}

			//Период не найден: округляем до order значащих цифр.
			var twice = rest.Clone();
			twice.ShiftLeft(1);
			if (Big.Compare(twice, scale) >= 0)
			{
				//Округление вверх у чисел около double.MaxValue может выйти за пределы double: тогда оставляем усечение.
				var roundedUp = FromParts(negative, new U128(0UL, digits + 1UL), new U128(0UL, 1UL), (long) orderValue - order);
				if (!double.IsInfinity(roundedUp))
				{
					return roundedUp;
				}
			}

			return FromParts(negative, new U128(0UL, digits), new U128(0UL, 1UL), (long) orderValue - order);
		}

		/// <summary>
		/// Ищет период в последних цифрах числа и возвращает соответствующую периодическую дробь или <see cref="NaN"/>.
		/// </summary>
		/// <param name="negative">Знак числа.</param>
		/// <param name="digits">Первые <paramref name="order"/> значащих цифр числа.</param>
		/// <param name="order">Число значащих цифр.</param>
		/// <param name="orderValue">Число цифр целой части: число равно 0.digits·10^orderValue.</param>
		/// <param name="rest">Отброшенный остаток: точное значение равно digits + rest/scale (в единицах последней цифры).</param>
		/// <param name="scale">Знаменатель остатка.</param>
		private static Fraction FindPeriod(bool negative, ulong digits, int order, int orderValue, Big rest, Big scale)
		{
			var digitArray = new int[order];
			var tmp = digits;
			for (var i = order - 1; i >= 0; i--)
			{
				digitArray[i] = (int) (tmp%10UL);
				tmp /= 10UL;
			}

			var starts = new int[order];
			var lengths = new int[order];
			var offsets = new int[order];
			var count = 0;

			var lastIdx = order - 1;
			var last = digitArray[lastIdx];
			for (var i = 0; i < lastIdx; i++)
			{
				if (digitArray[i] != last)
				{
					continue;
				}

				var offset = 1;
				while (offset <= i && digitArray[i - offset] == digitArray[lastIdx - offset])
				{
					offset++;
				}

				//Цифры с позиции start до конца периодичны с периодом length, и период подтверждён offset цифрами.
				//Период засчитывается, только если он подтверждён не менее чем двумя цифрами и либо целиком
				//повторился хотя бы дважды, либо подтверждён MinPeriodMatch цифрами (1/81 = 0.(012345679)).
				//Кроме того, ниже период проверяется на близость к исходному числу.
				//Прежде условие выбора всегда было истинно, и побеждал самый короткий «период» даже по одной
				//совпавшей цифре: (Fraction)0.31234567890123 = 5205240738889/16665·10⁻⁹.
				var length = lastIdx - i;
				if (offset < 2 || (offset < length && offset < MinPeriodMatch))
				{
					continue;
				}

				//Вставка с сортировкой: сначала периоды, целиком повторившиеся хотя бы дважды, затем подтверждённые
				//только MinPeriodMatch цифрами; среди равных — раньше начало периода, затем короче период. Без первого
				//условия длинный частично подтверждённый «период», начавшийся раньше, побеждал настоящий:
				//(Fraction)0.999999 давало 99999899999/99999999999 вместо 999999·10⁻⁶.
				var start = i - offset + 1;
				var k = count++;
				while (k > 0 && IsAfter(offsets[k - 1] < lengths[k - 1], starts[k - 1], lengths[k - 1], offset < length, start, length))
				{
					starts[k] = starts[k - 1];
					lengths[k] = lengths[k - 1];
					offsets[k] = offsets[k - 1];
					k--;
				}

				starts[k] = start;
				lengths[k] = length;
				offsets[k] = offset;
			}

			for (var k = 0; k < count; k++)
			{
				var length = lengths[k];

				//Продолжение периода добавляет к digits хвост L/W, где L — последние length цифр, W = 10^length - 1.
				//Период принимается, только если он не дальше от числа, чем округление до order цифр:
				//|L/W - rest/scale| ≤ min(rest/scale, 1 - rest/scale). Период, повторившийся целиком не меньше
				//StrongPeriodRepeats раз после первого вхождения, принимается и без этой проверки (как и прежде): значение,
				//отличающееся на несколько ulp от 9999999 или 9999.999, по-прежнему превращается в это число.
				var tail = digits%Pow10[length];
				var w = Pow10[length] - 1UL;

				var restW = rest.Clone();
				restW.Multiply(w);
				var complementW = scale.Clone();
				complementW.Subtract(rest);
				complementW.Multiply(w);
				var tailScale = scale.Clone();
				tailScale.Multiply(tail);

				Big difference;
				if (Big.Compare(tailScale, restW) >= 0)
				{
					difference = tailScale;
					difference.Subtract(restW);
				}
				else
				{
					difference = restW.Clone();
					difference.Subtract(tailScale);
				}

				if (offsets[k] < StrongPeriodRepeats*length &&
					(Big.Compare(difference, restW) > 0 || Big.Compare(difference, complementW) > 0))
				{
					continue;
				}

				//Число равно 0.P(Q)(Q)...·10^orderValue, где P — первые start цифр, Q — период.
				var start = starts[k];
				var afterStart = order - start;
				var prefix = digits/Pow10[afterStart];
				var periodDigits = digits/Pow10[afterStart - length]%Pow10[length];
				return FromParts(negative, new U128(0UL, prefix*w + periodDigits), new U128(0UL, w), (long) orderValue - start);
			}

			return NaN;
		}

		/// <summary>
		/// Возвращает значение, показывающее, должен ли кандидат в периоды (<paramref name="partial1"/>, <paramref name="start1"/>,
		/// <paramref name="length1"/>) проверяться после кандидата (<paramref name="partial2"/>, <paramref name="start2"/>, <paramref name="length2"/>).
		/// </summary>
		private static bool IsAfter(bool partial1, int start1, int length1, bool partial2, int start2, int length2)
		{
			if (partial1 != partial2)
			{
				return partial1;
			}

			return start1 > start2 || (start1 == start2 && length1 > length2);
		}

		#endregion

		#region Simplify items

		/// <summary>
		/// Упрощает дробь.
		/// </summary>
		public void Simplify()
		{
			if (!SimplifyNaN())
			{
				SimplifyNum();
			}
		}

		private bool SimplifyNaN()
		{
			if (IsNaN(this))
			{
				this = NaN;
				return true;
			}

			if (IsPositiveInfinity(this))
			{
				this = PositiveInfinity;
				return true;
			}

			if (IsNegativeInfinity(this))
			{
				this = NegativeInfinity;
				return true;
			}

			if (IsEmpty(this))
			{
				this = Empty;
				return true;
			}

			return false;
		}

		private const long Max10 = long.MaxValue/10L;

		private void SimplifyNum()
		{
			//Каноническая запись вычисляется точно. Прежде цикл «numerator <= Max10» был всегда истинным
			//для отрицательных числителей: числитель переполнялся, и дробь меняла знак.
			Fraction result;
			if (TryExact(Numerator < 0L, new U128(0UL, Magnitude(Numerator)), new U128(0UL, (ulong) Denominator), Order, out result) == CanonicalOk)
			{
				this = result;
			}
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другое дробное число.</param>
		/// <returns>Значение <b>true</b>, если два числа равны; в противном случае — значение <b>false</b>. Как и <see cref="double.Equals(double)"/>, два значения <see cref="NaN"/> считаются равными.</returns>
		public bool Equals(Fraction other)
		{
			if (IsNaN(this) || IsNaN(other))
			{
				return IsNaN(this) && IsNaN(other);
			}

			return Compare(this, other) == 0;
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Fraction"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Fraction && Equals((Fraction) obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		public override int GetHashCode()
		{
			//Прежде всегда возвращался 0, и хэш-таблицы с дробями работали за квадратичное время.
			//Хэш вычисляется по разложению значения: ±p/q·2^twos·5^fives, где p и q не делятся на 2 и 5.
			//Эти величины одинаковы у всех равных дробей независимо от их записи.
			if (IsNaN(this))
			{
				return 0x7FF80000;
			}

			if (IsInfinity(this))
			{
				return Numerator > 0L ? 0x7FF00000 : unchecked((int) 0xFFF00000);
			}

			if (Numerator == 0L)
			{
				return 0;
			}

			bool negative;
			ulong p, q;
			Reduced(this, out negative, out p, out q);

			var twos = (long) TrailingZeros64(p) - TrailingZeros64(q) + Order;
			p >>= TrailingZeros64(p);
			q >>= TrailingZeros64(q);
			var fives = (long) Order;
			while (p%5UL == 0UL)
			{
				p /= 5UL;
				fives++;
			}

			while (q%5UL == 0UL)
			{
				q /= 5UL;
				fives--;
			}

			unchecked
			{
				var hash = p.GetHashCode();
				hash = (hash*397) ^ q.GetHashCode();
				hash = (hash*397) ^ twos.GetHashCode();
				hash = (hash*397) ^ fives.GetHashCode();
				return negative ? ~hash : hash;
			}
		}

		#endregion

		#region IComparable members

		/// <summary>
		/// Сравнивает текущий экземпляр с другим объектом того же типа и возвращает целое число, которое показывает, расположен ли текущий экземпляр перед, после или на той же позиции в порядке сортировки, что и другой объект.
		/// </summary>
		/// <returns>
		/// 32-битовое целое число со знаком, указывающее, каков относительный порядок сравниваемых объектов. Возвращаемые значения представляют следующие результаты сравнения:
		/// Меньше нуля: Этот экземпляр меньше параметра <paramref name="other"/>.<br/>
		/// Нуль: Этот экземпляр и параметр <paramref name="other"/> равны.<br/>
		/// Больше нуля: Этот экземпляр больше параметра <paramref name="other"/>.<br/>
		/// Как и у <see cref="double.CompareTo(double)"/>, значение <see cref="NaN"/> меньше любого другого значения и равно самому себе.
		/// </returns>
		/// <param name="other">Объект для сравнения с данным экземпляром.</param>
		public int CompareTo(Fraction other)
		{
			var thisNaN = IsNaN(this);
			var otherNaN = IsNaN(other);
			if (thisNaN || otherNaN)
			{
				return thisNaN ? (otherNaN ? 0 : -1) : 1;
			}

			return Compare(this, other);
		}

		/// <summary>
		/// Сравнивает текущий экземпляр с другим объектом того же типа и возвращает целое число, которое показывает, расположен ли текущий экземпляр перед, после или на той же позиции в порядке сортировки, что и другой объект.
		/// </summary>
		/// <returns>
		/// 32-битовое целое число со знаком, указывающее, каков относительный порядок сравниваемых объектов. Возвращаемые значения представляют следующие результаты сравнения:
		/// Меньше нуля: Этот экземпляр меньше параметра <paramref name="obj"/>.<br/>
		/// Нуль: Этот экземпляр и параметр <paramref name="obj"/> равны.<br/>
		/// Больше нуля: Этот экземпляр больше параметра <paramref name="obj"/> или <paramref name="obj"/> равен <b>null</b>.
		/// </returns>
		/// <param name="obj">Объект для сравнения с данным экземпляром.</param>
		/// <exception cref="ArgumentException">Тип значения параметра <paramref name="obj"/> отличается от типа данного экземпляра.</exception>
		public int CompareTo(object obj)
		{
			//Прежде null приводил к NullReferenceException, а объект другого типа — к InvalidCastException.
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is Fraction))
			{
				throw new ArgumentException("Объект должен иметь тип " + nameof(Fraction) + ".", nameof(obj));
			}

			return CompareTo((Fraction) obj);
		}

		#endregion

		#region IConvertible members

		/// <summary>
		/// Возвращает <see cref="T:System.TypeCode"/> для этого экземпляра.
		/// </summary>
		/// <returns>
		/// Перечислимая константа, которая является <see cref="T:System.TypeCode"/> данного класса или типа значения, реализующего этот интерфейс.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему логическое значение с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Логическое значение, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public bool ToBoolean(IFormatProvider provider)
		{
			return !IsEmpty(this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентный символ Юникода с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Символ Юникода, эквивалентный значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public char ToChar(IFormatProvider provider)
		{
			return Convert.ToChar((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 8-битовое целое число со знаком с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 8-битовое целое число со знаком, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public sbyte ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 8-битовое целое число без знака с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 8-битовое целое число без знака, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public byte ToByte(IFormatProvider provider)
		{
			return Convert.ToByte((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 16-битовое целое число со знаком с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 16-битовое целое число со знаком, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public short ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 16-битовое целое число без знака с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 16-битовое целое число без знака, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public ushort ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 32-битовое целое число со знаком с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 32-битовое целое число со знаком, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public int ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 32-битовое целое число без знака с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 32-битовое целое число без знака, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public uint ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 64-битовое целое число со знаком с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 64-битовое целое число со знаком, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public long ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 64-битовое целое число без знака с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 64-битовое целое число без знака, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public ulong ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное число одинарной точности с плавающей запятой с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Число одинарной точности с плавающей запятой, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public float ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное число двойной точности с плавающей запятой с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Число двойной точности с плавающей запятой, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public double ToDouble(IFormatProvider provider)
		{
			return (double) this;
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное число типа <see cref="T:System.Decimal"/> с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Число типа <see cref="T:System.Decimal"/>, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public decimal ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентную строку <see cref="T:System.DateTime"/> с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Экземпляр <see cref="T:System.DateTime"/>, эквивалентный значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public DateTime ToDateTime(IFormatProvider provider)
		{
			return Convert.ToDateTime((double) this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентную строку <see cref="T:System.String"/> с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Экземпляр <see cref="T:System.String"/>, эквивалентный значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public string ToString(IFormatProvider provider)
		{
			return ToString();
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в объект <see cref="T:System.Object"/> указанного типа <see cref="T:System.Type"/>, имеющий эквивалентное значение, с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Экземпляр <see cref="T:System.Object"/> типа <paramref name="conversionType"/>, значение которого эквивалентно значению данного экземпляра.
		/// </returns>
		/// <param name="conversionType"><see cref="T:System.Type"/>, в который преобразуется значение данного экземпляра. </param><param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public object ToType(Type conversionType, IFormatProvider provider)
		{
			//Convert.ChangeType(frac, typeof(Fraction)) вызывает этот метод: прежде значение уходило в double,
			//и преобразование в собственный тип выбрасывало InvalidCastException.
			if (conversionType == typeof(Fraction))
			{
				return this;
			}

			// ReSharper disable once AssignNullToNotNullAttribute
			return Convert.ChangeType((double) this, conversionType);
		}

		#endregion

		#region IFormattable members

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
			return string.IsNullOrEmpty(format) ? ToString() : ((double) this).ToString(format, formatProvider);
		}

		#endregion

		/// <summary>
		/// Возвращает строковое представлеине исходной дроби.
		/// </summary>
		/// <returns>
		/// Строковое представлеине исходной дроби. После дроби через « = » выводится её значение в виде числа двойной точности,
		/// если оно в точности равно дроби, иначе — через « ≈ ». Целые числа выводятся без значения.
		/// </returns>
		/// <remarks>
		/// <code>
		/// var frac = new Fraction(3, -7, -6);
		/// Console.Write(frac); //Результат: -3/7·10⁻⁶ ≈ -4.2857142857142857E-07
		/// frac = (Fraction)6/18;
		/// Console.Write(frac); //Результат: 1/3 ≈ 0.3333333333333333
		/// frac = (Fraction) 1777.7777777777777;
		/// Console.Write(frac); //Результат: 16/9·10³ ≈ 1777.7777777777778
		/// frac = new Fraction(1, 8);
		/// Console.Write(frac); //Результат: 1/8 = 0.125
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			if (IsNaN(this))
			{
				return "NaN";
			}

			if (IsPositiveInfinity(this))
			{
				return "∞";
			}

			if (IsNegativeInfinity(this))
			{
				return "-∞";
			}

			if (IsEmpty(this))
			{
				return "0";
			}

			var frac = this;
			frac.Simplify();

			var num = frac.Numerator;
			var den = frac.Denominator;
			var ord = frac.Order;

			//Знак « = » ставится, только если напечатанное значение в точности равно дроби. Прежде он ставился
			//для любого знаменателя вида 2^n или 5^n: "9007199254740993/2 = 4503599627370496".
			var value = (double) frac;
			var suff = (IsPrintedExactly(frac, value) ? " = " : " ≈ ") + value.ToString("R");

			//Порядок ±1 вносится в дробь, только если числитель или знаменатель не переполнится
			//(прежде num *= 10 переполнялся: "-9223372036854775806/7 ≈ 1.3176…E+18").
			switch (ord)
			{
				case 1:
					if (num >= -Max10 && num <= Max10)
					{
						num *= 10;
						ord = 0;
					}
					break;
				case -1:
					if (den <= Max10)
					{
						den *= 10;
						ord = 0;
					}
					break;
			}

			//CStatic.GetIndex не принимает int.MinValue (модуль не помещается в int): последняя цифра выводится отдельно.
			var expText = "10" + (frac.Order == int.MinValue
				? CStatic.GetIndex(frac.Order/10, true) + CStatic.GetIndex(-(frac.Order%10), true)
				: CStatic.GetIndex(frac.Order, true));

			if (den == 1)
			{
				if (ord == 0)
				{
					return num.ToString();
				}

				switch (num)
				{
					case 1:
						return expText + suff;

					case -1:
						return  "-" + expText + suff;

					default:
						return num + "·" + expText + suff;
				}
			}


			return (ord == 0 ? num + "/" + den : num + "/" + den + "·" + expText) + suff;
		}

		/// <summary>
		/// Проверяет, что строка <paramref name="value"/>.ToString("R") в точности равна дроби.
		/// </summary>
		private static bool IsPrintedExactly(Fraction fraction, double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
			{
				return false;
			}

			var text = value.ToString("R", CultureInfo.InvariantCulture);
			var negative = false;
			var mantissa = 0UL;
			var exponent = 0L;
			var point = false;
			var i = 0;

			if (text.Length > 0 && text[0] == '-')
			{
				negative = true;
				i++;
			}

			for (; i < text.Length; i++)
			{
				var c = text[i];
				if (c >= '0' && c <= '9')
				{
					if (mantissa > (ulong.MaxValue - 9UL)/10UL)
					{
						return false;
					}

					mantissa = mantissa*10UL + (ulong) (c - '0');
					if (point)
					{
						exponent--;
					}
				}
				else if (c == '.' && !point)
				{
					point = true;
				}
				else if (c == 'E' || c == 'e')
				{
					int power;
					if (!int.TryParse(text.Substring(i + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out power))
					{
						return false;
					}

					exponent += power;
					break;
				}
				else
				{
					return false;
				}
			}

			if (mantissa == 0UL)
			{
				return fraction.Numerator == 0L;
			}

			return Compare(FromParts(negative, new U128(0UL, mantissa), new U128(0UL, 1UL), exponent), fraction) == 0;
		}

		#region Exact arithmetic

		//Точное ядро. Значение ±num/den·10^order хранится в модулях без знака; промежуточные произведения
		//вычисляются в 128 битах (U128), а выравнивание порядков при сложении — в длинной арифметике (Big),
		//потому что в net35 нет System.Numerics.BigInteger.

		/// <summary>
		/// Наибольшее число значащих цифр, которое понимает метод <see cref="Parse"/>: 10^18 помещается в long.
		/// </summary>
		private const int MaxDigits = 18;

		/// <summary>
		/// Число совпавших цифр, при котором период принимается, даже если он не повторился целиком дважды
		/// (например, у 99999/7 = 14285.(571428) после целой части видно меньше двух периодов).
		/// </summary>
		private const int MinPeriodMatch = 4;

		/// <summary>
		/// Число целых повторений периода, после которого он принимается без проверки близости к исходному числу.
		/// </summary>
		private const int StrongPeriodRepeats = 4;

		/// <summary>
		/// Модуль long.MinValue.
		/// </summary>
		private const ulong SignBit = 0x8000000000000000UL;

		/// <summary>
		/// Если порядки слагаемых различаются больше чем на столько, меньшее слагаемое меньше 2^-127 от большего,
		/// и точная сумма не представима никакой дробью с 64-разрядными числителем и знаменателем.
		/// </summary>
		private const long MaxAlignment = 76L;

		private const int CanonicalOk = 0;
		private const int CanonicalOverflow = 1;
		private const int CanonicalUnderflow = -1;
		private const int CanonicalInexact = 2;

		private static readonly ulong[] Pow10 =
		{
			1UL, 10UL, 100UL, 1000UL, 10000UL, 100000UL, 1000000UL, 10000000UL, 100000000UL, 1000000000UL,
			10000000000UL, 100000000000UL, 1000000000000UL, 10000000000000UL, 100000000000000UL,
			1000000000000000UL, 10000000000000000UL, 100000000000000000UL, 1000000000000000000UL,
			10000000000000000000UL
		};

		private static readonly ulong[] Pow5 =
		{
			1UL, 5UL, 25UL, 125UL, 625UL, 3125UL, 15625UL, 78125UL, 390625UL, 1953125UL, 9765625UL, 48828125UL,
			244140625UL, 1220703125UL, 6103515625UL, 30517578125UL, 152587890625UL, 762939453125UL,
			3814697265625UL, 19073486328125UL, 95367431640625UL, 476837158203125UL, 2384185791015625UL,
			11920928955078125UL, 59604644775390625UL, 298023223876953125UL, 1490116119384765625UL,
			7450580596923828125UL
		};

		/// <summary>
		/// floor((2^63 - 1)/5^s) для s от 0 до 27.
		/// </summary>
		private static readonly ulong[] MaxOverPow5 = GetMaxOverPow5(long.MaxValue);

		/// <summary>
		/// floor(2^63/5^s) для s от 0 до 27.
		/// </summary>
		private static readonly ulong[] MaxOverPow5Negative = GetMaxOverPow5(SignBit);

		private static ulong[] GetMaxOverPow5(ulong limit)
		{
			var table = new ulong[28];
			for (var i = 0; i < table.Length; i++)
			{
				table[i] = limit/Pow5[i];
			}

			return table;
		}

		private static ulong Magnitude(long value)
		{
			return value < 0L ? (ulong) (-(value + 1L)) + 1UL : (ulong) value;
		}

		private static int SignOf(Fraction x)
		{
			return x.Numerator > 0L ? 1 : (x.Numerator < 0L ? -1 : 0);
		}

		/// <summary>
		/// Возвращает знак и несократимые модули числителя и знаменателя конечной дроби.
		/// </summary>
		private static void Reduced(Fraction x, out bool negative, out ulong numerator, out ulong denominator)
		{
			negative = x.Numerator < 0L;
			numerator = Magnitude(x.Numerator);
			denominator = (ulong) x.Denominator;

			if (numerator == 0UL)
			{
				denominator = 1UL;
				return;
			}

			var gcd = Gcd(numerator, denominator);
			if (gcd != 1UL)
			{
				numerator /= gcd;
				denominator /= gcd;
			}
		}

		private static ulong Gcd(ulong x, ulong y)
		{
			if (x == 1UL || y == 1UL)
			{
				return 1UL;
			}

			while (y != 0UL)
			{
				if ((x | y) <= uint.MaxValue)
				{
					//32-разрядное деление заметно быстрее 64-разрядного.
					uint a = (uint) x, b = (uint) y;
					while (b != 0u)
					{
						var r = a%b;
						a = b;
						b = r;
					}

					return a;
				}

				var rem = x%y;
				x = y;
				y = rem;
			}

			return x;
		}

		private static int BitLength64(ulong x)
		{
			var n = 0;
			if (x >= 1UL << 32)
			{
				x >>= 32;
				n += 32;
			}

			if (x >= 1UL << 16)
			{
				x >>= 16;
				n += 16;
			}

			if (x >= 1UL << 8)
			{
				x >>= 8;
				n += 8;
			}

			if (x >= 1UL << 4)
			{
				x >>= 4;
				n += 4;
			}

			if (x >= 1UL << 2)
			{
				x >>= 2;
				n += 2;
			}

			if (x >= 1UL << 1)
			{
				x >>= 1;
				n += 1;
			}

			return x != 0UL ? n + 1 : n;
		}

		private static int TrailingZeros64(ulong x)
		{
			if (x == 0UL)
			{
				return 0;
			}

			var n = 0;
			if ((x & 0xFFFFFFFFUL) == 0UL)
			{
				x >>= 32;
				n += 32;
			}

			if ((x & 0xFFFFUL) == 0UL)
			{
				x >>= 16;
				n += 16;
			}

			if ((x & 0xFFUL) == 0UL)
			{
				x >>= 8;
				n += 8;
			}

			if ((x & 0xFUL) == 0UL)
			{
				x >>= 4;
				n += 4;
			}

			if ((x & 0x3UL) == 0UL)
			{
				x >>= 2;
				n += 2;
			}

			if ((x & 0x1UL) == 0UL)
			{
				n += 1;
			}

			return n;
		}

		/// <summary>
		/// Делит 128-разрядное число hi·2^64 + lo на divisor (требуется hi &lt; divisor).
		/// </summary>
		private static ulong DivRem128(ulong hi, ulong lo, ulong divisor, out ulong remainder)
		{
			var quotient = 0UL;
			for (var i = 0; i < 64; i++)
			{
				var carry = hi >> 63;
				hi = (hi << 1) | (lo >> 63);
				lo <<= 1;
				quotient <<= 1;
				if (carry != 0UL || hi >= divisor)
				{
					hi -= divisor;
					quotient |= 1UL;
				}
			}

			remainder = hi;
			return quotient;
		}

		/// <summary>
		/// Наибольшее s, при котором x·2^s ≤ limit (x нечётно, limit равно 2^63 - 1 или 2^63).
		/// </summary>
		private static long MaxShift2(ulong x, ulong limit)
		{
			//Для нечётного x > 1 произведение x·2^s не превышает 2^63 - 1 ровно при BitLength(x) + s ≤ 63;
			//единица, кроме того, даёт 2^63 = |long.MinValue|.
			return x == 1UL && limit == SignBit ? 63L : 63L - BitLength64(x);
		}

		/// <summary>
		/// Наибольшее s, при котором x·5^s ≤ limit (x ≥ 1, limit равно 2^63 - 1 или 2^63).
		/// </summary>
		private static long MaxShift5(ulong x, ulong limit)
		{
			//Двоичный поиск по таблице floor(limit/5^s): без делений.
			var table = limit == SignBit ? MaxOverPow5Negative : MaxOverPow5;
			int low = 0, high = 27;
			while (low < high)
			{
				var middle = (low + high + 1) >> 1;
				if (x <= table[middle])
				{
					low = middle;
				}
				else
				{
					high = middle - 1;
				}
			}

			return low;
		}

		/// <summary>
		/// Строит каноническую запись числа ±p/q·2^alpha·5^beta·10^order, где p и q не делятся ни на 2, ни на 5.
		/// </summary>
		/// <remarks>
		/// Каноническая запись n/d·10^e однозначно определяется значением: n и d взаимно просты и не делятся на 10,
		/// а порядок e — ближайший к нулю из допустимых. Для дробей, которые помещаются в long без порядка,
		/// это та же запись, что давало прежнее упрощение.
		/// </remarks>
		private static int TryCanonical(bool negative, ulong p, ulong q, long alpha, long beta, long order, out Fraction result)
		{
			result = NaN;
			var maxNumerator = negative ? SignBit : long.MaxValue;
			const ulong maxDenominator = long.MaxValue;

			if (p > maxNumerator || q > maxDenominator)
			{
				return CanonicalInexact;
			}

			//Сдвиг t переносит множители: n = p·2^(alpha - t)·5^(beta - t), d = q·2^(t - alpha)·5^(t - beta),
			//отрицательные степени уходят в другую часть дроби. При t от min(alpha, beta) до max(alpha, beta)
			//ни n, ни d не делятся на 10.
			var twos = alpha >= beta;
			long low, high;
			if (twos)
			{
				low = Max(beta, alpha - MaxShift2(p, maxNumerator));
				high = Min(alpha, beta + MaxShift5(q, maxDenominator));
			}
			else
			{
				low = Max(alpha, beta - MaxShift5(p, maxNumerator));
				high = Min(beta, alpha + MaxShift2(q, maxDenominator));
			}

			if (low > high)
			{
				return CanonicalInexact;
			}

			var t = -order < low ? low : (-order > high ? high : -order);
			ulong n, d;
			if (twos)
			{
				n = p << (int) (alpha - t);
				d = q*Pow5[t - beta];
			}
			else
			{
				n = p*Pow5[beta - t];
				d = q << (int) (t - alpha);
			}

			var e = order + t;
			if (e > int.MaxValue)
			{
				//Порядок не помещается в int: переносим лишние десятки в числитель, если он позволяет.
				var excess = e - int.MaxValue;
				if (t != Min(alpha, beta) || excess > 19L)
				{
					return CanonicalOverflow;
				}

				for (var i = 0L; i < excess; i++)
				{
					if (n > maxNumerator/10UL)
					{
						return CanonicalOverflow;
					}

					n *= 10UL;
				}

				e = int.MaxValue;
			}
			else if (e < int.MinValue)
			{
				var excess = int.MinValue - e;
				if (t != Max(alpha, beta) || excess > 19L)
				{
					return CanonicalUnderflow;
				}

				for (var i = 0L; i < excess; i++)
				{
					if (d > maxDenominator/10UL)
					{
						return CanonicalUnderflow;
					}

					d *= 10UL;
				}

				e = int.MinValue;
			}

			result = Raw(negative ? unchecked((long) (0UL - n)) : (long) n, (long) d, (int) e);
			return CanonicalOk;
		}

		private static int StripFives(ref U128 x)
		{
			var count = 0;
			while (!x.IsZero)
			{
				U128 quotient;
				if (U128.DivRem(x, 5u, out quotient) != 0u)
				{
					break;
				}

				x = quotient;
				count++;
			}

			return count;
		}

		/// <summary>
		/// Строит каноническую дробь, точно равную ±num/den·10^order.
		/// </summary>
		/// <returns>Одно из значений CanonicalOk, CanonicalOverflow, CanonicalUnderflow, CanonicalInexact.</returns>
		private static int TryExact(bool negative, U128 num, U128 den, long order, out Fraction result, bool reduced = false)
		{
			if (num.IsZero)
			{
				result = Empty;
				return CanonicalOk;
			}

			if (num.Hi == 0UL && den.Hi == 0UL)
			{
				return TryExact(negative, num.Lo, den.Lo, order, out result, reduced);
			}

			var gcd = reduced ? new U128(0UL, 1UL) : U128.Gcd(num, den);
			if (gcd.Hi != 0UL || gcd.Lo != 1UL)
			{
				U128 rem;
				num = U128.DivRem(num, gcd, out rem);
				den = U128.DivRem(den, gcd, out rem);
			}

			var a2 = num.TrailingZeros;
			var c2 = den.TrailingZeros;
			num = num.ShiftRight(a2);
			den = den.ShiftRight(c2);
			var b5 = StripFives(ref num);
			var d5 = StripFives(ref den);

			if (num.Hi != 0UL || den.Hi != 0UL)
			{
				result = NaN;
				return CanonicalInexact;
			}

			return TryCanonical(negative, num.Lo, den.Lo, a2 - c2, b5 - d5, order, out result);
		}

		/// <summary>
		/// То же, что и для 128-разрядных чисел, но быстрее: числитель (не равный нулю) и знаменатель помещаются в 64 бита.
		/// </summary>
		private static int TryExact(bool negative, ulong num, ulong den, long order, out Fraction result, bool reduced = false)
		{
			var gcd = reduced ? 1UL : Gcd(num, den);
			if (gcd != 1UL)
			{
				num /= gcd;
				den /= gcd;
			}

			var a2 = TrailingZeros64(num);
			var c2 = TrailingZeros64(den);
			num >>= a2;
			den >>= c2;

			var b5 = 0;
			while (num%5UL == 0UL)
			{
				num /= 5UL;
				b5++;
			}

			var d5 = 0;
			while (den%5UL == 0UL)
			{
				den /= 5UL;
				d5++;
			}

			return TryCanonical(negative, num, den, a2 - c2, b5 - d5, order, out result);
		}

		private static Fraction FromParts(bool negative, U128 num, U128 den, long order, bool reduced = false)
		{
			bool exact;
			return FromParts(negative, num, den, order, out exact, reduced);
		}

		/// <summary>
		/// Возвращает дробь, равную ±num/den·10^order, а если такая не представима — ближайшее приближение.
		/// </summary>
		private static Fraction FromParts(bool negative, U128 num, U128 den, long order, out bool exact, bool reduced = false)
		{
			Fraction result;
			exact = false;
			switch (TryExact(negative, num, den, order, out result, reduced))
			{
				case CanonicalOk:
					exact = true;
					return result;

				case CanonicalOverflow:
					return negative ? NegativeInfinity : PositiveInfinity;

				case CanonicalUnderflow:
					return Empty;

				default:
					return Approximate(negative, num, den, order);
			}
		}

		/// <summary>
		/// Возвращает дробь, равную ±num/den·10^order, где num и den взаимно просты.
		/// </summary>
		private static Fraction FromParts(bool negative, Big num, U128 den, long order, out bool exact)
		{
			if (num.BitLength <= 128)
			{
				return FromParts(negative, num.ToU128(), den, order, out exact, true);
			}

			//Длинный числитель представим, только если после переноса множителей 2 и 5 в порядок
			//он помещается в 64 бита.
			var work = num.Clone();
			var a2 = work.TrailingZeroBits;
			work.ShiftRight(a2);
			var b5 = 0;
			while (work.Mod(5u) == 0u)
			{
				work.DivRem(5u);
				b5++;
			}

			exact = false;
			if (work.BitLength <= 64)
			{
				var q = den;
				var c2 = q.TrailingZeros;
				q = q.ShiftRight(c2);
				var d5 = StripFives(ref q);
				if (q.Hi == 0UL)
				{
					Fraction result;
					switch (TryCanonical(negative, work.ToUInt64(), q.Lo, (long) a2 - c2, (long) b5 - d5, order, out result))
					{
						case CanonicalOk:
							exact = true;
							return result;

						case CanonicalOverflow:
							return negative ? NegativeInfinity : PositiveInfinity;

						case CanonicalUnderflow:
							return Empty;
					}
				}
			}

			var shrunk = num.Clone();
			while (shrunk.BitLength > 120)
			{
				shrunk.DivRem(1000000000u);
				order += 9L;
			}

			return Approximate(negative, shrunk.ToU128(), den, order);
		}

		/// <summary>
		/// Возвращает приближение ±num/den·10^order с 18 значащими цифрами.
		/// </summary>
		private static Fraction Approximate(bool negative, U128 num, U128 den, long order)
		{
			//Числитель и знаменатель меньше 2^120, чтобы умножение остатка на 10 не переполнялось.
			U128 quotient, rem;
			while (den.BitLength > 120)
			{
				U128.DivRem(den, 10u, out quotient);
				den = quotient;
				order--;
			}

			while (num.BitLength > 120)
			{
				U128.DivRem(num, 10u, out quotient);
				num = quotient;
				order++;
			}

			quotient = U128.DivRem(num, den, out rem);

			var upper = new U128(0UL, Pow10[MaxDigits]);
			var lower = new U128(0UL, Pow10[MaxDigits - 1]);
			bool roundUp;
			if (U128.Compare(quotient, upper) >= 0)
			{
				//Лишние цифры отбрасываются с округлением по первой отброшенной цифре.
				var dropped = 0u;
				while (U128.Compare(quotient, upper) >= 0)
				{
					dropped = U128.DivRem(quotient, 10u, out quotient);
					order++;
				}

				roundUp = dropped >= 5u;
			}
			else
			{
				while (U128.Compare(quotient, lower) < 0)
				{
					U128 scaled;
					U128.TryMultiply(rem, 10u, out scaled);
					U128 digit;
					digit = U128.DivRem(scaled, den, out rem);
					U128.TryMultiply(quotient, 10u, out quotient);
					quotient = U128.Add(quotient, digit);
					order--;
				}

				roundUp = U128.Compare(rem.ShiftLeft(1), den) >= 0;
			}

			var digits = quotient.Lo + (roundUp ? 1UL : 0UL);
			if (digits == Pow10[MaxDigits])
			{
				digits = Pow10[MaxDigits - 1];
				order++;
			}

			bool exact;
			return FromParts(negative, new U128(0UL, digits), new U128(0UL, 1UL), order, out exact);
		}

		/// <summary>
		/// Сравнивает две дроби, ни одна из которых не равна <see cref="NaN"/>.
		/// </summary>
		private static int Compare(Fraction x, Fraction y)
		{
			var sx = SignOf(x);
			var sy = SignOf(y);
			if (sx != sy)
			{
				return sx < sy ? -1 : 1;
			}

			if (sx == 0)
			{
				return 0;
			}

			var xInfinity = x.Denominator == 0L;
			var yInfinity = y.Denominator == 0L;
			if (xInfinity || yInfinity)
			{
				return xInfinity == yInfinity ? 0 : (xInfinity ? sx : -sx);
			}

			var result = CompareMagnitude(Magnitude(x.Numerator), (ulong) x.Denominator, x.Order,
				Magnitude(y.Numerator), (ulong) y.Denominator, y.Order);
			return sx > 0 ? result : -result;
		}

		/// <summary>
		/// Сравнивает n1/d1·10^k1 и n2/d2·10^k2 (все числа положительны).
		/// </summary>
		private static int CompareMagnitude(ulong n1, ulong d1, long k1, ulong n2, ulong d2, long k2)
		{
			var a = U128.Multiply(n1, d2);
			var b = U128.Multiply(n2, d1);
			var shift = k1 - k2;
			return shift >= 0L ? CompareScaled(a, shift, b) : -CompareScaled(b, -shift, a);
		}

		/// <summary>
		/// Возвращает знак a·10^shift - b.
		/// </summary>
		private static int CompareScaled(U128 a, long shift, U128 b)
		{
			for (; shift > 0L; shift--)
			{
				//a ≥ 1, поэтому, как только a превысило b или переполнилось, a·10^shift тем более больше b.
				if (U128.Compare(a, b) > 0 || !U128.TryMultiply(a, 10u, out a))
				{
					return 1;
				}
			}

			return U128.Compare(a, b);
		}

		/// <summary>
		/// Складывает или вычитает дроби точно, если результат представим; иначе возвращает приближение.
		/// </summary>
		private static Fraction Add(Fraction x, Fraction y, bool subtract, out bool exact)
		{
			exact = true;
			if (IsNaN(x) || IsNaN(y))
			{
				return NaN;
			}

			var xSign = SignOf(x);
			var ySign = subtract ? -SignOf(y) : SignOf(y);
			var xInfinity = x.Denominator == 0L;
			var yInfinity = y.Denominator == 0L;
			if (xInfinity || yInfinity)
			{
				//Бесконечности складываются по правилам IEEE. Прежде перекрёстное умножение нулевых знаменателей
				//давало 0/0, и +∞ + +∞ было NaN.
				if (xInfinity && yInfinity && xSign != ySign)
				{
					return NaN;
				}

				return (xInfinity ? xSign : ySign) > 0 ? PositiveInfinity : NegativeInfinity;
			}

			bool xNegative, yNegative;
			ulong n1, d1, n2, d2;
			Reduced(x, out xNegative, out n1, out d1);
			Reduced(y, out yNegative, out n2, out d2);
			if (subtract)
			{
				yNegative = !yNegative;
			}

			long k1 = x.Order, k2 = y.Order;

			if (n2 == 0UL)
			{
				return FromParts(xNegative, new U128(0UL, n1), new U128(0UL, d1), k1, out exact, true);
			}

			if (n1 == 0UL)
			{
				return FromParts(yNegative, new U128(0UL, n2), new U128(0UL, d2), k2, out exact, true);
			}

			if (k1 < k2)
			{
				Swap(ref xNegative, ref yNegative);
				Swap(ref n1, ref n2);
				Swap(ref d1, ref d2);
				Swap(ref k1, ref k2);
			}

			var shift = k1 - k2;
			if (shift > MaxAlignment)
			{
				exact = false;
				return FromParts(xNegative, new U128(0UL, n1), new U128(0UL, d1), k1, true);
			}

			//x = n1·10^shift/d1·10^k2: общие множители 10^shift и d1 сокращаются заранее.
			var d1Reduced = d1;
			var twos = Min((long) TrailingZeros64(d1Reduced), shift);
			d1Reduced >>= (int) twos;
			var fives = 0L;
			while (fives < shift && d1Reduced%5UL == 0UL)
			{
				d1Reduced /= 5UL;
				fives++;
			}

			//Сумма a/b + c/d по Кнуту: g = НОД(b, d), t = a·(d/g) ± c·(b/g), знаменатель (b/g)·(d/НОД(t, g)).
			var gcd = Gcd(d1Reduced, d2);
			var m1 = gcd == 1UL ? d2 : d2/gcd;
			var m2 = gcd == 1UL ? d1Reduced : d1Reduced/gcd;

			ulong scaled;
			if (TryScale(n1, shift - twos, shift - fives, out scaled))
			{
				var t1 = U128.Multiply(scaled, m1);
				var t2 = U128.Multiply(n2, m2);
				if (t1.Hi == 0UL && t2.Hi == 0UL && t1.Lo <= long.MaxValue && t2.Lo <= long.MaxValue)
				{
					bool negative;
					ulong sum;
					if (xNegative == yNegative)
					{
						sum = t1.Lo + t2.Lo;
						negative = xNegative;
					}
					else if (t1.Lo >= t2.Lo)
					{
						sum = t1.Lo - t2.Lo;
						negative = xNegative;
					}
					else
					{
						sum = t2.Lo - t1.Lo;
						negative = yNegative;
					}

					if (sum == 0UL)
					{
						return Empty;
					}

					var common = gcd == 1UL ? 1UL : Gcd(sum%gcd, gcd);
					if (common != 1UL)
					{
						sum /= common;
					}

					return FromParts(negative, new U128(0UL, sum), U128.Multiply(m2, common == 1UL ? d2 : d2/common), k2, out exact, true);
				}
			}

			var big1 = new Big(n1);
			big1.ShiftLeft(shift - twos);
			big1.MultiplyPow5(shift - fives);
			big1.Multiply(m1);
			var big2 = new Big(U128.Multiply(n2, m2));

			Big total;
			bool totalNegative;
			if (xNegative == yNegative)
			{
				big1.Add(big2);
				total = big1;
				totalNegative = xNegative;
			}
			else
			{
				var comparison = Big.Compare(big1, big2);
				if (comparison == 0)
				{
					return Empty;
				}

				if (comparison > 0)
				{
					big1.Subtract(big2);
					total = big1;
					totalNegative = xNegative;
				}
				else
				{
					big2.Subtract(big1);
					total = big2;
					totalNegative = yNegative;
				}
			}

			var totalCommon = Gcd(total.Mod(gcd), gcd);
			if (totalCommon != 1UL)
			{
				total.DivRem(totalCommon);
			}

			var result = FromParts(totalNegative, total, U128.Multiply(m2, d2/totalCommon), k2, out exact);
			if (exact)
			{
				return result;
			}

			//Точная сумма не представима. Если меньшее слагаемое меньше 10^-18 от большего, ближайшим
			//значением остаётся само большее слагаемое: приближение с 18 цифрами было бы дальше.
			if (CompareMagnitude(n1, d1, k1, n2, d2, k2) >= 0)
			{
				if (CompareMagnitude(n2, d2, k2 + 18L, n1, d1, k1) < 0)
				{
					return FromParts(xNegative, new U128(0UL, n1), new U128(0UL, d1), k1, true);
				}
			}
			else if (CompareMagnitude(n1, d1, k1 + 18L, n2, d2, k2) < 0)
			{
				return FromParts(yNegative, new U128(0UL, n2), new U128(0UL, d2), k2, true);
			}

			return result;
		}

		/// <summary>
		/// Вычисляет n·2^twos·5^fives, если результат не превышает long.MaxValue.
		/// </summary>
		private static bool TryScale(ulong n, long twos, long fives, out ulong result)
		{
			result = n;
			if (twos > 63L || fives > 27L)
			{
				return false;
			}

			for (var i = 0L; i < fives; i++)
			{
				if (result > long.MaxValue/5L)
				{
					return false;
				}

				result *= 5UL;
			}

			if (result > (ulong) long.MaxValue >> (int) twos)
			{
				return false;
			}

			result <<= (int) twos;
			return true;
		}

		private static void Swap<T>(ref T x, ref T y)
		{
			var tmp = x;
			x = y;
			y = tmp;
		}

		/// <summary>
		/// Умножает или делит дроби точно, если результат представим; иначе возвращает приближение.
		/// </summary>
		private static Fraction Multiply(Fraction x, Fraction y, bool divide, out bool exact)
		{
			exact = true;
			if (IsNaN(x) || IsNaN(y))
			{
				return NaN;
			}

			//Бесконечности и нули обрабатываются по правилам IEEE. Прежде знак бесконечности терялся:
			//-∞ / (Fraction)(-1) давало -∞.
			var xSign = SignOf(x);
			var ySign = SignOf(y);
			var xInfinity = x.Denominator == 0L;
			var yInfinity = y.Denominator == 0L;
			if (!divide)
			{
				if (xInfinity || yInfinity)
				{
					return xSign == 0 || ySign == 0 ? NaN : (xSign*ySign > 0 ? PositiveInfinity : NegativeInfinity);
				}

				if (xSign == 0 || ySign == 0)
				{
					return Empty;
				}
			}
			else
			{
				if (xInfinity)
				{
					return yInfinity ? NaN : ((ySign < 0 ? -xSign : xSign) > 0 ? PositiveInfinity : NegativeInfinity);
				}

				if (yInfinity)
				{
					return Empty;
				}

				if (ySign == 0)
				{
					return xSign == 0 ? NaN : (xSign > 0 ? PositiveInfinity : NegativeInfinity);
				}

				if (xSign == 0)
				{
					return Empty;
				}
			}

			bool xNegative, yNegative;
			ulong n1, d1, n2, d2;
			Reduced(x, out xNegative, out n1, out d1);
			Reduced(y, out yNegative, out n2, out d2);
			if (divide)
			{
				Swap(ref n2, ref d2);
			}

			//Перекрёстное сокращение: после него произведение уже несократимо.
			var g1 = Gcd(n1, d2);
			var g2 = Gcd(n2, d1);
			if (g1 != 1UL)
			{
				n1 /= g1;
				d2 /= g1;
			}

			if (g2 != 1UL)
			{
				n2 /= g2;
				d1 /= g2;
			}

			var order = divide ? (long) x.Order - y.Order : (long) x.Order + y.Order;
			return FromParts(xNegative != yNegative, U128.Multiply(n1, n2), U128.Multiply(d1, d2), order, out exact, true);
		}

		/// <summary>
		/// Вычисляет остаток от деления с частным, округлённым к нулю, как у оператора % для целых чисел.
		/// </summary>
		/// <param name="x">Делимое.</param>
		/// <param name="y">Делитель.</param>
		/// <param name="exact">Значение <b>true</b>, если остаток вычислен точно.</param>
		/// <returns>Остаток: <see cref="NaN"/>, если <paramref name="y"/> равно нулю или <paramref name="x"/> бесконечно.</returns>
		internal static Fraction Remainder(Fraction x, Fraction y, out bool exact)
		{
			exact = true;
			if (IsNaN(x) || IsNaN(y) || IsInfinity(x) || IsEmpty(y))
			{
				return NaN;
			}

			bool xNegative, yNegative;
			ulong n1, d1, n2, d2;
			Reduced(x, out xNegative, out n1, out d1);
			if (IsInfinity(y) || n1 == 0UL)
			{
				return FromParts(xNegative, new U128(0UL, n1), new U128(0UL, d1), x.Order, out exact, true);
			}

			Reduced(y, out yNegative, out n2, out d2);
			long k1 = x.Order, k2 = y.Order;
			if (CompareMagnitude(n1, d1, k1, n2, d2, k2) < 0)
			{
				return FromParts(xNegative, new U128(0UL, n1), new U128(0UL, d1), k1, out exact, true);
			}

			//|x|/|y| = A/B·10^shift, A и B взаимно просты. Остаток равен |y|·frac(|x|/|y|) = g1·R/НОК(d1, d2)·10^min(k1, k2),
			//где R — остаток от деления A·10^shift на B (или A на B·10^-shift).
			var g1 = Gcd(n1, n2);
			var g2 = Gcd(d1, d2);
			var a = U128.Multiply(n1/g1, d2/g2);
			var b = U128.Multiply(d1/g2, n2/g1);
			var shift = k1 - k2;

			U128 rem;
			if (shift >= 0L)
			{
				U128.DivRem(a, b, out rem);
				rem = U128.MultiplyMod(rem, U128.PowMod(10u, shift, b), b);
			}
			else
			{
				//|x| ≥ |y|, значит B·10^-shift ≤ A и не переполняется.
				var scaledB = b;
				for (var i = 0L; i < -shift; i++)
				{
					U128.TryMultiply(scaledB, 10u, out scaledB);
				}

				U128.DivRem(a, scaledB, out rem);
			}

			if (rem.IsZero)
			{
				return Empty;
			}

			var den = U128.Multiply(d1/g2, d2);
			var common = U128.Gcd(rem, den);
			U128 ignored;
			rem = U128.DivRem(rem, common, out ignored);
			den = U128.DivRem(den, common, out ignored);

			var num = new Big(rem);
			num.Multiply(g1);
			return FromParts(xNegative, num, den, Min(k1, k2), out exact);
		}

		/// <summary>
		/// Вычисляет частное от деления дробей, округлённое к нулю.
		/// </summary>
		/// <exception cref="DivideByZeroException">Делитель равен нулю.</exception>
		/// <exception cref="OverflowException">Частное не помещается в long или не определено.</exception>
		internal static long TruncatedQuotient(Fraction x, Fraction y)
		{
			if (IsEmpty(y))
			{
				throw new DivideByZeroException();
			}

			if (IsNaN(x) || IsNaN(y) || IsInfinity(x))
			{
				throw new OverflowException("Частное не является конечным числом.");
			}

			if (IsInfinity(y) || IsEmpty(x))
			{
				return 0L;
			}

			bool xNegative, yNegative;
			ulong n1, d1, n2, d2;
			Reduced(x, out xNegative, out n1, out d1);
			Reduced(y, out yNegative, out n2, out d2);

			var g1 = Gcd(n1, n2);
			var g2 = Gcd(d1, d2);
			var a = U128.Multiply(n1/g1, d2/g2);
			var b = U128.Multiply(d1/g2, n2/g1);
			var shift = (long) x.Order - y.Order;

			//|x|/|y| = A/B·10^shift, где A, B < 2^126. При shift ≥ 57 частное заведомо больше 2^63.
			ulong quotient;
			if (shift >= 57L)
			{
				throw new OverflowException("Частное не помещается в 64-разрядное целое число.");
			}

			if (shift < 0L)
			{
				var scaledB = b;
				var overflow = false;
				for (var i = 0L; i < -shift && !overflow; i++)
				{
					overflow = !U128.TryMultiply(scaledB, 10u, out scaledB);
				}

				U128 rem;
				var q = overflow ? default(U128) : U128.DivRem(a, scaledB, out rem);
				if (q.Hi != 0UL)
				{
					throw new OverflowException("Частное не помещается в 64-разрядное целое число.");
				}

				quotient = q.Lo;
			}
			else
			{
				var dividend = new Big(a);
				dividend.MultiplyPow10(shift);
				if (!TryDivide(dividend, b, out quotient))
				{
					throw new OverflowException("Частное не помещается в 64-разрядное целое число.");
				}
			}

			var negative = xNegative != yNegative;
			if (quotient > (negative ? SignBit : long.MaxValue))
			{
				throw new OverflowException("Частное не помещается в 64-разрядное целое число.");
			}

			return negative ? unchecked((long) (0UL - quotient)) : (long) quotient;
		}

		/// <summary>
		/// Делит длинное число на 128-разрядное, если частное помещается в 64 бита.
		/// </summary>
		private static bool TryDivide(Big dividend, U128 divisor, out ulong quotient)
		{
			quotient = 0UL;
			var rem = default(U128);
			for (var i = dividend.BitLength - 1; i >= 0; i--)
			{
				//divisor < 2^126, поэтому удвоенный остаток помещается в 128 бит.
				rem = rem.ShiftLeft(1);
				if (dividend.TestBit(i))
				{
					rem = U128.Add(rem, new U128(0UL, 1UL));
				}

				if ((quotient & SignBit) != 0UL)
				{
					return false;
				}

				quotient <<= 1;
				if (U128.Compare(rem, divisor) >= 0)
				{
					rem = U128.Subtract(rem, divisor);
					quotient |= 1UL;
				}
			}

			return true;
		}

		/// <summary>
		/// Возвращает точное представление положительного числа: value = mantissa·2^exponent.
		/// </summary>
		private static void Decompose(double value, out ulong mantissa, out int exponent)
		{
			var bits = BitConverter.DoubleToInt64Bits(value);
			var biased = (int) ((bits >> 52) & 0x7FFL);
			mantissa = (ulong) bits & 0xFFFFFFFFFFFFFUL;
			if (biased == 0)
			{
				exponent = -1074;
			}
			else
			{
				mantissa |= 1UL << 52;
				exponent = biased - 1075;
			}
		}

		/// <summary>
		/// Вычисляет точно: mantissa·2^exponent·10^power = digits + rest/scale, где 0 ≤ rest &lt; scale.
		/// </summary>
		/// <returns>Значение <b>false</b>, если целая часть не помещается в 64 бита.</returns>
		private static bool ScaleFloor(ulong mantissa, int exponent, int power, out ulong digits, out Big rest, out Big scale)
		{
			var twos = (long) exponent + power;
			var fives = (long) power;

			var numerator = new Big(mantissa);
			var denominator = new Big(1UL);
			if (twos >= 0L)
			{
				numerator.ShiftLeft(twos);
			}
			else
			{
				denominator.ShiftLeft(-twos);
			}

			if (fives >= 0L)
			{
				numerator.MultiplyPow5(fives);
			}
			else
			{
				denominator.MultiplyPow5(-fives);
			}

			//Знаменатель — произведение степеней 2 и 5, поэтому целая часть получается сдвигом и делением на 5^n.
			var quotient = numerator.Clone();
			if (twos < 0L)
			{
				quotient.ShiftRight(-twos);
			}

			if (fives < 0L)
			{
				quotient.DividePow5(-fives);
			}

			digits = 0UL;
			rest = numerator;
			scale = denominator;
			if (quotient.BitLength > 64)
			{
				return false;
			}

			digits = quotient.ToUInt64();
			var product = denominator.Clone();
			product.Multiply(digits);
			rest.Subtract(product);
			return true;
		}

		/// <summary>
		/// Умножает число на 10^order, не допуская переполнения промежуточного множителя.
		/// </summary>
		private static double ScaleByPowerOfTen(double value, long order)
		{
			while (order > 300L && !double.IsInfinity(value) && !value.Equals(0D))
			{
				value *= 1E+300;
				order -= 300L;
			}

			while (order < -300L && !double.IsInfinity(value) && !value.Equals(0D))
			{
				value *= 1E-300;
				order += 300L;
			}

			return order > 300L || order < -300L ? value : value*Exp10((int) order);
		}

		/// <summary>
		/// 128-разрядное целое число без знака для точных промежуточных вычислений.
		/// </summary>
		private struct U128
		{
			public readonly ulong Hi;
			public readonly ulong Lo;

			public U128(ulong hi, ulong lo)
			{
				Hi = hi;
				Lo = lo;
			}

			public bool IsZero => (Hi | Lo) == 0UL;

			public int BitLength => Hi != 0UL ? 64 + BitLength64(Hi) : BitLength64(Lo);

			public int TrailingZeros => Lo != 0UL ? TrailingZeros64(Lo) : (Hi != 0UL ? 64 + TrailingZeros64(Hi) : 0);

			public bool TestBit(int index)
			{
				return index < 64 ? ((Lo >> index) & 1UL) != 0UL : ((Hi >> (index - 64)) & 1UL) != 0UL;
			}

			public U128 ShiftLeft(int count)
			{
				if (count == 0)
				{
					return this;
				}

				if (count >= 128)
				{
					return default(U128);
				}

				return count >= 64
					? new U128(Lo << (count - 64), 0UL)
					: new U128((Hi << count) | (Lo >> (64 - count)), Lo << count);
			}

			public U128 ShiftRight(int count)
			{
				if (count == 0)
				{
					return this;
				}

				if (count >= 128)
				{
					return default(U128);
				}

				return count >= 64
					? new U128(0UL, Hi >> (count - 64))
					: new U128(Hi >> count, (Lo >> count) | (Hi << (64 - count)));
			}

			public static U128 Multiply(ulong x, ulong y)
			{
				ulong xLo = x & 0xFFFFFFFFUL, xHi = x >> 32, yLo = y & 0xFFFFFFFFUL, yHi = y >> 32;
				var p0 = xLo*yLo;
				var p1 = xLo*yHi;
				var p2 = xHi*yLo;
				var p3 = xHi*yHi;
				var middle = (p0 >> 32) + (p1 & 0xFFFFFFFFUL) + (p2 & 0xFFFFFFFFUL);
				return new U128(p3 + (p1 >> 32) + (p2 >> 32) + (middle >> 32), (middle << 32) | (p0 & 0xFFFFFFFFUL));
			}

			public static int Compare(U128 x, U128 y)
			{
				if (x.Hi != y.Hi)
				{
					return x.Hi < y.Hi ? -1 : 1;
				}

				return x.Lo < y.Lo ? -1 : (x.Lo > y.Lo ? 1 : 0);
			}

			public static U128 Add(U128 x, U128 y)
			{
				var lo = x.Lo + y.Lo;
				return new U128(x.Hi + y.Hi + (lo < x.Lo ? 1UL : 0UL), lo);
			}

			public static U128 Subtract(U128 x, U128 y)
			{
				return new U128(x.Hi - y.Hi - (x.Lo < y.Lo ? 1UL : 0UL), x.Lo - y.Lo);
			}

			public static bool TryMultiply(U128 x, uint factor, out U128 result)
			{
				var low = Multiply(x.Lo, factor);
				var high = Multiply(x.Hi, factor);
				var hi = high.Lo + low.Hi;
				result = new U128(hi, low.Lo);
				return high.Hi == 0UL && hi >= high.Lo;
			}

			/// <summary>
			/// Делит на 32-разрядное число и возвращает остаток.
			/// </summary>
			public static uint DivRem(U128 x, uint divisor, out U128 quotient)
			{
				var qHi = x.Hi/divisor;
				var rem = x.Hi%divisor;
				var middle = (rem << 32) | (x.Lo >> 32);
				var q1 = middle/divisor;
				rem = middle%divisor;
				var low = (rem << 32) | (x.Lo & 0xFFFFFFFFUL);
				var q0 = low/divisor;
				rem = low%divisor;
				quotient = new U128(qHi, (q1 << 32) | q0);
				return (uint) rem;
			}

			/// <summary>
			/// Делит на 128-разрядное число (деление столбиком по битам).
			/// </summary>
			public static U128 DivRem(U128 x, U128 divisor, out U128 remainder)
			{
				if (x.Hi == 0UL && divisor.Hi == 0UL)
				{
					remainder = new U128(0UL, x.Lo%divisor.Lo);
					return new U128(0UL, x.Lo/divisor.Lo);
				}

				var quotient = default(U128);
				var rem = default(U128);
				for (var i = x.BitLength - 1; i >= 0; i--)
				{
					var carry = (rem.Hi >> 63) != 0UL;
					rem = rem.ShiftLeft(1);
					if (x.TestBit(i))
					{
						rem = new U128(rem.Hi, rem.Lo | 1UL);
					}

					quotient = quotient.ShiftLeft(1);
					if (carry || Compare(rem, divisor) >= 0)
					{
						rem = Subtract(rem, divisor);
						quotient = new U128(quotient.Hi, quotient.Lo | 1UL);
					}
				}

				remainder = rem;
				return quotient;
			}

			public static U128 Gcd(U128 x, U128 y)
			{
				if (x.Hi == 0UL && y.Hi == 0UL)
				{
					return new U128(0UL, Fraction.Gcd(x.Lo, y.Lo));
				}

				if (x.IsZero)
				{
					return y;
				}

				if (y.IsZero)
				{
					return x;
				}

				//Бинарный алгоритм Евклида: только сдвиги и вычитания.
				var shift = Min(x.TrailingZeros, y.TrailingZeros);
				x = x.ShiftRight(x.TrailingZeros);
				do
				{
					y = y.ShiftRight(y.TrailingZeros);
					if (Compare(x, y) > 0)
					{
						var tmp = x;
						x = y;
						y = tmp;
					}

					y = Subtract(y, x);
				} while (!y.IsZero);

				return x.ShiftLeft(shift);
			}

			/// <summary>
			/// Вычисляет x·y mod modulus (x, y &lt; modulus &lt; 2^127) удвоением и сложением.
			/// </summary>
			public static U128 MultiplyMod(U128 x, U128 y, U128 modulus)
			{
				var result = default(U128);
				for (var i = y.BitLength - 1; i >= 0; i--)
				{
					result = AddMod(result, result, modulus);
					if (y.TestBit(i))
					{
						result = AddMod(result, x, modulus);
					}
				}

				return result;
			}

			private static U128 AddMod(U128 x, U128 y, U128 modulus)
			{
				var sum = Add(x, y);
				return Compare(sum, modulus) >= 0 ? Subtract(sum, modulus) : sum;
			}

			/// <summary>
			/// Вычисляет value^power mod modulus.
			/// </summary>
			public static U128 PowMod(uint value, long power, U128 modulus)
			{
				U128 result, factor;
				DivRem(new U128(0UL, 1UL), modulus, out result);
				DivRem(new U128(0UL, value), modulus, out factor);
				for (; power > 0L; power >>= 1)
				{
					if ((power & 1L) != 0L)
					{
						result = MultiplyMod(result, factor, modulus);
					}

					factor = MultiplyMod(factor, factor, modulus);
				}

				return result;
			}
		}

		/// <summary>
		/// Неотрицательное целое число произвольной длины (разряды по 32 бита, младшие первыми).
		/// </summary>
		private sealed class Big
		{
			private uint[] _digits;
			private int _length;

			public Big(ulong value)
			{
				_digits = new uint[4];
				_digits[0] = (uint) value;
				_digits[1] = (uint) (value >> 32);
				_length = 2;
				Trim();
			}

			public Big(U128 value)
			{
				_digits = new uint[6];
				_digits[0] = (uint) value.Lo;
				_digits[1] = (uint) (value.Lo >> 32);
				_digits[2] = (uint) value.Hi;
				_digits[3] = (uint) (value.Hi >> 32);
				_length = 4;
				Trim();
			}

			private Big(uint[] digits, int length)
			{
				_digits = digits;
				_length = length;
			}

			public int BitLength => _length == 0 ? 0 : 32*(_length - 1) + BitLength64(_digits[_length - 1]);

			public int TrailingZeroBits
			{
				get
				{
					for (var i = 0; i < _length; i++)
					{
						if (_digits[i] != 0u)
						{
							return 32*i + TrailingZeros64(_digits[i]);
						}
					}

					return 0;
				}
			}

			public bool TestBit(int index)
			{
				var i = index >> 5;
				return i < _length && ((_digits[i] >> (index & 31)) & 1u) != 0u;
			}

			public Big Clone()
			{
				var digits = new uint[_length + 2];
				Array.Copy(_digits, digits, _length);
				return new Big(digits, _length);
			}

			public ulong ToUInt64()
			{
				return (_length > 0 ? _digits[0] : 0UL) | (_length > 1 ? (ulong) _digits[1] << 32 : 0UL);
			}

			public U128 ToU128()
			{
				var hi = (_length > 2 ? _digits[2] : 0UL) | (_length > 3 ? (ulong) _digits[3] << 32 : 0UL);
				return new U128(hi, ToUInt64());
			}

			private void Reserve(int size)
			{
				if (_digits.Length >= size)
				{
					return;
				}

				var digits = new uint[Max(size, 2*_digits.Length)];
				Array.Copy(_digits, digits, _length);
				_digits = digits;
			}

			private void Trim()
			{
				while (_length > 0 && _digits[_length - 1] == 0u)
				{
					_length--;
				}
			}

			public void Multiply(uint factor)
			{
				var carry = 0UL;
				for (var i = 0; i < _length; i++)
				{
					var product = (ulong) _digits[i]*factor + carry;
					_digits[i] = (uint) product;
					carry = product >> 32;
				}

				if (carry != 0UL)
				{
					Reserve(_length + 1);
					_digits[_length++] = (uint) carry;
				}

				Trim();
			}

			public void Multiply(ulong factor)
			{
				var high = (uint) (factor >> 32);
				if (high == 0u)
				{
					Multiply((uint) factor);
					return;
				}

				var upper = Clone();
				upper.Multiply(high);
				upper.ShiftLeft(32L);
				Multiply((uint) factor);
				Add(upper);
			}

			public void MultiplyPow5(long count)
			{
				for (; count >= 13L; count -= 13L)
				{
					Multiply((uint) Pow5[13]);
				}

				if (count > 0L)
				{
					Multiply((uint) Pow5[count]);
				}
			}

			public void MultiplyPow10(long count)
			{
				for (; count >= 9L; count -= 9L)
				{
					Multiply((uint) Pow10[9]);
				}

				if (count > 0L)
				{
					Multiply((uint) Pow10[count]);
				}
			}

			public void DividePow5(long count)
			{
				for (; count >= 13L; count -= 13L)
				{
					DivRem((uint) Pow5[13]);
				}

				if (count > 0L)
				{
					DivRem((uint) Pow5[count]);
				}
			}

			public void ShiftLeft(long count)
			{
				if (_length == 0 || count == 0L)
				{
					return;
				}

				var limbs = (int) (count >> 5);
				var bits = (int) (count & 31L);
				Reserve(_length + limbs + 1);
				_digits[_length + limbs] = 0u;
				for (var i = _length - 1; i >= 0; i--)
				{
					if (bits != 0)
					{
						_digits[i + limbs + 1] |= _digits[i] >> (32 - bits);
						_digits[i + limbs] = _digits[i] << bits;
					}
					else
					{
						_digits[i + limbs] = _digits[i];
					}
				}

				for (var i = 0; i < limbs; i++)
				{
					_digits[i] = 0u;
				}

				_length += limbs + 1;
				Trim();
			}

			public void ShiftRight(long count)
			{
				if (count <= 0L)
				{
					return;
				}

				if (count >> 5 >= _length)
				{
					_length = 0;
					return;
				}

				var limbs = (int) (count >> 5);
				var bits = (int) (count & 31L);
				var length = _length - limbs;
				for (var i = 0; i < length; i++)
				{
					var value = _digits[i + limbs] >> bits;
					if (bits != 0 && i + limbs + 1 < _length)
					{
						value |= _digits[i + limbs + 1] << (32 - bits);
					}

					_digits[i] = value;
				}

				_length = length;
				Trim();
			}

			public uint DivRem(uint divisor)
			{
				var rem = 0UL;
				for (var i = _length - 1; i >= 0; i--)
				{
					var current = (rem << 32) | _digits[i];
					_digits[i] = (uint) (current/divisor);
					rem = current%divisor;
				}

				Trim();
				return (uint) rem;
			}

			public uint Mod(uint divisor)
			{
				var rem = 0UL;
				for (var i = _length - 1; i >= 0; i--)
				{
					rem = ((rem << 32) | _digits[i])%divisor;
				}

				return (uint) rem;
			}

			public ulong DivRem(ulong divisor)
			{
				if (divisor <= uint.MaxValue)
				{
					return DivRem((uint) divisor);
				}

				var rem = 0UL;
				for (var i = _length - 1; i >= 0; i--)
				{
					_digits[i] = (uint) DivRem128(rem >> 32, (rem << 32) | _digits[i], divisor, out rem);
				}

				Trim();
				return rem;
			}

			public ulong Mod(ulong divisor)
			{
				if (divisor <= uint.MaxValue)
				{
					return Mod((uint) divisor);
				}

				var rem = 0UL;
				for (var i = _length - 1; i >= 0; i--)
				{
					DivRem128(rem >> 32, (rem << 32) | _digits[i], divisor, out rem);
				}

				return rem;
			}

			public void Add(Big other)
			{
				var length = Max(_length, other._length);
				Reserve(length + 1);
				var carry = 0UL;
				for (var i = 0; i < length; i++)
				{
					var sum = carry + (i < _length ? _digits[i] : 0u) + (i < other._length ? other._digits[i] : 0u);
					_digits[i] = (uint) sum;
					carry = sum >> 32;
				}

				_digits[length] = (uint) carry;
				_length = length + 1;
				Trim();
			}

			/// <summary>
			/// Вычитает меньшее или равное число.
			/// </summary>
			public void Subtract(Big other)
			{
				var borrow = 0L;
				for (var i = 0; i < _length; i++)
				{
					var difference = (long) _digits[i] - (i < other._length ? other._digits[i] : 0u) - borrow;
					borrow = difference < 0L ? 1L : 0L;
					_digits[i] = (uint) (difference + (borrow << 32));
				}

				Trim();
			}

			public static int Compare(Big x, Big y)
			{
				if (x._length != y._length)
				{
					return x._length < y._length ? -1 : 1;
				}

				for (var i = x._length - 1; i >= 0; i--)
				{
					if (x._digits[i] != y._digits[i])
					{
						return x._digits[i] < y._digits[i] ? -1 : 1;
					}
				}

				return 0;
			}
		}

		#endregion
	}
}
