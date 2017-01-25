using System;
using Ruzil3D.Algebra;

namespace Ruzil3D
{
	/// <summary>
	/// Предоставляет константы и статические методы для тригонометрических, логарифмических и иных общих математических функций.
	/// </summary>
	public static class Math
	{
		#region Consts

		//public static Func<double, double> SQRT = System.Math.Sqrt;

		/// <summary>
		/// Представляет константу ⅓.
		/// </summary>
		public const double Third = 1D/3D;

		/// <summary>
		/// Представляет квадратный корень из числа 2.
		/// </summary>
		public const double Sqrt2 = 1.4142135623730950488;

		/// <summary>
		/// Представляет квадратный корень из числа 3.
		/// </summary>
		public const double Sqrt3 = 1.7320508075688772935;

		/// <summary>
		/// Представляет квадратный корень из константы π.
		/// </summary>
		public const double SqrtPi = 1.7724538509055160272;

		/// <summary>
		/// Представляет пропорцию золотого сечения определяемое константой φ.
		/// </summary>
		public const double Fi = 1.6180339887498948482;

		/// <summary>
		/// Представляет отношение длины окружности к ее диаметру, определяемое константой π.
		/// </summary>
		public const double Pi = 3.14159265358979323846;

		/// <summary>
		/// Представляет число 2π определяемое константой τ.
		/// </summary>
		public const double Tau = 2*Pi;

		/// <summary>
		/// Представляет основание натурального логарифма, определяемое константой e.
		/// </summary>
		public const double E = 2.71828182845904523536;

		#endregion

		#region Import

		/// <summary>
		/// Возвращает синус указанного угла.
		/// </summary>
		/// <param name="x"> Угол, измеряемый в радианах.</param>
		/// <returns>Синус <paramref name="x"/>. Если значение параметра <paramref name="x"/> равно <see cref="double.NaN"/>, <see cref="double.NegativeInfinity"/> или <see cref="double.PositiveInfinity"/>, то данный метод возвращает <see cref="double.NaN"/>.</returns>
		public static double Sin(double x) => System.Math.Sin(x);

		/// <summary>
		/// Возвращает косинус указанного угла.
		/// </summary>
		/// <param name="x">Угол, измеряемый в радианах.</param>
		/// <returns>Косинус x.</returns>
		public static double Cos(double x) => System.Math.Cos(x);

		/// <summary>
		/// Возвращает тангенс указанного угла.
		/// </summary>
		/// <param name="x">Угол, измеряемый в радианах.</param>
		/// <returns>Тангенс <paramref name="x"/>. Если значение параметра <paramref name="x"/> равно <see cref="double.NaN"/>,<see cref="double.NegativeInfinity"/> или <see cref="double.PositiveInfinity"/>, то данный метод возвращает <see cref="double.NaN"/>.</returns>
		public static double Tan(double x) => System.Math.Tan(x);

		/// <summary>
		/// Возвращает угол, косинус которого равен указанному числу.
		/// </summary>
		/// <param name="x"> Число, представляющее косинус, где -1 ≤ <paramref name="x"/> ≤ 1.</param>
		/// <returns>Угол, θ, измеренный в радианах, такой, что 0 ≤ θ ≤π или <see cref="double.NaN"/>, если <paramref name="x"/> &lt; -1 или <paramref name="x"/> &gt; 1.</returns>
		public static double Acos(double x) => System.Math.Acos(x);

		/// <summary>
		/// Возвращает угол, тангенс которого равен отношению двух указанных чисел.
		/// </summary>
		/// <param name="y">Координата y точки.</param>
		/// <param name="x">Координата х точки.</param>
		/// <returns>Угол, θ, измеренный в радианах, такой что -π ≤ θ ≤ π, и тангенс(θ) = <paramref name="y"/> / <paramref name="x"/>, где (<paramref name="x"/>, <paramref name="y"/>) — это точка в декартовой системе координат. Обратите внимание на следующее. Для (<paramref name="x"/>, <paramref name="y"/>) в квадранте 1, 0 &lt; θ &lt; π/2. Для (<paramref name="x"/>, <paramref name="y"/>) в квадранте 2, π/2 &lt; θ ≤ π. Для (<paramref name="x"/>, <paramref name="y"/>) в квадранте 3, -π &lt; θ &lt; -π/2. Для (<paramref name="x"/>, <paramref name="y"/>) в квадранте 4, -π/2 &lt; θ &lt; 0. Для точек за пределами указанных квадрантов возвращаемое значение указано ниже. Если <paramref name="y"/> равно 0 и <paramref name="x"/> не является отрицательным, θ = 0. Если <paramref name="y"/> равно 0 и <paramref name="x"/> является отрицательным, θ = π. Если <paramref name="y"/> является положительным и <paramref name="x"/> равно 0, θ = π/2. Если <paramref name="y"/> является отрицательным и <paramref name="x"/> равно 0, θ = -π/2.</returns>
		public static double Atan2(double y, double x) => System.Math.Atan2(y, x);

