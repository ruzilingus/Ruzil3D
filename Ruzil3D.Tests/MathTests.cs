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
