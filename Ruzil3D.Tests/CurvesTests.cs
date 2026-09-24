using System;
using System.Linq;
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

		[Fact]
		public void Beziers_EmptyPath_ThrowsInsteadOfHanging()
		{
			// Одна точка — это 3·0 + 1 точек, то есть последовательность без кривых. Прежде поиск в ней не завершался.
			var path = new Beziers(new[] {Point3D.Empty});

			Assert.Equal(0, path.Count);
			Assert.Throws<InvalidOperationException>(() => TestUtil.CompletesWithin(() => path.GetValue(0)));
			Assert.Throws<InvalidOperationException>(() => TestUtil.CompletesWithin(() => path.GetDetails(0)));
		}

		[Fact]
		public void Beziers_NestedAndRepeatedEnumeration()
		{
			var path = new Beziers(new[]
			{
				new Point3D(0, 0, 0), new Point3D(1, 1, 0), new Point3D(2, 1, 0), new Point3D(3, 0, 0),
				new Point3D(4, -1, 0), new Point3D(5, -1, 0), new Point3D(6, 0, 0)
			});

			// Прежде все обходы делили один перечислитель, и вложенный foreach не завершался.
			var pairs = TestUtil.CompletesWithin(() =>
			{
				var count = 0;
				foreach (var outer in path)
				{
					foreach (var inner in path)
					{
						count++;
					}
				}

				return count;
			});

			Assert.Equal(4, pairs);
			Assert.True(path.SequenceEqual(path));
			Assert.Equal(new[] {true, true}, path.Zip(path, ReferenceEquals).ToArray());
		}

		[Fact]
		public void GetDistance_TinyIntervalFarFromZero_DoesNotHang()
		{
			var curve = new BezierCurve(Point3D.Empty, new Point3D(1, 2, 0), new Point3D(3, 2, 0), new Point3D(4, 0, 0));

			var distance = TestUtil.CompletesWithin(() => curve.GetDistance(0.5, 0.5 + 1e-15));

			Assert.InRange(distance, 0, 1e-13);
		}
	}
}