		/// <summary>
		/// Возвращает значение, определяющее знак числа двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="x">Число со знаком.</param>
		/// <returns>Число, определяющее знак <paramref name="x"/>. Число Описание -1 Значение параметра <paramref name="x"/> меньше нуля. 0 Значение параметра <paramref name="x"/> равно нулю. 1 Значение параметра <paramref name="x"/> больше нуля.</returns>
		/// <exception cref="ArithmeticException">Параметр value имеет значение <see cref="double.NaN"/>.</exception>
		public static int Sign(double x) => System.Math.Sign(x);

		/// <summary>
		/// Возвращает указанное число, возведенное в указанную степень.
		/// </summary>
		/// <param name="x">Число двойной точности с плавающей запятой, возводимое в степень.</param>
		/// <param name="y">Число двойной точности с плавающей запятой, задающее степень.</param>
		/// <returns>Число <paramref name="x"/>, возведенное в степень <paramref name="y"/>.</returns>
		public static double Pow(double x, double y) => System.Math.Pow(x, y);
		
		/// <summary>
		/// Возвращает указанное число, возведенное в указанную степень.
		/// </summary>
		/// <param name="x">64-битовое целове число со знаком, возводимое в степень.</param>
		/// <param name="y">32-битовое целове число со знаком, задающее степень.</param>
		/// <returns>Число <paramref name="x"/>, возведенное в степень <paramref name="y"/>.</returns>
		public static long Pow(long x, int y)
		{
			if (y < 0)
			{
				switch (x)
				{
					case -1:
						return y % 2 == 0 ? 1 : -1;

					case 1:
						return 1;

					default:
						throw new ArithmeticException();
				}
			}

			if (y == 0)
			{
				if (x == 0L)
				{
					throw new ArithmeticException();
				}

				return 1;
			}

			switch (x)
			{
				case -1:
					return y%2 == 0 ? 1 : -1;

				case 0:
					return 0;

				case 1:
					return 1;

				default:
					if (y > 63)
					{
						throw new ArithmeticException();
					}

					var result = x;
					for (var i = 1; i < y; i++)
					{
						result *= x;
					}
					return result;
			}
		}

		/// <summary>
		/// Возвращает e, возведенное в указанную степень.
		/// </summary>
		/// <param name="x">Число, определяющее степень.</param>
		/// <returns>Число e, возведенное в степень <paramref name="x"/>. Если значение параметра <paramref name="x"/> равно <see cref="double.NaN"/> или <see cref="double.PositiveInfinity"/>, возвращается это значение. Если значение параметра <paramref name="x"/> равно <see cref="double.NegativeInfinity"/>, возвращается значение 0.</returns>
		public static double Exp(double x) => System.Math.Exp(x);

		/// <summary>
		/// Возвращает квадратный корень из указанного числа.
		/// </summary>
		/// <param name="x">Число.</param>
		/// <returns>Rвадратный корень из числа <paramref name="x"/>.</returns>
		public static double Sqrt(double x) => System.Math.Sqrt(x);

		/// <summary>
		/// Возвращает натуральный логарифм (с основанием e) указанного числа.
		/// </summary>
		/// <param name="x">Число, логарифм которого должен быть найден.</param>
		/// <returns>Натуральный логарифм (с основанием e) числа <paramref name="x"/>.</returns>
		public static double Ln(double x) => System.Math.Log(x);

