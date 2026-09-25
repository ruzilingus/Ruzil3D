using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ruzil3D.Algebra;
using Ruzil3D.Calculus;
using Ruzil3D.Curves;
using Ruzil3D.Filters;
using Ruzil3D.Geometry;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Коллекция тестов, которые не запускаются параллельно с другими: они сбрасывают общие статические кэши библиотеки.
	/// </summary>
	[CollectionDefinition(nameof(ConcurrencyCollection), DisableParallelization = true)]
	public class ConcurrencyCollection
	{
	}

	/// <summary>
	/// Тесты потокобезопасности общих кэшей и объектов.
	/// </summary>
	[Collection(nameof(ConcurrencyCollection))]
	public class ConcurrencyTests
	{
		private const int ThreadCount = 8;

		[Fact]
		public void ToStringCache_ConcurrentAccess()
		{
			// Прежде кэш ToString был общим словарём без блокировок: одновременные вызовы из разных потоков
			// необратимо портили его, и после этого ToString падал даже в одном потоке.
			const int count = 600;
			var polynomials = Enumerable.Range(0, count).Select(i => new Polynomial(i, i%7 + 1, 1)).ToArray();
			var linears = Enumerable.Range(0, count).Select(i => new Linear(new double[] {i%5 + 1, 2}, i)).ToArray();
			var expectedPolynomials = polynomials.Select(p => p.ToString()).ToArray();
			var expectedLinears = linears.Select(l => l.ToString()).ToArray();

			var wrong = 0;
			var errors = TestUtil.RunConcurrently(ThreadCount, thread =>
			{
				for (var round = 0; round < 3; round++)
				{
					for (var i = 0; i < count; i++)
					{
						var index = (i*(thread + 1) + round)%count;
						if (polynomials[index].ToString() != expectedPolynomials[index] ||
						    linears[index].ToString() != expectedLinears[index])
						{
							System.Threading.Interlocked.Increment(ref wrong);
						}
					}
				}
			});

			Assert.Empty(errors);
			Assert.Equal(0, wrong);
			Assert.Equal("x² + 2 x + 1", new Polynomial(1, 2, 1).ToString());
		}

		[Fact]
		public void ToStringCache_KeyedByValueAndCulture()
		{
			// Прежде ключом кэша служила сама структура Linear, делящая изменяемый массив A:
			// после изменения A кэш возвращал старую строку, в том числе для других уравнений.
			var linear = new Linear(new double[] {1, 2}, 3);
			var original = linear.ToString();

			linear.A[0] = 5;

			Assert.Equal("5 x₁ + 2 x₂ = 3", linear.ToString());
			Assert.Equal(original, new Linear(new double[] {1, 2}, 3).ToString());

			// Вывод чисел зависит от культуры, поэтому строки для разных культур кэшируются раздельно.
			var fractional = new Linear(new[] {0.123456789, 1}, 2);
			var russian = TestUtil.WithCulture("ru-RU", () => fractional.ToString());
			var invariant = TestUtil.WithCulture("", () => fractional.ToString());

			Assert.Contains("0,1234568", russian);
			Assert.Contains("0.1234568", invariant);
		}

		[Fact]
		public void LegendreCaches_ConcurrentFirstUse()
		{
			// Прежде одновременное первое обращение к корням и весам многочленов Лежандра портило общие списки-кэши.
			var degrees = Enumerable.Range(3, 8).ToArray();
			var expected = degrees.ToDictionary(n => n, n => Nodes(new LegendrePolynomial(n)));

			ResetLegendreCaches();

			var errors = TestUtil.RunConcurrently(ThreadCount*2, thread =>
			{
				var polynomial = new LegendrePolynomial(degrees[thread%degrees.Length]);
				Assert.Equal(expected[polynomial.Deg], Nodes(polynomial));
			});

			Assert.Empty(errors);
		}

		[Fact]
		public void BernsteinCache_ConcurrentUse()
		{
			var errors = TestUtil.RunConcurrently(ThreadCount, thread =>
			{
				for (var n = 1; n <= 12; n++)
				{
					// Базисные многочлены Бернштейна образуют разбиение единицы.
					var sum = Enumerable.Range(0, n + 1).Sum(k => Polynomial.GetBernstein(k, n).GetValue(0.3));
					Assert.Equal(1, sum, 12);
				}
			});

			Assert.Empty(errors);
		}

		[Fact]
		public void ParametricCurves_ConcurrentEnumeration()
		{
			// Прежде все обходы коллекции делили один перечислитель.
			var points = Enumerable.Range(0, 31).Select(i => new Point3D(i, i%3, 0)).ToArray();
			var path = new Beziers(points);

			var counts = new int[ThreadCount];
			var errors = TestUtil.RunConcurrently(ThreadCount, thread =>
			{
				for (var round = 0; round < 200; round++)
				{
					counts[thread] = path.Count();
					Assert.Equal(10, counts[thread]);
				}
			});

			Assert.Empty(errors);
		}

		[Fact]
		public void CurveBlurFilter_ConcurrentSearchCorners()
		{
			// Прежде точка излома хранилась в полях фильтра, и одновременные вызовы давали неверные результаты.
			var points = new List<Point3D>();
			for (var i = 0; i <= 40; i++)
			{
				points.Add(new Point3D(i*0.25, 0, 0));
			}

			for (var i = 1; i <= 40; i++)
			{
				points.Add(new Point3D(10, i*0.25, 0));
			}

			var filter = new CurveBlurFilter(points);
			var expected = filter.SearchCorners();

			Assert.Equal(new[] {40}, expected);

			var errors = TestUtil.RunConcurrently(ThreadCount, thread =>
			{
				for (var round = 0; round < 10; round++)
				{
					Assert.Equal(expected, filter.SearchCorners());
				}
			});

			Assert.Empty(errors);
		}

		[Fact]
		public void Isometry_Quaternion_ConcurrentFirstReads()
		{
			// Кватернион вычисляется при первом обращении и сохраняется. Прежний кэш в поле-структуре мог быть прочитан
			// другим потоком частично записанным; теперь все потоки, одновременно читающие новый объект, получают одно значение.
			const int count = 3000;
			var random = new Random(8);
			var angles = new double[count];
			var axes = new Point3D[count];
			for (var i = 0; i < count; i++)
			{
				angles[i] = random.NextDouble()*7;
				axes[i] = new Point3D(random.NextDouble() - 0.5, random.NextDouble() - 0.5, random.NextDouble() - 0.5);
			}

			var expected = Enumerable.Range(0, count).Select(i => new Isometry(angles[i], axes[i]).Quaternion).ToArray();
			var isometries = Enumerable.Range(0, count).Select(i => new Isometry(angles[i], axes[i])).ToArray();

			var wrong = 0;
			var errors = TestUtil.RunConcurrently(ThreadCount, thread =>
			{
				for (var i = 0; i < count; i++)
				{
					if (!isometries[i].Quaternion.Equals(expected[i]))
					{
						System.Threading.Interlocked.Increment(ref wrong);
					}
				}
			});

			Assert.Empty(errors);
			Assert.Equal(0, wrong);
		}

		[Fact]
		public void DistanceCompiler_CurveReplacedDuringUse()
		{
			// Кривая, замененная во время компиляции, могла навсегда остаться с аппроксимацией прежней кривой, а GetValue
			// применял к новой кривой параметр, найденный для прежней. Теперь каждое обращение использует кривую и
			// аппроксимацию из одного результата компиляции.
			var c1 = new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 1, 0), new Point3D(2, 1, 0), new Point3D(3, 0, 0));
			var c2 = new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 3, 0), new Point3D(4, 3, 0), new Point3D(6, 0, 0));
			var expected1 = new ParametricCurveDistanceCompiler<BezierCurve>(c1);
			var expected2 = new ParametricCurveDistanceCompiler<BezierCurve>(c2);
			var distances = Enumerable.Range(0, 8).Select(i => System.Math.Min(c1.Length, c2.Length)*i/7).ToArray();

			var wrong = 0;
			var stale = 0;

			for (var round = 0; round < 20; round++)
			{
				var compiler = new ParametricCurveDistanceCompiler<BezierCurve>(c1);
				var stop = 0;
				var reads = 0;

				var errors = TestUtil.RunConcurrently(ThreadCount, thread =>
				{
					if (thread == 0)
					{
						//Кривая заменяется, пока другие потоки не выполнят достаточно обращений.
						while (System.Threading.Interlocked.CompareExchange(ref reads, 0, 0) < 400)
						{
							compiler.Curve = c2;
							compiler.Curve = c1;
						}

						compiler.Curve = c2;
						System.Threading.Interlocked.Exchange(ref stop, 1);
						return;
					}

					while (System.Threading.Interlocked.CompareExchange(ref stop, 0, 0) == 0)
					{
						foreach (var distance in distances)
						{
							var point = compiler.GetValue(distance);
							if (point != expected1.GetValue(distance) && point != expected2.GetValue(distance))
							{
								System.Threading.Interlocked.Increment(ref wrong);
							}
						}

						System.Threading.Interlocked.Increment(ref reads);
					}
				});

				Assert.Empty(errors);
				if (distances.Any(distance => compiler.GetValue(distance) != expected2.GetValue(distance)))
				{
					stale++;
				}
			}

			Assert.Equal(0, wrong);
			Assert.Equal(0, stale);
		}

		private static double[] Nodes(LegendrePolynomial polynomial)
		{
			var result = new List<double>();
			for (var i = 0; i < polynomial.Deg; i++)
			{
				result.Add(polynomial.Root(i));
				result.Add(polynomial.GaussianWeight(i));
			}

			return result.ToArray();
		}

		/// <summary>
		/// Очищает закрытые статические кэши <see cref="LegendrePolynomial"/>, чтобы следующее обращение было первым.
		/// </summary>
		private static void ResetLegendreCaches()
		{
			const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
			var type = typeof(LegendrePolynomial);
			var cacheLock = type.GetField("CacheLock", flags).GetValue(null);

			lock (cacheLock)
			{
				foreach (var name in new[] {"Roots", "GaussianWeights", "Derivatives", "GaussianDenominators"})
				{
					((IList) type.GetField(name, flags).GetValue(null)).Clear();
				}
			}
		}
	}
}
