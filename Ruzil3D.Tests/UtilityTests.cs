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
	}
}
