using System;

namespace Ruzil3D.Calculus
{
	/// <summary>
	/// Представляет статический класс вычислительных методов.
	/// </summary>
	public static class Calculus
	{
		#region Integrate Items

		#region Private

		#region Static Items

		#region Rectangle Items

		private const int RectangleCount = 1000;
		private const double RectangleStep = 1D/RectangleCount;
		private const double RectangleStart = RectangleStep/2D;
		private const double RectangleNorm = RectangleStep;

		#endregion

		#region TrapezoidalItems

		private const int TrapezoidalCount = 1000;
		private const double TrapezoidalStep = 1D/TrapezoidalCount;
		private const double TrapezoidalStart = 0;
		private const double TrapezoidalNorm = TrapezoidalStep;

		#endregion

		#region Gauss-2 Items

		private const int Gauss2Count = 1000;
		private const double Gauss2Step = 1D/Gauss2Count;
		private const double Gauss2Offset0 = Gauss2Step/2D;
		private const double Gauss2Offset1 = (1 - 1/Math.Sqrt3)*Gauss2Offset0;
		private const double Gauss2Start = Gauss2Offset1;
		private const double Gauss2Offset2 = 2/Math.Sqrt3*Gauss2Offset0;

		#endregion

		#region Simpson Items

		private const int SimpsonCount = 1000;
		private const double SimpsonStep = 1D/SimpsonCount;
		private const double SimpsonHalfStep = SimpsonStep/2;
		private const double SimpsonStart = 0;
		private const double SimpsonNorm = SimpsonStep/3;

		#endregion

		private static class Gauss3
		{
			private static readonly LegendrePolynomial Polynom = new LegendrePolynomial(3);

			public const int Count = 100;
			public const double Step = 1D/Count;
			private const double HalfStep = Step/2;

			private static readonly double Ksi0 = Polynom.Root(0);
			private static readonly double Ksi1 = Polynom.Root(1);

			private static readonly double Weight0 = Polynom.GaussianWeight(0);
			private static readonly double Weight1 = Polynom.GaussianWeight(1);

			private static readonly double D = (1 + Ksi0)*HalfStep;

			public static readonly double D1 = (1 + Ksi1)*HalfStep - D;
			public static readonly double S0 = (1 - Ksi0)*HalfStep - D;
			public static readonly double C1 = Weight1/Weight0;

			public static readonly double Start = D;
			public static readonly double Norm = HalfStep*Weight0;
		}

		private static class Gauss5
		{
			private static readonly LegendrePolynomial Polynom = new LegendrePolynomial(5);

			public const int Count = 40;
			public const double Step = 1D/Count;
			private const double HalfStep = Step/2;

			private static readonly double Ksi0 = Polynom.Root(0);
			private static readonly double Ksi1 = Polynom.Root(1);
			private static readonly double Ksi2 = Polynom.Root(2);

			private static readonly double Weight0 = Polynom.GaussianWeight(0);
			private static readonly double Weight1 = Polynom.GaussianWeight(1);
			private static readonly double Weight2 = Polynom.GaussianWeight(2);

			private static readonly double D = (1 + Ksi0)*HalfStep;

			public static readonly double D1 = (1 + Ksi1)*HalfStep - D;
			public static readonly double D2 = (1 + Ksi2)*HalfStep - D;

			public static readonly double S0 = (1 - Ksi0)*HalfStep - D;
			public static readonly double S1 = (1 - Ksi1)*HalfStep - D;

			public static readonly double C1 = Weight1/Weight0;
			public static readonly double C2 = Weight2/Weight0;
			public static readonly double Start = D;
			public static readonly double Norm = HalfStep*Weight0;
		}

		private static class Gauss6
		{
			private static readonly LegendrePolynomial Polynom = new LegendrePolynomial(6);

			public const int Count = 25;
			public const double Step = 1D/Count;
			private const double HalfStep = Step/2;

			private static readonly double Ksi0 = Polynom.Root(0);
			private static readonly double Ksi1 = Polynom.Root(1);
			private static readonly double Ksi2 = Polynom.Root(2);

			private static readonly double Weight0 = Polynom.GaussianWeight(0);
			private static readonly double Weight1 = Polynom.GaussianWeight(1);
			private static readonly double Weight2 = Polynom.GaussianWeight(2);

