using System;
using Ruzil3D.Utility;
using static Ruzil3D.Math;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет дробное число.
	/// </summary>
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
			var sign = denominator < 0 ? -1 : 1;

			Numerator = sign*numerator;
			Denominator = sign*denominator;
			Order = 0;

			if (!SimplifyNaN() && simplify)
			{
				SimplifyNum();
			}
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
			var sign = denominator < 0 ? -1 : 1;

			Numerator = sign*numerator;
			Denominator = sign*denominator;
			Order = order;

			if (!SimplifyNaN() && simplify)
			{
				SimplifyNum();
			}
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

			return value.Order >= 0
				? value.Numerator*value.Exponent/value.Denominator
				: value.Numerator/Exp10(-value.Order)/value.Denominator;
		}

		/// <summary>
		/// Складывает два дробных числа.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator +(Fraction x, Fraction y)
		{
			var xNumerator = x.Order >= 0 ? (long) (x.Numerator*x.Exponent) : x.Numerator;
			var xDenominator = x.Order >= 0 ? x.Denominator : (long) (x.Denominator/x.Exponent);

			var yNumerator = y.Order >= 0 ? (long) (y.Numerator*y.Exponent) : y.Numerator;
			var yDenominator = y.Order >= 0 ? y.Denominator : (long) (y.Denominator/y.Exponent);

			return new Fraction(xNumerator*yDenominator + yNumerator*xDenominator, xDenominator*yDenominator, 0, true);

			/*
			var numerator = xNumerator * yDenominator + yNumerator * xDenominator;
			var denominator = xDenominator * yDenominator;

			if (numerator == 0L)
			{
				return denominator == 0L ? NaN : Empty;
			}

			return new Fraction(numerator, denominator, 0, true);
			
			*/
		}

		/// <summary>
		/// Складывает дробное число с 64-разрядным целым числом со знаком.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator +(Fraction x, long y)
		{
			var xNumerator = x.Order >= 0 ? (long) (x.Numerator*x.Exponent) : x.Numerator;
			var xDenominator = x.Order >= 0 ? x.Denominator : (long) (x.Denominator/x.Exponent);

			return new Fraction(xNumerator + y*xDenominator, xDenominator, 0, true);
		}

		/// <summary>
		/// Складывает 64-разрядное целое число со знаком с дробным числом.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator +(long x, Fraction y)
		{
			var yNumerator = y.Order >= 0 ? (long) (y.Numerator*y.Exponent) : y.Numerator;
			var yDenominator = y.Order >= 0 ? y.Denominator : (long) (y.Denominator/y.Exponent);

			return new Fraction(x*yDenominator + yNumerator, yDenominator, 0, true);
		}

		/// <summary>
		/// Вычитает дробное число из другого дробного числа.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator -(Fraction x, Fraction y)
		{
			//return x + -y;

			var xNumerator = x.Order >= 0 ? (long) (x.Numerator*x.Exponent) : x.Numerator;
			var xDenominator = x.Order >= 0 ? x.Denominator : (long) (x.Denominator/x.Exponent);

			var yNumerator = y.Order >= 0 ? (long) (y.Numerator*y.Exponent) : y.Numerator;
			var yDenominator = y.Order >= 0 ? y.Denominator : (long) (y.Denominator/y.Exponent);

			return new Fraction(xNumerator*yDenominator - yNumerator*xDenominator, xDenominator*yDenominator, 0, true);
		}

		/// <summary>
		/// Вычитает 64-разрядное целое число со знаком из дробного числа.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator -(Fraction x, long y)
		{
			var xNumerator = x.Order >= 0 ? (long) (x.Numerator*x.Exponent) : x.Numerator;
			var xDenominator = x.Order >= 0 ? x.Denominator : (long) (x.Denominator/x.Exponent);

			return new Fraction(xNumerator - y*xDenominator, xDenominator, 0, true);
		}

		/// <summary>
		/// Вычитает дробное число из 64-разрядного целого числа со знаком.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Fraction operator -(long x, Fraction y)
		{
			//return x + -y;

			var yNumerator = y.Order >= 0 ? (long) (y.Numerator*y.Exponent) : y.Numerator;
			var yDenominator = y.Order >= 0 ? y.Denominator : (long) (y.Denominator/y.Exponent);

			return new Fraction(x*yDenominator - yNumerator, yDenominator, 0, true);
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

			return new Fraction(x.Numerator*y.Numerator, x.Denominator*y.Denominator, x.Order + y.Order, true);

			/*
			var numerator = x.Numerator*y.Numerator;
			var denominator = x.Denominator*y.Denominator;

			if (numerator == 0L)
			{
				return denominator == 0L ? NaN : Empty;
			}
			
			return new Fraction(numerator, denominator, x.Order + y.Order, true);
			*/
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
			return new Fraction(x.Numerator*y, x.Denominator, x.Order, true);
		}

		/// <summary>
		/// Возвращает произведение 64-разрядного целого числа со знаком на дробное число.
		/// </summary>
		/// <param name="x">64-разрядное целое число со знаком для перемножения.</param>
		/// <param name="y">Дробное число для перемножения.</param>
		/// <returns>Произведение 64-разрядного целого числа со знаком на дробное число.</returns>
		public static Fraction operator *(long x, Fraction y)
		{
			return new Fraction(x*y.Numerator, y.Denominator, y.Order, true);
		}

		/// <summary>
		/// Делит одно дробное число на другое и возвращает результат.
		/// </summary>
		/// <param name="x">Дробное число - числитель.</param>
		/// <param name="y">Дробное число - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator /(Fraction x, Fraction y)
		{
			return new Fraction(x.Numerator*y.Denominator, x.Denominator*y.Numerator, x.Order - y.Order, true);

			/*
			var numerator = x.Numerator*y.Denominator;
			var denominator = x.Denominator*y.Numerator;
			var order = x.Order - y.Order;

			if (numerator == 0L)
			{
				return denominator == 0L ? NaN : Empty;
			}

			return new Fraction(numerator, denominator, order, true);
			*/
		}

		/// <summary>
		/// Делит дробное число на 64-разрядное целое число со знаком.
		/// </summary>
		/// <param name="x">Дробное число-числитель.</param>
		/// <param name="y">64-разрядное целое число со знаком - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator /(Fraction x, long y)
		{
			return new Fraction(x.Numerator, x.Denominator*y, x.Order, true);
		}

		/// <summary>
		/// Делит 64-разрядное целое число со знаком на дробное число.
		/// </summary>
		/// <param name="x">64-разрядное целое число со знаком - числитель.</param>
		/// <param name="y">Дробное число - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator /(long x, Fraction y)
		{
			return new Fraction(x*y.Numerator, y.Denominator, -y.Order, true);
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
		/// <returns>Остаток от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator %(Fraction x, Fraction y)
		{
			var div = (long) ((double) x/(double) y);
			return x - div*y;
		}

		/// <summary>
		/// Вычисляет остаток после деления дробного числа на 64-разрядное целое число со знаком.
		/// </summary>
		/// <param name="x">Дробное число - числитель.</param>
		/// <param name="y">64-разрядное целое число со знаком - знаменатель.</param>
		/// <returns>Остаток от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator %(Fraction x, long y)
		{
			var div = (long) ((double) x/y);
			return x - div*y;
		}

		/// <summary>
		/// Вычисляет остаток после деления 64-разрядного целого числа со знаком на дробное число.
		/// </summary>
		/// <param name="x">64-разрядное целое число со знаком - числитель.</param>
		/// <param name="y">Дробное число - знаменатель.</param>
		/// <returns>Остаток от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Fraction operator %(long x, Fraction y)
		{
			var div = (long) (x/(double) y);
			return x - div*y;
		}

		/// <summary>
		/// Возвращает значение указывающее что первое дробное число больше или равно второму.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд больше или равен второму, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator >=(Fraction x, Fraction y)
		{
			return (double) x >= (double) y;

			/*
            if (IsNaN(x) || IsNaN(y))
            {
                return false;
            }

            double compare1 = x.Numerator * y.Denominator;
            double compare2 = y.Numerator * x.Denominator;

            if (x.Order > 0)
            {
                compare1 *= x.Exponent;
            }
            else
            {
                compare2 /= x.Exponent;
            }

            if (y.Order > 0)
            {
                compare2 *= y.Exponent;
            }
            else
            {
                compare1 /= y.Exponent;
            }

            return compare1 >= compare2;
            */
		}

		/// <summary>
		/// Возвращает значение указывающее что первое дробное число меньше или равно второму.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд меньше или равен второму, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator <=(Fraction x, Fraction y)
		{
			return (double) y >= (double) x;
		}

		/// <summary>
		/// Возвращает значение указывающее что первое дробное число больше второго.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд больше второго, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator >(Fraction x, Fraction y)
		{
			return (double) x > (double) y;
		}

		/// <summary>
		/// Возвращает значение указывающее что первое дробное число меньше второго.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд меньше второго, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator <(Fraction x, Fraction y)
		{
			return (double) x < (double) y;
		}

		/// <summary>
		/// Возвращает значение указывающее на равенство дробных чисел.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд равен второму, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator ==(Fraction x, Fraction y)
		{
			return ((double) x).Equals(y);
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
		/// <param name="order">Число десятичных знаков на которое нужно учитывать.</param>
		/// <returns>Дробное число численно близкое к параметру <paramref name="value"/>.</returns>
		public static Fraction Parse(double value, int order = 15)
		{
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

			var sign = Sign(value);
			value *= sign;


			var doubleOrder = GetOrder(value);
			var orderValue = (int) Floor(doubleOrder) + 1;

			if (0D.Equals(doubleOrder%1))
			{
				//0.099999999999999992 => doubleOrder == 0 => 0.1;
				return new Fraction(sign, 1, orderValue - 1);
			}

			var exponent = Exp10(order - orderValue);
			var longValue = (long) Floor(value*exponent);

			var num = longValue;
			var digits = new int[order];
			for (var i = digits.Length - 1; i >= 0; i--)
			{
				var digit = num%10;
				digits[i] = (int) digit;
				num /= 10;
			}

			int start = digits.Length, len = 0;
			var lastIdx = digits.Length - 1;
			var last = digits[lastIdx];
			for (var i = 0; i < lastIdx; i++)
			{
				if (digits[i] != last)
				{
					continue;
				}

				var offset = 1;
				while (offset <= i)
				{
					if (digits[i - offset] != digits[lastIdx - offset])
					{
						break;
					}
					offset++;
				}

				var s = i - offset + 1;
				var l = lastIdx - i;

				if (s <= start || l <= len)
				{
					start = s;
					len = l;
				}
			}

			var exponentTail = (long)Exp10(order - start - len);
			var exponentPeriod = (long)Exp10(len);

			long period;
			var fraction = DivRem(longValue/exponentTail, exponentPeriod, out period);

			return fraction == 0L 
				? FromPeriodic(sign*period, period, len, len, orderValue) 
				: FromPeriodic(sign*fraction, period, start, len, orderValue);
		}

		/// <summary>
		/// Генерирует дробь из периодической десятичной дроби.
		/// </summary>
		/// <param name="fraction">Предпериод дроби.</param>
		/// <param name="period">Период дроби.</param>
		/// <param name="orderFraction">Порядок предпериода.</param>
		/// <param name="orderPeriod">Порядок периода.</param>
		/// <param name="order">Порядок числа.</param>
		/// <returns></returns>
		private static Fraction FromPeriodic(long fraction, long period, int orderFraction, int orderPeriod, int order)
		{
			if (period < 0L)
			{
				throw new ArgumentException();
			}

			var exponentPeriod = Exp10(orderPeriod);

			if (fraction == 0L)
			{
				return orderPeriod == 0 ? Empty : new Fraction(period, (long) (exponentPeriod - 1), order, true);
			}

			var sign = Sign(fraction);
			fraction *= sign;

			if (fraction == period && orderFraction == orderPeriod)
			{
				return new Fraction(sign*period, (long) (exponentPeriod - 1), order, true);
			}

			if (period == 0L)
			{
				return new Fraction(fraction*sign, 1, order - orderFraction, true);
			}

			var exponentFraction = Exp10(orderFraction);
			var numerator = fraction*exponentPeriod + period - fraction;
			var denominator = (exponentPeriod - 1)*exponentFraction;
			return new Fraction((long) numerator*sign, (long) denominator, order, true);
		}

		private static bool IsPow(long pow, long x)
		{
			while (pow != 1L && pow%x == 0L)
			{
				pow = pow/x;
			}

			return pow == 1L;
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
			var numerator = Numerator;
			var denominator = Denominator;
			var order = Order;

			Math.Simplify(ref numerator, ref denominator);

			while (order > 0 && numerator <= Max10)
			{
				order--;
				numerator *= 10L;
				Math.Simplify(ref numerator, ref denominator);
			}

			while (order < 0 && denominator <= Max10)
			{
				order++;
				denominator *= 10L;
				Math.Simplify(ref numerator, ref denominator);
			}

			while (numerator%10 == 0L)
			{
				numerator /= 10L;
				order++;
			}

			while (denominator%10 == 0L)
			{
				denominator /= 10L;
				order--;
			}

			Numerator = numerator;
			Denominator = denominator;
			Order = order;

			/*

			double numerator = Numerator;
			double denominator = Denominator;

			if (Order > 0)
			{
				numerator *= Exp10(Order);
			}
			else
			{
				denominator *= Exp10(-Order);
			}

			Math.Simplify(ref numerator, ref denominator);

			var order = 0;

			while (0D.Equals(numerator % 10))
			{
				numerator /= 10;
				order++;
			}

			while (0D.Equals(denominator % 10) || denominator > long.MaxValue)
			{
				denominator /= 10;
				order--;
			}

			Numerator = (long) numerator;
			Denominator = (long) denominator;
			Order = order;

			*/
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другое дробное число.</param>
		/// <returns>Значение <b>true</b>, если два числа равны; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Fraction other)
		{
			return this == other;
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
			return 0;
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
		/// Больше нуля: Этот экземпляр больше параметра <paramref name="other"/>.
		/// </returns>
		/// <param name="other">Объект для сравнения с данным экземпляром.</param>
		public int CompareTo(Fraction other)
		{
			return ((double) this).CompareTo(other);
		}

		/// <summary>
		/// Сравнивает текущий экземпляр с другим объектом того же типа и возвращает целое число, которое показывает, расположен ли текущий экземпляр перед, после или на той же позиции в порядке сортировки, что и другой объект.
		/// </summary>
		/// <returns>
		/// 32-битовое целое число со знаком, указывающее, каков относительный порядок сравниваемых объектов. Возвращаемые значения представляют следующие результаты сравнения:
		/// Меньше нуля: Этот экземпляр меньше параметра <paramref name="obj"/>.<br/>
		/// Нуль: Этот экземпляр и параметр <paramref name="obj"/> равны.<br/>
		/// Больше нуля: Этот экземпляр больше параметра <paramref name="obj"/>.
		/// </returns>
		/// <param name="obj">Объект для сравнения с данным экземпляром.</param>
		/// <exception cref="ArgumentException">Тип значения параметра <paramref name="obj"/> отличается от типа данного экземпляра.</exception>
		public int CompareTo(object obj)
		{
			return ((double) this).CompareTo((Fraction) obj);
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
		/// Строковое представлеине исходной дроби.
		/// </returns>
		/// <remarks>
		/// <code>
		/// var frac = new Fraction(3, -7, -6);
		/// Console.Write(frac); //Результат: -3/7·10⁻⁶
		/// frac = (Fraction)6/18;
		/// Console.Write(frac); //Результат: 1/3
		/// frac = (Fraction) 1777.7777777777777;
		/// Console.Write(frac); //Результат: 16/9·10³
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

			//var equal = 0D.Equals(Log(den, 2)%1) || 0D.Equals(Log(den, 5)%1);
			var equal = IsPow(den, 2) || IsPow(den, 5);
			var suff = (equal ? " = " : " ≈ ") + ((double) frac).ToString("R");

			//Math.Round(Math.Ln(den) / Math.Ln(2)) == Math.Ln(den) / Math.Ln(2)

			switch (ord)
			{
				case 1:
					num *= 10;
					ord = 0;
					break;
				case -1:
					den *= 10;
					ord = 0;
					break;
			}

			var expText = "10" + CStatic.GetIndex(frac.Order, true);

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
	}
}