using System;
using System.Linq;
using System.Reflection;
using Ruzil3D.Algebra;
using Ruzil3D.Approximation;
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

		private static BezierCurve StraightBezier(double length)
		{
			return new BezierCurve(Point3D.Empty, new Point3D(length/3, 0, 0), new Point3D(2*length/3, 0, 0),
				new Point3D(length, 0, 0));
		}

		/// <summary>
		/// Кривая с точкой возврата при t = 0.5.
		/// </summary>
		private static BezierCurve Cusp()
		{
			return new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 1, 0), new Point3D(0, 1, 0), new Point3D(1, 0, 0));
		}

		/// <summary>
		/// Кривая с резким поворотом («шпилька») при t = 0.5.
		/// </summary>
		private static BezierCurve Hairpin()
		{
			return new BezierCurve(new Point3D(0, 0, 0), new Point3D(10, 0, 0), new Point3D(10, 0.01, 0), new Point3D(0, 0.01, 0));
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

		/// <summary>
		/// Приводит угол к интервалу (−π, π].
		/// </summary>
		private static double Wrap(double angle)
		{
			return angle - 2*System.Math.PI*System.Math.Round(angle/(2*System.Math.PI));
		}

		/// <summary>
		/// Точная функция выпрямления: сумма длин мелких участков, на каждом из которых интегрирование точно.
		/// </summary>
		private sealed class ArcLength
		{
			private const int Count = 4096;
			private readonly ParametricCurve _curve;
			private readonly double[] _sums = new double[Count + 1];

			public ArcLength(ParametricCurve curve)
			{
				_curve = curve;
				for (var i = 1; i <= Count; i++)
				{
					_sums[i] = _sums[i - 1] + curve.GetDistance((i - 1D)/Count, (double) i/Count);
				}
			}

			public double Length => _sums[Count];

			public double At(double t)
			{
				var k = (int) System.Math.Min(Count - 1, System.Math.Floor(t*Count));
				return _sums[k] + _curve.GetDistance((double) k/Count, t);
			}
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

		#region Elliptic arcs

		[Fact]
		public void EllipticArc_SweepFollowsPolarAngle()
		{
			// Прежде развёртка параметра поправлялась только по знаку конечного параметра: для 144 из 544 пар
			// (начало, развёртка) дуга шла в обратную сторону или охватывала лишний оборот.
			foreach (var axes in new[] {new[] {2D, 1D}, new[] {1D, 3D}})
			{
				for (var i = -8; i <= 8; i++)
				{
					for (var j = -16; j <= 16; j++)
					{
						if (j == 0)
						{
							continue;
						}

						var start = i*System.Math.PI/8 + 0.05;
						var sweep = j*System.Math.PI/8 + 0.03;
						var arc = new EllipticArcCurve(axes[0], axes[1], start, sweep);

						var point = arc.GetValue(0);
						var previous = System.Math.Atan2(point.Y, point.X);
						Assert.Equal(0, Wrap(previous - start), 9);

						// Полярный угол точки дуги меняется монотонно в сторону развёртки и в сумме на развёртку.
						var angle = start;
						for (var k = 1; k <= 400; k++)
						{
							point = arc.GetValue(k/400D);
							var current = System.Math.Atan2(point.Y, point.X);
							var delta = Wrap(current - previous);

							Assert.True(delta*sweep >= -1e-12, "Дуга идёт против развёртки: start = " + start + ", sweep = " + sweep);
							angle += delta;
							previous = current;
						}

						Assert.Equal(start + sweep, angle, 9);
					}
				}
			}
		}

		[Fact]
		public void EllipticArc_Length()
		{
			// Для эллипса 2×1 дуга (π/2, −π/4) имела длину 10.59 вместо 0.90, а полный эллипс обходился дважды.
			var arc = new EllipticArcCurve(2, 1, System.Math.PI/2, -System.Math.PI/4);
			var stop = System.Math.Atan2(2*System.Math.Sin(System.Math.PI/4), System.Math.Cos(System.Math.PI/4));
			var expected = Polyline(theta => new Point3D(2*System.Math.Cos(theta), System.Math.Sin(theta), 0), stop, System.Math.PI/2, 100000);

			Assert.Equal(expected, arc.Length, 8);
			TestUtil.Near(new Point3D(2*System.Math.Cos(stop), System.Math.Sin(stop), 0), arc.GetValue(1), 1e-12);

			// Периметр эллипса с полуосями 2 и 1.
			Assert.Equal(9.688448220547675, new EllipticArcCurve(2, 1, 0, 2*System.Math.PI).Length, 9);
			Assert.Equal(9.688448220547675, new EllipticArcCurve(2, 1, 1, -2*System.Math.PI).Length, 9);
		}

		[Fact]
		public void EllipticArc_ClockwiseTangentAndCurvature()
		{
			// Прежде касательная и кривизна не учитывали знак развёртки: у дуги, идущей по часовой стрелке, касательная
			// была направлена против движения, а кривизна была положительной.
			var circle = new EllipticArcCurve(1, 0, -System.Math.PI/2);
			TestUtil.Near(new Point3D(0, -1, 0), circle.GetTangent(0), 1e-12);
			TestUtil.Near(new Point3D(0, 0, -1), circle.GetCurvature(0.5), 1e-12);

			var ellipse = new EllipticArcCurve(2, 1, 0, -System.Math.PI/2);
			TestUtil.Near(new Point3D(0, -1, 0), ellipse.GetTangent(0), 1e-12);
			TestUtil.Near(new Point3D(0, 0, -2), ellipse.GetCurvature(0), 1e-12);

			var arcs = new[]
			{
				circle, ellipse, new EllipticArcCurve(1, 0.5, 2), new EllipticArcCurve(2, 1, 0.3, 2),
				new EllipticArcCurve(1, 3, 0.5, -4), new EllipticArcCurve(3, 1.5, -1, -1)
			};

			foreach (var arc in arcs)
			{
				foreach (var t in new[] {0.1, 0.5, 0.9})
				{
					const double h = 1e-4;
					var d1 = (arc.GetValue(t + h) - arc.GetValue(t - h))/(2*h);
					var d2 = (arc.GetValue(t + h) - arc.GetValue(t)*2 + arc.GetValue(t - h))/(h*h);
					var curvature = d1*d2/System.Math.Pow(d1.Length, 3);

					TestUtil.Near(d1/d1.Length, arc.GetTangent(t), 1e-6);
					TestUtil.Near(curvature, arc.GetCurvature(t), 1e-5);
				}
			}
		}

		[Fact]
		public void EllipticArc_RecastToBezierCurves()
		{
			// Одна кубическая кривая приближает большую дугу плохо: для полной окружности её промежуточные точки уходили
			// в бесконечность. Новый метод делит дугу на участки не больше четверти оборота.
			var sweeps = new[] {2*System.Math.PI, -2*System.Math.PI, 1.5*System.Math.PI, 0.3};
			var counts = new[] {4, 4, 3, 1};
			for (var s = 0; s < sweeps.Length; s++)
			{
				var arc = new EllipticArcCurve(1, 0.2, sweeps[s]);
				var curves = arc.RecastToBezierCurves();

				Assert.Equal(counts[s], curves.Length);
				TestUtil.Near(arc.GetValue(0), curves[0].P0, 1e-12);
				TestUtil.Near(arc.GetValue(1), curves[curves.Length - 1].P3, 1e-12);

				var error = 0D;
				for (var k = 0; k < curves.Length; k++)
				{
					if (k > 0)
					{
						Assert.Equal(curves[k - 1].P3, curves[k].P0);
					}

					TestUtil.Near(arc.GetTangent((double) k/curves.Length), curves[k].GetTangent(0), 1e-9);

					for (var i = 0; i <= 100; i++)
					{
						error = System.Math.Max(error, System.Math.Abs(curves[k].GetValue(i/100D).Length - 1));
					}
				}

				Assert.InRange(error, 0, 3e-4);
			}

			var ellipse = new EllipticArcCurve(3, 1.5, 0.3, 5);
			foreach (var curve in ellipse.RecastToBezierCurves())
			{
				for (var i = 0; i <= 100; i++)
				{
					var point = curve.GetValue(i/100D);
					var radius = System.Math.Sqrt(point.X*point.X/9 + point.Y*point.Y/2.25);
					Assert.InRange(radius, 1 - 3e-4, 1 + 3e-4);
				}
			}
		}

		#endregion

		#region Distances

		[Fact]
		public void GetDistance_SignedForAllCurveTypes()
		{
			// Прежде знак GetDistance(0.75, 0.25) зависел от типа кривой: для отрезка и дуги окружности он был
			// положительным, для кривой Безье и эллиптической дуги — отрицательным.
			var curves = new ParametricCurve[]
			{
				new LineCurve(Point3D.Empty, new Point3D(4, 0, 0)), StraightBezier(4), new EllipticArcCurve(1, 0, System.Math.PI/2),
				new EllipticArcCurve(1, 0, -System.Math.PI/2), new EllipticArcCurve(2, 1, 0, System.Math.PI/2)
			};

			foreach (var curve in curves)
			{
				var forward = curve.GetDistance(0.25, 0.75);

				Assert.True(forward > 0);
				Assert.Equal(-forward, curve.GetDistance(0.75, 0.25), 12);
			}

			Assert.Equal(2, curves[0].GetDistance(0.25, 0.75), 12);
			Assert.Equal(System.Math.PI/4, curves[3].GetDistance(0.25, 0.75), 12);
		}

		[Fact]
		public void DistanceCompiler_CuspAtHighAccuracy()
		{
			// Прежде при acc = 12 и 14 расстояния до узлов переставали возрастать, и GetParameter(L/2) для кривой
			// с точкой возврата возвращал 0 вместо 0.5.
			var cusp = Cusp();
			foreach (var accuracy in new[] {12, 14})
			{
				var compiler = new ParametricCurveDistanceCompiler<BezierCurve>(cusp, accuracy);
				Assert.InRange(compiler.GetParameter(cusp.Length/2), 0.5 - 1e-6, 0.5 + 1e-6);
			}

			// По симметрии «шпильки» середина её длины соответствует t = 0.5, а скорость там всего 0.015, поэтому
			// расстоянию 7.500013 (на 3e-7 больше половины длины) соответствует t ≈ 0.5 ± 2e-4. Прежде получался конец кривой.
			var hairpin = Hairpin();
			foreach (var accuracy in new[] {12, 14})
			{
				var compiler = new ParametricCurveDistanceCompiler<BezierCurve>(hairpin, accuracy);
				Assert.InRange(compiler.GetParameter(hairpin.Length/2), 0.5 - 1e-9, 0.5 + 1e-9);
				Assert.InRange(compiler.GetParameter(7.500013), 0.4995, 0.5005);
			}
		}

		[Fact]
		public void DistanceCompiler_MonotoneAndAccurate()
		{
			var curves = new[]
			{
				Cusp(), Hairpin(),
				new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 2, 0), new Point3D(3, 2, 0), new Point3D(4, 0, 0))
			};

			foreach (var curve in curves)
			{
				var arcLength = new ArcLength(curve);

				foreach (var accuracy in new[] {6, 10, 14})
				{
					foreach (var type in new[] {EApproximationType.Default, EApproximationType.Linear})
					{
						var compiler = new ParametricCurveDistanceCompiler<BezierCurve>(curve, accuracy, type);
						var length = curve.Length;
						var tolerance = accuracy == 6 ? 1e-3 : accuracy == 10 ? 1e-5 : 1e-7;

						var previous = 0D;
						for (var i = 0; i <= 2000; i++)
						{
							var distance = System.Math.Min(length, length*i/2000);
							var parameter = compiler.GetParameter(distance);

							// Параметр не убывает, а доля длины до него совпадает с долей заданного расстояния.
							Assert.True(parameter >= previous, "Параметр убывает: acc = " + accuracy + ", s = " + distance);
							Assert.InRange(arcLength.At(parameter)/arcLength.Length - distance/length, -tolerance, tolerance);
							previous = parameter;
						}

						Assert.Equal(0, compiler.GetParameter(0));
						Assert.Equal(1, compiler.GetParameter(length));
					}
				}
			}
		}

		[Fact]
		public void DistanceCompiler_GetArgumentInvertsGetValue()
		{
			// Прежде обратная функция не была реализована ни в одной из внутренних аппроксимаций.
			var field = typeof(ParametricCurveDistanceCompiler<BernsteinCurve>).GetField("_approx", BindingFlags.NonPublic | BindingFlags.Instance);
			var curves = new BernsteinCurve[] {Cusp(), new LineCurve(Point3D.Empty, new Point3D(3, 4, 0))};

			foreach (var curve in curves)
			{
				foreach (var type in new[] {EApproximationType.Default, EApproximationType.Linear})
				{
					var compiler = new ParametricCurveDistanceCompiler<BernsteinCurve>(curve, 8, type);
					compiler.GetParameter(0);
					var approximation = (IMonotonicFunctionApproximation) field.GetValue(compiler);

					Assert.Equal(0, approximation.LArgument);
					Assert.Equal(curve.Length, approximation.RArgument);
					for (var i = 0; i <= 100; i++)
					{
						var distance = curve.Length*i/100;
						Assert.Equal(distance, approximation.GetArgument(approximation.GetValue(distance)), 9);
					}
				}
			}
		}

		[Fact]
		public void DistanceCompiler_ZeroLengthCurves()
		{
			// Прежде расстояние делилось на нулевую длину, и вместо начальной точки получались точки NaN.
			var point = new Point3D(1, 2, 3);

			var line = new ParametricCurveDistanceCompiler<LineCurve>(new LineCurve(point, point));
			Assert.Equal(point, line.GetValue(0));
			Assert.Equal(0, line.GetParameter(0));

			var bezier = new ParametricCurveDistanceCompiler<BezierCurve>(new BezierCurve(point, point, point, point));
			Assert.Equal(point, bezier.GetValue(0));
			Assert.Equal(0, bezier.GetParameter(0));

			var arc = new ParametricCurveDistanceCompiler<EllipticArcCurve>(new EllipticArcCurve(1, 0.3, 0));
			TestUtil.Near(new Point3D(System.Math.Cos(0.3), System.Math.Sin(0.3), 0), arc.GetValue(0));

			var path = new Beziers(new[]
			{
				new Point3D(0, 0, 0), new Point3D(1, 0, 0), new Point3D(2, 0, 0), new Point3D(3, 0, 0),
				new Point3D(3, 0, 0), new Point3D(3, 0, 0), new Point3D(3, 0, 0)
			});

			Assert.Equal(3, path.Length, 12);
			TestUtil.Near(new Point3D(3, 0, 0), path.GetValue(path.Length), 1e-12);

			var details = path.GetDetails(path.Length);
			Assert.Equal(0, details.Parameter);
			Assert.Equal(0, details.Distance);
		}

		[Fact]
		public void DistanceCompiler_RangeChecks()
		{
			// Прежде выход за длину кривой приводил для отрезка к ArgumentException без имени параметра,
			// а для кривой Безье — к ArgumentOutOfRangeException для аргумента x.
			var line = new LineCurve(Point3D.Empty, new Point3D(4, 0, 0));
			var lineCompiler = new ParametricCurveDistanceCompiler<LineCurve>(line);
			var bezier = new BezierCurve(new Point3D(0, 0, 0), new Point3D(1, 2, 0), new Point3D(3, 2, 0), new Point3D(4, 0, 0));
			var bezierCompiler = new ParametricCurveDistanceCompiler<BezierCurve>(bezier);

			Assert.Equal("distance", Assert.Throws<ArgumentOutOfRangeException>(() => lineCompiler.GetValue(4.0000000000000009)).ParamName);
			Assert.Equal("distance", Assert.Throws<ArgumentOutOfRangeException>(() => bezierCompiler.GetValue(bezier.Length*(1 + 1e-15))).ParamName);
			Assert.Equal("distance", Assert.Throws<ArgumentOutOfRangeException>(() => bezierCompiler.GetParameter(-1e-12)).ParamName);

			TestUtil.Near(new Point3D(4, 0, 0), lineCompiler.GetValue(4));
			TestUtil.Near(bezier.P3, bezierCompiler.GetValue(bezier.Length));
		}

		[Fact]
		public void ParametricCurves_CurveReplacedInCompiler()
		{
			// Прежде длины кривых сохранялись навсегда, и после замены кривой в компиляторе длина последовательности
			// оставалась прежней: GetValue(Length) возвращал (3, 0, 0) вместо (30, 0, 0).
			var single = new ParametricCurveDistanceCompiler<BezierCurve>(StraightBezier(3));
			var path = new ParametricCurves<BezierCurve>(new[] {single});
			Assert.Equal(3, path.Length, 10);

			single.Curve = StraightBezier(30);

			Assert.Equal(30, path.Length, 10);
			TestUtil.Near(new Point3D(30, 0, 0), path.GetValue(path.Length), 1e-9);

			var compiler = new ParametricCurveDistanceCompiler<BezierCurve>(StraightBezier(3));
			var curves = new ParametricCurves<BezierCurve>(new[] {compiler, new ParametricCurveDistanceCompiler<BezierCurve>(StraightBezier(1) + new Point3D(3, 0, 0))});

			Assert.Equal(4, curves.Length, 10);

			compiler.Curve = StraightBezier(30);

			Assert.Equal(31, curves.Length, 10);
			TestUtil.Near(new Point3D(29.5, 0, 0), curves.GetValue(29.5), 1e-9);
			TestUtil.Near(new Point3D(3.5, 0, 0), curves.GetValue(30.5), 1e-9);
			TestUtil.Near(new Point3D(4, 0, 0), curves.GetValue(curves.Length), 1e-9);
			Assert.Equal(0, curves.GetDetails(20).Index);
			Assert.Equal(20, curves.GetDetails(20).Distance, 9);
			Assert.Equal(1, curves.GetDetails(30.5).Index);
			Assert.Equal(0.5, curves.GetDetails(30.5).Distance, 9);
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
