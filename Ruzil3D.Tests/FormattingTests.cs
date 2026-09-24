using System;
using System.Collections.Generic;
using Ruzil3D.Utility;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты форматирования чисел: распознавание дробей и констант, знак, особые и очень малые значения.
	/// </summary>
	public class FormattingTests
	{
		public static IEnumerable<object[]> UnitNumeratorAngles()
		{
			yield return new object[] {System.Math.Atan(0.5), "arctg 1/2"};
			yield return new object[] {-System.Math.Atan(0.5), "-arctg 1/2"};
			yield return new object[] {System.Math.PI - System.Math.Atan(0.5), "π - arctg 1/2"};
			yield return new object[] {System.Math.Atan(0.5) + System.Math.PI, "arctg 1/2 - π"};
			yield return new object[] {System.Math.Atan(1D/3), "arctg 1/3"};
			yield return new object[] {System.Math.Atan(0.25), "arctg 1/4"};
			yield return new object[] {System.Math.Atan(2), "arctg 2"};
		}

		[Theory]
		[MemberData(nameof(UnitNumeratorAngles))]
		public void AngleToString_KeepsUnitNumerator(double angle, string expected)
		{
			// Прежде числитель, равный единице, терялся: "arctg /2" вместо "arctg 1/2".
			var text = TestUtil.WithCulture("", () => CStatic.AngleToString(angle));

			Assert.Equal(expected, text);
		}

		[Theory]
		[InlineData(-2.5e10, "-2.5000·10¹⁰")]
		[InlineData(-12345678D, "-12345678")]
		[InlineData(-1.5e15, "-1.5000·10¹⁵")]
		[InlineData(-1e15, "-1.0000·10¹⁵")]
		[InlineData(-2e18, "-2.0000·10¹⁸")]
		[InlineData(-1.3333333333333332E+18, "-1.3333·10¹⁸")]
		[InlineData(-3523588.023857953, "-36106033/√105")]
		public void DoubleToString_NegativeNumbers(double value, string expected)
		{
			// Прежде условие близости к целому не выполнялось для отрицательных чисел, и они выводились иначе,
			// чем положительные: "-25000000000", "-1.2346·10⁷", "-10¹⁵", "-2E+18", "-4/3·10¹⁸", "-3.5236·10⁶".
			var text = TestUtil.WithCulture("", () => CStatic.DoubleToString(value));

			Assert.Equal(expected, text);
		}

		[Theory]
		[InlineData(2.5e10)]
		[InlineData(12345678D)]
		[InlineData(1.5e15)]
		[InlineData(2e18)]
		[InlineData(1.3333333333333332E+18)]
		[InlineData(3.2e19)]
		[InlineData(4.5e20)]
		[InlineData(3523588.023857953)]
		[InlineData(133333333333333.33)]
		[InlineData(0.123456789)]
		[InlineData(0.75)]
		[InlineData(7.25e-5)]
		[InlineData(System.Math.PI/4)]
		[InlineData(2.8284271247461903)]
		[InlineData(42D)]
		[InlineData(1e-5)]
		[InlineData(1e300)]
		public void DoubleToString_NegativeIsMinusPositive(double value)
		{
			var positive = TestUtil.WithCulture("", () => CStatic.DoubleToString(value));
			var negative = TestUtil.WithCulture("", () => CStatic.DoubleToString(-value));

			Assert.Equal("-" + positive, negative);
		}

		[Fact]
		public void GetIndex_MinValue()
		{
			// Прежде модуль int.MinValue вызывал OverflowException.
			Assert.Equal("₋₂₁₄₇₄₈₃₆₄₈", CStatic.GetIndex(int.MinValue));
			Assert.Equal("⁻²¹⁴⁷⁴⁸³⁶⁴⁸", CStatic.GetIndex(int.MinValue, true));
			Assert.Equal("²¹⁴⁷⁴⁸³⁶⁴⁷", CStatic.GetIndex(int.MaxValue, true));
		}

		[Theory]
		[InlineData("", 1e-310, "1.0000·10⁻³¹⁰")]
		[InlineData("", -1e-310, "-1.0000·10⁻³¹⁰")]
		[InlineData("", double.Epsilon, "4.9407·10⁻³²⁴")]
		[InlineData("", 2.2250738585072014E-308, "2.2251·10⁻³⁰⁸")]
		[InlineData("", 2.2250738585072014E-307, "2.2251·10⁻³⁰⁷")]
		[InlineData("", 1.2345678901e-305, "1.2346·10⁻³⁰⁵")]
		[InlineData("", -1.2345678901e-305, "-1.2346·10⁻³⁰⁵")]
		[InlineData("ru-RU", 1.2345678901e-305, "1,2346·10⁻³⁰⁵")]
		public void DoubleToString_VerySmallNumbers(string culture, double value, string expected)
		{
			// Прежде для чисел меньше 10⁻³⁰⁸ мантисса вычислялась с переполнением, и получались строки вроде
			// "-64/1490116119384765625·10⁻²⁸⁷" (с другим знаком), а DoubleToStringSimple выводил "Infinity·10⁻³⁰⁵".
			var text = TestUtil.WithCulture(culture, () => CStatic.DoubleToString(value));

			Assert.Equal(expected, text);
		}

		[Fact]
		public void DoubleToString_Zero()
		{
			// Ноль больше не попадает в поиск дроби, где его порядок равнялся бы −∞; вывод прежний.
			Assert.Equal("0", TestUtil.WithCulture("", () => CStatic.DoubleToString(0D)));
			Assert.Equal("0", TestUtil.WithCulture("", () => CStatic.DoubleToString(-0D)));
			Assert.Equal("0", TestUtil.WithCulture("", () => CStatic.AngleToString(-0D)));
		}

		[Fact]
		public void DoubleToStringSimple_VerySmallNumbers()
		{
			Assert.Equal("1.2346·10⁻³⁰⁵", TestUtil.WithCulture("", () => CStatic.DoubleToStringSimple(1.2345678901e-305, 8)));
			Assert.Equal("-4.9·10⁻³²⁴", TestUtil.WithCulture("", () => CStatic.DoubleToStringSimple(-double.Epsilon, 5)));
			Assert.Equal("1.0000·10⁻³¹⁰", TestUtil.WithCulture("", () => CStatic.AngleToString(1e-310)));
		}

		[Theory]
		[InlineData("en-US")]
		[InlineData("de-DE")]
		[InlineData("ru-RU")]
		public void NumberInfo_ParsesStringsInvariantly(string culture)
		{
			// Прежде строка разбиралась в текущей культуре: "1.5" давало 15 при de-DE и FormatException при ru-RU.
			var value = TestUtil.WithCulture(culture, () => new CStatic.NumberInfo("1.5").DoubleValue);

			Assert.Equal(1.5, value);
		}

		[Fact]
		public void NumberInfo_EpsilonIsDistanceToShortNotation()
		{
			var info = new CStatic.NumberInfo(1.0000001);

			Assert.Equal(1.0000001 - 1, info.Epsilon, 15);
			Assert.Equal("1.00000 + ε", TestUtil.WithCulture("", () => info.StringValue));
			Assert.Equal(0D, new CStatic.NumberInfo(0.25).Epsilon);
		}
	}
}