			private static readonly double D = (1 + Ksi0)*HalfStep;

			public static readonly double D1 = (1 + Ksi1)*HalfStep - D;
			public static readonly double D2 = (1 + Ksi2)*HalfStep - D;

			public static readonly double S0 = (1 - Ksi0)*HalfStep - D;
			public static readonly double S1 = (1 - Ksi1)*HalfStep - D;
			public static readonly double S2 = (1 - Ksi2)*HalfStep - D;

			public static readonly double C1 = Weight1/Weight0;
			public static readonly double C2 = Weight2/Weight0;
			public static readonly double Start = D;
			public static readonly double Norm = HalfStep*Weight0;
		}

		private static class Gauss10
		{
			private static readonly LegendrePolynomial Polynom = new LegendrePolynomial(10);

			public const int Count = 10;
			public const double Step = 1D/Count;
			private const double HalfStep = Step/2;

			private static readonly double Ksi0 = Polynom.Root(0);
			private static readonly double Ksi1 = Polynom.Root(1);
			private static readonly double Ksi2 = Polynom.Root(2);
			private static readonly double Ksi3 = Polynom.Root(3);
			private static readonly double Ksi4 = Polynom.Root(4);

			private static readonly double Weight0 = Polynom.GaussianWeight(0);
			private static readonly double Weight1 = Polynom.GaussianWeight(1);
			private static readonly double Weight2 = Polynom.GaussianWeight(2);
			private static readonly double Weight3 = Polynom.GaussianWeight(3);
			private static readonly double Weight4 = Polynom.GaussianWeight(4);

			private static readonly double D = (1 + Ksi0)*HalfStep;

			public static readonly double D1 = (1 + Ksi1)*HalfStep - D;
			public static readonly double D2 = (1 + Ksi2)*HalfStep - D;
			public static readonly double D3 = (1 + Ksi3)*HalfStep - D;
			public static readonly double D4 = (1 + Ksi4)*HalfStep - D;

			public static readonly double S0 = (1 - Ksi0)*HalfStep - D;
			public static readonly double S1 = (1 - Ksi1)*HalfStep - D;
			public static readonly double S2 = (1 - Ksi2)*HalfStep - D;
			public static readonly double S3 = (1 - Ksi3)*HalfStep - D;
			public static readonly double S4 = (1 - Ksi4)*HalfStep - D;

			public static readonly double C1 = Weight1/Weight0;
			public static readonly double C2 = Weight2/Weight0;
			public static readonly double C3 = Weight3/Weight0;
			public static readonly double C4 = Weight4/Weight0;

			public static readonly double Start = D;
			public static readonly double Norm = HalfStep*Weight0;
		}

		#endregion


		private static double IntegrateRectangle(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*RectangleStart + offset;
			var step = scale*RectangleStep;

			//Счётчик цикла целочисленный: при накоплении t += step цикл не завершался,
			//если шаг меньше точности представления t (короткий отрезок вдали от нуля).
			var sum = 0D;
			for (var i = 0; i < RectangleCount; i++)
			{
				sum += function(start + i*step);
			}

			return sum*scale*RectangleNorm;
		}

		private static double IntegrateTrapezoidal(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*TrapezoidalStart + offset;
			var step = scale*TrapezoidalStep;

			var sum = (function(b) - function(a))/2D;
			for (var i = 0; i < TrapezoidalCount; i++)
			{
				sum += function(start + i*step);
			}

			return sum*scale*TrapezoidalNorm;
		}



		private static double IntegrateSimpson(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*SimpsonStart + offset;
			var step = scale*SimpsonStep;
			var halfStep = scale*SimpsonHalfStep;

			var sum = (function(b) - function(a))/2D;
			for (var i = 0; i < SimpsonCount; i++)
			{
				var t = start + i*step;
				sum += function(t) + 2D*function(t + halfStep);
			}

			return sum*scale*SimpsonNorm;
		}

		private static double IntegrateGauss2(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*Gauss2Start + offset;
			var step = scale*Gauss2Step;

			var s0 = scale*Gauss2Offset2;

			var sum = 0D;
			for (var i = 0; i < Gauss2Count; i++)
			{
				var t = start + i*step;
				sum += function(t) + function(t + s0);
			}

			return sum*scale*Gauss2Offset0;
		}

