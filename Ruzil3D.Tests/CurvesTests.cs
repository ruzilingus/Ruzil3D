using Ruzil3D.Algebra;
using Ruzil3D.Curves;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты кривых.
	/// </summary>
	public class CurvesTests
	{
		private static BezierCurve StraightBezier(double length)
		{
			return new BezierCurve(Point3D.Empty, new Point3D(length/3, 0, 0), new Point3D(2*length/3, 0, 0),
				new Point3D(length, 0, 0));
		}

		[Fact]
		public void BezierCurve_EndPointsAndLength()
		{
			var curve = new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 2, 0), new Point3D(3, 2, 1), new Point3D(4, 0, 1));

			TestUtil.Near(curve.P0, curve.GetValue(0));
			TestUtil.Near(curve.P3, curve.GetValue(1));
			Assert.Equal(3, StraightBezier(3).Length, 10);
		}

		[Fact]
		public void LineCurve_Length()
		{
			var line = new LineCurve(new Point3D(1, 1, 1), new Point3D(4, 5, 1));

			Assert.Equal(5, line.Length, 12);
		}

		[Fact]
		public void DistanceCompiler_StraightCurve()
		{
			var compiler = new ParametricCurveDistanceCompiler<BezierCurve>(StraightBezier(3));

			TestUtil.Near(new Point3D(1.5, 0, 0), compiler.GetValue(1.5), 1e-9);
		}

		[Fact]
		public void Beziers_ValueAlongPath()
		{
			var path = new Beziers(new[]
			{
				new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(2, 0, 0), new Point3D(3, 0, 0),
				new Point3D(4, 0, 0), new Point3D(5, 0, 0), new Point3D(6, 0, 0)
			});

			Assert.Equal(2, path.Count);
			Assert.Equal(6, path.Length, 10);
			TestUtil.Near(new Point3D(4.5, 0, 0), path.GetValue(4.5), 1e-9);
		}
	}
}