		/// <summary>
		/// Возвращает десятичный логарифм (с основанием 10) указанного числа.
		/// </summary>
		/// <param name="x">Число, логарифм которого должен быть найден.</param>
		/// <returns>Десятичный логарифм (с основанием 10) числа <paramref name="x"/>.</returns>
		public static double Lg(double x) => System.Math.Log10(x);

		/// <summary>
		/// Возвращает логорифм числа с указанным основанием.
		/// </summary>
		/// <param name="x">Число, логарифм которого должен быть найден.</param>
		/// <param name="newBase">Основание логорифма.</param>
		/// <returns>Логорифм числа <paramref name="x"/> с основанием <paramref name="newBase"/>.</returns>
		public static double Log(double x, double newBase) => System.Math.Log(x, newBase);

		/// <summary>
		/// Вычисляет целую часть заданного числа двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="x">Усекаемое число.</param>
		/// <returns>Целая часть <paramref name="x"/>; то есть, число, остающееся после отбрасывания дробной части.</returns>
		public static double Truncate(double x) => System.Math.Truncate(x);

		/// <summary>
		/// Возвращает наибольшее целое число, которое меньше или равно заданному числу двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="x">Число двойной точности с плавающей запятой.</param>
		/// <returns>Наибольшее целое число, которое меньше или равно <paramref name="x"/>.</returns>
		public static double Floor(double x) => System.Math.Floor(x);
		
		/// <summary>
		/// Возвращает наименьшее целое число, которое меньше или равно заданному числу двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="x">Число двойной точности с плавающей запятой.</param>
		/// <returns>Наименьшее целое число, которое меньше или равно <paramref name="x"/>.</returns>
		public static double Ceiling(double x) => System.Math.Ceiling(x);

		/// <summary>
		/// Вычисляет частное двух 32-битовых целых чисел со знаком и возвращает остаток в выходном параметре.
		/// </summary>
		/// <param name="x">Значение типа <see cref="int"/>, содержащее делимое.</param>
		/// <param name="y">Значение типа <see cref="int"/>, содержащее делитель.</param>
		/// <param name="result">Значение типа <see cref="int"/>, представляющее полученный остаток.</param>
		/// <returns>Значение типа <see cref="int"/>, содержащее частное указанных чисел.</returns>
		/// <exception cref="DivideByZeroException">Значение параметра <paramref name="y"/> равно нулю.</exception>
		public static int DivRem(int x, int y, out int result) => System.Math.DivRem(x, y, out result);

		/// <summary>
		/// Вычисляет частное двух 64-битовых целых чисел со знаком и возвращает остаток в выходном параметре.
		/// </summary>
		/// <param name="x">Значение типа <see cref="long"/>, содержащее делимое.</param>
		/// <param name="y">Значение типа <see cref="long"/>, содержащее делитель.</param>
		/// <param name="result">Значение типа <see cref="long"/>, представляющее полученный остаток.</param>
		/// <returns>Значение типа <see cref="long"/>, содержащее частное указанных чисел.</returns>
		/// <exception cref="DivideByZeroException">Значение параметра <paramref name="y"/> равно нулю.</exception>
		public static long DivRem(long x, long y, out long result) => System.Math.DivRem(x, y, out result);

		#endregion

		#region Min, Max, Abs

