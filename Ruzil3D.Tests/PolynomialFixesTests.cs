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
		#region Resolve

		[Fact]
		public void Resolve_Quintic_ReturnsOnlyRoots()
		{
			// Прежде метод Ньютона между критическими точками возвращал точки, не являющиеся корнями, и пропускал корни:
			// у (x + 10)(x + 1)x(x - 1)(x - 2) первым «корнем» было -8,93 (значение многочлена 8220).
			AssertRoots(new[] {-10D, -1, 0, 1, 2}, Polynomial.GetPolynomialByRoots(-10, -1, 0, 1, 2).Resolve(), 1e-12);
			AssertRoots(new[] {0.1, 0.2, 0.3, 0.4, 0.5}, Polynomial.GetPolynomialByRoots(0.1, 0.2, 0.3, 0.4, 0.5).Resolve(), 1e-12);
		}

		[Fact]
		public void Resolve_RandomIntegerRoots_AllFound()
		{
			// Прежде для многочленов пятой степени с различными целыми корнями из [-20, 20] ответ был неверен в 255 случаях из 308.
			var random = new Random(20260924);
			for (var degree = 3; degree <= 8; degree++)
			{
				for (var trial = 0; trial < 200; trial++)
				{
					var roots = Enumerable.Range(-100, 201).OrderBy(i => random.Next()).Take(degree)
						.Select(i => (double) i).OrderBy(root => root).ToArray();

					AssertRoots(roots, Polynomial.GetPolynomialByRoots(roots).Resolve(), 1e-6);
				}
			}
		}

		[Fact]
		public void Resolve_RandomRepeatedRoots_CountsMultiplicity()
		{
			// Прежде кратные корни в критических точках пропускались или повторялись неверное число раз.
			var random = new Random(777);
			for (var degree = 2; degree <= 8; degree++)
			{
				for (var trial = 0; trial < 300; trial++)
				{
					var roots = Enumerable.Range(0, degree).Select(i => (double) random.Next(-6, 7)).OrderBy(root => root).ToArray();
					var polynomial = Polynomial.GetPolynomialByRoots(roots);

					AssertRoots(roots, polynomial.Resolve(), 1e-6);
					AssertRoots(roots.Distinct().ToArray(), polynomial.Resolve(false), 1e-6);
				}
			}
		}

		[Fact]
		public void Resolve_ChebyshevPolynomial()
		{
			var expected = Enumerable.Range(1, 10).Select(k => System.Math.Cos((2*k - 1)*System.Math.PI/20)).OrderBy(x => x).ToArray();

			// Прежде у многочлена с корнями T₁₀ находился посторонний корень -0,3273, а три корня терялись.
			AssertRoots(expected, Polynomial.GetPolynomialByRoots(expected).Resolve(), 1e-13);

			// Многочлен Чебышёва с точными целыми коэффициентами из рекуррентного соотношения.
			var previous = Polynomial.Identity;
			var chebyshev = Polynomial.Up;
			for (var n = 2; n <= 10; n++)
			{
				var next = 2*Polynomial.Up*chebyshev - previous;
				previous = chebyshev;
				chebyshev = next;
			}

			AssertRoots(expected, chebyshev.Resolve(), 1e-13);
		}

		[Fact]
		public void Resolve_MultipleRoots()
		{
			// Прежде x⁵ давал восемь «корней», а Resolve(false) — четыре нуля; у (x - 1)⁴ параметр multiple не учитывался;
			// двукратный корень в критической точке (x - 1)²(x - 2)(x - 3)(x - 4) пропускался (получалось [2, 3, 4]).
			var monomial = Polynomial.GetPolynomial(5);
			Assert.Equal(new[] {0D, 0, 0, 0, 0}, monomial.Resolve());
			Assert.Equal(new[] {0D}, monomial.Resolve(false));

			var quadruple = Polynomial.GetPolynomialByRoots(1, 1, 1, 1);
			Assert.Equal(new[] {1D, 1, 1, 1}, quadruple.Resolve());
			Assert.Equal(new[] {1D}, quadruple.Resolve(false));

			var polynomial = Polynomial.GetPolynomialByRoots(1, 1, 2, 3, 4);
			AssertRoots(new[] {1D, 1, 2, 3, 4}, polynomial.Resolve(), 1e-9);
			AssertRoots(new[] {1D, 2, 3, 4}, polynomial.Resolve(false), 1e-9);

			AssertRoots(new[] {1D, 1, 1, 2, 2, 2}, Polynomial.GetPolynomialByRoots(1, 1, 1, 2, 2, 2).Resolve(), 1e-9);
		}

		[Fact]
		public void Resolve_QuadraticDoubleRoot_WithRoundedDiscriminant()
		{
			// Прежде дискриминант сравнивался с нулём точно, и двукратный корень терялся, когда после округления коэффициентов
			// дискриминант становился отрицательным: у x² - 0,42x + 0,0441 не было корней (так терялась пятая часть случаев).
			AssertRoots(new[] {0.21, 0.21}, new Polynomial(0.0441, -0.42, 1).Resolve(), 1e-12);

			for (var k = 1; k < 1000; k++)
			{
				var root = k/1000m;
				var expected = new[] {(double) root, (double) root};

				AssertRoots(expected, new Polynomial(Parse(root*root), Parse(-2*root), 1).Resolve(), 1e-7);
				AssertRoots(expected, new Polynomial(Parse(3*root*root), Parse(-6*root), 3).Resolve(), 1e-7);
			}
		}

		[Fact]
		public void Resolve_QuadraticWithoutCancellation()
		{
			// Прежде по школьной формуле малый корень x² - 1e8·x + 1 получался равным 7,45e-9 вместо 1e-8,
			// а у 1e-10·x² + x + 1e-6 — равным -1,11e-6 вместо -1e-6.
			var roots = new Polynomial(1, -1e8, 1).Resolve();
			Assert.Equal(2, roots.Length);
			Assert.InRange(roots[0], 1e-8*(1 - 1e-15), 1e-8*(1 + 1e-15));
			Assert.InRange(roots[1], 1e8*(1 - 1e-15), 1e8*(1 + 1e-15));

			roots = new Polynomial(1e-6, 1, 1e-10).Resolve();
			Assert.Equal(2, roots.Length);
			Assert.InRange(roots[0], -1e10*(1 + 1e-15), -1e10*(1 - 1e-15));
			Assert.InRange(roots[1], -1e-6*(1 + 1e-15), -1e-6*(1 - 1e-15));

			Assert.Equal(new[] {1D, 2D}, new Polynomial(2, -3, 1).Resolve());
		}

		[Fact]
		public void Resolve_CubicSpecialCases()
		{
			// Прежде проверка трёхкратного корня срабатывала и при двукратном: у (x + 1,5)²(x - 3) терялся корень 3.
			AssertRoots(new[] {-1.5, -1.5, 3}, new Polynomial(-6.75, -6.75, 0, 1).Resolve(), 1e-12);

			// Прежде точное сравнение с нулём теряло двукратный корень: получался один корень -7,79000000000002.
			AssertRoots(new[] {-7.79, -5.03, -5.03}, Polynomial.GetPolynomialByRoots(-5.03, -5.03, -7.79).Resolve(), 1e-9);

			// Прежде промежуточные величины переполнялись ([-∞, NaN, ∞]) или теряли точность ([1,99998, 2,00001, 2,00001]).
			AssertRoots(new[] {1D, 2, 3}, (Polynomial.GetPolynomialByRoots(1, 2, 3)*1e52).Resolve(), 1e-12);
			AssertRoots(new[] {1D, 2, 3}, (Polynomial.GetPolynomialByRoots(1, 2, 3)*1e-55).Resolve(), 1e-12);

			// Прежде относительная погрешность корня достигала 1,7e-8.
			AssertRoots(new[] {-103.538807421030535115268918}, new Polynomial(1.11e6, 1/3D, 0, 1).Resolve(), 1e-14);
		}

		[Fact]
		public void Resolve_RandomCubicsWithDoubleRoot()
		{
			// Прежде двукратный корень терялся почти в половине случаев (906 из 1919).
			var random = new Random(503);
			for (var trial = 0; trial < 500; trial++)
			{
				var doubleRoot = System.Math.Round(random.NextDouble()*20 - 10, 2);
				var simpleRoot = System.Math.Round(random.NextDouble()*20 - 10, 2);
				if (System.Math.Abs(doubleRoot - simpleRoot) < 0.1)
				{
					continue;
				}

				var expected = new[] {doubleRoot, doubleRoot, simpleRoot}.OrderBy(root => root).ToArray();

				AssertRoots(expected, Polynomial.GetPolynomialByRoots(doubleRoot, doubleRoot, simpleRoot).Resolve(), 1e-6);
			}
		}

		[Fact]
		public void Resolve_QuarticSpecialCases()
		{
			// Прежде у этих многочленов не находилось ни одного корня.
			AssertRoots(new[] {0.1, 0.2, 0.3, 0.4}, Polynomial.GetPolynomialByRoots(0.1, 0.2, 0.3, 0.4).Resolve(), 1e-12);
			AssertRoots(new[] {1000D, 2100, 3050, 4500}, Polynomial.GetPolynomialByRoots(1000, 2100, 3050, 4500).Resolve(), 1e-12);
			AssertRoots(new[] {1D, 2, 3, 4}, (Polynomial.GetPolynomialByRoots(1, 2, 3, 4)*1e-30).Resolve(), 1e-12);
		}

		[Fact]
		public void Resolve_RandomQuartics()
		{
			// Прежде для корней из [-1000, 1000] корни терялись в 238 случаях из 827: биквадратная ветвь требовала точного q = 0,
			// а абсолютный порог 1e-12 для мнимой части отбрасывал вещественные корни.
			var random = new Random(4242);
			for (var trial = 0; trial < 500; trial++)
			{
				var roots = Enumerable.Range(0, 4).Select(i => System.Math.Round(random.NextDouble()*2000 - 1000, 2)).OrderBy(root => root).ToArray();
				if (roots.Zip(roots.Skip(1), (a, b) => b - a).Min() < 1)
				{
					continue;
				}

				AssertRoots(roots, Polynomial.GetPolynomialByRoots(roots).Resolve(), 1e-6);
			}
		}

		[Fact]
		public void Resolve_ScaledPolynomial_SameRoots()
		{
			// Прежде при умножении многочлена на большое или малое число формулы переполнялись или теряли точность.
			var random = new Random(60);
			for (var trial = 0; trial < 100; trial++)
			{
				var degree = 2 + trial%7;
				var roots = Enumerable.Range(-30, 61).OrderBy(i => random.Next()).Take(degree).Select(i => i/3D).OrderBy(root => root).ToArray();
				var polynomial = Polynomial.GetPolynomialByRoots(roots);
				var expected = polynomial.Resolve();

				AssertRoots(roots, expected, 1e-6);
				AssertRoots(expected, (polynomial*1e60).Resolve(), 1e-9);
				AssertRoots(expected, (polynomial*-1e-60).Resolve(), 1e-9);

				// Умножение на степень двойки не округляет коэффициенты, поэтому корни совпадают до последнего бита.
				Assert.Equal(expected, (polynomial*System.Math.Pow(2, 200)).Resolve());
				Assert.Equal(expected, (polynomial*System.Math.Pow(2, -200)).Resolve());
			}
		}

		[Fact]
		public void ResolveCubicReal_SpecialCases()
		{
			// Прежде: [-1,5, -1,5, -1,5] (терялся корень 3), [-∞, NaN, ∞] и деление на 0 при a = 0.
			AssertRoots(new[] {-1.5, -1.5, 3}, Polynomial.ResolveCubicReal(1, 0, -6.75, -6.75), 1e-12);
			AssertRoots(new[] {-1.5, 3}, Polynomial.ResolveCubicReal(1, 0, -6.75, -6.75, false), 1e-12);
			AssertRoots(new[] {1D, 2, 3}, Polynomial.ResolveCubicReal(1e52, -6e52, 11e52, -6e52), 1e-12);
			AssertRoots(new[] {1D, 2, 3}, Polynomial.ResolveCubicReal(-1, 6, -11, 6, false), 1e-12);
			AssertRoots(new[] {1D, 2}, Polynomial.ResolveCubicReal(0, 1, -3, 2), 1e-12);
		}

		#endregion

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
		public void GetPolynomial_TwoPointsWithSameX_Throws()
		{
			// Прежде перегрузка для двух точек возвращала многочлен с бесконечными коэффициентами (или NaN для совпадающих точек).
			Assert.Equal("point1", Assert.Throws<ArgumentException>(() => Polynomial.GetPolynomial(new PointD(1, 1), new PointD(1, 2))).ParamName);
			Assert.Throws<ArgumentException>(() => Polynomial.GetPolynomial(new PointD(1, 1), new PointD(1, 1)));

			Assert.True(Polynomial.GetPolynomial(new PointD(1, 3), new PointD(3, 7)) == new Polynomial(1, 2));
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

		#region Helpers

		/// <summary>
		/// Проверяет, что найденные корни совпадают с ожидаемыми с учётом порядка и кратности.
		/// </summary>
		/// <param name="expected">Ожидаемые корни в порядке возрастания.</param>
		/// <param name="actual">Найденные корни.</param>
		/// <param name="tolerance">Допустимая погрешность относительно max(1, |корень|).</param>
		private static void AssertRoots(double[] expected, double[] actual, double tolerance)
		{
			var message = "Ожидалось " + Format(expected) + ", получено " + Format(actual) + ".";

			Assert.True(expected.Length == actual.Length, message);
			for (var i = 0; i < expected.Length; i++)
			{
				Assert.True(System.Math.Abs(actual[i] - expected[i]) <= tolerance*System.Math.Max(1, System.Math.Abs(expected[i])), message);
			}
		}

		private static string Format(IEnumerable<double> values)
		{
			return "[" + string.Join(", ", values.Select(value => value.ToString("R", CultureInfo.InvariantCulture))) + "]";
		}

		/// <summary>
		/// Возвращает ближайшее к десятичному значению число двойной точности, как при записи коэффициента литералом.
		/// </summary>
		private static double Parse(decimal value)
		{
			return double.Parse(value.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
		}

		#endregion
	}
}
