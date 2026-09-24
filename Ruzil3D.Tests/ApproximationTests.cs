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
		public void GetIndex_ReturnsCell()
		{
			var f = new LinearInterpolation(new[] {new PointD(0, 0), new PointD(1, 2), new PointD(3, 4)});

			Assert.Equal(0, f.GetIndex(0));
			Assert.Equal(0, f.GetIndex(0.5));
			Assert.Equal(1, f.GetIndex(1));
			Assert.Equal(1, f.GetIndex(3));
		}
	}
}