		private static double IntegrateGauss3(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*Gauss3.Start + offset;
			var step = scale*Gauss3.Step;

			var s0 = scale*Gauss3.S0;
			var d1 = scale*Gauss3.D1;

			var sum = 0D;
			for (var i = 0; i < Gauss3.Count; i++)
			{
				var t = start + i*step;
				sum +=
					function(t) + function(t + s0) +
					Gauss3.C1*function(t + d1);
			}
			return sum*scale*Gauss3.Norm;
		}

		private static double IntegrateGauss5(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*Gauss5.Start + offset;
			var step = scale*Gauss5.Step;

			var s0 = scale*Gauss5.S0;
			var s1 = scale*Gauss5.S1;
			var d1 = scale*Gauss5.D1;
			var d2 = scale*Gauss5.D2;

			var sum = 0D;
			for (var i = 0; i < Gauss5.Count; i++)
			{
				var t = start + i*step;
				sum +=
					function(t) + function(t + s0) +
					Gauss5.C1*(function(t + d1) + function(t + s1)) +
					Gauss5.C2*function(t + d2);

			}
			return sum*scale*Gauss5.Norm;
		}

		private static double IntegrateGauss6(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*Gauss6.Start + offset;
			var step = scale*Gauss6.Step;

			var s0 = scale*Gauss6.S0;
			var s1 = scale*Gauss6.S1;
			var s2 = scale*Gauss6.S2;
			var d1 = scale*Gauss6.D1;
			var d2 = scale*Gauss6.D2;

			var sum = 0D;
			for (var i = 0; i < Gauss6.Count; i++)
			{
				var t = start + i*step;
				sum +=
					function(t) + function(t + s0) +
					Gauss6.C1*(function(t + d1) + function(t + s1)) +
					Gauss6.C2*(function(t + d2) + function(t + s2));

			}
			return sum*scale*Gauss6.Norm;
		}

		private static double IntegrateGauss10(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*Gauss10.Start + offset;
			var step = scale*Gauss10.Step;


			var s0 = scale*Gauss10.S0;
			var s1 = scale*Gauss10.S1;
			var s2 = scale*Gauss10.S2;
			var s3 = scale*Gauss10.S3;
			var s4 = scale*Gauss10.S4;
			var d1 = scale*Gauss10.D1;
			var d2 = scale*Gauss10.D2;
			var d3 = scale*Gauss10.D3;
			var d4 = scale*Gauss10.D4;

			var sum = 0D;
			for (var i = 0; i < Gauss10.Count; i++)
			{
				var t = start + i*step;
				sum +=
					function(t) + function(t + s0) +
					Gauss10.C1*(function(t + d1) + function(t + s1)) +
					Gauss10.C2*(function(t + d2) + function(t + s2)) +
					Gauss10.C3*(function(t + d3) + function(t + s3)) +
					Gauss10.C4*(function(t + d4) + function(t + s4));

			}
			return sum*scale*Gauss10.Norm;
		}

		private static void CheckIntegrationBound(double value, string name)
		{
			//Отрезок делится на фиксированное число частей, поэтому при бесконечном или неопределённом пределе
			//прежде молча возвращался NaN или −∞.
			if (double.IsNaN(value) || double.IsInfinity(value))
			{
				throw new ArgumentOutOfRangeException(name, value,
					"Предел интегрирования \"" + name + "\" должен быть конечным числом: несобственные интегралы не поддерживаются.");
			}
		}

		#endregion

