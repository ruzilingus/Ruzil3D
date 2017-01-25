using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ruzil3D.Algebra;
using static Ruzil3D.Math;

namespace Ruzil3D.Utility
{

	/// <summary>
	/// Вспомогательный класс статических функций.
	/// </summary>
	public static class CStatic
	{
		#region Static Fields

		/// <summary>
		/// Представляет узкий неразрывный пробельный символ.
		/// </summary>
		public static readonly string NarrowNbSp = "\u202F";

		/// <summary>
		/// Представляет пробельный символ шириной заглавной буквы "M".
		/// </summary>
		public static readonly string EmSp = "\u2003";

		/// <summary>
		/// Представляет пробельный символ шириной заглавной буквы "N".
		/// </summary>
		public static readonly string EnSp = "\u2002";

		/// <summary>
		/// Комбинируемое надчёркивание
		/// </summary>
		public static readonly string Macron = "\u0305";

		/// <summary>
		/// Верхние индексы.
		/// </summary>
		private const string SupIdx = "⁰¹²³⁴⁵⁶⁷⁸⁹⁻⁺";

		/// <summary>
		/// Нижние индексы.
		/// </summary>
		private const string SubIdx = "₀₁₂₃₄₅₆₇₈₉₋₊";

		#endregion

		/// <summary>
		/// Возвращает строковое представление исходного числа в виде в индексной строки.
		/// </summary>
		/// <param name="num">Исходное число.</param>
		/// <param name="sup">Значение указывающее, что индекс должен быть верхним.</param>
		/// <returns>Строковое представление исходного числа в виде в индексной строки.</returns>
		public static string GetIndex(int num, bool sup = false)
		{
			var idx = sup ? SupIdx : SubIdx;
			var result = "";
			var abs = Abs(num);

			do
			{
				int rem;
				abs = DivRem(abs, 10, out rem);
				result = idx[rem] + result;
			} while (abs > 0);

			if (num < 0)
			{
				result = idx[10] + result;
			}

			return result;
		}

		/// <summary>
		/// Добавляет в строку новый член линейной комбинации.
		/// </summary>
		/// <param name="result">Строка результата.</param>
		/// <param name="num">Коэффициент линейной комбинации.</param>
		/// <param name="sym">Символ линейной комбинации.</param>
		public static bool AddLinearItem(ref string result, double num, string sym)
		{
			if (num.Equals(0D))
			{
				return false;
			}


			//Переводим число в строку
			var str = DoubleToString(Abs(num));


			if (string.IsNullOrEmpty(sym))
			{
				sym = "";
			}
			else if (str != "1")
			{
				sym = NarrowNbSp + sym;
			}
			else
			{
				str = "";
			}

			string sign;
			if (result == "")
			{
				//sign = num > 0 ? "": "−";
				sign = num > 0 ? "" : "-";
			}
			else
			{
				//sign = num > 0 ? " + " : " − ";
				sign = num > 0 ? " + " : " - ";
			}
			
			result += sign + str + sym;

			return true;
		}

		private static Fraction GetRationalFraction(double value, int accuracy = 14)
		{
			var sign = Sign(value);
			value *= sign;

			var eps = accuracy == 0 ? 0D : Exp10(-accuracy);
			var order = (int)Floor(GetOrder(value)); //Порядок числа
			value *= Exp10(-order); //Мантисса

			value += value*eps/10;

			//for (var i = 129; i <= 256; i++)
			for (var i = 500; i < 1000; i++)
			{
				var numerator = i*value;

				if (numerator - (int) numerator <= numerator * eps)
				{
					return new Fraction(sign*(int) numerator, i, order, true);
				}
			}

			return Fraction.NaN;
		}

		/// <summary>
		/// Пытается представить вещественное число в виде дроби.
		/// </summary>
		/// <param name="number">Исходное число.</param>
		/// <returns>Возвращает дробь, если исходное число представимо в виде дроби, в противном случае <see cref="Fraction.NaN"/>.</returns>
		private static Fraction DoubleToFraction(double number)
		{
			var fraction = GetRationalFraction(number);
			return Abs(fraction.Numerator) <= 999999 ? fraction : Fraction.NaN;
		}