		/// <summary>
		/// Возвращает большее из двух чисел двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых чисел двойной точности с плавающей запятой.</param>
		/// <param name="val2">Второе из двух сравниваемых чисел двойной точности с плавающей запятой.</param>
		/// <returns>Большее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>. Если хотя-бы один из параметров <paramref name="val1"/>, <paramref name="val2"/> равны <see cref="double.NaN"/>, возвращается значение <see cref="double.NaN"/>.</returns>
		public static double Max(double val1, double val2)
		{
			if (double.IsNaN(val1) || double.IsNaN(val2))
			{
				return double.NaN;
			}

			return val1 > val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает большее из двух чисел одинарной точности с плавающей запятой.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых чисел одинарной точности с плавающей запятой.</param>
		/// <param name="val2">Второе из двух сравниваемых чисел одинарной точности с плавающей запятой.</param>
		/// <returns>Большее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>. Если хотя-бы один из параметров <paramref name="val1"/>, <paramref name="val2"/> равны <see cref="float.NaN"/>, возвращается значение <see cref="float.NaN"/>.</returns>
		public static float Max(float val1, float val2)
		{
			if (float.IsNaN(val1) || float.IsNaN(val2))
			{
				return float.NaN;
			}

			return val1 > val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает большее из двух 32-битовых целых чисел со знаком.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых 32-битовых целых чисел со знаком.</param>
		/// <param name="val2">Второе из двух сравниваемых 32-битовых целых чисел со знаком.</param>
		/// <returns>Большее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>.</returns>
		public static int Max(int val1, int val2)
		{
			return val1 > val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает большее из двух 64-битовых целых чисел со знаком.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых 64-битовых целых чисел со знаком.</param>
		/// <param name="val2">Второе из двух сравниваемых 64-битовых целых чисел со знаком.</param>
		/// <returns>Большее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>.</returns>
		public static long Max(long val1, long val2)
		{
			return val1 > val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает большее из двух 8-битовых целых чисел со знаком.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых 8-битовых целых чисел со знаком.</param>
		/// <param name="val2">Второе из двух сравниваемых 8-битовых целых чисел со знаком.</param>
		/// <returns>Большее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>.</returns>
		public static sbyte Max(sbyte val1, sbyte val2)
		{
			return val1 > val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает большее из двух дробных чисел.
		/// </summary>
		/// <param name="val1">Первое из двух дробных чисел.</param>
		/// <param name="val2">Второе из двух дробных чисел.</param>
		/// <returns>Большее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>. Если хотя-бы один из параметров <paramref name="val1"/>, <paramref name="val2"/> равны <see cref="Fraction.NaN"/>, возвращается значение <see cref="Fraction.NaN"/>.</returns>
		public static Fraction Max(Fraction val1, Fraction val2)
		{
			if (Fraction.IsNaN(val1) || Fraction.IsNaN(val2))
			{
				return Fraction.NaN;
			}

			return val1 > val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает меньшее из двух чисел двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых чисел двойной точности с плавающей запятой.</param>
		/// <param name="val2">Второе из двух сравниваемых чисел двойной точности с плавающей запятой.</param>
		/// <returns>Меньшее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>. Если хотя-бы один из параметров <paramref name="val1"/>, <paramref name="val2"/> равны <see cref="double.NaN"/>, возвращается значение <see cref="double.NaN"/>.</returns>
		public static double Min(double val1, double val2)
		{
			if (double.IsNaN(val1) || double.IsNaN(val2))
			{
				return double.NaN;
			}

			return val1 < val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает меньшее из двух чисел одинарной точности с плавающей запятой.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых чисел одинарной точности с плавающей запятой.</param>
		/// <param name="val2">Второе из двух сравниваемых чисел одинарной точности с плавающей запятой.</param>
		/// <returns>Меньшее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>. Если хотя-бы один из параметров <paramref name="val1"/>, <paramref name="val2"/> равны <see cref="float.NaN"/>, возвращается значение <see cref="float.NaN"/>.</returns>
		public static float Min(float val1, float val2)
		{
			if (float.IsNaN(val1) || float.IsNaN(val2))
			{
				return float.NaN;
			}

			return val1 < val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает меньшее из двух 32-битовых целых чисел со знаком.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых 32-битовых целых чисел со знаком.</param>
		/// <param name="val2">Второе из двух сравниваемых 32-битовых целых чисел со знаком.</param>
		/// <returns>Меньшее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>.</returns>
		public static int Min(int val1, int val2)
		{
			return val1 < val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает меньшее из двух 64-битовых целых чисел со знаком.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых 64-битовых целых чисел со знаком.</param>
		/// <param name="val2">Второе из двух сравниваемых 64-битовых целых чисел со знаком.</param>
		/// <returns>Меньшее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>.</returns>
		public static long Min(long val1, long val2)
		{
			return val1 < val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает меньшее из двух 8-битовых целых чисел со знаком.
		/// </summary>
		/// <param name="val1">Первое из двух сравниваемых 8-битовых целых чисел со знаком.</param>
		/// <param name="val2">Второе из двух сравниваемых 8-битовых целых чисел со знаком.</param>
		/// <returns>Меньшее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>.</returns>
		public static sbyte Min(sbyte val1, sbyte val2)
		{
			return val1 < val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает меньшее из двух дробных чисел.
		/// </summary>
		/// <param name="val1">Первое из двух дробных чисел.</param>
		/// <param name="val2">Второе из двух дробей.</param>
		/// <returns>Меньшее из значений двух параметров, <paramref name="val1"/> или <paramref name="val2"/>. Если хотя-бы один из параметров <paramref name="val1"/>, <paramref name="val2"/> равны <see cref="Fraction.NaN"/>, возвращается значение <see cref="Fraction.NaN"/>.</returns>
		public static Fraction Min(Fraction val1, Fraction val2)
		{
			if (Fraction.IsNaN(val1) || Fraction.IsNaN(val2))
			{
				return Fraction.NaN;
			}

			return val1 < val2 ? val1 : val2;
		}

		/// <summary>
		/// Возвращает абсолютное значение числа двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="value">Число в диапазоне <see cref="double.MinValue"/> ≤ value ≤ <see cref="double.MaxValue"/>.</param>
		/// <returns>Число двойной точности с плавающей запятой, х, такое, что 0 ≤ x ≤ <see cref="double.MaxValue"/>.</returns>
		public static double Abs(double value)
		{
			return value > 0 ? value : -value;
		}

		/// <summary>
		/// Возвращает абсолютное значение числа одинарной точности с плавающей запятой.
		/// </summary>
		/// <param name="value">Число в диапазоне <see cref="float.MinValue"/> ≤ value ≤ <see cref="float.MaxValue"/>.</param>
		/// <returns>Число одинарной точности с плавающей запятой, х, такое, что 0 ≤ x ≤ <see cref="float.MaxValue"/>.</returns>
		public static float Abs(float value)
		{
			return value > 0 ? value : -value;
		}

		/// <summary>
		/// Возвращает абсолютное значение 32-битового целого числа со знаком.
		/// </summary>
		/// <param name="value">Число в диапазоне <see cref="int.MinValue"/> &lt; value ≤ <see cref="int.MaxValue"/>.</param>
		/// <returns>32-битовое знаковое целое число, х, такое, что 0 ≤ x ≤ <see cref="int.MaxValue"/>.</returns>
		/// <exception cref="OverflowException">Значение параметра value равно <see cref="int.MinValue"/>.</exception>
		public static int Abs(int value)
		{
			if (value == int.MinValue)
			{
				throw new OverflowException();
			}

			return value > 0 ? value : -value;
		}

		/// <summary>
		/// Возвращает абсолютное значение 64-битового целого числа со знаком.
		/// </summary>
		/// <param name="value">Число в диапазоне <see cref="long.MinValue"/> &lt; value ≤ <see cref="long.MaxValue"/>.</param>
		/// <returns>64-битовое знаковое целое число, х, такое, что 0 ≤ x ≤ <see cref="long.MaxValue"/>.</returns>
		/// <exception cref="OverflowException">Значение параметра value равно <see cref="long.MinValue"/>.</exception>
		public static long Abs(long value)
		{
			if (value == long.MinValue)
			{
				throw new OverflowException();
			}

			return value > 0 ? value : -value;
		}

		/// <summary>
		/// Возвращает абсолютное значение 8-битового целого числа со знаком.
		/// </summary>
		/// <param name="value">Число в диапазоне <see cref="sbyte.MinValue"/> &lt; value ≤ <see cref="sbyte.MaxValue"/>.</param>
		/// <returns>8-битовое знаковое целое число, х, такое, что 0 ≤ x ≤ <see cref="sbyte.MaxValue"/>.</returns>
		/// <exception cref="OverflowException">Значение параметра value равно <see cref="sbyte.MinValue"/>.</exception>
		public static sbyte Abs(sbyte value)
		{
			if (value == sbyte.MinValue)
			{
				throw new OverflowException();
			}

			return value > 0 ? value : (sbyte) -value;
		}

		/// <summary>
		/// Возвращает абсолютное значение дроби.
		/// </summary>
		/// <param name="value">Дробь.</param>
		/// <returns>Абсолютное значение дроби.</returns>
		public static Fraction Abs(Fraction value)
		{
			return new Fraction(Abs(value.Numerator), Abs(value.Denominator), value.Order);
		}

		#endregion

		#region Simplify Items

		/// <summary>
		/// Делит два 32-битовые целые числа со знаком на их наибольший общий делитель.
		/// </summary>
		/// <param name="numerator">Первое число.</param>
		/// <param name="divider">Второе число.</param>
		public static void Simplify(ref int numerator, ref int divider)
		{
			var div = GreatestDivisor(numerator, divider);
			numerator /= div;
			divider /= div;
		}

		/// <summary>
		/// Делит два 64-битовые целые числа со знаком на их наибольший общий делитель.
		/// </summary>
		/// <param name="numerator">Первое число.</param>
		/// <param name="divider">Второе число.</param>
		public static void Simplify(ref long numerator, ref long divider)
		{
			var div = GreatestDivisor(numerator, divider);
			numerator /= div;
			divider /= div;
		}

		/// <summary>
		/// Делит два числа двойной точности с плавающей запятой на их наибольший общий делитель.
		/// </summary>
		/// <param name="numerator">Первое число.</param>
		/// <param name="divider">Второе число.</param>
		public static void Simplify(ref double numerator, ref double divider)
		{
			var div = GreatestDivisor(numerator, divider);
			numerator /= div;
			divider /= div;
		}

		/// <summary>
		/// Находит наибольший общий делитель двух 32-битовых целых чисел со знаком. 
		/// </summary>
		/// <param name="num1">Первое число.</param>
		/// <param name="num2">Второе число.</param>
		/// <returns>Наибольший общий делитель.</returns>
		public static int GreatestDivisor(int num1, int num2)
		{
			num1 = Abs(num1);
			num2 = Abs(num2);

			if (num1.Equals(0))
			{
				if (num2.Equals(0)) throw new ArgumentException();
				return num2;
			}

			if (num2.Equals(0))
			{
				return num1;
			}

			var n1 = num1 > num2 ? num1 : num2;
			var n2 = num1 < num2 ? num1 : num2;

			do
			{
				var rem = n1%n2;
				n1 = n2;
				n2 = rem;
			} while (n2 != 0);

			return n1;
		}

		/// <summary>
		/// Находит наибольший общий делитель двух 64-битовых целых чисел со знаком. 
		/// </summary>
		/// <param name="num1">Первое число.</param>
		/// <param name="num2">Второе число.</param>
		/// <returns>Наибольший общий делитель.</returns>
		public static long GreatestDivisor(long num1, long num2)
		{
			num1 = Abs(num1);
			num2 = Abs(num2);

			if (num1.Equals(0L))
			{
				if (num2.Equals(0L)) throw new ArgumentException();
				return num2;
			}

			if (num2.Equals(0L))
			{
				return num1;
			}

			var n1 = num1 > num2 ? num1 : num2;
			var n2 = num1 < num2 ? num1 : num2;

			do
			{
				var rem = n1%n2;
				n1 = n2;
				n2 = rem;
			} while (n2 != 0);

			return n1;
		}

		/// <summary>
		/// Находит наибольший общий делитель двух чисел двойной точности с плавающей запятой. 
		/// </summary>
		/// <param name="num1">Первое число.</param>
		/// <param name="num2">Второе число.</param>
		/// <returns>Наибольший общий делитель.</returns>
		public static double GreatestDivisor(double num1, double num2)
		{
			num1 = Abs(num1);
			num2 = Abs(num2);

			if (num1.Equals(0))
			{
				return num2.Equals(0) ? double.NaN : num2;
			}

			if (num2.Equals(0))
			{
				return num1;
			}

			var n1 = num1 > num2 ? num1 : num2;
			var n2 = num1 < num2 ? num1 : num2;

			do
			{
				var rem = n1%n2;
				n1 = n2;
				n2 = rem;
			} while (!n2.Equals(0D));

			return n1;
		}

		/// <summary>
		/// Находит наибольший общий делитель двух дробных чисел.
		/// </summary>
		/// <param name="num1">Первое число.</param>
		/// <param name="num2">Второе число.</param>
		/// <returns>Наибольший общий делитель.</returns>
		public static Fraction GreatestDivisor(Fraction num1, Fraction num2)
		{
			num1 = Abs(num1);
			num2 = Abs(num2);

			if (num1.Equals(Fraction.Empty))
			{
				if (num2.Equals(Fraction.Empty)) throw new ArgumentException();
				return num2;
			}

			if (num2.Equals(Fraction.Empty))
			{
				return num1;
			}

			var n1 = num1 > num2 ? num1 : num2;
			var n2 = num1 < num2 ? num1 : num2;

			do
			{
				Fraction rem;
				DivRem(n1, n2, out rem);
				n1 = n2;
				n2 = rem;
			} while (!n2.Equals(Fraction.Empty));

			return n1;
		}

		/// <summary>
		/// Вычисляет частное двух дробных чисел и возвращает остаток в выходном параметре.
		/// </summary>
		/// <param name="a">Значение типа <see cref="Fraction"/>, содержащее делимое.</param>
		/// <param name="b">Значение типа <see cref="Fraction"/>, представляющее полученный остаток.</param>
		/// <param name="result">Значение типа  <see cref="Fraction"/>, представляющее полученный остаток.</param>
		/// <returns>Значение типа <see cref="Fraction"/>, содержащее частное указанных дробных чисел.</returns>
		/// <exception cref="DivideByZeroException"> Значение параметра <paramref name="b"/> равно нулю.</exception>
		public static long DivRem(Fraction a, Fraction b, out Fraction result)
		{
			if (Fraction.IsEmpty(b))
			{
				throw new DivideByZeroException();
			}

			var div = (long) (a/b);
			result = a - div*b;
			return div;
		}

		#endregion

		#region Rounding Items

		/// <summary>
		/// Округляет заданное значение число двойной точности с плавающей запятой до ближайшего целого.
		/// </summary>
		/// <param name="value"> Округляемое число двойной точности с плавающей запятой.</param>
		/// <returns>Целое число, ближайшее к значению параметра <paramref name="value"/>. Если дробная часть a находится на равном расстоянии от двух целых чисел, возвращается больший.</returns>
		/// <remarks>Обратите внимание, на отличие от функции <see cref="System.Math.Round(double)"/>, которая если дробная часть a находится на равном расстоянии от двух целых чисел, возвращает чётный.</remarks>
		public static double Round(double value) => System.Math.Floor(0.5 + value);

		/// <summary>
		/// Округляет число до значимых десятичных символов указанных параметром <paramref name="digits"/>.
		/// </summary>
		/// <param name="number">Округляемое вещественное число.</param>
		/// <param name="digits">Количество значимых десятичных символов.</param>
		/// <returns>Округлённое число до значимых десятичных символов.</returns>
		public static double Round(double number, int digits)
		{
			if (number.Equals(0D) || double.IsInfinity(number) || double.IsNaN(number))
			{
				return number;
			}

			var order = Exp10(digits - (int)Ceiling(GetOrder(number)));
			var sign = Round(number*order);
			return sign/order;
		}

		#endregion

		private static readonly double[] Exponents =
		{
			1E-15, 1E-14, 1E-13, 1E-12, 1E-11, 1E-10, 1E-9, 1E-8, 1E-7, 1E-6, 1E-5, 1E-4, 1E-3, 1E-2, 1E-1,
			1,
			1E+1, 1E+2, 1E+3, 1E+4, 1E+5, 1E+6, 1E+7, 1E+8, 1E+9, 1E+10, 1E+11, 1E+12, 1E+13, 1E+14, 1E+15
		};

		/// <summary>
		/// Возвращает число 10, возведенное в указанную степень.
		/// </summary>
		/// <param name="order">Показатель степени.</param>
		/// <returns>Число 10, возведенное в указанную степень.</returns>
		public static double Exp10(int order)
		{
			if (order > -16 && order < 16)
			{
				return Exponents[order + 15];
			}

			return System.Math.Pow(10D, order);
		}

		/// <summary>
		/// Возвращает порядок числа в десятичном представлении.
		/// </summary>
		/// <param name="value">Исходное число.</param>
		/// <returns>Порядок числа в десятичном представлении.</returns>
		public static double GetOrder(double value) => Lg(Abs(value));
	}
}