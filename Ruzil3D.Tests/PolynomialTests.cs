using System.Linq;
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
		public void Division_WithTrailingZeroCoefficients()
		{
			// Прежде деление падало, если у делимого были нулевые старшие коэффициенты.
			var withZero = new Polynomial(1, 2, 3, 0);
			var x = Polynomial.Up;
			var afterArithmetic = (x*x*x - 1) - x*x*x + x*x;

			Polynomial rem;
			var quotient = Polynomial.DivRem(withZero, new Polynomial(1, 1), out rem);

			Assert.True(quotient == new Polynomial(-1, 3));
			Assert.True(rem == new Polynomial(2D));
			Assert.True(afterArithmetic/new Polynomial(-1, 1) == new Polynomial(1, 1));
			Assert.True(Polynomial.IsEmpty(afterArithmetic%new Polynomial(-1, 1)));
			Assert.NotNull(new Polynomial(-1, 0, 1, 0).ResolveString);
		}

		[Fact]
		public void GetDerivative_OrderAboveDegree_IsZero()
		{
			// Прежде создавался массив отрицательной длины (OverflowException).
			Assert.True(Polynomial.IsEmpty(new Polynomial(1, 2).GetDerivative(3)));
			Assert.True(Polynomial.IsEmpty(Polynomial.Empty.GetDerivative(2)));
		}

		[Fact]
		public void Resolve_QuadraticWithDistinctRoots()
		{
			var roots = new Polynomial(2, -3, 1).Resolve();

			Assert.Equal(new[] {1D, 2D}, roots);
		}

		[Fact]
		public void Resolve_QuinticWithSmallRoots()
		{
			var roots = Polynomial.GetPolynomialByRoots(-2, -1, 0, 1, 2).Resolve();

			Assert.Equal(5, roots.Length);
			Assert.Equal(new[] {-2D, -1D, 0D, 1D, 2D}, roots.Select(root => System.Math.Round(root, 9)).ToArray());
		}

		[Fact]
		public void Resolve_HugeCoefficients_DoesNotHang()
		{
			// Прежде NaN в критических точках (переполнение в формулах для производной) зацикливал метод Ньютона.
			var polynomial = Polynomial.GetPolynomialByRoots(1, 2, 3, 4, 5)*1e60;

			TestUtil.CompletesWithin(() => polynomial.Resolve());
		}

		[Fact]
		public void Resolve_HighDegree_IsNotExponential()
		{
			// Прежде каждый уровень заново решал обе свои производные: степень 32 решалась около 3 секунд, степень 40 — минуты.
			var polynomial = Polynomial.GetPolynomialByRoots(Enumerable.Range(1, 40).Select(i => i/10D).ToArray());

			TestUtil.CompletesWithin(() => polynomial.Resolve(), 5000);
		}
	}
}