		private static string FractionToString(Fraction fraction, int digits, string sym, bool isNumerator,
			out long denominator)
		{
			//Упрощаем
			if (Abs(fraction.Order) == 1)
			{
				var num = fraction.Numerator;
				var den = fraction.Denominator;
				if (fraction.Order > 0)
				{
					num *= 10L;
				}
				else
				{
					den *= 10L;
				}

				Simplify(ref num, ref den);
				fraction = new Fraction(num, den, 0);
			}

			denominator = fraction.Denominator;

			string symbol;

			//Характеристика: 10^n
			string exponent;

			//Если дробь делимая
			if (fraction.Denominator == 1)
			{
				var val = fraction.Numerator*fraction.Exponent;
				
				if (val.Equals(1))
				{
					return isNumerator ? sym : "1/" + sym;
				}

				if (val.Equals(-1))
				{
					return "-" + (isNumerator ? sym : "1/" + sym);
				}
				
				symbol = sym == "1" ? "" : (isNumerator ? sym : "/" + sym);

				/*
				if ((int) GetOrder(val) < digits)
				{
					return DoubleToStringSimple(val, digits) + symbol;
				}
				*/


				switch (fraction.Numerator)
				{
					case 1:
						exponent = fraction.Order.Equals(0) ? "" : "10" + GetIndex(fraction.Order, true);
						return exponent + symbol;

					case -1:
						exponent = fraction.Order.Equals(0) ? "" : "10" + GetIndex(fraction.Order, true);
						return "-" + exponent + symbol;

					default:
						/*
						string mantissa;
						string character;
						DoubleToStringSimple(fraction.Numerator, digits, out mantissa, out character);
						return mantissa + symbol + character;
						*/


						return val + symbol;
						
						/*
						exponent = fraction.Order.Equals(0) ? "" : "10" + GetIndex(fraction.Order, true);
						return exponent == ""
							? fraction.Numerator + symbol
							: fraction.Numerator + symbol + "·" + exponent;
						*/
						
				}
			}

			exponent = fraction.Order.Equals(0) ? "" : "10" + GetIndex(fraction.Order, true);

			if (isNumerator)
			{
				switch (fraction.Numerator)
				{
					case 1:
						symbol = sym;
						return exponent == ""
							? symbol + "/" + fraction.Denominator
							: symbol + "/" + fraction.Denominator + "·" + exponent;

					case -1:
						symbol = sym;
						return exponent == ""
							? "-" + symbol + "/" + fraction.Denominator
							: "-" + symbol + "/" + fraction.Denominator + "·" + exponent;

					default:
						symbol = sym == "1" ? "" : sym;
						return exponent == ""
							? fraction.Numerator + symbol + "/" + fraction.Denominator
							: fraction.Numerator + symbol + "/" + fraction.Denominator + "·" + exponent;
				}
			}
			else
			{
				switch (fraction.Numerator)
				{
					case 1:
						symbol = sym == "1" ? "" : sym;
						return exponent == ""
							? "1/" + fraction.Denominator + symbol
							: "1/" + fraction.Denominator + symbol + "·" + exponent;

					case -1:
						symbol = sym == "1" ? "" : sym;
						return exponent == ""
							? "-1/" + fraction.Denominator + symbol
							: "-1/" + fraction.Denominator + symbol + "·" + exponent;

					default:
						symbol = sym == "1" ? "" : sym;
						return exponent == ""
							? fraction.Numerator + "/" + fraction.Denominator + symbol
							: fraction.Numerator + "/" + fraction.Denominator + symbol + "·" + exponent;
				}
			}

		}

		/// <summary>
		/// Возаращает строковое представление угла.
		/// </summary>
		/// <param name="angle">Угол в радианах.</param>
		/// <returns>Строковое представление угла.</returns>
		public static string AngleToString(double angle)
		{
			if (0D.Equals(angle))
			{
				return "0";
			}

			var result = TryDoubleToString(angle, 8);
			if (result != null)
			{
				return result;
			}

			
			var tan = Abs(Tan(angle));

			var fraction = DoubleToFraction(tan);

			if (Fraction.IsNaN(fraction))
			{
				return DoubleToString(angle);
			}

			long denominator;
			var tanStr = FractionToString(fraction, 8, null, true, out denominator);

			angle = (angle%Tau + Tau)%Tau;


			if (angle <= Tau/4D)
			{
				return "arctg" + NarrowNbSp + tanStr;
			}

			if (angle <= Tau/2D)
			{
				return "π - arctg" + NarrowNbSp + tanStr;
			}

			if (angle <= 3D*Tau/4D)
			{
				return "arctg" + NarrowNbSp + tanStr + " - π";
			}

			return "-arctg" + NarrowNbSp + tanStr;
		}




