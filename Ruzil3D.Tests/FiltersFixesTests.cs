using System;
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
	}
}
