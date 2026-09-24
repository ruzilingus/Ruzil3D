using System;
using System.Collections.Generic;
using System.Globalization;
using Ruzil3D.Algebra;
using Xunit;
using BigInteger = System.Numerics.BigInteger;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты дробных и комплексных чисел и функций класса <see cref="Math"/> для них.
	/// </summary>
	public class NumbersTests
	{
		private const long TwoPow53 = 9007199254740992L;

		#region Fraction

		[Fact]
		public void Fraction_LongDividedByFraction()
		{
			// Прежде long / Fraction вычислял x·N/D вместо x·D/N: 1 / new Fraction(1, 2) давало 1/2.
			Assert.True(1/new Fraction(1, 2) == 2);
			Assert.True(3L/new Fraction(2, 5) == new Fraction(15, 2));
			Assert.True(2/new Fraction(1, 3, 2) == new Fraction(3, 50));
			Assert.True(Fraction.IsPositiveInfinity(1/Fraction.Empty));
			Assert.True(Fraction.IsNaN(0/Fraction.Empty));
		}

		[Fact]
		public void Fraction_NegativeValues_KeepSign()
		{
			// Прежде при упрощении отрицательный числитель переполнялся, и знак менялся: (Fraction)(-1.5e19) был положительным.
			Assert.Equal(-1.5e19, (double) (Fraction) (-1.5e19));
			Assert.Equal(-3e20, (double) (Fraction) (-3e20));
			Assert.True((Fraction) (-3)*(Fraction) 1e20 == new Fraction(-3, 1, 20));
		}

		[Fact]
		public void Fraction_SumAndDifference_AreExact()
		{
			// Прежде порядок переносился в числитель и знаменатель через double с усечением: 10⁻⁵ + 10⁻⁵ = 2/99999.
			for (var k = 0; k <= 8; k++)
			{
				for (var a = 0; a <= 99; a++)
				{
					for (var b = 0; b <= 99; b++)
					{
						var sum = new Fraction(a, 1, -k) + new Fraction(b, 1, -k);
						Assert.True(sum == new Fraction(a + b, 1, -k), a + "·10^-" + k + " + " + b + "·10^-" + k + " = " + sum);
					}
				}
			}

			Assert.True((Fraction) 1e-5 + (Fraction) 1e-5 == new Fraction(2, 1, -5));
			Assert.True((Fraction) 1e-5 + 1L == new Fraction(100001, 1, -5));
			Assert.True(new Fraction(TwoPow53 + 1) - new Fraction(TwoPow53) == 1);
			Assert.True(new Fraction(1, 3) - new Fraction(1, 6) == new Fraction(1, 6));
		}

		[Fact]
		public void Fraction_LargeOperands_DoNotOverflow()
		{
			// Прежде перекрёстные произведения молча переполнялись: π + e давало 1.55, а 10²⁰ + 1 — отрицательное число.
			Near(System.Math.PI + System.Math.E, (Fraction) System.Math.PI + (Fraction) System.Math.E, 1e-13);
			Near(System.Math.PI*System.Math.E, (Fraction) System.Math.PI*(Fraction) System.Math.E, 1e-13);
			Assert.Equal(1e20, (double) ((Fraction) 1e20 + 1));
			Assert.Equal(1e10, (double) ((Fraction) 1e10 + (Fraction) 1e-10));
			Assert.Equal(1D, (double) ((Fraction) 1e-20 + 1));

			var inverse = new Fraction(1, 1L << 32);
			Assert.True(inverse + inverse == new Fraction(1, 1L << 31));
			Assert.True(Fraction.IsEmpty(inverse - inverse));

			// Результат, который помещается в дробь, получается точно, даже если промежуточные произведения не помещаются в long.
			Assert.True(new Fraction(long.MaxValue, 3)*new Fraction(3, long.MaxValue) == 1);
			Assert.True(new Fraction(1, 3, 400)*new Fraction(1, 3, 400) == new Fraction(1, 9, 800));
		}

		[Fact]
		public void Fraction_RandomArithmetic_MatchesExactRationals()
		{
			// Результат совпадает с точным, если он представим дробью, иначе отличается от него не больше чем на 10⁻¹⁷.
			var random = new Random(20260924);
			for (var i = 0; i < 300; i++)
			{
				var x = RandomFraction(random);
				var y = RandomFraction(random);
				var ex = Exact(x);
				var ey = Exact(y);

				CheckResult(x + y, Reduce(ex[0]*ey[1] + ey[0]*ex[1], ex[1]*ey[1]), x + " + " + y);
				CheckResult(x - y, Reduce(ex[0]*ey[1] - ey[0]*ex[1], ex[1]*ey[1]), x + " - " + y);
				CheckResult(x*y, Reduce(ex[0]*ey[0], ex[1]*ey[1]), x + " * " + y);
				CheckResult(x/y, Reduce(ex[0]*ey[1], ex[1]*ey[0]), x + " / " + y);

				var comparison = (ex[0]*ey[1]).CompareTo(ey[0]*ex[1]);
				Assert.Equal(comparison, System.Math.Sign(x.CompareTo(y)));
				Assert.Equal(comparison == 0, x == y);
				Assert.Equal(comparison < 0, x < y);
			}
		}

		[Fact]
		public void Fraction_Comparison_IsExact()
		{
			// Прежде дроби сравнивались через double: 1/300000 не равнялась (1/3)/100000, а 2^53 + 1 равнялось 2^53.
			Assert.True(new Fraction(1, 300000) == new Fraction(1, 3)/100000L);
			for (var n = 1; n <= 30; n++)
			{
				for (var d = 1L; d <= 80; d++)
				{
					for (var k = 0; k <= 9; k++)
					{
						var p = new Fraction(n, d*(long) System.Math.Pow(10, k));
						var q = new Fraction(n, d, -k);
						Assert.True(p == q && p.Equals(q) && p.CompareTo(q) == 0, p + " ≠ " + q);
					}
				}
			}

			var larger = new Fraction(TwoPow53 + 1);
			var smaller = new Fraction(TwoPow53);
			Assert.True(larger != smaller && larger > smaller && smaller < larger && larger.CompareTo(smaller) > 0);
			Assert.Equal(TwoPow53 + 1, Math.Max(larger, smaller).Numerator);
			Assert.Equal(TwoPow53, Math.Min(larger, smaller).Numerator);

			var huge = new Fraction(1, 1, 400);
			Assert.False(Fraction.IsInfinity(huge));
			Assert.True(huge != new Fraction(2, 1, 400) && huge < new Fraction(2, 1, 400) && huge < Fraction.PositiveInfinity);

			// NaN не равно самому себе, как и у double, но Equals считает два NaN равными.
			var nan = Fraction.NaN;
			var otherNaN = new Fraction(0, 0);
			Assert.False(nan == otherNaN);
			Assert.True(nan != otherNaN);
			Assert.True(nan.Equals(otherNaN));
			Assert.True(Fraction.NaN.CompareTo(Fraction.NegativeInfinity) < 0);
		}

		[Fact]
		public void Fraction_GetHashCode_MatchesEquality()
		{
			// Прежде хэш-код всегда был равен нулю, и хэш-таблицы с дробями работали за квадратичное время.
			var hashes = new HashSet<int>();
			for (var i = 0; i < 1000; i++)
			{
				hashes.Add(new Fraction(i, 7).GetHashCode());
			}

			Assert.True(hashes.Count > 900);

			Assert.Equal(new Fraction(1, 3).GetHashCode(), new Fraction(20, 60).GetHashCode());
			Assert.Equal(new Fraction(1, 300000).GetHashCode(), (new Fraction(1, 3)/100000L).GetHashCode());
			Assert.Equal(new Fraction(30, 1).GetHashCode(), new Fraction(3, 1, 1).GetHashCode());
			Assert.Equal(new Fraction(-5).GetHashCode(), new Fraction(-5, 10, 1).GetHashCode());

			var set = new HashSet<Fraction>();
			for (var i = 0; i < 20000; i++)
			{
				set.Add(new Fraction(i, 7));
			}

			Assert.Equal(20000, set.Count);
			Assert.Contains(new Fraction(20, 70, 1), set);
		}

		[Fact]
		public void Fraction_Infinities_FollowIeeeRules()
		{
			// Прежде знак бесконечности терялся: +∞ + +∞ = NaN, -∞ / (-1) = -∞.
			var plus = Fraction.PositiveInfinity;
			var minus = Fraction.NegativeInfinity;

			Assert.True(Fraction.IsPositiveInfinity(plus + plus));
			Assert.True(Fraction.IsNegativeInfinity(minus + minus));
			Assert.True(Fraction.IsPositiveInfinity(plus - minus));
			Assert.True(Fraction.IsNaN(plus + minus));
			Assert.True(Fraction.IsPositiveInfinity(minus/(Fraction) (-1)));
			Assert.True(Fraction.IsNegativeInfinity(plus/(Fraction) (-2)));
			Assert.True(Fraction.IsNegativeInfinity(plus/-2L));
			Assert.True(Fraction.IsNaN(plus*0));
			Assert.True(Fraction.IsEmpty(new Fraction(5)/minus));
			Assert.True(Fraction.IsPositiveInfinity(plus + 5));
		}

		[Fact]
		public void Fraction_Remainder_IsExact()
		{
			// Прежде частное вычислялось в double: 0.3 % 0.1 = 1/10, а 5 % 0 = 5.
			Assert.True(Fraction.IsEmpty((Fraction) 0.3%(Fraction) 0.1));
			Assert.True(Fraction.IsNaN((Fraction) 5%(Fraction) 0));

			var nines = new Fraction(999999999999999999L, 1000000000000000000L);
			Assert.True(nines%1 == nines);

			Assert.True((Fraction) (-7)%3 == -1);
			Assert.True((Fraction) 7%-3 == 1);
			Assert.True(new Fraction(1, 1, 30)%7 == 1);
			Assert.True(new Fraction(7, 2)%new Fraction(1, 3) == new Fraction(1, 6));
			Assert.True(Fraction.IsNaN(Fraction.PositiveInfinity%3));
			Assert.True((Fraction) 5%Fraction.PositiveInfinity == 5);
		}

		[Fact]
		public void Fraction_Parse_FindsOnlyGenuinePeriods()
		{
			// Прежде условие выбора периода было всегда истинным, и побеждал «период», подтверждённый одной цифрой:
			// (Fraction)(12344.0/99999) давало 11109711097111/9·10⁻¹³.
			AssertFraction(12344, 99999, 0, (Fraction) (12344.0/99999));
			AssertFraction(31234567890123, 1, -14, (Fraction) 0.31234567890123);

			// Результаты для обычных чисел не изменились.
			AssertFraction(1, 2, 0, (Fraction) 0.5);
			AssertFraction(1, 1, -1, (Fraction) 0.1);
			AssertFraction(1, 4, 0, (Fraction) 0.25);
			AssertFraction(1, 3, 0, (Fraction) (1.0/3));
			AssertFraction(2, 7, 0, (Fraction) (2.0/7));
			AssertFraction(16, 9, 3, (Fraction) 1777.7777777777777);
			AssertFraction(25000000372529, 25, -13, (Fraction) 0.1f);
			AssertFraction(1, 81, 0, (Fraction) (1.0/81));
			AssertFraction(99999, 7, 0, (Fraction) (99999.0/7));
		}

		[Fact]
		public void Fraction_Parse_IsNotWorseThanTruncation()
		{
			// Прежде ложные периоды уводили результат дальше от числа, чем простое усечение до 15 значащих цифр
			// (так было примерно у каждого пятого случайного числа).
			var random = new Random(20260924);
			for (var i = 0; i < 2000; i++)
			{
				var value = random.NextDouble()*System.Math.Pow(10, random.Next(-30, 31));
				if (value.Equals(0D))
				{
					continue;
				}

				var exact = Exact(value);
				var fraction = (Fraction) value;
				Assert.True(CompareDistance(Exact(fraction), Truncate15(exact), exact) <= 0, value.ToString("R") + " → " + fraction);
			}
		}

		[Fact]
		public void Fraction_Parse_TinyAndHugeValues()
		{
			// Прежде множитель 10^n переполнялся, и (Fraction)1.5e-300 выбрасывал ArgumentException прямо из неявного преобразования.
			foreach (var value in new[] {1.5e-300, 1e-320, double.Epsilon, -double.Epsilon, double.MaxValue, -double.MaxValue, 2.2250738585072014E-308})
			{
				var fraction = (Fraction) value;
				Assert.False(Fraction.IsNaN(fraction) || Fraction.IsInfinity(fraction), value.ToString("R"));
				Assert.True(System.Math.Abs((double) fraction - value) <= 1e-14*System.Math.Abs(value), value.ToString("R") + " → " + (double) fraction);
			}
		}

		[Fact]
		public void Fraction_Parse_RejectsInvalidOrder()
		{
			// Прежде порядок не проверялся: 0 приводил к IndexOutOfRangeException, -1 — к OverflowException, 19 — к ArgumentException.
			Assert.Throws<ArgumentOutOfRangeException>(() => Fraction.Parse(1.5, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => Fraction.Parse(1.5, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => Fraction.Parse(1.5, 19));

			for (var order = 1; order <= 18; order++)
			{
				Assert.True(System.Math.Abs((double) Fraction.Parse(System.Math.PI, order) - System.Math.PI) <= 0.5*System.Math.Pow(10, 1 - order) + 1e-15);
			}
		}

		[Fact]
		public void Fraction_CompareToObject_FollowsContract()
		{
			// Прежде null приводил к NullReferenceException, а объект другого типа — к InvalidCastException.
			Assert.True(new Fraction(1).CompareTo(null) > 0);
			Assert.Throws<ArgumentException>(() => new Fraction(1).CompareTo("1"));
			Assert.True(new Fraction(1).CompareTo((object) new Fraction(2)) < 0);
		}

		[Fact]
		public void Fraction_ToString_MarksInexactValues()
		{
			// Прежде « = » ставился для любого знаменателя вида 2^n или 5^n, а внесение порядка в числитель переполнялось.
			Assert.Equal("9007199254740993/2 ≈ 4503599627370496", TestUtil.WithCulture("", () => new Fraction(TwoPow53 + 1, 2).ToString()));
			Assert.StartsWith("922337203685477581/7·10¹ ≈ 1.3176", TestUtil.WithCulture("", () => new Fraction(922337203685477581L, 7, 1).ToString()));

			// Примеры из документации.
			Assert.Equal("-3/7·10⁻⁶ ≈ -4.2857142857142857E-07", TestUtil.WithCulture("", () => new Fraction(3, -7, -6).ToString()));
			Assert.Equal("1/3 ≈ 0.3333333333333333", TestUtil.WithCulture("", () => ((Fraction) 6/18).ToString()));
			Assert.Equal("16/9·10³ ≈ 1777.7777777777778", TestUtil.WithCulture("", () => ((Fraction) 1777.7777777777777).ToString()));
			Assert.Equal("1/8 = 0.125", TestUtil.WithCulture("", () => new Fraction(1, 8).ToString()));
		}

		[Fact]
		public void Fraction_LongMinValue_KeepsSign()
		{
			// Прежде смена знака long.MinValue переполнялась: 1/long.MinValue становилась положительной,
			// а new Fraction(long.MinValue, 3, true) выбрасывала OverflowException.
			var inverse = new Fraction(1, long.MinValue);
			Assert.True(inverse < 0);
			Assert.Equal(-1.0842021724855044E-19, (double) inverse);

			Assert.True(new Fraction(long.MinValue, 3, true) == new Fraction(long.MinValue, 3));
			Assert.True(new Fraction(long.MinValue, -3) > 0);
			Assert.True(-new Fraction(long.MinValue) == new Fraction(1L << 62)*2);
			Assert.True(Math.Abs(new Fraction(long.MinValue, 7)) == -new Fraction(long.MinValue, 7));
		}

		[Fact]
		public void Fraction_ChangeType_ReturnsFraction()
		{
			// Прежде значение уходило в double, и Convert.ChangeType(frac, typeof(Fraction)) выбрасывал InvalidCastException.
			var fraction = new Fraction(1, 3);
			var converted = Convert.ChangeType(fraction, typeof(Fraction));

			Assert.IsType<Fraction>(converted);
			Assert.True((Fraction) converted == fraction);
			Assert.Equal(0.25, Convert.ChangeType(new Fraction(1, 4), typeof(double)));
		}

		#endregion

		#region Math

		[Fact]
		public void Math_DivRemFraction_IsExact()
		{
			// Прежде частное бралось через (long)(double): DivRem(2^53 + 1, 1) давало частное 2^53 и остаток 0.
			Fraction remainder;
			Assert.Equal(TwoPow53 + 1, Math.DivRem(new Fraction(TwoPow53 + 1), new Fraction(1), out remainder));
			Assert.True(Fraction.IsEmpty(remainder));

			Assert.Equal(-10L, Math.DivRem(new Fraction(-7, 2), new Fraction(1, 3), out remainder));
			Assert.True(remainder == new Fraction(-1, 6));

			Assert.Throws<OverflowException>(() => Math.DivRem(new Fraction(1, 1, 30), new Fraction(1, 3), out remainder));
			Assert.Throws<DivideByZeroException>(() => Math.DivRem(new Fraction(1), Fraction.Empty, out remainder));
		}

		[Fact]
		public void Math_PowLong_ThrowsOnOverflow()
		{
			// Прежде Pow(long, int) молча переполнялся: Pow(10, 19) = -8446744073709551616, Pow(2, 63) = long.MinValue.
			Assert.Throws<OverflowException>(() => Math.Pow(10L, 19));
			Assert.Throws<OverflowException>(() => Math.Pow(10L, 20));
			Assert.Throws<OverflowException>(() => Math.Pow(2L, 63));
			Assert.Throws<OverflowException>(() => Math.Pow(3L, 40));
			Assert.Throws<OverflowException>(() => Math.Pow(2L, 1000));

			Assert.Equal(1000000000000000000L, Math.Pow(10L, 18));
			Assert.Equal(long.MinValue, Math.Pow(-2L, 63));
			Assert.Equal(4052555153018976267L, Math.Pow(3L, 39));

			// Особые случаи не изменились.
			Assert.Equal(-1L, Math.Pow(-1L, int.MaxValue));
			Assert.Equal(1L, Math.Pow(1L, -5));
			Assert.Equal(0L, Math.Pow(0L, 5));
			Assert.Throws<ArithmeticException>(() => Math.Pow(0L, 0));
			Assert.Throws<ArithmeticException>(() => Math.Pow(2L, -1));
		}

		[Fact]
		public void Math_Round_RoundsHalvesUpExactly()
		{
			// Прежде Floor(0.5 + x) округлял сумму: Round(0.49999999999999994) было 1, нечётные числа около 2^52 увеличивались.
			Assert.Equal(0D, Math.Round(0.49999999999999994));
			Assert.Equal(4503599627370497D, Math.Round(4503599627370497D));
			Assert.Equal(-4503599627370497D, Math.Round(-4503599627370497D));

			Assert.Equal(3D, Math.Round(2.5));
			Assert.Equal(-2D, Math.Round(-2.5));
			Assert.Equal(0D, Math.Round(-0.5));
			Assert.Equal(1235D, Math.Round(1234.5));
		}

		[Fact]
		public void Math_GreatestDivisorFraction_IsExact()
		{
			// Прежде остатки вычислялись через double, и алгоритм Евклида выходил за пределы точности дробей.
			Assert.True(Math.GreatestDivisor(new Fraction(314159265358979, 1, -14), new Fraction(1)) == new Fraction(1, 1, -14));
			Assert.True(Math.GreatestDivisor(new Fraction(1, 1L << 40), new Fraction(1, 3)) == new Fraction(1, 3L << 40));
			Assert.True(Math.GreatestDivisor(new Fraction(1, 1, 30), new Fraction(1, 1, -30)) == new Fraction(1, 1, -30));
			Assert.True(Math.GreatestDivisor(new Fraction(6, 5), new Fraction(4, 15)) == new Fraction(2, 15));
		}

		#endregion

		#region Complex

		[Fact]
		public void Complex_Division_ExtremeMagnitudes()
		{
			// Прежде деление вычисляло R² + I², которое переполнялось или обращалось в ноль: (2e160) / (1e160) = (NaN, 0).
			Assert.Equal(new Complex(2, 0), new Complex(2e160, 0)/new Complex(1e160, 0));
			Assert.Equal(new Complex(1, 0), new Complex(1e-200, 1e-200)/new Complex(1e-200, 1e-200));

			var inverse = 1.0/new Complex(1e-170, 0);
			Assert.Equal(1e170, inverse.R);
			Assert.Equal(0D, inverse.I);

			var ratio = new Complex(3e200, 4e200)/new Complex(1e200, 2e200);
			Assert.Equal(2.2, ratio.R, 1e-15);
			Assert.Equal(-0.4, ratio.I, 1e-15);

			// Обычные значения вычисляются по прежней формуле.
			Assert.Equal(new Complex(1, 2), new Complex(-5, 10)/new Complex(3, 4));
			Assert.Equal(new Complex(0.2, -0.4), 1.0/new Complex(1, 2));
		}

		[Fact]
		public void Complex_AbsPowAndRoots_ExtremeMagnitudes()
		{
			// Прежде модуль (1e200, 1e200) был равен ∞, (3e-200, 4e-200) — нулю, а Complex(1e200, 0).Pow(0.5) давал (∞, NaN).
			Assert.Equal(1.4142135623730951e200, new Complex(1e200, 1e200).Abs);
			Assert.Equal(5e-200, new Complex(3e-200, 4e-200).Abs, 1e-214);
			Assert.Equal(5D, new Complex(3, 4).Abs);

			var root = new Complex(1e200, 0).Pow(0.5);
			Assert.Equal(1e100, root.R);
			Assert.Equal(0D, root.I);

			foreach (var value in new Complex(-1e200, 0).GetRoots(2))
			{
				Assert.Equal(1e100, value.Abs, 1e86);
			}
		}

		[Fact]
		public void Complex_ToStringWithFormat_UsesFormatAndProvider()
		{
			// Прежде формат и провайдер игнорировались: ToString("F2") выводил "1.2345000 - 4.5678000 i".
			var number = new Complex(1.2345, -4.5678);
			var comma = new NumberFormatInfo {NumberDecimalSeparator = ","};

			Assert.Equal("1.23 - 4.57 i", number.ToString("F2", CultureInfo.InvariantCulture));
			Assert.Equal("1,23 - 4,57 i", number.ToString("F2", comma));
			Assert.Equal("0.3 + 0.7 i", string.Format(CultureInfo.InvariantCulture, "{0:F1}", new Complex(0.26, 0.74)));
			Assert.Equal("-2.0 i", new Complex(0, -2).ToString("F1", CultureInfo.InvariantCulture));

			// Без формата вывод прежний.
			Assert.Equal("2 - i", TestUtil.WithCulture("", () => new Complex(2, -1).ToString(null, CultureInfo.InvariantCulture)));
			Assert.Equal("-4 + 2 i", TestUtil.WithCulture("", () => (-2*new Complex(2, -1)).ToString()));
		}

		#endregion

		#region Helpers

		private static void Near(double expected, Fraction actual, double tolerance)
		{
			Assert.True(System.Math.Abs((double) actual - expected) <= tolerance, "Ожидалось " + expected.ToString("R") + ", получено " + actual + ".");
		}

		private static void AssertFraction(long numerator, long denominator, int order, Fraction actual)
		{
			Assert.True(actual.Numerator == numerator && actual.Denominator == denominator && actual.Order == order,
				"Ожидалось " + numerator + "/" + denominator + "·10^" + order + ", получено " +
				actual.Numerator + "/" + actual.Denominator + "·10^" + actual.Order + ".");
		}

		private static Fraction RandomFraction(Random random)
		{
			long numerator, denominator;
			switch (random.Next(3))
			{
				case 0:
					numerator = random.Next(-1000, 1001);
					denominator = random.Next(1, 1001);
					break;
				case 1:
					numerator = random.Next(int.MinValue, int.MaxValue);
					denominator = random.Next(1, int.MaxValue);
					break;
				default:
					numerator = ((long) random.Next(int.MinValue, int.MaxValue) << 31) + random.Next();
					denominator = ((long) random.Next(1, int.MaxValue) << 31) + random.Next();
					break;
			}

			if (numerator == 0L)
			{
				numerator = 1L;
			}

			return new Fraction(numerator, denominator, random.Next(-25, 26), random.Next(2) == 0);
		}

		/// <summary>
		/// Проверяет результат арифметической операции: точный, если он представим дробью, иначе близкий к точному.
		/// </summary>
		private static void CheckResult(Fraction actual, BigInteger[] expected, string operation)
		{
			Assert.False(Fraction.IsNaN(actual) || Fraction.IsInfinity(actual), operation);
			var exact = Exact(actual);
			if (IsRepresentable(expected))
			{
				Assert.True(exact[0] == expected[0] && exact[1] == expected[1], operation + " = " + actual);
				return;
			}

			var error = BigInteger.Abs(exact[0]*expected[1] - expected[0]*exact[1])*BigInteger.Pow(10, 17);
			Assert.True(error <= BigInteger.Abs(expected[0]*exact[1]), operation + " ≈ " + actual);
		}

		/// <summary>
		/// Проверяет, записывается ли несократимая дробь num/den в виде n/d·10^e с числителем и знаменателем типа long.
		/// </summary>
		private static bool IsRepresentable(BigInteger[] value)
		{
			if (value[0].IsZero)
			{
				return true;
			}

			var center = (int) System.Math.Floor(BigInteger.Log10(BigInteger.Abs(value[0])) - BigInteger.Log10(value[1]));
			for (var e = center - 40; e <= center + 40; e++)
			{
				var n = e >= 0 ? value[0] : value[0]*BigInteger.Pow(10, -e);
				var d = e >= 0 ? value[1]*BigInteger.Pow(10, e) : value[1];
				var r = Reduce(n, d);
				var fits = r[0].Sign >= 0 ? r[0] <= long.MaxValue : -r[0] <= BigInteger.One << 63;
				if (fits && r[1] <= long.MaxValue)
				{
					return true;
				}
			}

			return false;
		}

		private static BigInteger[] Reduce(BigInteger numerator, BigInteger denominator)
		{
			if (denominator.Sign < 0)
			{
				numerator = -numerator;
				denominator = -denominator;
			}

			var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
			return gcd.IsZero || gcd.IsOne ? new[] {numerator, denominator} : new[] {numerator/gcd, denominator/gcd};
		}

		/// <summary>
		/// Точное значение конечной дроби в виде несократимой пары (числитель, знаменатель).
		/// </summary>
		private static BigInteger[] Exact(Fraction value)
		{
			BigInteger numerator = value.Numerator, denominator = value.Denominator;
			if (value.Order >= 0)
			{
				numerator *= BigInteger.Pow(10, value.Order);
			}
			else
			{
				denominator *= BigInteger.Pow(10, -value.Order);
			}

			return Reduce(numerator, denominator);
		}

		/// <summary>
		/// Точное значение положительного числа двойной точности.
		/// </summary>
		private static BigInteger[] Exact(double value)
		{
			var bits = BitConverter.DoubleToInt64Bits(value);
			var exponent = (int) ((bits >> 52) & 0x7FF);
			var mantissa = bits & 0xFFFFFFFFFFFFFL;
			if (exponent == 0)
			{
				exponent = 1;
			}
			else
			{
				mantissa |= 1L << 52;
			}

			exponent -= 1075;
			return exponent >= 0
				? Reduce(new BigInteger(mantissa) << exponent, BigInteger.One)
				: Reduce(new BigInteger(mantissa), BigInteger.One << -exponent);
		}

		/// <summary>
		/// Точное усечение положительного числа до 15 значащих цифр.
		/// </summary>
		private static BigInteger[] Truncate15(BigInteger[] value)
		{
			var lower = BigInteger.Pow(10, 14);
			var upper = BigInteger.Pow(10, 15);
			var scale = 14 - (int) System.Math.Floor(BigInteger.Log10(value[0]) - BigInteger.Log10(value[1]));
			while (true)
			{
				var digits = scale >= 0
					? value[0]*BigInteger.Pow(10, scale)/value[1]
					: value[0]/(value[1]*BigInteger.Pow(10, -scale));

				if (digits >= upper)
				{
					scale--;
				}
				else if (digits < lower)
				{
					scale++;
				}
				else
				{
					return scale >= 0 ? Reduce(digits, BigInteger.Pow(10, scale)) : Reduce(digits*BigInteger.Pow(10, -scale), BigInteger.One);
				}
			}
		}

		/// <summary>
		/// Сравнивает расстояния |a - value| и |b - value|.
		/// </summary>
		private static int CompareDistance(BigInteger[] a, BigInteger[] b, BigInteger[] value)
		{
			var distanceA = BigInteger.Abs(a[0]*value[1] - value[0]*a[1])*b[1];
			var distanceB = BigInteger.Abs(b[0]*value[1] - value[0]*b[1])*a[1];
			return distanceA.CompareTo(distanceB);
		}

		#endregion
	}
}