		private static string TryConvertToFraction(double value, double divider, int digits, string sym, bool isNumerator, out long denominator)
		{
			var number = isNumerator ? value / divider : value * divider;

			Fraction fraction;
			if (Abs(Round(number) - number) < 1E-13 * number && Abs(number) > 1E-13)
			{
				var numerator = (long)Round(number);
				var numOrder = (int)GetOrder(numerator) + 1;

				if (numOrder > digits)
				{
					denominator = 1;
					return DoubleToStringSimple(value, digits);
				}

				/*
				if (numOrder > digits)
				{
					var ord = numOrder - digits;
					if (!0D.Equals(numerator % Exp10(ord)))
					{
						//Количество значимых цифр больше чем digits
						denominator = 1;
						return DoubleToStringSimple(value, digits);
					}
				}
				*/
									
				fraction = new Fraction(numerator, 1L, 0, true);
			}
			else
			{
				fraction = DoubleToFraction(number);
				if (Fraction.IsNaN(fraction))
				{
					denominator = 0;
					return null;
				}
			}

			return FractionToString(fraction, digits, sym, isNumerator, out denominator);
		}
		
		/// <summary>
		/// Пытается перевести число в дробь алгебраически зависимый от друго числа и возвращает его строковое представление.
		/// </summary>
		/// <param name="value">Исходное число.</param>
		/// <param name="divider">Число, алгебраическая зависимость которого ищется.</param>
		/// <param name="sym">Символьное представление числа.</param>
		/// <param name="digits">Порядок округления.</param>
		/// <returns></returns>
		private static string TryConvertConstant(double value, double divider, string sym, int digits)
		{
			long denominator;

			//Представление числа в в виде int*sym
			var result1 = TryConvertToFraction(value, divider, digits, sym, true, out denominator);

			if (denominator == 1)
			{
				return result1;
			}

			//Представление числа в в виде int/sym
			var result2 = TryConvertToFraction(value, divider, digits, sym, false, out denominator);
			if (denominator == 1)
			{
				return result2;
			}

			return result1 ?? result2;
		}

		private static string DoubleToStringConstants(double value, int digits)
		{
			long denominator;

			//По возрастанию меры иррациональности

			var result =
				//Единица
				TryConvertToFraction(value, 1, digits, "1", true, out denominator) ??
				//ln 2
				TryConvertConstant(value, Ln(2), "ln2", digits) ??
				//√π
				TryConvertConstant(value, Sqrt(Pi), "√π", digits) ??
				//√℮
				TryConvertConstant(value, Sqrt(E), "√℮", digits) ??
				//√φ
				TryConvertConstant(value, Sqrt(Fi), "√φ", digits);

			//πⁿ
			for (var i = 1; i <= 9 && result == null; i++)
			{
				result = i == 1
					? TryConvertConstant(value, Pi, "π", digits)
					: TryConvertConstant(value, Pow(Pi, i), "π" + SupIdx[i], digits);
			}

			//℮ⁿ
			for (var i = 1; i <= 9 && result == null; i++)
			{
				result = i == 1
					? TryConvertConstant(value, E, "℮", digits)
					: TryConvertConstant(value, Exp(i), "℮" + SupIdx[i], digits);
			}

			//φⁿ
			for (var i = 1; i <= 9 && result == null; i++)
			{
				result = i == 1
					? TryConvertConstant(value, Fi, "φ", digits)
					: TryConvertConstant(value, Pow(Fi, i), "φ" + SupIdx[i], digits);
			}
			
			//√n
			var squares = new List<int>();
			for (var i = 2; i < 1000 && result == null; i++)
			{
				//Провверяем все свободные от квадратов числа.
				if (squares.TakeWhile(num => num*num <= i).Any(num => i%num == 0))
				{
					continue;
				}

				var sqrt = Sqrt(i);

				if (sqrt.Equals((int) sqrt))
				{
					squares.Add(i);
					continue;
				}

				result = TryConvertConstant(value, sqrt, "√" + i, digits);
			}

			return result;
		}

		/// <summary>
		/// Представляет детальную информацию о числе.
		/// </summary>
		public class NumberInfo
		{
			/// <summary>
			/// Инициализирует новый экземпляр класса.
			/// </summary>
			/// <param name="number"></param>
			public NumberInfo(object number)
			{
				Value = number;
				DoubleValue = Convert.ToDouble(number);
			}

			private bool _isEpsilonCalculated;

