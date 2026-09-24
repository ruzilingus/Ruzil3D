using Ruzil3D.Algebra;
using Ruzil3D.Geometry;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты геометрических объектов.
	/// </summary>
	public class GeometryTests
	{
		[Fact]
		public void Line_NormalForm()
		{
			// Прямая x + y - 2 = 0 проходит через (2, 0) и (0, 2) на расстоянии √2 от начала координат.
			var line = new Line(new PointD(2, 0), new PointD(0, 2));

			Assert.Equal(System.Math.Sqrt(2), line.P, 12);
			Assert.Equal(System.Math.PI/4, line.Theta, 12);
		}

		[Fact]
		public void Line_ThroughCoincidentPoints_ToStringDoesNotThrow()
		{
			// Прежде Math.Sign(NaN) выбрасывал ArithmeticException из Theta, P и ToString.
			var line = new Line(new PointD(1, 1), new PointD(1, 1));

			Assert.True(double.IsNaN(line.Theta));
			Assert.True(double.IsNaN(line.P));
			Assert.Contains("NaN", line.ToString());
		}

		[Fact]
		public void Plane_WithNaN_ToStringDoesNotThrow()
		{
			var collinear = new Plane(Point3D.Empty, Point3D.UnitX, 2*Point3D.UnitX);

			Assert.Contains("NaN", new Plane(1, 0, 0, double.NaN).ToString());
			Assert.Contains("NaN", collinear.ToString());
		}
	}
}
