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

		#region Bernstein polynomials and derivatives

		[Fact]
		public void Bernstein_HighDegree_UsesExactBinomials()
		{
			// Прежде биномиальные коэффициенты вычислялись в int и переполнялись, начиная с n = 13:
			// B(9, 17)(0,5) был равен 0,0049 вместо 0,1855, а B(15, 31) — тождественно нулю.
			Assert.Equal(0.1854705810546875, Polynomial.GetBernstein(9, 17).GetValue(0.5));

			var expected = 300540195/2147483648D;
			Assert.InRange(Polynomial.GetBernstein(15, 31).GetValue(0.5), expected*(1 - 1e-12), expected*(1 + 1e-12));

			// Базисные многочлены Бернштейна образуют разбиение единицы (прежде при n = 20 сумма была равна 0,79).
			var sum20 = Enumerable.Range(0, 21).Sum(k => Polynomial.GetBernstein(k, 20).GetValue(0.3));
			Assert.InRange(sum20, 1 - 1e-12, 1 + 1e-12);

			var sum31 = Enumerable.Range(0, 32).Sum(k => Polynomial.GetBernstein(k, 31).GetValue(0.3));
			Assert.InRange(sum31, 1 - 1e-9, 1 + 1e-9);
		}

		[Fact]
		public void GetDerivative_HighOrder_DoesNotOverflow()
		{
			// Прежде убывающий факториал вычислялся в int: x¹³ после 13 дифференцирований давал 1932053504 вместо 13!.
			Assert.True(Polynomial.GetPolynomial(13).GetDerivative(13) == new Polynomial(6227020800D));
			Assert.True(Polynomial.GetPolynomial(20).GetDerivative(18) == new Polynomial(0, 0, 1216451004088320000D));
		}

		#endregion

		#region Interpolation, equality and conversions

		[Fact]
		public void GetPolynomial_SinglePoint_IsConstant()
		{
			// Прежде для одной точки возвращался null.
			var polynomial = Polynomial.GetPolynomial(new[] {new PointD(2, 5)});

			Assert.NotNull(polynomial);
			Assert.True(polynomial == new Polynomial(5D));
		}

		[Fact]
		public void GetPolynomial_DuplicateX_Throws()
		{
			// Прежде деление на ноль давало многочлен с бесконечными коэффициентами.
			Assert.Throws<ArgumentException>(() => Polynomial.GetPolynomial(new[] {new PointD(1, 1), new PointD(1, 2)}));
			Assert.Throws<ArgumentException>(() => Polynomial.GetPolynomial(new[] {new PointD(0, 0), new PointD(1, 1), new PointD(0, 3)}));

			// Различные узлы по-прежнему допустимы.
			Assert.True(Polynomial.GetPolynomial(new[] {new PointD(0, 1), new PointD(1, 3), new PointD(2, 7)}) == new Polynomial(1, 1, 1));
		}

		[Fact]
		public void GetHashCode_IsConsistentWithEquality()
		{
			// Прежде хэш-код всегда был равен 0.
			Assert.Equal(new Polynomial(1, 2).GetHashCode(), new Polynomial(1, 2, 0, 0).GetHashCode());
			Assert.Equal(new Polynomial(-0D, 1).GetHashCode(), new Polynomial(0D, 1).GetHashCode());
			Assert.Equal(Polynomial.Empty.GetHashCode(), new Polynomial(0, -0D).GetHashCode());
			Assert.Equal(new Polynomial(double.NaN, 1).GetHashCode(), new Polynomial(-double.NaN, 1).GetHashCode());

			var polynomials = Enumerable.Range(0, 1000).Select(i => new Polynomial(i%10, i/10%10, i/100)).ToArray();
			Assert.True(polynomials.Select(p => p.GetHashCode()).Distinct().Count() > 900);
			Assert.Equal(1000, new HashSet<Polynomial>(polynomials).Count);
		}

		[Fact]
		public void Equals_IsConsistentWithOperator()
		{
			// Прежде Equals требовал совпадения типов: LegendrePolynomial(1) == Polynomial.Up, но Equals возвращал false.
			var legendre = new LegendrePolynomial(1);

			Assert.True(legendre == Polynomial.Up);
			Assert.True(legendre.Equals(Polynomial.Up));
			Assert.True(Polynomial.Up.Equals(legendre));
			Assert.Equal(legendre.GetHashCode(), Polynomial.Up.GetHashCode());
			Assert.False(Polynomial.Up.Equals(null));
			Assert.False(Polynomial.Up.Equals("x"));
		}

		[Fact]
		public void Pow_Zero_IsIdentity()
		{
			// Прежде Polynomial.Empty.Pow(0) возвращал 0, хотя new Polynomial(0).Pow(0) возвращал 1.
			Assert.True(Polynomial.IsIdentity(Polynomial.Empty.Pow(0)));
			Assert.True(Polynomial.IsIdentity(new Polynomial(0D).Pow(0)));
			Assert.True(Polynomial.IsEmpty(Polynomial.Empty.Pow(3)));
		}

		[Fact]
		public void ToType_StringAndOwnType()
		{
			// Прежде ToType(typeof(string)) выбрасывал InvalidCastException, а преобразование в LegendrePolynomial пыталось получить число.
			IConvertible polynomial = new Polynomial(1, 1);
			var legendre = new LegendrePolynomial(3);

			TestUtil.WithCulture("", () =>
			{
				Assert.Equal(polynomial.ToString(), polynomial.ToType(typeof(string), CultureInfo.InvariantCulture));
				Assert.Equal(Convert.ToString(polynomial, CultureInfo.InvariantCulture), polynomial.ToType(typeof(string), CultureInfo.InvariantCulture));
				return 0;
			});

			Assert.Same(polynomial, polynomial.ToType(typeof(Polynomial), null));
			Assert.Same(legendre, ((IConvertible) legendre).ToType(typeof(LegendrePolynomial), null));
			Assert.Same(legendre, ((IConvertible) legendre).ToType(typeof(Polynomial), null));
			Assert.Equal(5D, ((IConvertible) new Polynomial(5D)).ToType(typeof(double), null));
		}

		#endregion
	}
}
