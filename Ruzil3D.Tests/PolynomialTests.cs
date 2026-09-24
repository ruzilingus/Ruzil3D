using Ruzil3D.Algebra;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты многочленов.
	/// </summary>
	public class PolynomialTests
	{
		[Fact]
		public void Multiplication()
		{
			var product = new Polynomial(1, 1)*new Polynomial(-1, 1);

			Assert.True(product == new Polynomial(-1, 0, 1));
		}

		[Fact]
		public void GetValue_UsesAllCoefficients()
		{
			var p = new Polynomial(1, 2, 3);

			Assert.Equal(17, p.GetValue(2D));
			Assert.Equal(1, p.GetValue(0D));
		}

		[Fact]
		public void GetDerivative()
		{
			var cube = new Polynomial(0, 0, 0, 1);

			Assert.True(cube.GetDerivative() == new Polynomial(0, 0, 3));
			Assert.True(cube.GetDerivative(2) == new Polynomial(0, 6));
			Assert.True(cube.GetDerivative(3) == new Polynomial(6D));
		}

		[Fact]
		public void DivRem_WithoutTrailingZeros()
		{
			// x³ - 1 = (x - 1)(x² + x + 1)
			Polynomial rem;
			var quotient = Polynomial.DivRem(new Polynomial(-1, 0, 0, 1), new Polynomial(-1, 1), out rem);

			Assert.True(quotient == new Polynomial(1, 1, 1));
			Assert.True(Polynomial.IsEmpty(rem));
		}

		[Fact]
		public void Resolve_QuadraticWithDistinctRoots()
		{
			var roots = new Polynomial(2, -3, 1).Resolve();

			Assert.Equal(new[] {1D, 2D}, roots);
		}
	}
}