			private void CalculateEpsilon()
			{
				if (_isEpsilonCalculated)
				{
					return;
				}

				_isEpsilonCalculated = true;

				if (DoubleValue.Equals(0D))
				{
					return;
				}

				if (double.IsInfinity(DoubleValue))
				{
					return;
				}

				if (double.IsNaN(DoubleValue))
				{
					return;
				}

				var sign = Sign(DoubleValue);
				var value = Abs(DoubleValue);

				var order = (int)Floor(GetOrder(value));
				var normVal = value*Exp10(-order);

				double major;
				double minor;


				while (true)
				{
					major = Floor(normVal);
					minor = normVal - major;
					if (minor < 0.000001)
					{
						break;
					}

					major++;
					minor = normVal - major;
					if (-minor < 0.000001)
					{
						break;
					}

					order--;
					normVal *= 10D;
				}

				var factor = sign*Exp10(order);
				_omega = minor.Equals(0D) ? DoubleValue : major*factor;
				_epsilon = Abs(DoubleValue - Omega);
				_eps = sign*Sign(value - Abs(Omega))*Epsilon;
				//Epsilon = Omega.Equals(DoubleValue) ? 0 : minor*factor;

			}

			private double _omega;

			private double Omega
			{
				get
				{
					CalculateEpsilon();
					return _omega;
				}
			}

			private double _eps;

			private double Eps
			{
				get
				{
					CalculateEpsilon();
					return _eps;
				}
			}


			private double _epsilon;

			public double Epsilon
			{
				get
				{
					CalculateEpsilon();
					return _epsilon;
				}
			}


			/// <summary>
			/// Получает представление числа в виде числа двойной точности с плавающей запятой.
			/// </summary>
			public double DoubleValue { get; }

			/// <summary>
			/// Само число.
			/// </summary>
			public object Value { get; }

			/// <summary>
			/// Порядок числа в десятичном представлении.
			/// </summary>
			public double Order => GetOrder(DoubleValue);

			private Type _type;

			/// <summary>
			/// Тип числа.
			/// </summary>
			public Type Type => _type ?? (_type = Value.GetType());

			/// <summary>
			/// Получает строковое представление числа.
			/// </summary>
			public string StringValue
			{
				get
				{
					if (Epsilon.Equals(0D))
					{
						return DoubleToString(DoubleValue);
					}

					return DoubleToStringSimple(Omega, 6) + (Eps > 0 ? " + ε" : " - ε");
				}
			}

			/// <summary>
			/// Возвращает строковое представление объекта.
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				var orderStr = double.IsNaN(Order)
					? "NaN"
					: double.IsPositiveInfinity(Order)
						? "∞"
						: double.IsNegativeInfinity(Order)
							? "-∞"
							: DoubleToStringSimple(Order, 5);

				return
					"Order: " + orderStr + EmSp +
					"Value: " + StringValue + EmSp +
					"Type: " + Type.Name;
			}
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходный объект для представления в виде числа.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this object x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this Fraction x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this sbyte x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this byte x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this short x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this ushort x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this int x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this uint x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this long x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this ulong x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this float x)
		{
			return new NumberInfo(x);
		}

		/// <summary>
		/// Получает детальную информацию о числе.
		/// </summary>
		/// <param name="x">Исходное число.</param>
		/// <returns>Детальная информация о числе.</returns>
		public static NumberInfo GetInfo(this double x)
		{
			return new NumberInfo(x);
		}

		/*
		public static double Sin(this double x)
		{
			return System.Math.Sin(x);
		}

		public static double Cos(this double x)
		{
			return System.Math.Sin(x);
		}
		*/



		private static void DoubleToStringSimple(double num, int digits, out string mantissa, out string character)
		{
			if (num.Equals(0D))
			{
				mantissa = "0." + new string('0', digits - 1);
				character = "";
				return;
			}

			//var order = 10;

			var exp1 = Exp10(digits);
			var exp2 = Exp10(digits - 1);
			var exp3 = Exp10(3);
			var exp4 = Exp10(-3);

			var abs = Abs(num);
			//var ord = abs.Equals(0D) ? 0 : (int)GetOrder(abs) + 1;
			var ord = abs.Equals(0D) ? 0 : (int)Floor(GetOrder(abs)) + 1;


			var isInt = ord > digits && 0D.Equals(abs % Exp10(ord - digits));


			if (exp2 <= abs && abs < exp1 && isInt)
			{
				mantissa = Round(num).ToString(CultureInfo.InvariantCulture);
				character = "";
				return;
			}

			if (1 <= abs && abs < exp3)
			{
				var exp = Exp10(digits - ord);
				mantissa = (Round(num * exp) / exp).ToString("0." + new string('0', digits - ord));
				character = "";
				return;
			}

			if (exp4 <= abs && abs < 1)
			{
				mantissa = (Round(num * exp2) / exp2).ToString("0." + new string('0', digits - 1));
				character = "";
				return;
			}
			else
			{
				var exp = Exp10(digits - ord);
				const int offset = 3;
				var expOffset = Exp10(offset);

				mantissa = (Round(num * exp / expOffset) * expOffset / exp2).ToString("0." + new string('0', digits - offset - 1));
				character = "·10" + GetIndex(ord - 1, true); ;
				return;
			}

		}

