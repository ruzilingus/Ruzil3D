using System;
using Ruzil3D.Calculus;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты численного интегрирования.
	/// </summary>
	public class CalculusTests
	{
		public static TheoryData<EIntegrateRule, double> Rules => new TheoryData<EIntegrateRule, double>
		{
			{EIntegrateRule.Default, 1e-12},
			{EIntegrateRule.Rectangle, 1e-5},
			{EIntegrateRule.Trapezoidal, 1e-5},
			{EIntegrateRule.Simpson, 1e-12},
			{EIntegrateRule.Gauss2, 1e-12},
			{EIntegrateRule.Gauss3, 1e-12},
			{EIntegrateRule.Gauss5, 1e-12},
			{EIntegrateRule.Gauss6, 1e-12},
			{EIntegrateRule.Gauss10, 1e-10}
		};

		[Theory]
		[MemberData(nameof(Rules))]
		public void Integrate_Sine(EIntegrateRule rule, double tolerance)
		{
			var value = Calculus.Calculus.Integrate(System.Math.Sin, 0, System.Math.PI, rule);

			Assert.InRange(value, 2 - tolerance, 2 + tolerance);
		}

		[Theory]
		[MemberData(nameof(Rules))]
		public void Integrate_Polynomial(EIntegrateRule rule, double tolerance)
		{
			var value = Calculus.Calculus.Integrate(x => x*x*x*x, 0, 1, rule);

			Assert.InRange(value, 0.2 - tolerance, 0.2 + tolerance);
		}

		[Fact]
		public void Integrate_ReversedBoundsChangeSign()
		{
			Func<double, double> f = x => x*x;

			var forward = Calculus.Calculus.Integrate(f, 1, 4);
			var backward = Calculus.Calculus.Integrate(f, 4, 1);

			Assert.Equal(21, forward, 10);
			Assert.Equal(-forward, backward);
		}
	}
}
