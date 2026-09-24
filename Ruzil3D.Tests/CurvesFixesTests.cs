using System;
using System.Linq;
using Ruzil3D.Algebra;
using Ruzil3D.Curves;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Регрессионные тесты исправлений в кривых.
	/// </summary>
	public class CurvesFixesTests
	{
		#region Helpers

		/// <summary>
		/// Кривая с точкой возврата при t = 0.5.
		/// </summary>
		private static BezierCurve Cusp()
		{
			return new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 1, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
		}

		private static Point3D DeCasteljau(Point3D[] points, double t)
		{
			var p = (Point3D[]) points.Clone();
			for (var r = 1; r < p.Length; r++)
			{
				for (var i = 0; i < p.Length - r; i++)
				{
					p[i] = p[i]*(1 - t) + p[i + 1]*t;
				}
			}

			return p[0];
		}

		private static double Polyline(Func<double, Point3D> function, double t0, double t1, int count)
		{
			var result = 0D;
			var previous = function(t0);
			for (var i = 1; i <= count; i++)
			{
				var current = function(t0 + (t1 - t0)*i/count);
				result += previous.Distance(current);
				previous = current;
			}

			return result;
		}

		private class ValueOnlyCurve : ParametricCurve
		{
			public override Point3D GetValue(double parameter)
			{
				return new Point3D(parameter, parameter*parameter, 0);
			}
		}

		#endregion

		#region Bernstein and Bezier curves

		[Fact]
		public void BernsteinCurve_LengthOfQuarticCurve()
		{
			// Прежде подынтегральная функция учитывала только коэффициенты a0…a4 многочлена |P′|², и длина x(t) = t⁴ была 0.
			var curve = new BernsteinCurve(new[] {Point3D.Empty, Point3D.Empty, Point3D.Empty, Point3D.Empty, new Point3D(1, 0, 0)});

			Assert.Equal(1, curve.Length, 12);
			Assert.Equal(0.5*0.5*0.5*0.5, curve.GetDistance(0, 0.5), 12);
		}

		[Fact]
		public void BernsteinCurve_HighDegreeLengthMatchesPolyline()
		{
			// Прежде для случайной кривой пятой степени длина была 216.45 вместо 13.31. Для высоких степеней производная
			// вычисляется в базисе Бернштейна: многочлен |P′|² в степенном базисе для 21 точки давал ошибку больше 40 %.
			foreach (var degree in new[] {5, 12, 20})
			{
				var random = new Random(degree);
				var points = Enumerable.Range(0, degree + 1)
					.Select(i => new Point3D(random.NextDouble()*10, random.NextDouble()*10, random.NextDouble()*10)).ToArray();
				var curve = new BernsteinCurve(points);
				Func<double, Point3D> exact = t => DeCasteljau(points, t);

				// Длины ломаных уточнены экстраполяцией Ричардсона.
				var length = (4*Polyline(exact, 0, 1, 20000) - Polyline(exact, 0, 1, 10000))/3;
				var part = (4*Polyline(exact, 0.2, 0.7, 20000) - Polyline(exact, 0.2, 0.7, 10000))/3;

				// Сумма длин мелких участков проверяет саму функцию выпрямления. Длина всей кривой вычисляется одним
				// фиксированным правилом интегрирования, поэтому для неё допуск больше.
				var pieces = Enumerable.Range(0, 128).Sum(i => curve.GetDistance(i/128D, (i + 1)/128D));
				Assert.InRange(pieces/length - 1, -1e-9, 1e-9);
				Assert.InRange(curve.Length/length - 1, -1e-5, 1e-5);
				Assert.InRange(curve.GetDistance(0.2, 0.7)/part - 1, -1e-5, 1e-5);

				// Компилятор расстояний использует ту же функцию выпрямления.
				var compiler = new ParametricCurveDistanceCompiler<BernsteinCurve>(curve, 10);
				var middle = compiler.GetParameter(curve.Length/2);
				Assert.InRange(Polyline(exact, 0, middle, 20000)/length - 0.5, -1e-4, 1e-4);
			}
		}

		[Fact]
		public void BezierCurve_DistanceNearCuspIsNotNaN()
		{
			// Прежде около точки возврата многочлен |P′(t)|² из-за округления становился отрицательным, и корень из него
			// давал NaN: например, GetDistance(0.5 − 1e-9, 0.5 + 1e-9) для кривой с точкой возврата при t = 0.5.
			var cusp = Cusp();

			for (var i = -1000; i <= 1000; i++)
			{
				var speed = cusp.RectificationDerivativeFunction(0.5 + i*1e-10);
				Assert.False(double.IsNaN(speed), "Скорость NaN при t = 0.5 + " + i + "e-10.");
			}

			Assert.InRange(cusp.GetDistance(0.5 - 1e-9, 0.5 + 1e-9), 0, 1e-15);
		}

		[Fact]
		public void BezierCurve_TangentAtDegenerateEnd()
		{
			// Прежде производная в конце кривой вычислялась в степенном базисе и вместо нуля была порядка 1e-15,
			// поэтому особая точка не распознавалась, и касательная имела случайное направление.
			var p0 = new Point3D(0.1, 0.7, 0.3);
			var p1 = new Point3D(1.3, 2.9, 0.4);
			var p2 = new Point3D(3.7, 1.1, 2.3);
			var curve = new BezierCurve(p0, p1, p2, p2);
			var expected = (p2 - p1)/(p2 - p1).Length;

			TestUtil.Near(expected, curve.GetTangent(1), 1e-12);
			TestUtil.Near(expected, curve.GetTangent(1 - 1e-15), 1e-9);

			var random = new Random(3);
			for (var i = 0; i < 1000; i++)
			{
				var a = new Point3D(random.NextDouble()*100, random.NextDouble()*100, random.NextDouble()*100);
				var c = new Point3D(random.NextDouble()*100, random.NextDouble()*100, random.NextDouble()*100);
				var direction = (c - a)/(c - a).Length;
				var bezier = new BezierCurve(a, a, c, c);

				TestUtil.Near(direction, bezier.GetTangent(0), 1e-9);
				TestUtil.Near(direction, bezier.GetTangent(1), 1e-9);
			}
		}

		[Fact]
		public void BezierCurve_FromPointsKeepsPrecision()
		{
			// Прежде узловые точки получались из многочленов сплайна, записанных относительно t = 0: для 1000 точек
			// концы кривых отклонялись от заданных точек на 1e-6, для 50 000 — на 0.2.
			var random = new Random(5);
			var points = Enumerable.Range(0, 1000)
				.Select(i => new Point3D(random.NextDouble(), random.NextDouble(), random.NextDouble())).ToList();

			var curves = BezierCurve.FromPoints(points);

			Assert.Equal(points.Count - 1, curves.Length);
			for (var i = 0; i < curves.Length; i++)
			{
				Assert.Equal(points[i], curves[i].P0);
				Assert.Equal(points[i + 1], curves[i].P3);
			}

			// Кривые образуют натуральный кубический сплайн: первая и вторая производные непрерывны, на концах вторая
			// производная равна нулю.
			for (var i = 1; i < curves.Length; i++)
			{
				TestUtil.Near(curves[i - 1].P3 - curves[i - 1].P2, curves[i].P1 - curves[i].P0, 1e-12);
				TestUtil.Near(curves[i - 1].P3 - curves[i - 1].P2*2 + curves[i - 1].P1, curves[i].P2 - curves[i].P1*2 + curves[i].P0, 1e-11);
			}

			TestUtil.Near(Point3D.Empty, curves[0].P2 - curves[0].P1*2 + curves[0].P0, 1e-12);
			var last = curves[curves.Length - 1];
			TestUtil.Near(Point3D.Empty, last.P3 - last.P2*2 + last.P1, 1e-12);

			// Для двух точек получается отрезок с равномерной параметризацией.
			var line = BezierCurve.FromPoints(new[] {new Point3D(0, 0, 0), new Point3D(3, 0, 0)});
			TestUtil.Near(new Point3D(1, 0, 0), line[0].P1);
			TestUtil.Near(new Point3D(2, 0, 0), line[0].P2);
		}

		[Fact]
		public void RecastToBezierCurve_ExactForLowDegrees()
		{
			// Прежде для самой кривой Безье и для квадратичной кривой метод не был реализован.
			var bezier = new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 2, 0), new Point3D(3, 2, 1), new Point3D(4, 0, 1));
			var copy = bezier.RecastToBezierCurve();
			Assert.NotSame(bezier, copy);
			Assert.Equal(new[] {bezier.P0, bezier.P1, bezier.P2, bezier.P3}, new[] {copy.P0, copy.P1, copy.P2, copy.P3});

			var quadratic = new BernsteinCurve(new[] {new Point3D(0, 0, 0), new Point3D(1, 2, 0), new Point3D(2, 0, 1)});
			var cubic = quadratic.RecastToBezierCurve();
			foreach (var t in new[] {0, 0.1, 0.25, 0.5, 0.8, 1})
			{
				TestUtil.Near(quadratic.GetValue(t), cubic.GetValue(t));
			}

			// Прежде промежуточные точки совпадали с концами отрезка: параметризация менялась (line(0.25) = (1, 0, 0),
			// а recast(0.25) = (0.625, 0, 0)), а скорость на концах была нулевой.
			var line = new LineCurve(new Point3D(0, 0, 0), new Point3D(4, 0, 0));
			var recast = line.RecastToBezierCurve();
			foreach (var t in new[] {0, 0.25, 0.5, 0.9, 1})
			{
				TestUtil.Near(line.GetValue(t), recast.GetValue(t));
			}

			Assert.Equal(4, recast.Length, 12);

			var general = new BernsteinCurve(new[] {new Point3D(1, 1, 1), new Point3D(2, 3, 5)});
			foreach (var t in new[] {0, 0.3, 1})
			{
				TestUtil.Near(general.GetValue(t), general.RecastToBezierCurve().GetValue(t));
			}

			// Кривую четвёртой степени нельзя представить одной кубической кривой.
			var quartic = new BernsteinCurve(new[] {Point3D.Empty, Point3D.UnitX, Point3D.UnitY, Point3D.UnitZ, Point3D.UnitX});
			Assert.Throws<NotImplementedException>(() => quartic.RecastToBezierCurve());
		}

		[Fact]
		public void BernsteinCurve_SplitPointsValidatesBuffers()
		{
			// Прежде степень кривой бралась из длины первого массива: для массивов из пяти точек кубическая кривая
			// делилась не в той точке.
			var curve = new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 2, 0), new Point3D(3, 2, 0), new Point3D(4, 0, 0));

			var error = Assert.Throws<ArgumentException>(() => curve.SplitPoints(0.5, new Point3D[5], new Point3D[5]));
			Assert.Equal("points1", error.ParamName);
			Assert.Throws<ArgumentException>(() => curve.SplitPoints(0.5, new Point3D[4], new Point3D[3]));
			Assert.Throws<ArgumentNullException>(() => curve.SplitPoints(0.5, null, new Point3D[4]));

			var shared = new Point3D[4];
			Assert.Throws<ArgumentException>(() => curve.SplitPoints(0.5, shared, shared));

			var left = new Point3D[4];
			var right = new Point3D[4];
			curve.SplitPoints(0.5, left, right);
			TestUtil.Near(new Point3D(2, 1.5, 0), left[3]);
			TestUtil.Near(new Point3D(2, 1.5, 0), right[0]);
		}

		[Fact]
		public void BezierCurve_GetPartFromEndOfCurve()
		{
			// Прежде участок вычислялся с делением на 1 − t0: при t0 = 1 точки были NaN, а при t0, близком к 1,
			// результат терял точность.
			var curve = new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 2, 0), new Point3D(3, 2, 1), new Point3D(4, 0, 1));

			var part = curve.GetPart(1, 0.5);
			TestUtil.Near(curve.GetValue(1), part.GetValue(0));
			TestUtil.Near(curve.GetValue(0.75), part.GetValue(0.5));
			TestUtil.Near(curve.GetValue(0.5), part.GetValue(1));

			var nearEnd = curve.GetPart(1 - 1e-12, 0.5);
			TestUtil.Near(curve.GetValue(0.5), nearEnd.GetValue(1), 1e-9);
			TestUtil.Near(curve.GetValue(0.75), nearEnd.GetValue(0.5), 1e-9);

			var reversed = curve.GetPart(1, 0);
			foreach (var t in new[] {0, 0.3, 1})
			{
				TestUtil.Near(curve.GetValue(1 - t), reversed.GetValue(t));
			}

			var point = curve.GetPart(0.3, 0.3);
			TestUtil.Near(curve.GetValue(0.3), point.GetValue(0.7));

			// Обычные участки, в том числе в обратном направлении, не изменились.
			var middle = curve.GetPart(0.2, 0.7);
			TestUtil.Near(curve.GetValue(0.45), middle.GetValue(0.5));
			var backward = curve.GetPart(0.8, 0.2);
			TestUtil.Near(curve.GetValue(0.8), backward.GetValue(0));
			TestUtil.Near(curve.GetValue(0.2), backward.GetValue(1));
		}

		[Fact]
		public void BernsteinCurve_ParameterRangeChecks()
		{
			// Прежде GetValue выбрасывал ArgumentException без имени параметра, а GetTangent и GetCurvature принимали
			// любые значения.
			var curve = new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 2, 0), new Point3D(3, 2, 0), new Point3D(4, 0, 0));

			Assert.Equal("parameter", Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetValue(1.0000000000000002)).ParamName);
			Assert.Equal("parameter", Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetValue(-1e-300)).ParamName);
			Assert.Equal("parameter", Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetTangent(1.5)).ParamName);
			Assert.Equal("parameter", Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetCurvature(-1)).ParamName);

			var line = new LineCurve(Point3D.Empty, new Point3D(1, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => line.GetValue(2));

			TestUtil.Near(new Point3D(4, 0, 0), curve.GetValue(1));
			TestUtil.Near(new Point3D(1, 0, 0), curve.GetTangent(0.5));
		}

		#endregion

		[Fact]
		public void ParametricCurve_ToStringWithoutLength()
		{
			// Прежде ToString вычислял длину и выбрасывал NotImplementedException для кривых, в которых реализован только GetValue.
			Assert.Equal(nameof(ValueOnlyCurve), new ValueOnlyCurve().ToString());

			var line = new LineCurve(Point3D.Empty, new Point3D(1.5, 0, 0));
			Assert.Equal("LineCurve; Length: 1.5", TestUtil.WithCulture("", () => line.ToString()));
			Assert.Equal("LineCurve; Length: 1,5", TestUtil.WithCulture("ru-RU", () => line.ToString()));
		}
	}
}
