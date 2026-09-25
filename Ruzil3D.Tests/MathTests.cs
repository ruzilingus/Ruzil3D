using System;
using Ruzil3D.Algebra;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты общих математических функций класса <see cref="Math"/>.
	/// </summary>
	public class MathTests
	{
		[Fact]
		public void GreatestDivisor_Integers()
		{
			Assert.Equal(6, Math.GreatestDivisor(12, -18));
			Assert.Equal(7L, Math.GreatestDivisor(0L, 7L));
		}

		[Fact]
		public void GreatestDivisor_MinValue()
		{
			// Прежде Abs(MinValue) выбрасывал OverflowException, хотя эти делители представимы.
			Assert.Equal(2L, Math.GreatestDivisor(long.MinValue, 6L));
			Assert.Equal(1L, Math.GreatestDivisor(-3L, long.MinValue));
			Assert.Equal(1L << 62, Math.GreatestDivisor(long.MinValue, 1L << 62));
			Assert.Equal(2, Math.GreatestDivisor(int.MinValue, -6));
			Assert.Equal(1 << 30, Math.GreatestDivisor(1 << 30, int.MinValue));

			// 2⁶³ и 2³¹ не представимы.
			Assert.Throws<OverflowException>(() => Math.GreatestDivisor(long.MinValue, 0L));
			Assert.Throws<OverflowException>(() => Math.GreatestDivisor(long.MinValue, long.MinValue));
			Assert.Throws<OverflowException>(() => Math.GreatestDivisor(0, int.MinValue));
			Assert.Throws<ArgumentException>(() => Math.GreatestDivisor(0L, 0L));

			var numerator = long.MinValue;
			var divider = 6L;
			Math.Simplify(ref numerator, ref divider);
			Assert.Equal(long.MinValue/2, numerator);
			Assert.Equal(3L, divider);

			// Прежние результаты не изменились.
			Assert.Equal(6L, Math.GreatestDivisor(-12L, 18L));
			Assert.Equal(7, Math.GreatestDivisor(7, 0));
			Assert.Equal(long.MaxValue, Math.GreatestDivisor(long.MaxValue, -long.MaxValue));
		}

		[Fact]
		public void GreatestDivisor_DoubleNaNOrInfinity_DoesNotHang()
		{
			// Прежде остаток был NaN, и алгоритм Евклида не завершался.
			Assert.True(double.IsNaN(Math.GreatestDivisor(1D, double.NaN)));
			Assert.True(double.IsNaN(Math.GreatestDivisor(double.PositiveInfinity, 6D)));

			var numerator = double.NaN;
			var divider = 4D;
			Math.Simplify(ref numerator, ref divider);
			Assert.True(double.IsNaN(numerator));

			Assert.Equal(1.5, Math.GreatestDivisor(4.5, 6D));
		}

		[Fact]
		public void GreatestDivisor_Fractions()
		{
			var gcd = Math.GreatestDivisor(new Fraction(1, 2), new Fraction(1, 3));

			Assert.True(gcd == new Fraction(1, 6));
		}

		[Fact]
		public void GreatestDivisor_FractionOverflow_DoesNotHang()
		{
			// Прежде после переполнения промежуточных дробей алгоритм Евклида зацикливался.
			var errors = new[]
			{
				Record.Exception(() => TestUtil.CompletesWithin(() => Math.GreatestDivisor((Fraction) System.Math.PI, new Fraction(1)))),
				Record.Exception(() => TestUtil.CompletesWithin(() => Math.GreatestDivisor(new Fraction(1, 1L << 40), new Fraction(1, 3))))
			};

			foreach (var error in errors)
			{
				Assert.True(error == null || error is OverflowException, error?.ToString());
			}
		}
	}
}
