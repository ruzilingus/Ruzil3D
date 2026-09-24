using System;
using Ruzil3D.Utility;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты вспомогательных функций форматирования.
	/// </summary>
	public class UtilityTests
	{
		[Theory]
		[InlineData(System.Math.PI/4, "π/4")]
		[InlineData(0.5, "1/2")]
		[InlineData(-3, "-3")]
		[InlineData(double.PositiveInfinity, "∞")]
		[InlineData(double.NaN, "NaN")]
		public void DoubleToString_RecognizesFractionsAndConstants(double value, string expected)
		{
			var text = TestUtil.WithCulture("", () => CStatic.DoubleToString(value));

			Assert.Equal(expected, text);
		}

		[Fact]
		public void DoubleToString_RecognizesRadicals()
		{
			var text = TestUtil.WithCulture("", () => CStatic.DoubleToString(2*System.Math.Sqrt(2)));

			Assert.Equal("2√2", text);
		}

		[Fact]
		public void GetIndex_BuildsSubscriptsAndSuperscripts()
		{
			Assert.Equal("₁₂", CStatic.GetIndex(12));
			Assert.Equal("⁻³", CStatic.GetIndex(-3, true));
		}

		[Fact]
		public void TickCounter_WorksOnEveryPlatform()
		{
			// Прежде счётчик вызывал kernel32.dll и вне Windows выбрасывал TypeInitializationException.
			var start = TickCounter.TickCount;
			var end = TickCounter.TickCount;

			Assert.True(start > 0);
			Assert.True(end >= start);
		}

		[Theory]
		[InlineData(5D, 2, "5.0")]
		[InlineData(999.5, 2, "1000")]
		[InlineData(12345D, 2, "1·10⁴")]
		[InlineData(12345.678, 8, "1.2346·10⁴")]
		[InlineData(double.NaN, 5, "NaN")]
		[InlineData(double.NegativeInfinity, 5, "-∞")]
		public void DoubleToStringSimple_AnyDigitsAndSpecialValues(double value, int digits, string expected)
		{
			// Прежде при малом числе значащих цифр отрицательная длина строки нулей приводила к исключению.
			var text = TestUtil.WithCulture("", () => CStatic.DoubleToStringSimple(value, digits));

			Assert.Equal(expected, text);
		}

		[Fact]
		public void DoubleToStringSimple_RejectsNonPositiveDigits()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => CStatic.DoubleToStringSimple(1, 0));
		}
	}
}
