using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ruzil3D.Algebra;
using Ruzil3D.Filters;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Регрессионные тесты исправлений в сглаживающих фильтрах.
	/// </summary>
	public class FiltersFixesTests
	{
		#region Вспомогательные методы

		/// <summary>
		/// Ломаная в форме буквы L: 100 отрезков длиной 0.05 вдоль X, затем 100 вдоль Y; вершина излома имеет индекс 100.
		/// </summary>
		private static List<Point3D> LShape(double offset)
		{
			var points = new List<Point3D>();
			for (var i = 0; i <= 100; i++)
			{
				points.Add(new Point3D(offset + i*0.05, offset, 0));
			}

			for (var i = 1; i <= 100; i++)
			{
				points.Add(new Point3D(offset + 5, offset + i*0.05, 0));
			}

			return points;
		}

		private static Point3D[] StraightLine(Point3D direction, int count, double step)
		{
			var points = new Point3D[count];
			for (var i = 0; i < count; i++)
			{
				points[i] = (i*step)*direction;
			}

			return points;
		}

		private static BlurFunctionHandler Gaussian(double sigma)
		{
			return r => System.Math.Exp(-r*r/(2*sigma*sigma));
		}

		/// <summary>
		/// Ядро Ланцоша с a = 2: отрицательно при 1 &lt; r &lt; 2 и равно нулю при r ≥ 2.
		/// </summary>
		private static double Lanczos2(double r)
		{
			if (r < 1e-12)
			{
				return 1;
			}

			if (r >= 2)
			{
				return 0;
			}

			var a = System.Math.PI*r;
			return System.Math.Sin(a)/a*System.Math.Sin(a/2)/(a/2);
		}

		private static bool IsFinite(Point3D point)
		{
			return !double.IsNaN(point.X) && !double.IsNaN(point.Y) && !double.IsNaN(point.Z) &&
			       !double.IsInfinity(point.X) && !double.IsInfinity(point.Y) && !double.IsInfinity(point.Z);
		}

		/// <summary>
		/// Интерполяция с недопустимым режимом экстраполяции.
		/// </summary>
		private sealed class InvalidModeInterpolation : ICurveInterpolation
		{
			private readonly CurveInterpolationCompiler _inner = new CurveInterpolationCompiler(new[] {Point3D.Empty, Point3D.UnitX});

			public Point3D GetValue3D(double d) => _inner.GetValue3D(d);

			public ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror) => _inner.LoadFrom(curve, mode);

			public double DMax => _inner.DMax;

			public double GetD(int index) => _inner.GetD(index);

			public Point3D this[int index] => _inner[index];

			public ExtrapolationMode Mode => (ExtrapolationMode) 7;

			public int Length => _inner.Length;

			public bool IsClosed => false;
		}

		#endregion

		#region BlurCompiler

		[Fact]
		public void BlurCompiler_KernelIsCalledWithNonNegativeRadius()
		{
			// Прежде центральный сектор передавал ядру радиус −0.091, и exp(−√r) давал NaN во всех весах.
			var minRadius = double.MaxValue;
			var compiler = new BlurCompiler(r =>
			{
				minRadius = System.Math.Min(minRadius, r);
				return System.Math.Exp(-System.Math.Sqrt(r));
			});

			Assert.True(minRadius >= 0, "Наименьший радиус: " + minRadius);
			Assert.Equal(1, compiler.GetValue(x => 1.0, 0.3), 12);
			Assert.Equal(0.7, compiler.GetValue(x => x, 0.7), 12);
		}

		[Fact]
		public void BlurCompiler_KernelWithZeroAtCenter_Compiles()
		{
			// Прежде перебор секторов останавливался по порогу 1e-50·f(0) = 0 и заканчивался System.Exception.
			var compiler = TestUtil.CompletesWithin(() => new BlurCompiler(r => r*r*System.Math.Exp(-r*r)));

			Assert.Equal(1, compiler.GetValue(x => 1.0, 2), 12);

			// Второй момент плотности x²·exp(−x²) равен 1.5 (с точностью дискретизации ядра).
			Assert.InRange(compiler.GetValue(x => x*x, 0), 1.45, 1.55);
		}

		[Fact]
		public void BlurCompiler_KernelWithNegativeLobes_IsNotTruncated()
		{
			// Прежде ядро обрезалось на первом отрицательном секторе (r ≈ 1.36), хотя ядро Ланцоша отлично от нуля до r = 2.
			var compiler = new BlurCompiler(Lanczos2);

			var outer = compiler.GetValue(x => System.Math.Abs(x) > 1.5 && System.Math.Abs(x) < 2.1 ? 1.0 : 0.0, 0);

			Assert.True(outer < 0, "Вклад отрицательного лепестка: " + outer);
			Assert.Equal(1, compiler.GetValue(x => 1.0, 0), 12);
		}

		[Fact]
		public void BlurCompiler_KernelWithZeroGap_KeepsOuterPart()
		{
			// Прежде ядро с нулевым промежутком 0.3 < r < 1 обрезалось на радиусе 0.43, и его часть 1 < r < 2 терялась.
			var compiler = new BlurCompiler(r => r < 0.3 || (r > 1 && r < 2) ? 1 : 0);

			var outer = compiler.GetValue(x => System.Math.Abs(x) > 1.2 && System.Math.Abs(x) < 1.9 ? 1.0 : 0.0, 0);

			Assert.InRange(outer, 0.3, 0.8);
		}

		[Fact]
		public void BlurCompiler_HeavyTailedKernel_RadiusIsLimited()
		{
			// Прежде ядро Коши перебиралось до 763 секторов и радиуса 1.6e49, и функция вызывалась на таком удалении.
			var compiler = TestUtil.CompletesWithin(() => new BlurCompiler(r => 1/(1 + r*r)));

			var maxArgument = 0D;
			var value = compiler.GetValue(x =>
			{
				maxArgument = System.Math.Max(maxArgument, System.Math.Abs(x));
				return 1.0;
			}, 0);

			Assert.Equal(1, value, 12);
			Assert.InRange(maxArgument, 1e3, 1e7);
		}

		[Fact]
		public void BlurCompiler_KernelThatCannotBeNormalized_Throws()
		{
			// Прежде вейвлет Рикера с нулевой суммой молча нормировался делением на погрешность,
			// а нулевое ядро и ядро со значениями NaN приводили к System.Exception после 1000 секторов.
			Assert.Throws<ArgumentException>(() => new BlurCompiler(r => (1 - r*r)*System.Math.Exp(-r*r/2)));
			Assert.Throws<ArgumentException>(() => TestUtil.CompletesWithin(() => new BlurCompiler(r => 0)));
			Assert.Throws<ArgumentException>(() => TestUtil.CompletesWithin(() => new BlurCompiler(r => r < 1 ? 1 : double.NaN)));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		[InlineData(double.NaN)]
		[InlineData(double.PositiveInfinity)]
		public void BlurCompiler_InvalidAccuracy_Throws(double accuracy)
		{
			// Прежде при 0 все веса были NaN, при −1 получались два бессмысленных сектора,
			// а при NaN и бесконечности перебор заканчивался System.Exception.
			var error = Assert.Throws<ArgumentOutOfRangeException>(() => new BlurCompiler(accuracy));
			Assert.Equal("accuracy", error.ParamName);
			Assert.Throws<ArgumentOutOfRangeException>(() => new BlurCompiler(BlurCompiler.DefaultFilter, accuracy));
		}

		#endregion

		#region CurveBlurFilter2

		[Theory]
		[InlineData(1, 2, 3)]
		[InlineData(1, 1, 0)]
		public void CurveBlurFilter2_StraightLine_HasNoNaN(double x, double y, double z)
		{
			// Прежде косинус угла на прямой из-за округления бывал меньше −1, Acos давал NaN,
			// и через сглаживание значения угла NaN получался во всех точках кривой.
			var direction = new Point3D(x, y, z);
			var filter = new CurveBlurFilter2(StraightLine(direction, 151, 0.05), BlurCompiler.DefaultFilter);
			var unit = direction/direction.Length;

			for (var k = 0; k <= 300; k++)
			{
				var d = filter.DMax*k/300;
				TestUtil.Near(d*unit, filter.GetValue3D(d), 1e-9);
			}

			Assert.Equal(1, filter.GetCornerValue(filter.DMax/2), 6);
		}

		[Fact]
		public void CurveBlurFilter2_DiagonalRunAndCorner_HasNoNaN()
		{
			var points = new List<Point3D>();
			for (var i = 0; i <= 60; i++)
			{
				points.Add(new Point3D(0.05*i, 0.05*i, 0));
			}

			for (var i = 1; i <= 60; i++)
			{
				points.Add(new Point3D(3 + 0.05*i, 3, 0));
			}

			var filter = new CurveBlurFilter2(points.ToArray(), BlurCompiler.DefaultFilter);

			for (var k = 0; k <= 400; k++)
			{
				Assert.True(IsFinite(filter.GetValue3D(filter.DMax*k/400)));
			}

			// Излом в 45° сглаживается слабее прямых участков.
			var corner = filter.GetCornerValue(filter.GetD(60));
			Assert.InRange(corner, 0.5, 0.99);
		}

		[Fact]
		public void CurveBlurFilter2_FarFromOrigin_MatchesShiftedCurve()
		{
			// Прежде при координатах порядка 1e7 приращение с абсолютным шагом 1e-9 терялось в ошибке округления,
			// и почти все точки сглаженной кривой были NaN.
			const double offset = 1e7;
			var shift = new Point3D(offset, offset, 0);
			var near = new CurveBlurFilter2(LShape(0).ToArray(), BlurCompiler.DefaultFilter);
			var far = new CurveBlurFilter2(LShape(offset).ToArray(), BlurCompiler.DefaultFilter);

			for (var k = 0; k <= 200; k++)
			{
				var d = near.DMax*k/200;
				TestUtil.Near(near.GetValue3D(d) + shift, far.GetValue3D(d), 1e-6);
			}
		}

		[Fact]
		public void CurveBlurFilter2_LoadFrom_CreatesFilterWithSameKernel()
		{
			// Прежде метод выбрасывал NotImplementedException.
			var points = LShape(0).ToArray();
			var filter = new CurveBlurFilter2(points, Gaussian(1));

			var loaded = filter.LoadFrom(points);

			Assert.IsType<CurveBlurFilter2>(loaded);
			TestUtil.Near(filter.GetValue3D(5), loaded.GetValue3D(5), 1e-15);
			TestUtil.Near(filter.GetValue3D(2.5), loaded.GetValue3D(2.5), 1e-15);
		}

		#endregion

		#region CurveBlurFilter

		[Fact]
		public void CurveBlurFilter_LoadFrom_KeepsKernel()
		{
			// Прежде LoadFrom создавал фильтр с гауссовым ядром по умолчанию, и заданное ядро терялось.
			var points = LShape(0);
			var filter = new CurveBlurFilter(points, Gaussian(1));

			var loaded = filter.LoadFrom(points.ToArray());

			TestUtil.Near(filter.GetValue3D(5), loaded.GetValue3D(5), 1e-15);
			TestUtil.Near(filter.GetValue3D(4.2), loaded.GetValue3D(4.2), 1e-15);
		}

		[Fact]
		public void CurveBlurFilter_Resolve_KeepsThirdCoordinate()
		{
			// Прежде кривая Безье строилась двумерным ResolveXY, и у пространственной кривой терялась координата Z.
			var helix = new List<Point3D>();
			for (var i = 0; i <= 200; i++)
			{
				var t = i*0.05;
				helix.Add(new Point3D(3*System.Math.Cos(t), 3*System.Math.Sin(t), 2*t));
			}

			var filter = new CurveBlurFilter(helix);

			var bezier = filter.Resolve(2, 4);

			TestUtil.Near(filter.GetValue3D(2), bezier.P0);
			TestUtil.Near(filter.GetValue3D(4), bezier.P3);
			TestUtil.Near(filter.GetValue3D(3), bezier.GetValue(0.5), 0.01);
		}

		[Theory]
		[InlineData(1e6)]
		[InlineData(1e7)]
		[InlineData(1e8)]
		public void CurveBlurFilter_SearchCorners_FarFromOrigin(double offset)
		{
			// Прежде приращение бралось с абсолютным шагом 1e-9 в абсолютных координатах: при смещении 1e6 находился
			// ложный излом, при 1e7 излом смещался, а при 1e8 пропадал, и вместо косинусов получался NaN.
			var expected = new double[201];
			new CurveBlurFilter(LShape(0)).SearchCorners(ref expected);

			var points = LShape(offset);
			var filter = new CurveBlurFilter(points);
			var derivative = new double[points.Count];

			Assert.Equal(new[] {100}, filter.SearchCorners(ref derivative));
			Assert.Equal(new[] {100}, filter.SearchCorners(0.5, 170));
			for (var i = 0; i < derivative.Length; i++)
			{
				Assert.InRange(derivative[i], expected[i] - 1e-6, expected[i] + 1e-6);
			}
		}

		[Fact]
		public void CurveBlurFilter_SearchCorners_StraightLineFarFromOrigin()
		{
			// Прежде у прямой при смещении 1e6 косинус угла падал до 0.986, а при 1e7 становился NaN.
			foreach (var offset in new[] {1e6, 1e7})
			{
				var points = new List<Point3D>();
				for (var i = 0; i <= 200; i++)
				{
					points.Add(new Point3D(offset + 0.05*i, offset + 0.02*i, 0));
				}

				var filter = new CurveBlurFilter(points);
				var derivative = new double[points.Count];

				Assert.Empty(filter.SearchCorners(ref derivative, 0.5, 179));
				for (var i = 15; i < 185; i++)
				{
					Assert.InRange(derivative[i], 1 - 1e-9, 1 + 1e-9);
				}
			}
		}

		[Fact]
		public void CurveBlurFilter_GetCorrection_StraightSegments_ReturnZero()
		{
			// Прежде на отрезке, параллельном оси, получалось (NaN, NaN, NaN), а на остальных — шум порядка 1e-8.
			var axis = new CurveBlurFilter(StraightLine(Point3D.UnitX, 101, 0.05));
			var generic = new CurveBlurFilter(StraightLine(new Point3D(0.037, 0.071, 0.013), 101, 1));

			Assert.Equal(0, axis.GetCorrection(2.5).Length);
			Assert.Equal(0, generic.GetCorrection(generic.DMax/2).Length);
		}

		[Fact]
		public void CurveBlurFilter_GetCorrection_Spike_IsFinite()
		{
			// Прежде у острия угол был равен нулю, и множитель (π − angle)/angle давал бесконечность.
			var points = new List<Point3D>();
			for (var i = 0; i <= 50; i++)
			{
				points.Add(new Point3D(0.05*i, 0, 0));
			}

			for (var i = 49; i >= 0; i--)
			{
				points.Add(new Point3D(0.05*i, 0, 0));
			}

			var spike = new CurveBlurFilter(points).GetCorrection(2.5);

			Assert.True(IsFinite(spike));
			Assert.True(spike.X > 0);
			Assert.Equal(0, spike.Y, 12);
			Assert.Equal(0, spike.Z, 12);

			// Для прямого угла поправка не изменилась: единичный вектор по внешней биссектрисе.
			var corner = new CurveBlurFilter(LShape(0)).GetCorrection(5);
			TestUtil.Near(new Point3D(System.Math.Sqrt(0.5), -System.Math.Sqrt(0.5), 0), corner, 1e-6);
		}

		[Fact]
		public void CurveBlurFilter_FilterPoints_CountIsLimited()
		{
			// Прежде для кривой длиннее ~8.6e7 число точек переполняло int (OverflowException),
			// а для кривой длиной 0.02 получалось всего 3 точки.
			var longCurve = new CurveBlurFilter(new[] {Point3D.Empty, new Point3D(1e8, 0, 0)});
			var shortCurve = new CurveBlurFilter(new[] {Point3D.Empty, new Point3D(0.01, 0, 0), new Point3D(0.01, 0.01, 0)});

			var longPoints = TestUtil.CompletesWithin(() => longCurve.FilterPoints(), 60000);
			var shortPoints = shortCurve.FilterPoints();

			Assert.Equal(1000000, longPoints.Length);
			TestUtil.Near(Point3D.Empty, longPoints[0]);
			TestUtil.Near(new Point3D(1e8, 0, 0), longPoints[longPoints.Length - 1], 1e-6);
			Assert.Equal(10, shortPoints.Length);
			TestUtil.Near(new Point3D(0.01, 0.01, 0), shortPoints[shortPoints.Length - 1]);
		}

		[Fact]
		public void CurveBlurFilter_CornerOrdering_UsesGenericComparable()
		{
			// Прежде структура реализовывала только необобщённый IComparable: сортировка упаковывала значения,
			// а CompareTo(null) выбрасывал NullReferenceException.
			var type = typeof(CurveBlurFilter).GetNestedType("CIndexValue", BindingFlags.NonPublic);

			Assert.NotNull(type);
			Assert.True(typeof(IComparable<>).MakeGenericType(type).IsAssignableFrom(type));
		}

		#endregion

		#region SurfaceBlurFilter

		[Fact]
		public void SurfaceBlurFilter_WideKernel_UsesWholeKernel()
		{
			// Прежде кольца строились только до радиуса 3, и для σ = 2 получалось 5.35 вместо 2σ² = 8.
			var filter = new SurfaceBlurFilter(Gaussian(2));

			Assert.InRange(filter.GetValue(0, 0, (x, y) => x*x + y*y), 7.95, 8.05);
			Assert.Equal(1, filter.GetValue(3, -1, (x, y) => 1), 12);
		}

		[Fact]
		public void SurfaceBlurFilter_NarrowKernel_StillSmooths()
		{
			// Прежде ширина центрального круга 0.01 не зависела от ядра, и ядро с σ = 0.001 целиком попадало в него:
			// фильтр возвращал функцию без сглаживания.
			var filter = new SurfaceBlurFilter(Gaussian(0.001));

			Assert.InRange(filter.GetValue(0, 0, (x, y) => x*x + y*y), 1.98e-6, 2.02e-6);
		}

		[Fact]
		public void SurfaceBlurFilter_ZeroKernel_Throws()
		{
			// Прежде нулевое ядро давало NaN во всех весах.
			Assert.Throws<ArgumentException>(() => new SurfaceBlurFilter(r => 0));
		}

		[Fact]
		public void SurfaceBlurFilter_WithoutFunction_ThrowsInvalidOperation()
		{
			// Прежде GetValue(x, y) фильтра, созданного без функции, выбрасывал NullReferenceException.
			var filter = new SurfaceBlurFilter(BlurCompiler.DefaultFilter);

			Assert.Throws<InvalidOperationException>(() => filter.GetValue(0, 0));
			Assert.Equal(7, filter.GetValue(0, 0, (x, y) => 7), 12);
		}

		#endregion

		#region InterpolationCompiler

		[Fact]
		public void InterpolationCompiler_ArgumentsNotStartingAtZero()
		{
			// Прежде функция отражалась относительно нуля, а не первой точки, а период считался равным X последней точки.
			var mirror = new InterpolationCompiler(new[] {new PointD(5, 0), new PointD(6, 1), new PointD(10, 1)});
			var closed = new InterpolationCompiler(new[] {new PointD(5, 0), new PointD(6, 1), new PointD(10, 0)}, ExtrapolationMode.Closed);

			Assert.Equal(0.5, mirror.GetValue(5.5), 12);
			Assert.Equal(-1, mirror.GetValue(4), 12);
			Assert.Equal(-1, mirror.GetValue(2), 12);
			Assert.Equal(1, mirror.GetValue(12), 12);
			Assert.Equal(1, closed.GetValue(11), 12);
			Assert.Equal(0, closed.GetValue(0), 12);
			Assert.Equal(0.25, closed.GetValue(4), 12);

			// Отрицательные аргументы допустимы.
			var negative = new InterpolationCompiler(new[] {new PointD(-5, 1), new PointD(-1, 5)});
			Assert.Equal(3, negative.GetValue(-3), 12);
		}

		[Fact]
		public void InterpolationCompiler_RejectsUnsortedArguments()
		{
			// Прежде неупорядоченные аргументы молча давали неверные значения (7 вместо 6.5 для y = x² в точке 2.5).
			var points = new[] {new PointD(0, 0), new PointD(3, 9), new PointD(1, 1), new PointD(2, 4), new PointD(4, 16)};

			Assert.Throws<ArgumentException>(() => new InterpolationCompiler(points));
			Assert.Throws<ArgumentException>(() => new InterpolationCompiler(new[] {new PointD(0, 0), new PointD(double.NaN, 1), new PointD(2, 2)}));
		}

		[Fact]
		public void InterpolationCompiler_CopiesCallerArray()
		{
			var points = new[] {new PointD(0, 0), new PointD(1, 1), new PointD(2, 4)};
			var compiler = new InterpolationCompiler(points);

			points[1] = new PointD(1.5, 100);

			Assert.Equal(0.5, compiler.GetValue(0.5), 12);
		}

		#endregion

		#region CurveInterpolationCompiler

		[Fact]
		public void CurveInterpolationCompiler_CopiesCallerArray()
		{
			// Прежде в режиме Mirror сохранялась ссылка на массив вызывающего кода: после его изменения точка кривой
			// становилась (50.5, 50, 0), а DMax оставался прежним.
			var points = new[] {Point3D.Empty, new Point3D(1, 0, 0), new Point3D(2, 0, 0)};
			var compiler = new CurveInterpolationCompiler(points);

			points[2] = new Point3D(100, 100, 0);

			TestUtil.Near(new Point3D(1.5, 0, 0), compiler.GetValue3D(1.5));
			Assert.Equal(2, compiler.DMax, 12);
		}

		[Fact]
		public void CurveInterpolationCompiler_GetMean_IsCached()
		{
			// Прежде флаг кэша был readonly и всегда равнялся false, поэтому среднее вычислялось при каждом вызове.
			var square = new[] {Point3D.Empty, new Point3D(4, 0, 0), new Point3D(4, 4, 0), new Point3D(0, 4, 0)};
			var open = new CurveInterpolationCompiler(square);
			var closed = new CurveInterpolationCompiler(square, ExtrapolationMode.Closed);

			TestUtil.Near(new Point3D(8/3D, 2, 0), open.GetMean());
			TestUtil.Near(new Point3D(2, 2, 0), closed.GetMean());
			TestUtil.Near(new Point3D(2, 2, 0), closed.GetMean());

			var flag = typeof(CurveInterpolationCompiler).GetField("_meanCalculated", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(flag);
			Assert.True((bool) flag.GetValue(closed));
		}

		#endregion

		#region Исключения

		[Fact]
		public void Filters_InvalidMode_Throws()
		{
			// Прежде недопустимый режим принимался и приводил к ArgumentOutOfRangeException без имени параметра
			// только в CurveBlurFilter.GetValue3D на концах кривой.
			var points = new[] {Point3D.Empty, Point3D.UnitX};
			const ExtrapolationMode invalid = (ExtrapolationMode) 7;

			Assert.Equal("mode", Assert.Throws<ArgumentOutOfRangeException>(() => new CurveBlurFilter(points, invalid)).ParamName);
			Assert.Equal("mode", Assert.Throws<ArgumentOutOfRangeException>(() => new CurveInterpolationCompiler(points, invalid)).ParamName);
			Assert.Equal("mode", Assert.Throws<ArgumentOutOfRangeException>(() => new CurveBlurFilter2(points, BlurCompiler.DefaultFilter, invalid)).ParamName);
			Assert.Equal("mode", Assert.Throws<ArgumentOutOfRangeException>(() => new InterpolationCompiler(new[] {new PointD(0, 0), new PointD(1, 1)}, invalid)).ParamName);
			Assert.Equal("interpolator", Assert.Throws<ArgumentException>(() => new CurveBlurFilter(new BlurCompiler(), new InvalidModeInterpolation())).ParamName);
		}

		[Fact]
		public void Filters_NullArguments_ThrowArgumentNull()
		{
			// Прежде null приводил к NullReferenceException при первом использовании или к исключению с чужим именем параметра.
			var points = new[] {Point3D.Empty, Point3D.UnitX};

			Assert.Equal("curve", Assert.Throws<ArgumentNullException>(() => new CurveBlurFilter((IEnumerable<Point3D>) null)).ParamName);
			Assert.Equal("curve", Assert.Throws<ArgumentNullException>(() => new CurveInterpolationCompiler((IEnumerable<Point3D>) null)).ParamName);
			Assert.Equal("filter", Assert.Throws<ArgumentNullException>(() => new CurveBlurFilter(points, (BlurCompiler) null)).ParamName);
			Assert.Equal("filter", Assert.Throws<ArgumentNullException>(() => new CurveBlurFilter(points, (BlurFunctionHandler) null)).ParamName);
			Assert.Equal("interpolator", Assert.Throws<ArgumentNullException>(() => new CurveBlurFilter(new BlurCompiler(), null)).ParamName);
			Assert.Equal("filter", Assert.Throws<ArgumentNullException>(() => new CurveBlurFilter2(points, (BlurCompiler) null)).ParamName);
			Assert.Equal("filter", Assert.Throws<ArgumentNullException>(() => new BlurCompiler((BlurFunctionHandler) null)).ParamName);
			Assert.Equal("filter", Assert.Throws<ArgumentNullException>(() => new SurfaceBlurFilter(null)).ParamName);
			Assert.Equal("function", Assert.Throws<ArgumentNullException>(() => new BlurCompiler().GetValue3D(null, 0)).ParamName);
		}

		#endregion
	}
}
