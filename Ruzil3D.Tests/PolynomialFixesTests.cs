using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ruzil3D.Algebra;
using Ruzil3D.Calculus;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Регрессионные тесты исправлений многочленов: поиска корней, многочленов Лежандра и Бернштейна,
	/// производной, интерполяции, равенства и преобразований.
	/// </summary>
	public class PolynomialFixesTests
	{
		#region Legendre polynomials

		[Fact]
		public void LegendreP2_HasCorrectCoefficients()
		{
			// Прежде P₂(x) = 1,5x² - 1 вместо (3x² - 1)/2: P₂(1) = 0,5.
			var legendre = new LegendrePolynomial(2);

			Assert.Equal(1, legendre.GetValue(1D));
			Assert.Equal(-0.5, legendre.GetValue(0D));
			Assert.EndsWith("(3 x² - 1) / 2", TestUtil.WithCulture("", () => legendre.ToString()));
		}

		[Fact]
		public void LegendreP2_GaussRuleIntegratesQuadratics()
		{
			// Прежде корни были равны ±0,8165 вместо ±1/√3, и двухточечное правило давало ∫x² = 1,33 вместо 2/3.
			var legendre = new LegendrePolynomial(2);

			Assert.InRange(legendre.Root(1), 1/System.Math.Sqrt(3) - 1e-15, 1/System.Math.Sqrt(3) + 1e-15);
			Assert.Equal(-legendre.Root(1), legendre.Root(0));

			var integral = Enumerable.Range(0, 2).Sum(i => legendre.GaussianWeight(i)*legendre.Root(i)*legendre.Root(i));
			Assert.InRange(integral, 2/3D - 1e-15, 2/3D + 1e-15);
		}

		[Fact]
		public void LegendreWeights_AreAccurate()
		{
			// Прежде веса вычислялись по раскрытому многочлену (1 - x²)·P′(x)² с коэффициентами до 3,5e7:
			// при n = 10 относительная погрешность весов достигала 2e-11, а их сумма отличалась от 2 на 5,6e-12.
			var expected = new[]
			{
				0.0666713443086881375935688, 0.1494513491505805931457763, 0.2190863625159820439955349,
				0.2692667193099963550912269, 0.2955242247147528701738930
			};

			var legendre = new LegendrePolynomial(10);
			for (var i = 0; i < 10; i++)
			{
				var weight = expected[i < 5 ? i : 9 - i];
				Assert.InRange(legendre.GaussianWeight(i), weight*(1 - 1e-13), weight*(1 + 1e-13));
			}

			foreach (var n in new[] {8, 9, 10})
			{
				var polynomial = new LegendrePolynomial(n);
				var sum = Enumerable.Range(0, n).Sum(i => polynomial.GaussianWeight(i));
				Assert.InRange(sum, 2 - 5e-14, 2 + 5e-14);
			}
		}

		[Fact]
		public void Gauss10_IntegratesPolynomialsExactly()
		{
			// Прежде из-за неточных весов правило Гаусса-10 было примерно в 1000 раз менее точным, чем Гаусса-5.
			var value = Calculus.Calculus.Integrate(x => x*x*x*x, 0, 1, EIntegrateRule.Gauss10);

			Assert.InRange(value, 0.2 - 1e-14, 0.2 + 1e-14);
		}

		#endregion
	}
}
