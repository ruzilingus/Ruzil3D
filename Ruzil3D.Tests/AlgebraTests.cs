using Ruzil3D.Algebra;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Базовые тесты алгебраических структур: точки, матрицы 3×3, кватернионы, комплексные числа, дроби.
	/// </summary>
	public class AlgebraTests
	{
		[Fact]
		public void Point3D_CrossProduct_FollowsRightHandRule()
		{
			Assert.Equal(Point3D.UnitZ, Point3D.UnitX*Point3D.UnitY);
			Assert.Equal(Point3D.UnitX, Point3D.UnitY*Point3D.UnitZ);
			Assert.Equal(-Point3D.UnitZ, Point3D.UnitY*Point3D.UnitX);
		}

		[Fact]
		public void Point3D_DotProductLengthAndDistance()
		{
			var a = new Point3D(1, 2, 3);
			var b = new Point3D(4, -5, 6);

			Assert.Equal(12, a.DotProduct(b));
			Assert.Equal(5, new Point3D(3, 4, 0).Length);
			Assert.Equal(5, new Point3D(1, 1, 1).Distance(new Point3D(4, 5, 1)));
		}

		[Fact]
		public void Matrix3D_RotationZ_RotatesCounterClockwise()
		{
			var rotated = Matrix3D.GetRotationZ(System.Math.PI/2)*Point3D.UnitX;

			TestUtil.Near(Point3D.UnitY, rotated);
		}

		[Fact]
		public void Matrix3D_TimesInverse_IsIdentity()
		{
			var m = new Matrix3D(2, -1, 0, 1, 3, 4, 0, 5, 7);
			var product = m*m.GetInvert();

			TestUtil.Near(Point3D.UnitX, product.Line1);
			TestUtil.Near(Point3D.UnitY, product.Line2);
			TestUtil.Near(Point3D.UnitZ, product.Line3);
		}

		[Fact]
		public void Matrix3D_Determinant()
		{
			var m = new Matrix3D(2, -1, 0, 1, 3, 4, 0, 5, 7);

			Assert.Equal(9, m.GetDeterminant(), 12);
		}

		[Fact]
		public void Quaternion_BasisProducts()
		{
			var i = new Quaternion(0, 1, 0, 0);
			var j = new Quaternion(0, 0, 1, 0);
			var k = new Quaternion(0, 0, 0, 1);

			Assert.Equal(k, i*j);
			Assert.Equal(-k, j*i);
			Assert.Equal(new Quaternion(-1, 0, 0, 0), i*j*k);
		}

		[Fact]
		public void Quaternion_RotationMatchesMatrixRotation()
		{
			var axis = new Point3D(1, -2, 3);
			const double angle = 0.7;
			var vector = new Point3D(-4, 0.5, 2);

			var byQuaternion = Quaternion.GetRotation(angle, axis).Rotate(vector);
			var byMatrix = Matrix3D.GetRotation(angle, axis)*vector;

			TestUtil.Near(byMatrix, byQuaternion);
		}

		[Fact]
		public void Complex_Multiplication()
		{
			var product = new Complex(1, 2)*new Complex(3, 4);

			Assert.Equal(new Complex(-5, 10), product);
		}

		[Fact]
		public void Fraction_SumOfSmallFractions()
		{
			var sum = new Fraction(1, 2) + new Fraction(1, 3);

			Assert.True(sum == new Fraction(5, 6));
			Assert.Equal(5D/6D, (double) sum, 15);
		}
	}
}
