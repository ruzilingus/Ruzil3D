using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;
using Ruzil3D.Approximation;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты сеточной интерполяции.
	/// </summary>
	public class ApproximationTests
	{
		[Fact]
		public void LinearInterpolation_BetweenNodes()
		{
			var f = new LinearInterpolation(new[] {new PointD(0, 0), new PointD(1, 2), new PointD(3, 4)});

			Assert.Equal(1, f.GetValue(0.5), 12);
			Assert.Equal(3, f.GetValue(2), 12);
			Assert.Equal(4, f.GetValue(3), 12);
		}

		[Fact]
		public void CubicInterpolation_NaturalSpline()
		{
			var f = new CubicInterpolation(new[] {new PointD(0, 0), new PointD(1, 1), new PointD(2, 0)});

			Assert.Equal(0.6875, f.GetValue(0.5), 12);
			Assert.Equal(1, f.GetValue(1), 12);
		}

		[Fact]
		public void SingleNode_IsRejected()
		{
			// Прежде сетка из одного узла создавалась, а GetValue и GetIndex для неё не завершались.
			var node = new PointD(1, 5);

			Assert.Throws<ArgumentException>(() => new LinearInterpolation(new[] {node}));
			Assert.Throws<ArgumentException>(() => new LinearInterpolation(new List<PointD> {node}));
			Assert.Throws<ArgumentException>(() => new CubicInterpolation(new[] {node}));
			Assert.Throws<ArgumentException>(() => new CubicInterpolation(new[] {node, node}, true));
		}

		[Fact]
		public void EmptyOrNullNodes_AreRejected()
		{
			Assert.Throws<ArgumentException>(() => new LinearInterpolation(new PointD[0]));
			Assert.Throws<ArgumentNullException>(() => new LinearInterpolation((PointD[]) null));
			Assert.Throws<ArgumentNullException>(() => new CubicInterpolation((IEnumerable<PointD>) null));
		}

		[Fact]
		public void GetIndex_ReturnsCell()
		{
			var f = new LinearInterpolation(new[] {new PointD(0, 0), new PointD(1, 2), new PointD(3, 4)});

			Assert.Equal(0, f.GetIndex(0));
			Assert.Equal(0, f.GetIndex(0.5));
			Assert.Equal(1, f.GetIndex(1));
			Assert.Equal(1, f.GetIndex(3));
		}

		/// <summary>
		/// Кубическая интерполяция, многочлены которой сдвинуты на 100.
		/// </summary>
		private sealed class ShiftedCubic : CubicInterpolation
		{
			public ShiftedCubic(PointD[] points) : base(points)
			{
			}

			protected override void InitPolynomials(ref Polynomial[] result)
			{
				base.InitPolynomials(ref result);
				for (var i = 0; i < result.Length; i++)
				{
					result[i] += new Polynomial(100D);
				}
			}
		}

		/// <summary>
		/// Линейная интерполяция, многочлены которой сдвинуты на 100.
		/// </summary>
		private sealed class ShiftedLinear : LinearInterpolation
		{
			public ShiftedLinear(PointD[] points) : base(points)
			{
			}

			protected override void InitPolynomials(ref Polynomial[] result)
			{
				base.InitPolynomials(ref result);
				for (var i = 0; i < result.Length; i++)
				{
					result[i] += new Polynomial(100D);
				}
			}
		}

		[Fact]
		public void Interpolation_OverriddenInitPolynomials_AffectsValues()
		{
			// Как и прежде, значения наследника, переопределившего InitPolynomials, вычисляются по его многочленам;
			// промежуточная версия вычисляла их в локальной координате и не учитывала переопределение.
			var points = new[] {new PointD(0, 0), new PointD(1, System.Math.Sin(1)), new PointD(2, System.Math.Sin(2))};

			Assert.Equal(100 + System.Math.Sin(1), new ShiftedCubic(points).GetValue(1), 12);
			Assert.Equal(100 + System.Math.Sin(1), new ShiftedLinear(points).GetValue(1), 12);
			Assert.Equal(System.Math.Sin(1), new CubicInterpolation(points).GetValue(1), 15);
		}

		[Fact]
		public void LinearInterpolation_DuplicateNodesWithoutCheck_GetPolynomDoesNotThrow()
		{
			// При check = false узлы должны быть различными. Как и прежде, одинаковые X дают многочлен с бесконечными
			// коэффициентами или NaN, а не исключение, а остальные клетки не затрагиваются.
			var f = new LinearInterpolation(new[] {new PointD(0, 0), new PointD(1, 1), new PointD(1, 2), new PointD(2, 3)});

			Assert.True(new Polynomial(0D, 1D) == f.GetPolynom(0));
			Assert.True(double.IsInfinity(f.GetPolynom(1)[1]) || double.IsNaN(f.GetPolynom(1)[1]));
			Assert.True(new Polynomial(1D, 1D) == f.GetPolynom(2));
		}
	}
}