		/// <summary>
		/// Возвращает строковое представление числа с учетом порядка.
		/// </summary>
		/// <param name="num">Исходное число.</param>
		/// <param name="digits">Порядок округления числа</param>
		/// <returns>Строковое представление числа с учетом порядка.</returns>
		public static string DoubleToStringSimple(double num, int digits)
		{
			string mantissa;
			string character;
			DoubleToStringSimple(num, digits, out mantissa, out character);
			return mantissa + character;

			/*
			if (num.Equals(0D))
			{
				return "0." + new string('0', digits - 1);
			}

			//var order = 10;

			var exp1 = Exp10(digits);
			var exp2 = Exp10(digits - 1);
			var exp3 = Exp10(3);
			var exp4 = Exp10(-3);

			var abs = Abs(num);
			//var ord = abs.Equals(0D) ? 0 : (int) GetOrder(abs) + 1;
			var ord = abs.Equals(0D) ? 0 : (int)Floor(GetOrder(abs)) + 1;
			var isInt = ord > digits && 0D.Equals(abs%Exp10(ord - digits));


			if (exp2 <= abs && abs < exp1 && isInt)
			{
				return Round(num).ToString(CultureInfo.InvariantCulture);
			}

			if (1 <= abs && abs < exp3)
			{
				var exp = Exp10(digits - ord);
				return (Round(num*exp)/exp).ToString("0." + new string('0', digits - ord));
			}

			if (exp4 <= abs && abs < 1)
			{
				return (Round(num*exp2)/exp2).ToString("0." + new string('0', digits - 1));
			}
			else
			{
				var exp = Exp10(digits - ord);
				const int offset = 3;
				var expOffset = Exp10(offset);

				return (Round(num*exp/expOffset)*expOffset/exp2).ToString("0." + new string('0', digits - offset - 1)) + "·10" +
				       GetIndex(ord - 1, true);
			}
			*/
		}

		/// <summary>
		///  Возвращает строковое представление числа с учетом математических констант, дробей и бесконечностей.
		/// </summary>
		/// <param name="num">Исходное число.</param>
		/// <returns>Строковое представление числа с учетом математических констант, дробей и бесконечностей.</returns>
		public static string DoubleToString(double num)
		{
			return DoubleToString(num, 8);
		}

		/// <summary>
		/// Возвращает строковое представление числа с учетом порядка, математических констант, дробей и бесконечностей.
		/// </summary>
		/// <param name="num">Исходное число.</param>
		/// <param name="digits">Порядок округления числа</param>
		/// <returns>Строковое представление числа с учетом порядка, математических констант, дробей и бесконечностей.</returns>
		private static string DoubleToString(double num, int digits)
		{
			return TryDoubleToString(num, digits) ?? DoubleToStringSimple(num, digits);
		}

		private static string TryDoubleToString(double num, int digits)
		{
			if (double.IsNaN(num))
			{
				return "NaN";
			}

			if (double.IsPositiveInfinity(num))
			{
				return "∞";
			}

			if (double.IsNegativeInfinity(num))
			{
				return "-∞";
			}

			return DoubleToStringConstants(num, digits);
		}


		private static Dictionary<object, string> _toStringHash = new Dictionary<object, string>();

		public static void AddToStringHashValue(object key, string value)
		{
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException(nameof(value), "Параметр \"" + nameof(value) + "\" не должен равняться null.");
			}

			lock (_toStringHash)
			{
				if (_toStringHash.Count > 256)
				{
					for (var i = 0; i < 128; i++)
					{
						_toStringHash.Remove(_toStringHash.Keys.First());
					}
				}
			}

			_toStringHash[key] = value;
		}

		public static string GetToStringHashValue(object key)
		{
			return _toStringHash.ContainsKey(key) ? _toStringHash[key] : null;
		}
	}
}