using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
			//Модуль вычисляется в long: модуль int.MinValue не представим в int, и прежде выбрасывалось OverflowException.
			var abs = Abs((long) num);

			do
			{
				long rem;
				abs = DivRem(abs, 10L, out rem);
				result = idx[(int) rem] + result;
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

		/// <summary>
		/// Пытается представить число в виде k/i·10ⁿ, где i — знаменатель от 500 до 999.
		/// </summary>
		/// <param name="value">Исходное число.</param>
		/// <param name="accuracy">Относительная точность совпадения: 10^(-accuracy).</param>
		/// <returns>Дробь или <see cref="Fraction.NaN"/>, если число не представимо такой дробью.</returns>
		private static Fraction GetRationalFraction(double value, int accuracy = 14)
		{
			//Прежде для нуля порядок равнялся −∞ и ответ получался только за счёт переполнений,
			//а для NaN метод Sign выбрасывал исключение.
			if (value.Equals(0D))
			{
				return Fraction.Empty;
			}

			if (double.IsNaN(value) || double.IsInfinity(value))
			{
				return Fraction.NaN;
			}

			//Дробь строится для модуля числа, а знак добавляется к готовой дроби, чтобы -x выводилось как x со знаком
			//минус: прежде при упрощении дроби с отрицательным числителем и большим порядком переполнялся long.
			var sign = Sign(value);
			value *= sign;

			var eps = accuracy == 0 ? 0D : Exp10(-accuracy);
			var order = (int)Floor(GetOrder(value)); //Порядок числа
			value *= Exp10(-order); //Мантисса

			//У чисел меньше 10⁻³⁰⁸ множитель 10^(-order) не представим, и прежде из бесконечной мантиссы
			//получалась случайная дробь, в том числе с другим знаком.
			if (double.IsInfinity(value))
			{
				return Fraction.NaN;
			}

			value += value*eps/10;

			//for (var i = 129; i <= 256; i++)
			for (var i = 500; i < 1000; i++)
			{
				var numerator = i*value;

				if (numerator - (int) numerator <= numerator * eps)
				{
					var fraction = new Fraction((int) numerator, i, order, true);
					return sign > 0 ? fraction : new Fraction(-fraction.Numerator, fraction.Denominator, fraction.Order);
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
			//Обозначение "1", как у обычных дробей: прежде передавался null, и числитель, равный единице,
			//терялся ("arctg /2" вместо "arctg 1/2").
			var tanStr = FractionToString(fraction, 8, "1", true, out denominator);

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

			//Частное субнормального числа и константы может обратиться в ноль, и без этой проверки
			//получалось бы "0π".
			if (number.Equals(0D) && !value.Equals(0D))
			{
				denominator = 0;
				return null;
			}

			Fraction fraction;
			//Допуск берётся от модуля числа: прежде для отрицательных чисел условие не выполнялось никогда,
			//и они выводились иначе, чем положительные (-25000000000 вместо -2.5000·10¹⁰).
			if (Abs(Round(number) - number) < 1E-13 * Abs(number) && Abs(number) > 1E-13)
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

		/// <summary>
		/// Константа, кратные которой распознаёт <see cref="DoubleToStringConstants"/>.
		/// </summary>
		private struct NamedConstant
		{
			/// <summary>
			/// Значение константы.
			/// </summary>
			public readonly double Value;

			/// <summary>
			/// Обозначение константы.
			/// </summary>
			public readonly string Symbol;

			/// <summary>
			/// Десятичный логарифм значения константы.
			/// </summary>
			public readonly double Order;

			public NamedConstant(double value, string symbol)
			{
				Value = value;
				Symbol = symbol;
				Order = GetOrder(value);
			}
		}

		/// <summary>
		/// Таблицы для <see cref="DoubleToStringConstants"/>. Строятся один раз при первом форматировании числа:
		/// прежде степени, корни и строки обозначений констант вычислялись заново для каждого числа.
		/// </summary>
		private static class Tables
		{
			/// <summary>
			/// Константы в порядке перебора (по возрастанию меры иррациональности).
			/// </summary>
			internal static readonly NamedConstant[] Constants = CreateConstants();

			/// <summary>
			/// Отметки частей [0, 1/2], близких к дробям со знаменателями до 999, для <see cref="IsFractionCandidate"/>.
			/// </summary>
			internal static readonly uint[] Fractions = CreateFractions();
		}

		private static NamedConstant[] CreateConstants()
		{
			var constants = new List<NamedConstant>
			{
				//ln 2
				new NamedConstant(Ln(2), "ln2"),
				//√π
				new NamedConstant(Sqrt(Pi), "√π"),
				//√℮
				new NamedConstant(Sqrt(E), "√℮"),
				//√φ
				new NamedConstant(Sqrt(Fi), "√φ")
			};

			//πⁿ
			for (var i = 1; i <= 9; i++)
			{
				constants.Add(i == 1
					? new NamedConstant(Pi, "π")
					: new NamedConstant(Pow(Pi, i), "π" + SupIdx[i]));
			}

			//℮ⁿ
			for (var i = 1; i <= 9; i++)
			{
				constants.Add(i == 1
					? new NamedConstant(E, "℮")
					: new NamedConstant(Exp(i), "℮" + SupIdx[i]));
			}

			//φⁿ
			for (var i = 1; i <= 9; i++)
			{
				constants.Add(i == 1
					? new NamedConstant(Fi, "φ")
					: new NamedConstant(Pow(Fi, i), "φ" + SupIdx[i]));
			}

			//√n
			//Проверка задумывалась как отбор чисел, свободных от квадратов, но squares содержит сами квадраты,
			//поэтому пропускаются только кратные 4 от 16, кратные 9 от 81 и кратные 25 от 625, а, например, √8
			//перебирается. Проверка сохранена, так как от неё зависит вывод: число 5001·√8/997 выводится как
			//"5001√8/997", а без √8 выводилось бы "14.187527".
			var squares = new List<int>();
			for (var i = 2; i < 1000; i++)
			{
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

				constants.Add(new NamedConstant(sqrt, "√" + i.ToString(CultureInfo.InvariantCulture)));
			}

			return constants.ToArray();
		}

		private static string DoubleToStringConstants(double value, int digits)
		{
			long denominator;

			//Единица
			var result = TryConvertToFraction(value, 1, digits, "1", true, out denominator);
			if (result != null)
			{
				return result;
			}

			//Константа пропускается, только если TryConvertConstant для неё заведомо вернёт null, поэтому
			//результат тот же, что при вызове TryConvertConstant для каждой константы. Прежде для обычного числа
			//более 1300 раз перебирались все 500 знаменателей, и одно число форматировалось 1–5 мс.
			var order = GetOrder(value);
			var constants = Tables.Constants;
			for (var i = 0; i < constants.Length; i++)
			{
				var constant = constants[i];
				if (!MayConvert(value / constant.Value, order - constant.Order) &&
				    !MayConvert(value * constant.Value, order + constant.Order))
				{
					continue;
				}

				result = TryConvertConstant(value, constant.Value, constant.Symbol, digits);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		/// <summary>
		/// Проверяет, может ли <see cref="TryConvertToFraction"/> вернуть строку для числа number,
		/// частного или произведения ненулевого числа и константы.
		/// </summary>
		/// <param name="number">Частное или произведение, вычисленное так же, как в <see cref="TryConvertToFraction"/>.</param>
		/// <param name="order">Десятичный логарифм модуля number с погрешностью меньше 10⁻⁹.</param>
		/// <returns>Значение <b>false</b>, только если <see cref="TryConvertToFraction"/> заведомо вернёт <b>null</b>.</returns>
		/// <remarks>Здесь используется System.Math.Abs без ветвлений: результаты те же, что у <see cref="Math.Abs(double)"/>,
		/// но непредсказуемый знак аргумента не замедляет проверку.</remarks>
		private static bool MayConvert(double number, double order)
		{
			//То же условие, что в TryConvertToFraction: близкое к целому число выводится всегда.
			if (System.Math.Abs(Round(number) - number) < 1E-13 * System.Math.Abs(number) && System.Math.Abs(number) > 1E-13)
			{
				return true;
			}

			//Порядок берётся с запасом вниз, поэтому мантисса равна мантиссе из GetRationalFraction или больше
			//неё в 10 раз, если логарифм близок к целому. Ноль получается только при исчезновении порядка.
			return !number.Equals(0D) && IsFractionCandidate(System.Math.Abs(number)*Exp10(-(int) Floor(order - 1E-9)));
		}

		/// <summary>
		/// Наибольший знаменатель дроби (после сокращения), которую может найти <see cref="GetRationalFraction"/>.
		/// </summary>
		private const int MaxDenominator = 999;

		/// <summary>
		/// Число равных частей, на которые <see cref="Tables.Fractions"/> делит отрезок [0, 1/2].
		/// </summary>
		private const int FractionCells = 1 << 19;

		/// <summary>
		/// Знаменатель, до которого округляется дробная часть мантиссы в <see cref="IsFractionCandidate"/>: 2³².
		/// </summary>
		private const double FractionScale = 4294967296D;

		/// <summary>
		/// Проверяет, может ли <see cref="GetRationalFraction"/> найти дробь для числа с мантиссой m.
		/// </summary>
		/// <param name="mantissa">Число 10ʲ·m (j = 0 или 1) с относительной погрешностью до 2·10⁻¹⁵.</param>
		/// <returns>Значение <b>false</b>, только если <see cref="GetRationalFraction"/> заведомо вернёт
		/// <see cref="Fraction.NaN"/>.</returns>
		/// <remarks>
		/// <see cref="GetRationalFraction"/> находит знаменатель i от 500 до 999, для которого дробная часть i·m
		/// не больше 10⁻¹⁴·i·m (m ≤ 10 с точностью до округления). Тогда для целого k |i·m − k| ≤ 2.02·10⁻¹⁰
		/// (допуск и погрешность умножения при m ≤ 20) и |m − k/i| ≤ 4.04·10⁻¹³. Значит, проверяемое число
		/// отличается от несократимой дроби P/Q = 10ʲ·k/i (Q ≤ 999) не больше чем на 4.4·10⁻¹², его дробная часть f —
		/// от дроби со знаменателем Q, а 2f и 3f — от дробей со знаменателями не больше Q не больше чем на 1.4·10⁻¹¹.
		/// Поэтому, если хотя бы одно из этих чисел не попадает в отмеченную часть [0, 1/2] (см.
		/// <see cref="CreateFractions"/>), дробь не будет найдена. Так отсеивается более 90% чисел.
		/// <para>Для остальных f округляется до 2⁻³², и получается число, отличающееся от дроби со знаменателем Q
		/// не больше чем на 1.3·10⁻¹⁰. Это меньше 1/(2Q²), поэтому по теореме Лежандра эта дробь — подходящая дробь
		/// цепной дроби числа, а так как две разные дроби со знаменателями до 999 отличаются больше чем на 10⁻⁶,
		/// она единственная такая и совпадает с последней подходящей дробью со знаменателем до 999. Для неё
		/// |10ʲ·m·Q − P| ≤ 4.4·10⁻⁹, поэтому при большем отклонении дробь не будет найдена. Подходящие дроби
		/// вычисляются алгоритмом Евклида, где все числа — целые меньше 2⁵³, поэтому вычисления в double точны.</para>
		/// </remarks>
		private static bool IsFractionCandidate(double mantissa)
		{
			//Оценки получены для мантисс не больше 200; для остальных значений проверка не выполняется.
			if (!(mantissa >= 0.5 && mantissa <= 200D))
			{
				return true;
			}

			var fractions = Tables.Fractions;
			var fraction = mantissa - Floor(mantissa);
			var doubled = 2*fraction;
			var tripled = 3*fraction;
			if (!IsNearFraction(fractions, fraction) ||
			    !IsNearFraction(fractions, doubled - Floor(doubled)) ||
			    !IsNearFraction(fractions, tripled - Floor(tripled)))
			{
				return false;
			}

			//Знаменатель последней подходящей дроби со знаменателем до 999 для дробной части num/2³².
			var num = Floor(fraction*FractionScale + 0.5);
			var den = FractionScale;
			double q0 = 0, q1 = 1;

			while (num > 0D)
			{
				var a = Floor(den/num);
				var q2 = a*q1 + q0;
				if (q2 > MaxDenominator)
				{
					break;
				}

				q0 = q1;
				q1 = q2;

				var rem = den - a*num;
				den = num;
				num = rem;
			}

			var product = mantissa*q1;
			return System.Math.Abs(product - Floor(product + 0.5)) <= 1E-8;
		}

		/// <summary>
		/// Проверяет, отмечена ли в <paramref name="fractions"/> часть [0, 1/2], в которую попадает число из [0, 1]
		/// или дополняющее его до единицы (дроби p/q и 1 − p/q имеют один знаменатель).
		/// </summary>
		private static bool IsNearFraction(uint[] fractions, double value)
		{
			var folded = 0.5 - System.Math.Abs(0.5 - value);
			var cell = Min((int) (folded*(2*FractionCells)), FractionCells - 1);
			return (fractions[cell >> 5] & (1u << (cell & 31))) != 0;
		}

		/// <summary>
		/// Отмечает части [0, 1/2] длиной 2⁻²⁰, отстоящие меньше чем на 10⁻¹⁰ от дробей p/q, где q ≤ 999.
		/// </summary>
		/// <returns>Битовая маска (64 КБ); отмечено около 29% частей.</returns>
		private static uint[] CreateFractions()
		{
			const double delta = 1E-10;
			var fractions = new uint[FractionCells/32];

			for (var q = 1; q <= MaxDenominator; q++)
			{
				for (var p = 0; 2*p <= q; p++)
				{
					var value = (double) p/q;
					var first = (int) Max(0D, Floor((value - delta)*(2*FractionCells)));
					var last = (int) Min(FractionCells - 1D, Floor((value + delta)*(2*FractionCells)));
					for (var cell = first; cell <= last; cell++)
					{
						fractions[cell >> 5] |= 1u << (cell & 31);
					}
				}
			}

			return fractions;
		}

		/// <summary>
		/// Представляет детальную информацию о числе.
		/// </summary>
		public class NumberInfo
		{
			/// <summary>
			/// Инициализирует новый экземпляр класса.
			/// </summary>
			/// <param name="number">Число. Строка разбирается в текущей культуре, как в <see cref="Convert.ToDouble(object)"/>.</param>
			public NumberInfo(object number)
			{
				Value = number;
				//Строка разбирается в текущей культуре, как и прежде: в ru-RU "1,5" означает 1.5.
				DoubleValue = Convert.ToDouble(number);
			}

			//Признак устанавливается после записи результатов (volatile — чтобы другой поток, увидевший признак, увидел и их).
			//Прежде признак устанавливался до вычисления, и другой поток мог прочитать нулевую погрешность.
			//Одновременные вычисления дают одни и те же значения.
			private volatile bool _isEpsilonCalculated;

			private void CalculateEpsilon()
			{
				if (_isEpsilonCalculated)
				{
					return;
				}

				//Для нуля, бесконечностей и NaN погрешность остается нулевой.
				if (!DoubleValue.Equals(0D) && !double.IsInfinity(DoubleValue) && !double.IsNaN(DoubleValue))
				{
					CalculateEpsilonCore();
				}

				_isEpsilonCalculated = true;
			}

			private void CalculateEpsilonCore()
			{
				var sign = Sign(DoubleValue);
				var value = Abs(DoubleValue);

				var order = (int)Floor(GetOrder(value));

				//У чисел меньше примерно 10⁻³⁰⁸ множитель 10^(-order) не представим в double: прежде normVal был
				//бесконечным, minor — NaN, и цикл ниже не завершался. Такие числа масштабируются в два шага.
				var normVal = ScaleByPowerOf10(value, -order);

				double major;
				double minor;

				//Для конечного normVal цикл завершается не более чем за двадцать с небольшим шагов: после них у normVal
				//не остаётся дробной части. Ограничение числа шагов — страховка.
				for (var step = 0; ; step++)
				{
					major = Floor(normVal);
					minor = normVal - major;
					if (minor < 0.000001 || step >= 400 || double.IsInfinity(normVal))
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

				//Поля читаются напрямую, а не через свойства: свойства снова вызвали бы вычисление, пока признак не установлен.
				_omega = minor.Equals(0D) || double.IsInfinity(normVal) ? DoubleValue : sign*ScaleByPowerOf10(major, order);
				_epsilon = Abs(DoubleValue - _omega);
				_eps = sign*Sign(value - Abs(_omega))*_epsilon;
				//Epsilon = Omega.Equals(DoubleValue) ? 0 : minor*factor;

			}

			/// <summary>
			/// Умножает число на 10^<paramref name="power"/> без переполнения и потери значимости промежуточного множителя.
			/// </summary>
			private static double ScaleByPowerOf10(double value, int power)
			{
				if (power > 300)
				{
					return value*Exp10(300)*Exp10(power - 300);
				}

				if (power < -300)
				{
					return value*Exp10(power + 300)*Exp10(-300);
				}

				return value*Exp10(power);
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

			/// <summary>
			/// Получает погрешность числа относительно его краткой десятичной записи: модуль разности между числом
			/// и ближайшим числом с наименьшим количеством значащих цифр, от которого оно отличается меньше чем
			/// на 10⁻⁶ единицы последнего разряда. Например, для 1.0000001 краткая запись — 1, а погрешность
			/// примерно равна 10⁻⁷.
			/// </summary>
			/// <remarks>Для нуля, NaN, бесконечностей и чисел, совпадающих со своей краткой записью, равна нулю.
			/// Если погрешность не равна нулю, <see cref="StringValue"/> выводит краткую запись с пометкой «+ ε» или «- ε».</remarks>
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
			if (digits < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(digits), "Количество значащих цифр должно быть больше нуля.");
			}

			character = "";

			if (double.IsNaN(num))
			{
				mantissa = "NaN";
				return;
			}

			if (double.IsInfinity(num))
			{
				mantissa = num > 0 ? "∞" : "-∞";
				return;
			}

			if (num.Equals(0D))
			{
				mantissa = "0." + new string('0', digits - 1);
				return;
			}

			if (num < 0)
			{
				//Отрицательное число выводится как модуль со знаком: округление половин вверх несимметрично, и прежде,
				//например, -12345.5 выводилось как -1.2345·10⁴, а 12345.5 — как 1.2346·10⁴.
				DoubleToStringSimple(-num, digits, out mantissa, out character);
				mantissa = CultureInfo.CurrentCulture.NumberFormat.NegativeSign + mantissa;
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
				return;
			}

			if (1 <= abs && abs < exp3)
			{
				//Если значащих цифр меньше, чем цифр в целой части, дробная часть не выводится
				//(прежде отрицательная длина строки нулей приводила к исключению).
				var exp = Exp10(digits - ord);
				mantissa = (Round(num * exp) / exp).ToString("0." + new string('0', Max(0, digits - ord)));
				return;
			}

			if (exp4 <= abs && abs < 1)
			{
				mantissa = (Round(num * exp2) / exp2).ToString("0." + new string('0', digits - 1));
				return;
			}
			else
			{
				//Мантисса экспоненциальной записи содержит digits - 3 значащих цифр, но не меньше одной.
				const int offset = 3;
				var significant = Max(digits, offset + 1);
				var exp = Exp10(significant - ord);
				var expOffset = Exp10(offset);

				//У чисел меньше примерно 10⁻³⁰⁰ множитель 10^(significant - ord) не представим в double, и прежде
				//выводилось "Infinity·10⁻³⁰⁵", поэтому такие числа масштабируются в два шага.
				var scaled = double.IsInfinity(exp) ? num * Exp10(300) * Exp10(significant - ord - 300) : num * exp;

				mantissa = (Round(scaled / expOffset) * expOffset / Exp10(significant - 1)).ToString("0." + new string('0', significant - offset - 1));
				character = "·10" + GetIndex(ord - 1, true);
				return;
			}

		}

		/// <summary>
		/// Возвращает строковое представление числа с учетом порядка.
		/// </summary>
		/// <param name="num">Исходное число.</param>
		/// <param name="digits">Порядок округления числа</param>
		/// <returns>Строковое представление числа с учетом порядка.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Значение параметра <paramref name="digits"/> меньше 1.</exception>
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


		#region ToString cache

		/// <summary>
		/// Запись общего кэша строковых представлений. Неизменяема и публикуется одной записью ссылки.
		/// </summary>
		private sealed class ToStringCacheEntry
		{
			public readonly object Key;
			public readonly int Hash;
			public readonly string Value;
			public readonly int Stamp;

			public ToStringCacheEntry(object key, int hash, string value, int stamp)
			{
				Key = key;
				Hash = hash;
				Value = value;
				Stamp = stamp;
			}
		}

		/// <summary>
		/// Ячейка общего кэша строковых представлений.
		/// </summary>
		/// <remarks>Запись хранится в volatile-поле: Thread.VolatileRead и Thread.VolatileWrite для элементов массива
		/// приводят к аварийному завершению JIT-компилятора Mono 6.8.</remarks>
		private sealed class ToStringCacheSlot
		{
			public volatile ToStringCacheEntry Entry;
		}

		/// <summary>
		/// Общий кэш строковых представлений: 128 наборов по две записи (не больше 256 записей, как и прежде).
		/// </summary>
		/// <remarks>Кэш работает без блокировок: запись заменяется одной записью ссылки, а чтение не может увидеть её
		/// частично записанной. Прежде все обращения шли под общей блокировкой, и под ней выполнялись GetHashCode и Equals
		/// ключей, в том числе пользовательских: ключ, методы которого сами берут блокировку, мог привести к взаимной
		/// блокировке потоков. Ещё раньше кэш был словарём без блокировок, который одновременные вызовы портили.</remarks>
		private static readonly ToStringCacheSlot[] ToStringCache = CreateToStringCache();

		private static ToStringCacheSlot[] CreateToStringCache()
		{
			var slots = new ToStringCacheSlot[256];
			for (var i = 0; i < slots.Length; i++)
			{
				slots[i] = new ToStringCacheSlot();
			}

			return slots;
		}

		//Порядковый номер записи: из двух записей набора заменяется более старая. Одновременные увеличения могут
		//потеряться, это лишь немного меняет выбор заменяемой записи.
		private static int _toStringCacheStamp;

		/// <summary>
		/// Сохраняет строковое представление объекта в общем кэше.
		/// </summary>
		/// <param name="key">Ключ кэша. Должен однозначно определяться значением объекта и не изменяться после сохранения.</param>
		/// <param name="value">Строковое представление.</param>
		/// <remarks>Метод потокобезопасен. Кэш ограничен по размеру, поэтому сохранённое значение может быть вытеснено.</remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="key"/> или <paramref name="value"/> равно <b>null</b>.</exception>
		public static void AddToStringHashValue(object key, string value)
		{
			if (ReferenceEquals(value, null))
			{
				throw new ArgumentNullException(nameof(value), "Параметр \"" + nameof(value) + "\" не должен равняться null.");
			}

			if (ReferenceEquals(key, null))
			{
				throw new ArgumentNullException(nameof(key));
			}

			var hash = key.GetHashCode();
			var index = GetToStringCacheSet(hash);

			//В наборе заменяется запись с тем же ключом, иначе пустая, иначе более старая.
			var first = ToStringCache[index].Entry;
			var second = ToStringCache[index + 1].Entry;
			if (!IsSameKey(first, key, hash) &&
			    (IsSameKey(second, key, hash) || (first != null && (second == null || second.Stamp - first.Stamp < 0))))
			{
				index++;
			}

			//Ключ, построенный по массиву вызывающего кода, получает собственную копию до того, как станет виден другим потокам.
			var valueKey = key as ToStringValueKey;
			if (valueKey != null)
			{
				valueKey.Freeze();
			}

			var stamp = ++_toStringCacheStamp;
			ToStringCache[index].Entry = new ToStringCacheEntry(key, hash, value, stamp);
		}

		/// <summary>
		/// Возвращает строковое представление объекта из общего кэша.
		/// </summary>
		/// <param name="key">Ключ кэша.</param>
		/// <returns>Сохранённое строковое представление или <b>null</b>, если его нет в кэше.</returns>
		/// <remarks>Метод потокобезопасен.</remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="key"/> равно <b>null</b>.</exception>
		public static string GetToStringHashValue(object key)
		{
			if (ReferenceEquals(key, null))
			{
				throw new ArgumentNullException(nameof(key));
			}

			var hash = key.GetHashCode();
			var index = GetToStringCacheSet(hash);

			var entry = ToStringCache[index].Entry;
			if (IsSameKey(entry, key, hash))
			{
				return entry.Value;
			}

			entry = ToStringCache[index + 1].Entry;
			return IsSameKey(entry, key, hash) ? entry.Value : null;
		}

		/// <summary>
		/// Возвращает индекс первой записи набора кэша для хэш-кода ключа.
		/// </summary>
		private static int GetToStringCacheSet(int hash)
		{
			//Биты хэш-кода перемешиваются: у близких хэш-кодов младшие биты часто совпадают.
			var mixed = unchecked((uint) hash*2654435761U);
			return (int) (mixed >> 25) << 1;
		}

		private static bool IsSameKey(ToStringCacheEntry entry, object key, int hash)
		{
			//Как и в словаре, сохранённый ключ сравнивается с заданным методом Equals сохранённого ключа.
			return entry != null && entry.Hash == hash && entry.Key.Equals(key);
		}

		/// <summary>
		/// Культура и символы её формата чисел, от которых зависит строковое представление.
		/// </summary>
		private sealed class CultureSignature
		{
			private readonly string[] _texts;
			public readonly int Hash;

			public CultureSignature(string[] texts)
			{
				_texts = texts;

				var hash = 0;
				foreach (var text in texts)
				{
					hash = unchecked(hash*31 + (text == null ? 0 : text.GetHashCode()));
				}

				Hash = hash;
			}

			/// <summary>
			/// Возвращает символы культуры и её формата чисел.
			/// </summary>
			public static string[] GetTexts(CultureInfo culture, NumberFormatInfo numberFormat)
			{
				return new[]
				{
					culture.Name,
					numberFormat.NumberDecimalSeparator, numberFormat.NumberGroupSeparator,
					numberFormat.NegativeSign, numberFormat.PositiveSign, numberFormat.NaNSymbol,
					numberFormat.PositiveInfinitySymbol, numberFormat.NegativeInfinitySymbol
				};
			}

			/// <summary>
			/// Проверяет, что символы культуры и формата чисел совпадают с сохранёнными.
			/// </summary>
			public bool Matches(CultureInfo culture, NumberFormatInfo numberFormat)
			{
				//Свойства обычно возвращают те же экземпляры строк, поэтому сравнение сводится к сравнению ссылок.
				return
					string.Equals(_texts[0], culture.Name) &&
					string.Equals(_texts[1], numberFormat.NumberDecimalSeparator) &&
					string.Equals(_texts[2], numberFormat.NumberGroupSeparator) &&
					string.Equals(_texts[3], numberFormat.NegativeSign) &&
					string.Equals(_texts[4], numberFormat.PositiveSign) &&
					string.Equals(_texts[5], numberFormat.NaNSymbol) &&
					string.Equals(_texts[6], numberFormat.PositiveInfinitySymbol) &&
					string.Equals(_texts[7], numberFormat.NegativeInfinitySymbol);
			}

			public bool ContentEquals(CultureSignature other)
			{
				if (ReferenceEquals(this, other))
				{
					return true;
				}

				if (other.Hash != Hash)
				{
					return false;
				}

				for (var i = 0; i < _texts.Length; i++)
				{
					if (!string.Equals(_texts[i], other._texts[i]))
					{
						return false;
					}
				}

				return true;
			}
		}

		//Последняя использованная культура: её символы не приходится заново собирать и хэшировать при каждом обращении.
		private static volatile CultureSignature _cultureSignature;

		private static CultureSignature GetCultureSignature()
		{
			var culture = CultureInfo.CurrentCulture;
			var numberFormat = culture.NumberFormat;

			var signature = _cultureSignature;
			if (signature == null || !signature.Matches(culture, numberFormat))
			{
				signature = new CultureSignature(CultureSignature.GetTexts(culture, numberFormat));
				_cultureSignature = signature;
			}

			return signature;
		}

		/// <summary>
		/// Ключ общего кэша для объекта, заданного набором чисел: вид объекта, формат, культура, символы формата чисел
		/// и сами числа (побитово).
		/// </summary>
		private sealed class ToStringValueKey
		{
			private readonly string _kind;
			private readonly string _format;
			private readonly CultureSignature _culture;
			private readonly int _hash;

			//До сохранения в кэше ключ ссылается на массив вызывающего кода, а при сохранении получает свою копию:
			//так при попадании в кэш массив не копируется.
			private double[] _values;
			private bool _frozen;

			public ToStringValueKey(string kind, string format, CultureSignature culture, double[] values)
			{
				_kind = kind;
				_format = format;
				_culture = culture;
				_values = values;

				var hash = unchecked(culture.Hash*31 + values.Length);
				hash = unchecked(hash*31 + (kind == null ? 0 : kind.GetHashCode()));
				hash = unchecked(hash*31 + (format == null ? 0 : format.GetHashCode()));
				foreach (var value in values)
				{
					var bits = BitConverter.DoubleToInt64Bits(value);
					hash = unchecked(hash*31 + ((int) bits ^ (int) (bits >> 32)));
				}

				_hash = hash;
			}

			/// <summary>
			/// Заменяет массив вызывающего кода собственной копией. Вызывается до сохранения ключа в кэше.
			/// </summary>
			public void Freeze()
			{
				if (!_frozen)
				{
					var copy = new double[_values.Length];
					Array.Copy(_values, copy, copy.Length);
					_values = copy;
					_frozen = true;
				}
			}

			public override int GetHashCode()
			{
				return _hash;
			}

			public override bool Equals(object obj)
			{
				var other = obj as ToStringValueKey;
				if (other == null || other._hash != _hash || other._values.Length != _values.Length ||
				    !string.Equals(other._kind, _kind) || !string.Equals(other._format, _format) ||
				    !other._culture.ContentEquals(_culture))
				{
					return false;
				}

				//Числа сравниваются побитово: у 0 и -0, как и у NaN с разными битами, строки могут различаться.
				var values = _values;
				var otherValues = other._values;
				for (var i = 0; i < values.Length; i++)
				{
					if (BitConverter.DoubleToInt64Bits(values[i]) != BitConverter.DoubleToInt64Bits(otherValues[i]))
					{
						return false;
					}
				}

				return true;
			}
		}

		/// <summary>
		/// Возвращает ключ общего кэша строковых представлений для объекта, заданного набором чисел.
		/// </summary>
		/// <param name="kind">Вид объекта.</param>
		/// <param name="format">Формат вывода.</param>
		/// <param name="values">Числа, однозначно задающие объект.</param>
		/// <returns>Ключ кэша.</returns>
		/// <remarks>Ключ строится по значениям, а не по объекту: прежде ключом служили изменяемые структуры и хэш-код
		/// массива, и кэш возвращал устаревшие или чужие строки. В ключ входят текущая культура и символы её формата чисел,
		/// от которых зависит вывод: культура, изменённая пользователем, может иметь то же имя, что и стандартная.
		/// Числа хранятся в ключе побитово, а не текстом: построение текстового ключа замедляло попадание в кэш в 7–23 раза
		/// и занимало в несколько раз больше памяти. Массив копируется в ключ только при сохранении в кэше.</remarks>
		internal static object GetToStringHashKey(string kind, string format, double[] values)
		{
			return new ToStringValueKey(kind, format, GetCultureSignature(), values);
		}

		/// <summary>
		/// Возвращает ключ общего кэша строковых представлений для объекта, заданного набором чисел.
		/// </summary>
		/// <param name="kind">Вид объекта.</param>
		/// <param name="format">Формат вывода.</param>
		/// <param name="values">Числа, однозначно задающие объект.</param>
		/// <returns>Ключ кэша.</returns>
		internal static object GetToStringHashKey(string kind, string format, IEnumerable<double> values)
		{
			return GetToStringHashKey(kind, format, values as double[] ?? values.ToArray());
		}

		#endregion
	}
}