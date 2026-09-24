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

		[Fact]
		public void Point3D_ToString_ShortVectors()
		{
			// Прежде для векторов длиной не больше 0.001 System.Math.Round получал больше 15 знаков и выбрасывал исключение.
			var text = TestUtil.WithCulture("", () => new Point3D(0.001, 0, 0).ToString());

			Assert.Equal("X = 0.001 Y = 0 Z = 0", text);
			Assert.Contains("1E-20", TestUtil.WithCulture("", () => new Point3D(1e-20, 2e-20, 0).ToString()));
			Assert.Equal("X = 1 Y = 2 Z = 3", TestUtil.WithCulture("", () => new Point3D(1, 2, 3).ToString()));
		}

		[Fact]
		public void Quaternion_ToString_SmallQuaternion()
		{
			var text = TestUtil.WithCulture("", () => new Quaternion(0.001, 0, 0, 0).ToString());

			Assert.False(string.IsNullOrEmpty(text));
			Assert.Equal("0", new Quaternion(0, 0, 0, 0).ToString());
		}

		private static Matrix Rows(params double[][] rows)
		{
			var lines = new Vector[rows.Length];
			for (var i = 0; i < rows.Length; i++)
			{
				lines[i] = new Vector(rows[i]);
			}

			return new Matrix(lines);
		}

		[Fact]
		public void Matrix_TimesVector_HasLengthOfRowCount()
		{
			// Прежде длина результата бралась по длине вектора: 3×2 · (1, 1) падал, 2×3 · (1, 1, 1) давал вектор длины 3.
			var tall = Rows(new[] {1D, 2}, new[] {3D, 4}, new[] {5D, 6});
			var wide = Rows(new[] {1D, 2, 3}, new[] {4D, 5, 6});

			var product1 = tall*new Vector(1, 1);
			var product2 = wide*new Vector(1, 1, 1);

			Assert.Equal(3, product1.Length);
			Assert.Equal(new[] {3D, 7, 11}, new[] {product1[0], product1[1], product1[2]});
			Assert.Equal(2, product2.Length);
			Assert.Equal(new[] {6D, 15}, new[] {product2[0], product2[1]});
		}

		[Fact]
		public void Matrix_SumOfDifferentRowCounts()
		{
			// Прежде 3 строки + 2 строки падало, а в обратном порядке сумма делила строки со слагаемым.
			var three = Rows(new[] {1D, 2}, new[] {3D, 4}, new[] {5D, 6});
			var two = Rows(new[] {10D, 20}, new[] {30D, 40});

			var sum1 = three + two;
			var sum2 = two + three;
			var difference = three - two;

			Assert.Equal(3, sum1.Length);
			Assert.True(sum1 == sum2);
			Assert.Equal(44, sum1[1, 1]);
			Assert.Equal(6, sum1[2, 1]);
			Assert.Equal(-36, difference[1, 1]);

			sum2[2, 1] = 777;
			Assert.Equal(6, three[2, 1]);
		}

		[Fact]
		public void Matrix_Identity_IsSquare()
		{
			var identity = Matrix.GetIdentity(3);

			// Прежде строка i имела длину i + 1, и запись правее диагонали падала.
			identity[0, 2] = 5;

			Assert.Equal(5, identity[0, 2]);
			Assert.Equal(3, identity[0].Length);
		}

		[Fact]
		public void Matrix_Inverse()
		{
			var inverse = Rows(new[] {4D, 7}, new[] {2D, 6}).GetInverse();

			Assert.Equal(0.6, inverse[0, 0], 12);
			Assert.Equal(-0.7, inverse[0, 1], 12);
			Assert.Equal(-0.2, inverse[1, 0], 12);
			Assert.Equal(0.4, inverse[1, 1], 12);
		}
	}
}
