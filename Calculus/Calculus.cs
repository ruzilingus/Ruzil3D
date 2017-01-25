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
		private const double RectangleStop = 1D;
		private const double RectangleNorm = RectangleStep;

		#endregion

		#region TrapezoidalItems

		private const int TrapezoidalCount = 1000;
		private const double TrapezoidalStep = 1D/TrapezoidalCount;
		private const double TrapezoidalStart = 0;
		private const double TrapezoidalStop = 1D - TrapezoidalStep/2D;
		private const double TrapezoidalNorm = TrapezoidalStep;

		#endregion

		#region Gauss-2 Items

		private const int Gauss2Count = 1000;
		private const double Gauss2Step = 1D/Gauss2Count;
		private const double Gauss2Offset0 = Gauss2Step/2D;
		private const double Gauss2Offset1 = (1 - 1/Math.Sqrt3)*Gauss2Offset0;
		private const double Gauss2Start = Gauss2Offset1;
		private const double Gauss2Offset2 = 2/Math.Sqrt3*Gauss2Offset0;
		private const double Gauss2Stop = 1 - Gauss2Offset0 + Gauss2Offset1;

		#endregion

		#region Simpson Items

		private const int SimpsonCount = 1000;
		private const double SimpsonStep = 1D/SimpsonCount;
		private const double SimpsonHalfStep = SimpsonStep/2;
		private const double SimpsonStart = 0;
		private const double SimpsonStop = 1 - SimpsonHalfStep;
		private const double SimpsonNorm = SimpsonStep/3;

		#endregion

		private static class Gauss3
		{
			private static readonly LegendrePolynomial Polynom = new LegendrePolynomial(3);

			private const int Count = 100;
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
			public static readonly double Stop = 1 - HalfStep + D;
			public static readonly double Norm = HalfStep*Weight0;
		}

		private static class Gauss5
		{
			private static readonly LegendrePolynomial Polynom = new LegendrePolynomial(5);

			private const int Count = 40;
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
			public static readonly double Stop = 1 - HalfStep + D;
			public static readonly double Norm = HalfStep*Weight0;
		}

		private static class Gauss6
		{
			private static readonly LegendrePolynomial Polynom = new LegendrePolynomial(6);

			private const int Count = 25;
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
			public static readonly double Stop = 1 - HalfStep + D;
			public static readonly double Norm = HalfStep*Weight0;
		}

		private static class Gauss10
		{
			private static readonly LegendrePolynomial Polynom = new LegendrePolynomial(10);

			private const int Count = 10;
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
			public static readonly double Stop = 1 - HalfStep + D;
			public static readonly double Norm = HalfStep*Weight0;
		}

		#endregion


		private static double IntegrateRectangle(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*RectangleStart + offset;
			var stop = scale*RectangleStop + offset;
			var step = scale*RectangleStep;

			var sum = 0D;
			for (var t = start; t < stop; t += step)
			{
				sum += function(t);
			}

			return sum*scale*RectangleNorm;
		}

		private static double IntegrateTrapezoidal(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*TrapezoidalStart + offset;
			var stop = scale*TrapezoidalStop + offset;
			var step = scale*TrapezoidalStep;

			var sum = (function(b) - function(a))/2D;
			for (var t = start; t < stop; t += step)
			{
				sum += function(t);
			}

			return sum*scale*TrapezoidalNorm;
		}



		private static double IntegrateSimpson(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*SimpsonStart + offset;
			var stop = scale*SimpsonStop + offset;
			var step = scale*SimpsonStep;
			var halfStep = scale*SimpsonHalfStep;

			var sum = (function(b) - function(a))/2D;
			for (var t = start; t < stop; t += step)
			{
				sum += function(t) + 2D*function(t + halfStep);
			}

			return sum*scale*SimpsonNorm;
		}

		private static double IntegrateGauss2(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*Gauss2Start + offset;
			var stop = scale*Gauss2Stop + offset;
			var step = scale*Gauss2Step;

			var s0 = scale*Gauss2Offset2;

			var sum = 0D;
			for (var t = start; t < stop; t += step)
			{
				sum += function(t) + function(t + s0);
			}

			return sum*scale*Gauss2Offset0;
		}

		private static double IntegrateGauss3(Func<double, double> function, double a = 0D, double b = 1D)
		{
			var scale = b - a;
			var offset = a;

			var start = scale*Gauss3.Start + offset;
			var stop = scale*Gauss3.Stop + offset;
			var step = scale*Gauss3.Step;

			var s0 = scale*Gauss3.S0;
			var d1 = scale*Gauss3.D1;

			var sum = 0D;
			for (var t = start; t < stop; t += step)
			{
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
			var stop = scale*Gauss5.Stop + offset;
			var step = scale*Gauss5.Step;

			var s0 = scale*Gauss5.S0;
			var s1 = scale*Gauss5.S1;
			var d1 = scale*Gauss5.D1;
			var d2 = scale*Gauss5.D2;

			var sum = 0D;
			for (var t = start; t < stop; t += step)
			{
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
			var stop = scale*Gauss6.Stop + offset;
			var step = scale*Gauss6.Step;

			var s0 = scale*Gauss6.S0;
			var s1 = scale*Gauss6.S1;
			var s2 = scale*Gauss6.S2;
			var d1 = scale*Gauss6.D1;
			var d2 = scale*Gauss6.D2;

			var sum = 0D;
			for (var t = start; t < stop; t += step)
			{
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
			var stop = scale*Gauss10.Stop + offset;
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
			for (var t = start; t < stop; t += step)
			{
				sum +=
					function(t) + function(t + s0) +
					Gauss10.C1*(function(t + d1) + function(t + s1)) +
					Gauss10.C2*(function(t + d2) + function(t + s2)) +
					Gauss10.C3*(function(t + d3) + function(t + s3)) +
					Gauss10.C4*(function(t + d4) + function(t + s4));

			}
			return sum*scale*Gauss10.Norm;
		}

		#endregion

		/// <summary>
		/// Возвращает одномерный определенный численный интеграл функции на отрезке [<paramref name="a"/>, <paramref name="b"/>].
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="a">Начальная точка.</param>
		/// <param name="b">Конечная точка.</param>
		/// <param name="rule">Метод численного интегрирования.</param>
		/// <returns>Одномерный определенный интеграл функции <paramref name="function"/> на отрезке [<paramref name="a"/>, <paramref name="b"/>].</returns>
		public static double Integrate(Func<double, double> function, double a, double b,
			EIntegrateRule rule = EIntegrateRule.Default)
		{
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

		private static double DifferentiateLeft(Func<double, double> function, double x, double eps, int order = 1)
		{
			switch (order)
			{
				case 1:
					return (function(x) - function(x - eps))/eps;

				case 2:
					return (function(x) - 2*function(x - eps) + function(x - 2*eps))/(eps*eps);

				default:
					throw new NotImplementedException();
			}

		}

		private static double DifferentiateRight(Func<double, double> function, double x, double eps, int order = 1)
		{
			switch (order)
			{
				case 1:
					return (function(x + eps) - function(x))/eps;

				case 2:
					return (function(x + 2*eps) - 2*function(x + eps) + function(x))/(eps*eps);

				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// <b>Не реализовано.</b>
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="x">Точка, в которой нужно вычислить производную.</param>
		/// <param name="eps">Окрестность точки, для вычисления производной.</param>
		/// <param name="side">Сторона для вычислеиня производной.</param>
		/// <returns>Производная функции первой степени.</returns>
		public static double Differentiate(Func<double, double> function, double x, double eps,
			ELimitSide side = ELimitSide.Both)
		{
			return Differentiate(function, x, eps, side, 1);
		}

		/// <summary>
		/// <b>Не реализовано.</b>
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="x">Точка, в которой нужно вычислить производную.</param>
		/// <param name="eps">Окрестность точки, для вычисления производной.</param>
		/// <param name="order">Степень вычисляемой производной.</param>
		/// <returns>Производная функции степени <paramref name="order"/>.</returns>
		public static double Differentiate(Func<double, double> function, double x, double eps, int order)
		{
			switch (order)
			{
				case 1:
					return (function(x + eps) - function(x - eps))/(2*eps);

				case 2:
					return (function(x + 2*eps) - 2*function(x) + function(x - 2*eps))/(4*eps*eps);

				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// <b>Не реализовано.</b>
		/// </summary>
		/// <param name="function">Исходная функция.</param>
		/// <param name="x">Точка, в которой нужно вычислить производную.</param>
		/// <param name="eps">Окрестность точки, для вычисления производной.</param>
		/// <param name="side">Сторона для вычислеиня производной.</param>
		/// <param name="order">Степень вычисляемой производной.</param>
		/// <returns>Производная функции степени <paramref name="order"/>.</returns>
		public static double Differentiate(Func<double, double> function, double x, double eps, ELimitSide side,
			int order)
		{
			if (order == 0)
			{
				return function(x);
			}

			if (order < 0)
			{
				throw new NotImplementedException();
			}

			switch (side)
			{
				case ELimitSide.Left:
					return DifferentiateLeft(function, x, eps, order);

				case ELimitSide.Right:
					return DifferentiateRight(function, x, eps, order);

				default:
					return Differentiate(function, x, eps, order);
			}
		}

		#endregion

		#region Root-finding

		public static double FindRoot(Func<double, double> function, double a, double b, out bool result)
		{
			var aValue = function(a);
			var bValue = function(b);
			
			if (aValue.Equals(0D))
			{
				result = true;
				return a;
			}

			if (bValue.Equals(0D))
			{
				result = true;
				return b;
			}

			const double eps = 1E-12;
			if (aValue > eps && bValue > eps || aValue < -eps && bValue < -eps)
			{
				result = false;
				return double.NaN;
			}

			if (a <= double.MinValue)
			{
				var arg = Math.Min(-1, b);
				double val;
				do
				{
					arg *= 2;
					val = function(arg);
				}
				while (arg > a && val * bValue > 0);

				if (arg > a)
				{
					a = arg;
					aValue = function(a);
				}
			}

			if (b >= double.MaxValue)
			{
				var arg = Math.Max(1, a);
				double val;
				do
				{
					arg *= 2;
					val = function(arg);
				}
				while (arg < b && val * aValue > 0);

				if (arg < b)
				{
					b = arg;
					bValue = function(b);
				}
			}

			double m;
			do
			{
				m = (a + b)/2D;
				var mValue = function(m);

				if (mValue.Equals(0D)) break;

				if (aValue*mValue < 0)
				{
					if (m.Equals(b)) break;
					b = m;
				}
				else
				{
					if (m.Equals(a)) break;
					a = m;
				}
			} while (true);

			result = true;
			return m;

			/*
				double l = a, r = b, lValue = aValue, rValue = bValue;
				double m;
				double mValue;

				do
				{
					m = (l + r)/2D;

					mValue = function(m);

					if (lValue*mValue < 0)
					{
						if (r.Equals(m)) break;
						r = m;
					}
					else
					{
						if (l.Equals(m)) break;
						l = m;

						lValue = mValue;
					}
				} while (true);



				result = true;
				return System.Math.Abs(mValue) < System.Math.Abs(bValue)
					? System.Math.Abs(aValue) < System.Math.Abs(mValue)
						? a
						: m
					: System.Math.Abs(aValue) < System.Math.Abs(bValue)
						? a
						: b;
				*/
		}

		#endregion
	}
}
