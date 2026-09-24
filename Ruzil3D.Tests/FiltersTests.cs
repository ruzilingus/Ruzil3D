using System;
using Ruzil3D.Algebra;
using Ruzil3D.Filters;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты фильтров сглаживания.
	/// </summary>
	public class FiltersTests
	{
		private static Point3D[] StraightLine(int count)
		{
			var points = new Point3D[count];
			for (var i = 0; i < count; i++)
			{
				points[i] = new Point3D(i*0.5, i, 0);
			}

			return points;
		}

		[Fact]
		public void CurveInterpolationCompiler_InterpolatesBetweenPoints()
		{
			var compiler = new CurveInterpolationCompiler(new[] {Point3D.Empty, new Point3D(2, 0, 0), new Point3D(2, 3, 0)});

			Assert.Equal(5, compiler.DMax, 12);
			TestUtil.Near(new Point3D(1, 0, 0), compiler.GetValue3D(1));
			TestUtil.Near(new Point3D(2, 1, 0), compiler.GetValue3D(3));
		}

		[Fact]
		public void InterpolationCompiler_RejectsDegenerateInput()
		{
			// Прежде с одной точкой GetValue не завершался.
			Assert.Throws<ArgumentException>(() => new InterpolationCompiler(new[] {new PointD(0, 1)}));
			Assert.Throws<ArgumentException>(() => new InterpolationCompiler(new[] {new PointD(0, 1), new PointD(0, 2)}));
			Assert.Throws<ArgumentException>(() => new InterpolationCompiler(new PointD[0]));
			Assert.Throws<ArgumentNullException>(() => new InterpolationCompiler(null));
		}

		[Fact]
		public void InterpolationCompiler_InterpolatesBetweenPoints()
		{
			var compiler = new InterpolationCompiler(new[] {new PointD(0, 1), new PointD(2, 5), new PointD(3, 5)});

			Assert.Equal(3, compiler.GetValue(1), 12);
			Assert.Equal(5, compiler.GetValue(2.5), 12);
		}

		[Fact]
		public void CurveBlurFilter_KeepsStraightLine()
		{
			var points = StraightLine(21);
			var filter = new CurveBlurFilter(points);

			for (var d = 0D; d <= filter.DMax; d += filter.DMax/40)
			{
				var value = filter.GetValue3D(d);
				// Точка остаётся на прямой y = 2x, z = 0.
				Assert.InRange(value.Y - 2*value.X, -1e-9, 1e-9);
				Assert.InRange(value.Z, -1e-9, 1e-9);
			}
		}
	}
}