		/// <summary>
		/// Возвращает одномерный определенный численный интеграл функции на отрезке [<paramref name="a"/>, <paramref name="b"/>].
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="a">Начальная точка. Должна быть конечным числом.</param>
		/// <param name="b">Конечная точка. Должна быть конечным числом.</param>
		/// <param name="rule">Метод численного интегрирования.</param>
		/// <returns>Одномерный определенный интеграл функции <paramref name="function"/> на отрезке [<paramref name="a"/>, <paramref name="b"/>]. Если <paramref name="b"/> меньше <paramref name="a"/>, возвращается интеграл по отрезку [<paramref name="b"/>, <paramref name="a"/>] с обратным знаком.</returns>
		/// <remarks>Отрезок делится на фиксированное число частей, поэтому несобственные интегралы (с бесконечными пределами) не поддерживаются.</remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="function"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Предел <paramref name="a"/> или <paramref name="b"/> бесконечен или равен <see cref="double.NaN"/>, или метода <paramref name="rule"/> не существует.</exception>
		public static double Integrate(Func<double, double> function, double a, double b,
			EIntegrateRule rule = EIntegrateRule.Default)
		{
			if (function == null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			CheckIntegrationBound(a, nameof(a));
			CheckIntegrationBound(b, nameof(b));

			if (b < a)
			{
				return -Integrate(function, b, a, rule);
			}

			switch (rule)
			{
				case EIntegrateRule.Default:
					return IntegrateGauss5(function, a, b);

				case EIntegrateRule.Rectangle:
					return IntegrateRectangle(function, a, b);

				case EIntegrateRule.Trapezoidal:
					return IntegrateTrapezoidal(function, a, b);

				case EIntegrateRule.Simpson:
					return IntegrateSimpson(function, a, b);

				case EIntegrateRule.Gauss2:
					return IntegrateGauss2(function, a, b);

				case EIntegrateRule.Gauss3:
					return IntegrateGauss3(function, a, b);

				case EIntegrateRule.Gauss5:
					return IntegrateGauss5(function, a, b);

				case EIntegrateRule.Gauss6:
					return IntegrateGauss6(function, a, b);

				case EIntegrateRule.Gauss10:
					return IntegrateGauss10(function, a, b);

				default:
					throw new ArgumentOutOfRangeException(nameof(rule),
						"Метода интегрирования, заданного параметром \"" + nameof(rule) + "\", не существует.");
			}
		}

		#endregion

		#region Differentiate

		//Разности делятся на шаги между фактическими узлами, а не на номинальный eps: прежде вдали от нуля
		//узел x ± eps округлялся, и, например, производная x в точке 1e8 при eps = 1e-8 получалась равной 1.49.
		//Если узлы не округляются, результат совпадает с прежним.

		private static double DifferentiateLeft(Func<double, double> function, double x, double eps, int order)
		{
			switch (order)
			{
				case 1:
				{
					var x1 = x - eps;
					var step = Step(x1, x, eps);
					return (function(x) - function(x1))/step;
				}

				case 2:
				{
					var x1 = x - eps;
					var x2 = x - 2*eps;
					var step1 = Step(x2, x1, eps);
					var step2 = Step(x1, x, eps);
					var value = function(x);
					var value1 = function(x1);
					return SecondDifference(function(x2), value1, value, step1, step2);
				}

				default:
					throw new NotImplementedException();
			}

		}

		private static double DifferentiateRight(Func<double, double> function, double x, double eps, int order)
		{
			switch (order)
			{
				case 1:
				{
					var x1 = x + eps;
					var step = Step(x, x1, eps);
					return (function(x1) - function(x))/step;
				}

				case 2:
				{
					var x1 = x + eps;
					var x2 = x + 2*eps;
					var step1 = Step(x, x1, eps);
					var step2 = Step(x1, x2, eps);
					var value2 = function(x2);
					var value1 = function(x1);
					return SecondDifference(function(x), value1, value2, step1, step2);
				}

				default:
					throw new NotImplementedException();
			}
		}

		private static double DifferentiateBoth(Func<double, double> function, double x, double eps, int order)
		{
			switch (order)
			{
				case 1:
				{
					var x1 = x - eps;
					var x2 = x + eps;
					var step = Step(x1, x2, eps);
					return (function(x2) - function(x1))/step;
				}

				case 2:
				{
					var x1 = x - 2*eps;
					var x2 = x + 2*eps;
					var step1 = Step(x1, x, eps);
					var step2 = Step(x, x2, eps);
					var value2 = function(x2);
					var value = function(x);
					return SecondDifference(function(x1), value, value2, step1, step2);
				}

				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Возвращает шаг между соседними узлами разностной схемы.
		/// </summary>
		/// <param name="x1">Левый узел.</param>
		/// <param name="x2">Правый узел.</param>
		/// <param name="eps">Номинальный шаг.</param>
		/// <returns>Фактический шаг <paramref name="x2"/> − <paramref name="x1"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Узлы совпали: шаг меньше точности представления аргумента.</exception>
		private static double Step(double x1, double x2, double eps)
		{
			var step = x2 - x1;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (step == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(eps), eps,
					"Шаг \"" + nameof(eps) + "\" меньше точности представления аргумента: узлы разностной схемы совпадают.");
			}

			return step;
		}

		/// <summary>
		/// Возвращает вторую разностную производную по значениям в трёх узлах x₀ &lt; x₁ &lt; x₂.
		/// </summary>
		/// <param name="value0">Значение функции в узле x₀.</param>
		/// <param name="value1">Значение функции в узле x₁.</param>
		/// <param name="value2">Значение функции в узле x₂.</param>
		/// <param name="step1">Шаг x₁ − x₀.</param>
		/// <param name="step2">Шаг x₂ − x₁.</param>
		/// <returns>Приближённое значение второй производной.</returns>
		private static double SecondDifference(double value0, double value1, double value2, double step1, double step2)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (step1 == step2)
			{
				return (value2 - 2*value1 + value0)/(step1*step2);
			}

			//Узлы округлились по-разному: разность разделённых разностей для неравномерной сетки.
			return 2*((value2 - value1)/step2 - (value1 - value0)/step1)/(step1 + step2);
		}

		/// <summary>
		/// Возвращает производную функции первого порядка, вычисленную конечными разностями.
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="x">Точка, в которой нужно вычислить производную.</param>
		/// <param name="eps">Шаг разностной схемы: положительное конечное число.</param>
		/// <param name="side">Сторона для вычисления производной: центральная (по умолчанию), левая или правая разность.</param>
		/// <returns>Приближённое значение производной функции первого порядка в точке <paramref name="x"/>.</returns>
		/// <remarks>Разность делится на фактический шаг между узлами x ± <paramref name="eps"/> с учётом их округления.</remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="function"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Значение <paramref name="eps"/> не является положительным конечным числом или меньше точности представления <paramref name="x"/>, или значение <paramref name="side"/> не определено.</exception>
		public static double Differentiate(Func<double, double> function, double x, double eps,
			ELimitSide side = ELimitSide.Both)
		{
			return Differentiate(function, x, eps, side, 1);
		}

		/// <summary>
		/// Возвращает производную функции указанного порядка, вычисленную центральными конечными разностями.
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="x">Точка, в которой нужно вычислить производную.</param>
		/// <param name="eps">Шаг разностной схемы: положительное конечное число.</param>
		/// <param name="order">Порядок производной: 0 (значение функции), 1 или 2.</param>
		/// <returns>Приближённое значение производной порядка <paramref name="order"/> в точке <paramref name="x"/>.</returns>
		/// <remarks>Первая производная вычисляется по узлам x ± <paramref name="eps"/>, вторая — по узлам x − 2·<paramref name="eps"/>, x, x + 2·<paramref name="eps"/>.
		/// Разности делятся на фактические шаги между узлами с учётом их округления.</remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="function"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Значение <paramref name="eps"/> не является положительным конечным числом или меньше точности представления <paramref name="x"/>, или значение <paramref name="order"/> отрицательно.</exception>
		/// <exception cref="NotImplementedException">Порядок <paramref name="order"/> больше 2.</exception>
		public static double Differentiate(Func<double, double> function, double x, double eps, int order)
		{
			//Прежде порядок 0 здесь выбрасывал NotImplementedException, хотя перегрузка со стороной возвращала значение функции.
			return Differentiate(function, x, eps, ELimitSide.Both, order);
		}

		/// <summary>
		/// Возвращает производную функции указанного порядка, вычисленную конечными разностями с указанной стороны.
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="x">Точка, в которой нужно вычислить производную.</param>
		/// <param name="eps">Шаг разностной схемы: положительное конечное число.</param>
		/// <param name="side">Сторона для вычисления производной: центральная, левая или правая разность.</param>
		/// <param name="order">Порядок производной: 0 (значение функции), 1 или 2.</param>
		/// <returns>Приближённое значение производной порядка <paramref name="order"/> в точке <paramref name="x"/>.</returns>
		/// <remarks>Левая разность использует узлы x, x − <paramref name="eps"/>, x − 2·<paramref name="eps"/>, правая — x, x + <paramref name="eps"/>, x + 2·<paramref name="eps"/>,
		/// центральная — как <see cref="Differentiate(Func{double, double}, double, double, int)"/>. Разности делятся на фактические шаги между узлами с учётом их округления.</remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="function"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Значение <paramref name="eps"/> не является положительным конечным числом или меньше точности представления <paramref name="x"/>, значение <paramref name="side"/> не определено или значение <paramref name="order"/> отрицательно.</exception>
		/// <exception cref="NotImplementedException">Порядок <paramref name="order"/> больше 2.</exception>
		public static double Differentiate(Func<double, double> function, double x, double eps, ELimitSide side,
			int order)
		{
			if (function == null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			//Прежде eps не проверялся: при eps = 0 возвращался NaN, а отрицательный eps молча менял сторону
			//(правая производная |x| в нуле получалась равной −1).
			if (!(eps > 0) || double.IsPositiveInfinity(eps))
			{
				throw new ArgumentOutOfRangeException(nameof(eps), eps,
					"Шаг \"" + nameof(eps) + "\" должен быть положительным конечным числом.");
			}

			if (side != ELimitSide.Both && side != ELimitSide.Left && side != ELimitSide.Right)
			{
				throw new ArgumentOutOfRangeException(nameof(side), side,
					"Стороны, заданной параметром \"" + nameof(side) + "\", не существует.");
			}

			if (order < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(order), order,
					"Порядок производной \"" + nameof(order) + "\" не может быть отрицательным.");
			}

			if (order == 0)
			{
				return function(x);
			}

			switch (side)
			{
				case ELimitSide.Left:
					return DifferentiateLeft(function, x, eps, order);

				case ELimitSide.Right:
					return DifferentiateRight(function, x, eps, order);

				default:
					return DifferentiateBoth(function, x, eps, order);
			}
		}

		#endregion

		#region Root-finding

		/// <summary>
		/// Находит корень функции на отрезке между <paramref name="a"/> и <paramref name="b"/> методом деления отрезка пополам.
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="a">Одна из границ отрезка. Может быть бесконечной.</param>
		/// <param name="b">Другая граница отрезка. Может быть бесконечной.</param>
		/// <param name="result">Значение <b>true</b>, если корень найден; в противном случае — значение <b>false</b>.</param>
		/// <returns>Найденный корень или <see cref="double.NaN"/>, если корень не найден.</returns>
		/// <remarks>
		/// <para>Границы можно задавать в любом порядке. Если функция равна нулю на границе, возвращается эта граница.
		/// Иначе значения функции на границах должны иметь разные знаки. Значение <see cref="double.NaN"/> в границе
		/// или в значении функции означает, что корень не найден.</para>
		/// <para>Для бесконечной границы (а также для <see cref="double.MinValue"/> и <see cref="double.MaxValue"/>) сначала ищется
		/// конечная точка, в которой знак функции отличается от знака на другой границе: аргумент удваивается, начиная с ±1
		/// или с другой границы, если она дальше от нуля.</para>
		/// <para>Отрезок делится пополам, пока функция не обратится в нуль или концы отрезка не станут соседними числами двойной точности;
		/// из двух концов возвращается тот, где модуль функции меньше. Если этот модуль больше, чем на обоих концах исходного отрезка
		/// (для бесконечной границы — в найденной конечной точке), то знак меняется в точке разрыва (например, в полюсе функции 1/x),
		/// а не в корне, и корень считается ненайденным.</para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="function"/> равно <b>null</b>.</exception>
		public static double FindRoot(Func<double, double> function, double a, double b, out bool result)
		{
			if (function == null)
			{
				throw new ArgumentNullException(nameof(function));
			}

			result = false;

			//Прежде для границы NaN возвращался NaN с признаком успеха.
			if (double.IsNaN(a) || double.IsNaN(b))
			{
				return double.NaN;
			}

			var aValue = function(a);
			var bValue = function(b);

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (aValue == 0)
			{
				result = true;
				return a;
			}

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (bValue == 0)
			{
				result = true;
				return b;
			}

			//Знаки сравниваются, а не перемножаются: прежде значения по модулю меньше 1e-12 пропускали проверку смены знака
			//(x + 1e-13 на [0, 5] давал «корень» 5), произведение малых значений обращалось в нуль,
			//а значение NaN на границе (sqrt(x) − 1 на [−1, 4]) не отклонялось.
			if (double.IsNaN(aValue) || double.IsNaN(bValue) || (aValue < 0) == (bValue < 0))
			{
				return double.NaN;
			}

			//Прежде считалось, что a < b, и для бесконечной границы, заданной первой (x − 1 на [+∞, 0]), возвращалась бесконечность.
			if (b < a)
			{
				var save = a;
				a = b;
				b = save;

				save = aValue;
				aValue = bValue;
				bValue = save;
			}

			//Для бесконечной границы ищется конечная точка со сменой знака; в ней функция может оказаться равной нулю.
			if (a <= double.MinValue)
			{
				if (!FindFiniteBound(function, ref a, ref aValue, b, bValue, -1))
				{
					return double.NaN;
				}

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (aValue == 0)
				{
					result = true;
					return a;
				}
			}

			if (b >= double.MaxValue)
			{
				if (!FindFiniteBound(function, ref b, ref bValue, a, aValue, 1))
				{
					return double.NaN;
				}

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (bValue == 0)
				{
					result = true;
					return b;
				}
			}

			var limit = Math.Max(Math.Abs(aValue), Math.Abs(bValue));

			while (true)
			{
				//Прежде середина (a + b)/2 переполнялась у больших границ ([1e308, 1.7e308] давал +∞).
				var m = (a + b)/2D;
				if (double.IsInfinity(m))
				{
					m = a/2D + b/2D;
				}

				if (!(m > a && m < b))
				{
					break;
				}

				var mValue = function(m);

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (mValue == 0)
				{
					result = true;
					return m;
				}

				if (double.IsNaN(mValue))
				{
					return double.NaN;
				}

				if ((mValue < 0) == (aValue < 0))
				{
					a = m;
					aValue = mValue;
				}
				else
				{
					b = m;
					bValue = mValue;
				}
			}

			double root, rootValue;
			if (Math.Abs(aValue) <= Math.Abs(bValue))
			{
				root = a;
				rootValue = aValue;
			}
			else
			{
				root = b;
				rootValue = bValue;
			}

			//Прежде полюс (1/x на [−1, 1]) выдавался за корень: у корня модуль функции мал по сравнению с концами отрезка,
			//а у точки разрыва — нет.
			if (Math.Abs(rootValue) > limit)
			{
				return double.NaN;
			}

			result = true;
			return root;
		}

		/// <summary>
		/// Заменяет бесконечную или предельную границу отрезка конечной точкой, в которой знак функции отличается от знака на другой границе.
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="bound">Заменяемая граница: <see cref="double.NegativeInfinity"/> или <see cref="double.MinValue"/> для левой границы,
		/// <see cref="double.PositiveInfinity"/> или <see cref="double.MaxValue"/> для правой.</param>
		/// <param name="boundValue">Значение функции на заменяемой границе.</param>
		/// <param name="other">Другая граница.</param>
		/// <param name="otherValue">Значение функции на другой границе.</param>
		/// <param name="direction">Направление поиска: −1 для левой границы, 1 для правой.</param>
		/// <returns>Значение <b>false</b>, если функция меняет знак только на бесконечности или встретилось значение NaN.</returns>
		private static bool FindFiniteBound(Func<double, double> function, ref double bound, ref double boundValue,
			double other, double otherValue, int direction)
		{
			var arg = direction < 0 ? Math.Min(-1, other) : Math.Max(1, other);
			while (true)
			{
				arg *= 2;
				if (direction < 0 ? !(arg > bound) : !(arg < bound))
				{
					break;
				}

				var value = function(arg);
				if (double.IsNaN(value))
				{
					return false;
				}

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (value == 0 || (value < 0) != (otherValue < 0))
				{
					bound = arg;
					boundValue = value;
					return true;
				}
			}

			if (!double.IsInfinity(bound))
			{
				//Граница double.MinValue или double.MaxValue конечна: отрезок до неё делится пополам.
				return true;
			}

			//Прежде при неудачном поиске граница оставалась бесконечной и возвращалась как корень.
			//Проверяем последнюю конечную точку перед бесконечностью.
			var last = direction < 0 ? double.MinValue : double.MaxValue;
			var lastValue = function(last);

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (double.IsNaN(lastValue) || lastValue != 0 && (lastValue < 0) == (otherValue < 0))
			{
				return false;
			}

			bound = last;
			boundValue = lastValue;
			return true;
		}

		#endregion
	}
}
