using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;
using Ruzil3D.Geometry;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Регрессионные тесты исправлений в геометрии, точках, кватернионах и матрицах 3×3.
	/// </summary>
	public class GeometryFixesTests
	{
		private static void AssertRelative(double expected, double actual, double tolerance = 1e-15)
		{
			Assert.True(System.Math.Abs(actual - expected) <= tolerance*System.Math.Abs(expected),
				"Ожидалось " + expected.ToString("R") + ", получено " + actual.ToString("R") + ".");
		}

		private static bool IsNaN(Quaternion q)
		{
			return double.IsNaN(q.W) || q.U.IsNaN;
		}

		private static void AssertSameMatrix(Matrix3D expected, Matrix3D actual, double tolerance = 1e-12)
		{
			TestUtil.Near(expected.Line1, actual.Line1, tolerance);
			TestUtil.Near(expected.Line2, actual.Line2, tolerance);
			TestUtil.Near(expected.Line3, actual.Line3, tolerance);
		}

		private static Point3D RandomPoint(Random random, double range)
		{
			return new Point3D(
				(random.NextDouble()*2 - 1)*range,
				(random.NextDouble()*2 - 1)*range,
				(random.NextDouble()*2 - 1)*range);
		}

		#region Point3D, PointD, Quaternion, Matrix3D

		[Fact]
		public void Point3D_IsNaN_ChecksAllCoordinates()
		{
			// Прежде IsNaN дважды проверял Y и не проверял Z, поэтому и Matrix3D.IsNaN пропускал NaN в третьем столбце.
			Assert.True(new Point3D(0, 0, double.NaN).IsNaN);
			Assert.True(new Point3D(double.NaN, 0, 0).IsNaN);
			Assert.True(new Point3D(0, double.NaN, 0).IsNaN);
			Assert.False(new Point3D(1, 2, 3).IsNaN);

			var matrix = new Matrix3D(0, 0, double.NaN, 0, 1, 0, 0, 0, 1);

			Assert.True(matrix.IsNaN);
			Assert.Equal("NaN", TestUtil.WithCulture("", () => matrix.ToString()));
		}

		[Fact]
		public void Point3D_Angle_ParallelVectorsAreNotNaN()
		{
			// Прежде косинус для одинаковых векторов (1, 1, 1) получался равным 1.0000000000000002, и Acos возвращал NaN
			// (примерно для четверти пар параллельных векторов).
			var vector = new Point3D(1, 1, 1);

			Assert.Equal(1, vector.Cos(vector));
			Assert.Equal(0, vector.Angle(vector));
			Assert.Equal(-1, vector.Cos(-vector));
			Assert.Equal(System.Math.PI, vector.Angle(-vector));

			var random = new Random(1);
			for (var i = 0; i < 10000; i++)
			{
				var a = RandomPoint(random, 10);
				var k = (random.NextDouble() + 0.1)*(random.Next(2) == 0 ? 1 : -1);
				var angle = a.Angle(k*a);

				// Около ±1 арккосинус чувствителен к погрешности косинуса в 1 ulp: угол отличается от 0 или π на ~1e-8.
				Assert.False(double.IsNaN(angle));
				Assert.Equal(k > 0 ? 0 : System.Math.PI, angle, 1e-7);
			}

			// Для нулевого вектора угол не определен.
			Assert.True(double.IsNaN(Point3D.Empty.Angle(vector)));
			Assert.True(double.IsNaN(vector.Cos(Point3D.Empty)));
		}

		[Fact]
		public void Point3D_Angle_VeryLongAndVeryShortVectors()
		{
			// Прежде произведение длин и скалярное произведение переполнялись или обращались в ноль, и угол был NaN.
			Assert.Equal(System.Math.PI/4, new Point3D(1e200, 0, 0).Angle(new Point3D(1e200, 1e200, 0)), 1e-15);
			Assert.Equal(System.Math.PI/2, new Point3D(1e-200, 0, 0).Angle(new Point3D(0, 1e-200, 0)), 1e-15);
			Assert.Equal(System.Math.PI/4, new Point3D(1e-160, 0, 0).Angle(new Point3D(1e-160, 1e-160, 0)), 1e-15);
		}

		[Fact]
		public void Lengths_DoNotOverflowOrUnderflow()
		{
			// Прежде сумма квадратов переполнялась (длина ∞) или обращалась в ноль (длина 0).
			AssertRelative(System.Math.Sqrt(2)*1e200, new Point3D(1e200, 1e200, 0).Length);
			AssertRelative(5e-170, new Point3D(3e-170, 4e-170, 0).Length);
			AssertRelative(System.Math.Sqrt(3)*1e300, new Point3D(1e300, -1e300, 1e300).Length);
			AssertRelative(1e-320, new Point3D(0, 1e-320, 0).Length, 1e-3);

			AssertRelative(System.Math.Sqrt(2)*1e200, new PointD(1e200, 1e200).Length);
			AssertRelative(5e-170, new PointD(3e-170, 4e-170).Length);

			AssertRelative(System.Math.Sqrt(2)*1e200, new Quaternion(1e200, 1e200, 0, 0).Abs);
			AssertRelative(5e-170, new Quaternion(0, 3e-170, 0, 4e-170).Abs);

			AssertRelative(5e200, new Point3D(3e200, 0, 0).Distance(new Point3D(0, 4e200, 0)));
			AssertRelative(5e-170, new PointD(3e-170, 0).Distance(new PointD(0, 4e-170)));

			// Особые значения не изменились.
			Assert.Equal(0, Point3D.Empty.Length);
			Assert.Equal(double.PositiveInfinity, new Point3D(double.NegativeInfinity, 1, 0).Length);
			Assert.True(double.IsNaN(new Point3D(double.PositiveInfinity, double.NaN, 0).Length));
			Assert.Equal(5, new Point3D(3, 4, 0).Length);
		}

		[Fact]
		public void Rotations_AboutVeryLongAndVeryShortAxes()
		{
			// Прежде ось длиной 1e200 нормировалась в нулевой вектор (матрица ≈ 6e-17·I, кватернион (0.707; 0, 0, 0)),
			// а ось длиной 1e-200 — в вектор из ∞ и NaN.
			foreach (var axis in new[] {new Point3D(0, 0, 1e200), new Point3D(0, 0, 1e-200)})
			{
				TestUtil.Near(Point3D.UnitY, Matrix3D.GetRotation(System.Math.PI/2, axis)*Point3D.UnitX);

				var q = Quaternion.GetRotation(System.Math.PI/2, axis);
				Assert.Equal(System.Math.Sqrt(0.5), q.W, 1e-15);
				TestUtil.Near(new Point3D(0, 0, System.Math.Sqrt(0.5)), q.U, 1e-15);
				TestUtil.Near(Point3D.UnitY, q.Rotate(Point3D.UnitX));
			}
		}

		#endregion

		#region Line3D

		[Fact]
		public void Line3D_Constructor_VeryCloseAndVeryFarPoints()
		{
			// Прежде точки на расстоянии 1e-200 считались совпадающими, а для далеких точек направляющая была нулевой.
			Assert.Equal(Point3D.UnitX, new Line3D(Point3D.Empty, new Point3D(1e-200, 0, 0)).S);
			TestUtil.Near(new Point3D(System.Math.Sqrt(0.5), System.Math.Sqrt(0.5), 0),
				new Line3D(Point3D.Empty, new Point3D(1e200, 1e200, 0)).S, 1e-15);
			Assert.Equal(Point3D.UnitX, new Line3D(new Point3D(-1e308, 0, 0), new Point3D(1e308, 0, 0)).S);

			// Бесконечные координаты отклоняются, как и NaN (прежде направляющая получалась из нулей и NaN).
			Assert.Throws<ArgumentException>(() => new Line3D(Point3D.Empty, new Point3D(double.PositiveInfinity, 0, 0)));
			Assert.Throws<ArgumentException>(() => new Line3D(Point3D.Empty, new Point3D(double.NaN, 0, 0)));
			Assert.Throws<ArgumentException>(() => new Line3D(Point3D.UnitX, Point3D.UnitX));
		}

		[Fact]
		public void Line3D_ToString_DoesNotChangeLine()
		{
			// Прежде при S.X = 0 и S.Y < 0 ToString менял знак самого свойства S (опечатка S *= -1 вместо s *= -1).
			var line = new Line3D(Point3D.Empty, new Point3D(0, -1, 0));

			var text = TestUtil.WithCulture("", () => line.ToString());

			Assert.Equal(new Point3D(0, -1, 0), line.S);
			Assert.Equal(new Point3D(0, -1, 0), line.GetValue(1));
			Assert.EndsWith("(0, 1, 0)", text);
		}

		[Fact]
		public void Line3D_ToString_IsCanonical()
		{
			// Прежде при S.Y = 0 и S.Z < 0 знак менялся без проверки S.X, и одна прямая печаталась по-разному.
			var line1 = new Line3D(Point3D.Empty, new Point3D(0.6, 0, -0.8));
			var line2 = new Line3D(Point3D.Empty, new Point3D(-0.6, 0, 0.8));
			var line3 = new Line3D(new Point3D(1, 2, 3), new Point3D(1, 2, 2));
			var line4 = new Line3D(new Point3D(1, 2, 3), new Point3D(1, 2, 4));

			var text1 = TestUtil.WithCulture("", () => line1.ToString());
			var text2 = TestUtil.WithCulture("", () => line2.ToString());
			var text3 = TestUtil.WithCulture("", () => line3.ToString());
			var text4 = TestUtil.WithCulture("", () => line4.ToString());

			Assert.Equal(text1, text2);
			Assert.EndsWith("(3/5, 0, -4/5)", text1);
			Assert.Equal(text3, text4);
			Assert.EndsWith("(0, 0, 1)", text3);
		}

		[Fact]
		public void Line3D_TimesMatrix_HasUnitDirection()
		{
			// Прежде направляющая умножалась на матрицу без нормировки, и расстояния искажались в масштаб матрицы.
			var axis = new Line3D(Point3D.Empty, Point3D.UnitX);

			var doubled = 2*Matrix3D.Identity*axis;
			Assert.Equal(Point3D.UnitX, doubled.S);
			Assert.Equal(1, doubled.GetDistance(new Point3D(0, 1, 0)), 1e-15);
			TestUtil.Near(new Point3D(5, 0, 0), doubled.NormalPoint(new Point3D(5, 1, 0)));

			var stretched = new Matrix3D(3, 0, 0, 0, 1, 0, 0, 0, 1)*axis;
			Assert.Equal(1, stretched.GetDistance(new Point3D(0, 1, 0)), 1e-15);

			var moved = new Affinity(2*Matrix3D.Identity, new Point3D(0, 0, 1))*axis;
			Assert.Equal(1, moved.S.Length, 1e-15);
			Assert.Equal(1, moved.GetDistance(new Point3D(7, 1, 1)), 1e-15);

			// Прежде направляющая вычислялась как M·(m + s) - M·m и терялась вдали от начала координат.
			var far = new Line3D(new Point3D(1e17, 0, 0), new Point3D(1e17 + 16, 0, 0));
			Assert.Equal(Point3D.UnitX, (Matrix3D.Identity*far).S);

			var shifted = new Line3D(new Point3D(1e8, 1e8, 1e8), new Point3D(1e8 + 1, 1e8 + 2, 1e8 + 3));
			Assert.Equal(1, (Matrix3D.Identity*shifted).S.Length, 1e-15);
		}

		#endregion

		#region Plane

		[Fact]
		public void Plane_TimesMatrix_KeepsPlanePosition()
		{
			// Прежде точка плоскости бралась на расстоянии -D/|n| вместо -D/|n|²: плоскость 2x - 2 = 0 (x = 1)
			// после умножения на единичную матрицу превращалась в x = 2.
			var plane = new Plane(2, 0, 0, -2);

			foreach (var image in new[] {Matrix3D.Identity*plane, Affinity.Identity*plane})
			{
				Assert.Equal(0, image.GetValue(new Point3D(1, 5, -3)), 1e-15);
				Assert.Equal(1, image.GetValue(new Point3D(2, 0, 0)), 1e-15);
			}
		}

		[Fact]
		public void Plane_TimesMatrix_TransformsNormalByInverseTranspose()
		{
			// Прежде нормаль умножалась на саму матрицу, и для любого преобразования, кроме поворота и
			// равномерного масштабирования, получалась другая плоскость.
			var plane = new Plane(1, 1, 1, -1);
			var stretch = new Matrix3D(2, 0, 0, 0, 1, 0, 0, 0, 1);
			var stretched = stretch*plane;

			foreach (var point in new[] {Point3D.UnitX, Point3D.UnitY, Point3D.UnitZ})
			{
				Assert.Equal(0, stretched.GetValue(stretch*point), 1e-15);
			}

			var shear = new Matrix3D(1, 1, 0, 0, 1, 0, 0, 0, 1);
			var sheared = shear*new Plane(1, 0, 0, 0);

			Assert.Equal(0, sheared.GetValue(shear*new Point3D(0, 1, 0)), 1e-15);
			Assert.Equal(0, sheared.GetValue(shear*new Point3D(0, -2, 3)), 1e-15);

			// Образы точек плоскости лежат на образе плоскости при случайных аффинных преобразованиях.
			var random = new Random(2);
			for (var i = 0; i < 1000; i++)
			{
				var matrix = new Matrix3D(RandomPoint(random, 2), RandomPoint(random, 2), RandomPoint(random, 2));
				if (System.Math.Abs(matrix.GetDeterminant()) < 0.1)
				{
					continue;
				}

				var transform = new Affinity(matrix, RandomPoint(random, 5));
				var p0 = RandomPoint(random, 5);
				var p1 = RandomPoint(random, 5);
				var p2 = RandomPoint(random, 5);
				var image = transform*new Plane(p0, p1, p2);

				foreach (var point in new[] {p0, p1, p2, p0 + 0.5*(p1 - p0) - 1.5*(p2 - p0)})
				{
					Assert.True(System.Math.Abs(image.GetValue(transform*point)) < 1e-9);
				}
			}
		}

		private static void AssertOnPlanes(Line3D line, Plane plane1, Plane plane2)
		{
			Assert.Equal(1, line.S.Length, 1e-15);

			foreach (var t in new[] {-10D, 0, 10})
			{
				var point = line.GetValue(t);
				Assert.True(System.Math.Abs(plane1.GetValue(point)) < 1e-12, "Точка не лежит на первой плоскости.");
				Assert.True(System.Math.Abs(plane2.GetValue(point)) < 1e-12, "Точка не лежит на второй плоскости.");
			}
		}

		[Fact]
		public void Plane_TimesPlane_SmallCoefficients()
		{
			// Прежде опорным брался первый ненулевой коэффициент, даже очень маленький: -5.55e-17 из-за округления
			// давал точку на расстоянии 0.3 от плоскости, а 1e-310 — исключение о нечисловых координатах.
			var tilted = new Plane(new Point3D(0, 0, .3), new Point3D(1, 0, .1 + .2), new Point3D(0, 1, .3));
			var vertical = new Plane(1, 0, 0, -0.7);

			AssertOnPlanes(tilted*vertical, tilted, vertical);
			AssertOnPlanes(vertical*tilted, tilted, vertical);

			var nearlyHorizontal = new Plane(1e-17, 0, 1, -1);
			var x03 = new Plane(1, 0, 0, -0.3);

			AssertOnPlanes(nearlyHorizontal*x03, nearlyHorizontal, x03);
			AssertOnPlanes(x03*nearlyHorizontal, nearlyHorizontal, x03);

			var subnormal = new Plane(1e-310, 0, 1, -1);

			AssertOnPlanes(subnormal*x03, subnormal, x03);
			AssertOnPlanes(x03*subnormal, subnormal, x03);
			TestUtil.Near(new Point3D(0.3, 0, 1), (subnormal*x03).M);
		}

		[Fact]
		public void Plane_TimesPlane_RandomPlanes()
		{
			// Прежде малый ненулевой коэффициент A первой плоскости становился опорным, и точка прямой уходила с плоскостей.
			var random = new Random(3);
			for (var i = 0; i < 1000; i++)
			{
				var plane1 = new Plane(RandomPoint(random, 10), RandomPoint(random, 10), RandomPoint(random, 10));
				var plane2 = new Plane(RandomPoint(random, 10), RandomPoint(random, 10), RandomPoint(random, 10));
				var tilted = new Plane((random.NextDouble() - 0.5)*1e-16, plane1.B, plane1.C, plane1.D);

				foreach (var pair in new[] {new[] {plane1, plane2}, new[] {tilted, plane2}, new[] {plane2, tilted}})
				{
					var line = pair[0]*pair[1];

					foreach (var t in new[] {-10D, 0, 10})
					{
						var point = line.GetValue(t);
						var scale = 1 + point.Length;
						Assert.True(System.Math.Abs(pair[0].GetValue(point)) < 1e-12*scale);
						Assert.True(System.Math.Abs(pair[1].GetValue(point)) < 1e-12*scale);
					}
				}
			}
		}

		[Fact]
		public void Plane_Parallel_Throws()
		{
			var pa = new Point3D(0.1, 0.2, 0.3);
			var pb = new Point3D(1.7, -0.3, 0.9);
			var pc = new Point3D(-0.4, 2.2, 1.3);
			var offset = new Point3D(0.1, 0.1, 0.9);
			var plane = new Plane(pa, pb, pc);

			// Прямая параллельна плоскости, но из-за округления знаменатель равен 5.55e-17: прежде возвращалась
			// «точка пересечения» на расстоянии ~1e16.
			var line = new Line3D(pa + offset, pc + offset);
			var lineError = Assert.Throws<ArgumentException>(() => plane*line);
			Assert.Equal("Плоскость и прямая параллельны.", lineError.Message);

			// Плоскости, построенные по сдвинутым точкам, параллельны: прежде возвращалась прямая на расстоянии ~1e15,
			// а сообщение об ошибке зависело от порядка плоскостей.
			var shifted = new Plane(pa + offset, pb + offset, pc + offset);
			var error1 = Assert.Throws<ArgumentException>(() => plane*shifted);
			var error2 = Assert.Throws<ArgumentException>(() => shifted*plane);
			var error3 = Assert.Throws<ArgumentException>(() => plane*plane);

			Assert.Equal("Плоскости параллельны!", error1.Message);
			Assert.Equal(error1.Message, error2.Message);
			Assert.Equal(error1.Message, error3.Message);

			// Прежде для нулевой нормали второй плоскости сообщалось о параллельности, а для первой — об ошибке параметра.
			var notPlane = new Plane(0, 0, 0, 1);
			Assert.Contains("не является плоскостью", Assert.Throws<ArgumentException>(() => notPlane*plane).Message);
			Assert.Contains("не является плоскостью", Assert.Throws<ArgumentException>(() => plane*notPlane).Message);
		}

		#endregion

		#region Line, Plane: равенство и нормальный вид

		[Fact]
		public void Line_Equality_IsGeometric()
		{
			// Прежде прямые, отличающиеся знаком коэффициентов при C = 0 или погрешностью округления, были неравны.
			Assert.True(new Line(new PointD(0, 0), new PointD(1, 1)) == new Line(new PointD(1, 1), new PointD(0, 0)));
			Assert.True(new Line(1, -1, 0) == new Line(-2, 2, 0));
			Assert.True(new Line(1, 2, 3) == new Line(-0.1, -0.2, -0.3));

			var random = new Random(4);
			for (var i = 0; i < 10000; i++)
			{
				var a = new PointD(random.NextDouble()*20 - 10, random.NextDouble()*20 - 10);
				var b = new PointD(random.NextDouble()*20 - 10, random.NextDouble()*20 - 10);
				Assert.True(new Line(a, b) == new Line(b, a));

				var k = (random.NextDouble()*10 + 0.01)*(random.Next(2) == 0 ? 1 : -1);
				double la = random.NextDouble()*2 - 1, lb = random.NextDouble()*2 - 1, lc = random.NextDouble()*2 - 1;
				Assert.True(new Line(la, lb, lc) == new Line(k*la, k*lb, k*lc));
				Assert.True(new Line(la, lb, lc).Equals((object) new Line(k*la, k*lb, k*lc)));
			}

			// Разные прямые не равны.
			Assert.False(new Line(1, 0, -1) == new Line(1, 0, -1.000001));
			Assert.False(new Line(1, 0, 0) == new Line(1, 0.000001, 0));
			Assert.True(new Line(1, 0, 0) != new Line(0, 1, 0));
		}

		[Fact]
		public void Line_Equals_IsReflexive()
		{
			// Прежде new Line(0, 0, 0) не была равна самой себе и не находилась в списке,
			// а все прямые с коэффициентами NaN были равны друг другу.
			var zero = new Line(0, 0, 0);
			var zeroCopy = zero;

			Assert.True(zero == zeroCopy);
			Assert.True(zero.Equals(zeroCopy));
			Assert.Contains(zero, new List<Line> {zero});
			Assert.False(zero == new Line(1, 0, 0));

			var nan1 = new Line(new PointD(1, 1), new PointD(1, 1));
			var nan1Copy = nan1;
			var nan2 = new Line(new PointD(5, -7), new PointD(5, -7));

			Assert.False(nan1 == nan2);
			Assert.False(nan1 == nan1Copy);
			Assert.True(nan1 != nan1Copy);
			Assert.True(nan1.Equals(nan1Copy));
			Assert.True(nan1.Equals((object) nan1));
			Assert.Contains(nan1, new List<Line> {nan1});
			Assert.False(nan1 == new Line(1, 0, 0));
		}

		[Fact]
		public void Plane_Equality_IsGeometric()
		{
			// Прежде плоскости, построенные по тем же точкам в другом порядке, почти всегда были неравны.
			Assert.True(new Plane(Point3D.Empty, Point3D.UnitX, Point3D.UnitY) == new Plane(Point3D.Empty, Point3D.UnitY, Point3D.UnitX));
			Assert.True(new Plane(1, 2, 3, 0) == new Plane(-2, -4, -6, 0));

			var random = new Random(5);
			for (var i = 0; i < 10000; i++)
			{
				var p0 = RandomPoint(random, 10);
				var p1 = RandomPoint(random, 10);
				var p2 = RandomPoint(random, 10);

				Assert.True(new Plane(p0, p1, p2) == new Plane(p1, p2, p0));
				Assert.True(new Plane(p0, p1, p2) == new Plane(p1, p0, p2));
				Assert.True(new Plane(p0, p1, p2).Equals((object) new Plane(p2, p0, p1)));
			}

			Assert.False(new Plane(0, 0, 1, -1) == new Plane(0, 0, 1, -1.000001));
			Assert.False(new Plane(0, 0, 1, -1) == new Plane(0, 0.000001, 1, -1));
			Assert.True(new Plane(1, 0, 0, 0) != new Plane(0, 1, 0, 0));
		}

		[Fact]
		public void Plane_Equals_IsReflexive()
		{
			// Прежде new Plane(0, 0, 0, 0) не была равна самой себе, а все плоскости с коэффициентами NaN были равны друг другу.
			var zero = new Plane(0, 0, 0, 0);
			var zeroCopy = zero;

			Assert.True(zero == zeroCopy);
			Assert.True(zero.Equals(zeroCopy));
			Assert.Contains(zero, new List<Plane> {zero});

			var nan1 = new Plane(Point3D.Empty, Point3D.UnitX, 2*Point3D.UnitX);
			var nan1Copy = nan1;
			var nan2 = new Plane(Point3D.UnitY, Point3D.UnitZ, Point3D.UnitZ);

			Assert.False(nan1 == nan2);
			Assert.False(nan1 == nan1Copy);
			Assert.True(nan1.Equals(nan1Copy));
			Assert.Contains(nan1, new List<Plane> {nan1});
		}

		[Fact]
		public void Line_ChangedCoefficients_AreNotTreatedAsNormalized()
		{
			// Прежде флаг нормального вида сохранялся после изменения коэффициентов: у прямой y = 1 после B = 2
			// (прямая y = 0.5) расстояние P оставалось равным 1, а Normalize ничего не делал.
			var line = new Line(new PointD(0, 1), new PointD(1, 1));
			Assert.Equal(1, line.P, 1e-15);

			line.B = 2;
			Assert.Equal(0.5, line.P, 1e-15);

			line.Normalize();
			Assert.Equal(1, line.B, 1e-15);
			Assert.Equal(-0.5, line.C, 1e-15);
			Assert.Equal(0.5, line.P, 1e-15);
		}

		[Fact]
		public void Plane_ChangedCoefficients_AreNotTreatedAsNormalized()
		{
			// Прежде плоскость z = 1 после C = 2 (плоскость z = 0.5) считалась нормализованной, и Normalize ничего не делал.
			var plane = new Plane(new Point3D(0, 0, 1), new Point3D(1, 0, 1), new Point3D(0, 1, 1));
			Assert.True(plane.IsNormalized);

			plane.C = 2;
			Assert.False(plane.IsNormalized);

			plane.Normalize();
			Assert.True(plane.IsNormalized);
			Assert.Equal(1, plane.C, 1e-15);
			Assert.Equal(-0.5, plane.D, 1e-15);
			Assert.Equal(-0.5, plane.GetValue(Point3D.Empty), 1e-15);

			// Признак вычисляется по коэффициентам и для плоскости, заданной уже нормализованным уравнением.
			Assert.True(new Plane(0, 0, 1, -1).IsNormalized);
			Assert.False(new Plane(0, 0, 1, 1).IsNormalized);
			Assert.False(new Plane(0, 0, 2, -1).IsNormalized);
		}

		#endregion

		#region Line, Triangle

		[Fact]
		public void Line_IntersectsWith_ComparesSigns()
		{
			// Прежде значения на концах отрезка перемножались, и произведение 1e-200·1e-200 обращалось в ноль.
			var axis = new Line(0, 1, 0);

			Assert.False(axis.IntersectsWith(new PointD(0, 1e-200), new PointD(1, 1e-200)));
			Assert.False(axis.IntersectsWith(new PointD(0, -1e-200), new PointD(1, -1e-200)));
			Assert.True(axis.IntersectsWith(new PointD(0, -1e-200), new PointD(1, 1e-200)));
			Assert.True(axis.IntersectsWith(new PointD(0, 0), new PointD(1, 1)));
			Assert.True(axis.IntersectsWith(new PointD(0, -1), new PointD(1, 1)));
			Assert.False(axis.IntersectsWith(new PointD(0, 1), new PointD(1, 2)));
		}

		[Fact]
		public void Triangle_Circumscribed_FarFromOrigin()
		{
			// Прежде использовались квадраты абсолютных координат: при смещении 1e7 центр сдвигался на ~0.016,
			// а для прямоугольного треугольника в точке (1e8, 1e8) радиус был нулевым.
			const double radius = 0.81406100047846540;

			foreach (var offset in new[] {0, 1e5, 1e6, 1e7})
			{
				var triangle = new Triangle(
					new PointD(offset + 0.1, offset + 0.2),
					new PointD(offset + 1.3, offset - 0.4),
					new PointD(offset + 0.7, offset + 1.1));

				var circle = triangle.GetCircumscribed();

				Assert.Equal(radius, circle.Radius, 1e-8);
				Assert.Equal(0.90625, circle.Center.X - offset, 1e-8);
				Assert.Equal(0.3125, circle.Center.Y - offset, 1e-8);
			}

			var right = new Triangle(new PointD(1e8, 1e8), new PointD(1e8 + 1, 1e8), new PointD(1e8, 1e8 + 1));
			var rightCircle = right.GetCircumscribed();

			Assert.Equal(System.Math.Sqrt(0.5), rightCircle.Radius, 1e-12);
			Assert.Equal(1e8 + 0.5, rightCircle.Center.X, 1e-6);
			Assert.Equal(1e8 + 0.5, rightCircle.Center.Y, 1e-6);
		}

		#endregion

		#region Isometry, Affinity

		private static void AssertQuaternionMatchesMatrix(Isometry isometry)
		{
			var q = isometry.Quaternion;

			Assert.False(IsNaN(q), "Кватернион содержит NaN.");
			Assert.Equal(1, q.Abs, 1e-12);
			Assert.True(q.W >= 0);
			AssertSameMatrix(isometry.Matrix, new Isometry(q).Matrix);
		}

		[Fact]
		public void Isometry_Quaternion_HalfTurns()
		{
			// Прежде для поворотов на π знаки недиагональных разностей обращались в ноль: кватернион (0; 1, 0, 0)
			// превращался в (0; 0, 0, 0), а (0; 0.6, 0.8, 0) — в (NaN; 0, 0, 0).
			var q1 = new Isometry(new Quaternion(0, 1, 0, 0)).Quaternion;
			Assert.Equal(0, q1.W, 1e-15);
			TestUtil.Near(Point3D.UnitX, q1.U, 1e-15);

			var q2 = new Isometry(new Quaternion(0, 0.6, 0.8, 0)).Quaternion;
			Assert.Equal(0, q2.W, 1e-15);
			TestUtil.Near(new Point3D(0.6, 0.8, 0), q2.U, 1e-15);

			var random = new Random(6);
			for (var i = 0; i < 1000; i++)
			{
				AssertQuaternionMatchesMatrix(new Isometry(System.Math.PI, RandomPoint(random, 1)));
			}
		}

		[Fact]
		public void Isometry_Quaternion_IntegerAxes()
		{
			// Прежде округление делало подкоренное выражение отрицательным: для угла 1.1 и оси (0, -5, 1)
			// компонента X была NaN.
			var q = new Isometry(1.1, new Point3D(0, -5, 1)).Quaternion;
			Assert.False(IsNaN(q));
			Assert.Equal(0, q.U.X, 1e-15);

			for (var angle = 0.1; angle < 6.3; angle += 0.5)
			{
				for (var j = -10; j <= 10; j++)
				{
					for (var k = -10; k <= 10; k++)
					{
						if (j != 0 || k != 0)
						{
							AssertQuaternionMatchesMatrix(new Isometry(angle, new Point3D(0, j, k)));
							AssertQuaternionMatchesMatrix(new Isometry(angle, new Point3D(j, k, 1)));
						}
					}
				}
			}
		}

		[Fact]
		public void Isometry_Quaternion_SmallAnglesAndGenericAxes()
		{
			// Прежде компоненты вычислялись через разность близких чисел под корнем: для угла 1e-9 получалось x = 0.
			var small = new Isometry(1e-9, Point3D.UnitX).Quaternion;
			AssertRelative(5e-10, small.U.X, 1e-6);

			var random = new Random(7);
			for (var i = 0; i < 1000; i++)
			{
				var axis = RandomPoint(random, 1);
				var angle = random.NextDouble()*2*System.Math.PI;
				var expected = Quaternion.GetRotation(angle, axis);
				if (expected.W < 0)
				{
					expected = -expected;
				}

				var actual = new Isometry(angle, axis).Quaternion;

				Assert.Equal(expected.W, actual.W, 1e-14);
				TestUtil.Near(expected.U, actual.U, 1e-14);
			}
		}

		[Fact]
		public void Affinity_EqualsMatchesOperator()
		{
			// Прежде оператор == сравнивал только матрицы и смещения, а Equals требовал совпадения типов.
			var affinity = new Affinity(Matrix3D.Identity, new Point3D(1, 2, 3));
			var isometry = new Isometry(new Point3D(1, 2, 3));

			Assert.True(affinity == isometry);
			Assert.True(affinity.Equals(isometry));
			Assert.True(isometry.Equals(affinity));
			Assert.Equal(affinity.GetHashCode(), isometry.GetHashCode());

			Assert.True(Affinity.Identity == Isometry.Identity);
			Assert.True(Affinity.Identity.Equals(Isometry.Identity));

			Assert.False(affinity.Equals(new Affinity(new Point3D(1, 2, 4))));
			Assert.False(affinity.Equals(null));
			Assert.False(affinity.Equals(new Point3D(1, 2, 3)));
		}

		[Fact]
		public void Isometry_GetInvert_ThroughAffinityReference()
		{
			// Прежде Isometry.GetInvert только скрывал метод Affinity, и через ссылку на Affinity возвращалась не изометрия.
			Affinity transform = new Isometry(0.5, Point3D.UnitZ, new Point3D(1, 2, 3));
			var inverse = transform.GetInvert();

			Assert.IsType<Isometry>(inverse);

			var point = new Point3D(-4, 0.5, 2);
			TestUtil.Near(point, inverse*(transform*point));

			var affinity = new Affinity(new Matrix3D(2, 0, 0, 0, 1, 0, 0, 0, 1), new Point3D(1, 0, 0));
			Assert.IsType<Affinity>(affinity.GetInvert());
			TestUtil.Near(point, affinity.GetInvert()*(affinity*point));
		}

		#endregion

		[Fact]
		public void ObsoleteMembers_HaveMessages()
		{
			// Прежде устаревшие методы были помечены атрибутом Obsolete без пояснения, чем их заменить.
			var methods = new[]
			{
				typeof(Line3D).GetMethod("ResolveByX"),
				typeof(Line3D).GetMethod("ResolveByY"),
				typeof(Plane).GetMethod("GetZValue"),
				typeof(Triangle3D).GetMethod("Contains")
			};

			foreach (var method in methods)
			{
				var attribute = (ObsoleteAttribute) Attribute.GetCustomAttribute(method, typeof(ObsoleteAttribute));

				Assert.NotNull(attribute);
				Assert.False(string.IsNullOrEmpty(attribute.Message));
			}
		}
	}
}
