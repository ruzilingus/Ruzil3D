using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;
using Ruzil3D.Utility;
using static Ruzil3D.Math;

namespace Ruzil3D.Calculus
{
	/// <summary>
	/// Представляет полиномы Лежандра.
	/// </summary>
	/// <remarks>Названы в честь Адриен Мари Лежандра.</remarks>
	public class LegendrePolynomial : Polynomial
	{
		/// <summary>
		/// Получает степень полинома Лежандра.
		/// </summary>
		/// <remarks>Задается конструктором.</remarks>
		public readonly int Deg;

		//Кэшы
		private static readonly List<double[]> Roots = new List<double[]>();
		private static readonly List<double[]> GaussianWeights = new List<double[]>();
		private static readonly List<Polynomial> Derivatives = new List<Polynomial>();
		private static readonly List<Polynomial> GaussianDenominators = new List<Polynomial>();

		/// <summary>
		/// Получает производную от полинома Лежандра.
		/// </summary>
		private Polynomial Derivative
		{
			get
			{
				if (Derivatives.Count <= Deg)
				{
					Derivatives.AddRange(new Polynomial[Deg - Derivatives.Count + 1]);
				}

				var result = Derivatives[Deg];

				if (result == null)
				{
					Derivatives[Deg] = result = GetDerivative();
				}

				return result;
			}

		}

		/// <summary>
		/// Получает полином <i>(1-x²)[ρ`(x)]²</i> для вычисления весов для квадратурного метода Гаусса.
		/// </summary>
		private Polynomial GaussianDenominator
		{
			get
			{
				if (GaussianDenominators.Count <= Deg)
				{
					GaussianDenominators.AddRange(new Polynomial[Deg - GaussianDenominators.Count + 1]);
				}

				var result = GaussianDenominators[Deg];

				if (result == null)
				{
					GaussianDenominators[Deg] = result = Derivative * Derivative * new Polynomial(1, 0, -1);
				}

				return result;
			}

		}

		/// <summary>
		/// Находит корень полинома Лежандра по индексу.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private double ResolveRoot(int index)
		{
			var derivative = Derivative;

			//Начальное приближение
			var result = Cos(Pi * (4 * (Deg - index) - 1) / (4 * Deg + 2));
			var value = GetValue(result);

			//Находим корни методом Ньютона
			for (var j = 0; j < 10; j++)
			{
				result -= value / derivative.GetValue(result);
				value = GetValue(result);
			}

			var optimumResult = result;
			var optimumValue = Abs(value);

			for (var j = 0; j < 5; j++)
			{
				result -= value / derivative.GetValue(result);
				value = GetValue(result);

				var abs = Abs(value);
				if (abs < optimumValue)
				{
					optimumResult = result;
					optimumValue = abs;
				}
			}

			return optimumResult;
		}

		/// <summary>
		/// Возвращает корень полинома Лежандра соответствующий индексу.
		/// </summary>
		/// <param name="index">Индекс корня полинома.</param>
		/// <returns>Корень полинома Лежандра соответствующий индексу <paramref name="index"/></returns>
		public double Root(int index)
		{
			if (index >= Deg)
			{
				throw new IndexOutOfRangeException();
			}

			if (index > (Deg - 1) / 2)
			{
				return -Root(Deg - index - 1);
			}

			if (Deg % 2 == 1 && index == Deg / 2)
			{
				return 0;
			}

			if (Roots.Count <= Deg)
			{
				Roots.AddRange(new double[Deg - Roots.Count + 1][]);
			}

			if (Roots[Deg] == null)
			{
				Roots[Deg] = new double[Deg / 2];
			}

			var result = Roots[Deg][index];

			if (result.Equals(0D))
			{
				Roots[Deg][index] = result = ResolveRoot(index);
			}

			return result;
		}

		/// <summary>
		/// Возрвщает вес интегрирования для метода Гаусса соответствующий индексу.
		/// </summary>
		/// <param name="index">Индекс веса интегрирования.</param>
		/// <returns>Вес интегрирования для метода Гаусса соответствующий индексу <paramref name="index"/>.</returns>
		public double GaussianWeight(int index)
		{
			if (index >= Deg)
			{
				throw new IndexOutOfRangeException();
			}

			if (index > (Deg - 1) / 2)
			{
				return GaussianWeight(Deg - index - 1);
			}

			if (GaussianWeights.Count <= Deg)
			{
				GaussianWeights.AddRange(new double[Deg - GaussianWeights.Count + 1][]);
			}

			if (GaussianWeights[Deg] == null)
			{
				GaussianWeights[Deg] = new double[(Deg + 1) / 2];
			}

			var result = GaussianWeights[Deg][index];

			if (result.Equals(0))
			{
				GaussianWeights[Deg][index] = result = 2 / GaussianDenominator.GetValue(Root(index));
			}

			return result;
		}

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="LegendrePolynomial"/> соответствующей степени.
		/// </summary>
		/// <param name="deg">Степень полинома Лежандра.</param>
		/// <exception cref="NotImplementedException">Полиномы Лежандра соответствующей степени не поддерживаются.</exception>
		public LegendrePolynomial(int deg)
		{
			Deg = deg;

			switch (deg)
			{
				case 0:
					A = new[] { 1D };
					return;

				case 1:
					A = new[] { 0, 1D };
					return;

				case 2:
					A = new[] { -1D, 0, 1.5D };
					return;

				case 3:
					A = new[] { 0, -3 / 2D, 0, 5 / 2D };
					return;

				case 4:
					A = new[] { 3 / 8D, 0, -30 / 8D, 0, 35 / 8D };
					return;

				case 5:
					A = new[] { 0, 15 / 8D, 0, -70 / 8D, 0, 63 / 8D };
					return;

				case 6:
					A = new[] { -5 / 16D, 0, 105 / 16D, 0, -315 / 16D, 0, 231 / 16D };
					return;

				case 7:
					A = new[] { 0, -35 / 16D, 0, 315 / 16D, 0, -693 / 16D, 0, 429 / 16D };
					return;

				case 8:
					A = new[] { 35 / 128D, 0, -1260 / 128D, 0, 6930 / 128D, 0, -12012 / 128D, 0, 6435 / 128D };
					return;

				case 9:
					A = new[] { 0, 315 / 128D, 0, -4620 / 128D, 0, 18018 / 128D, 0, -25740 / 128D, 0, 12155 / 128D };
					return;

				case 10:
					A = new[] { -63 / 256D, 0, 3465 / 256D, 0, -30030 / 256D, 0, 90090 / 256D, 0, -109395 / 256D, 0, 46189 / 256D };
					return;

			}

			throw new NotImplementedException("Полиномы Лежандра степени " + deg + " не поддерживаются.");
		}
		/// <summary>
		/// Возвращает строковое представлеине исходного многочлена Лежандра.
		/// </summary>
		/// <returns>Строковое представлеине исходного многочлена Лежандра.</returns>
		/// <remarks>
		/// <code>
		/// var pol = new LegendrePolynomial(6);
		/// Console.Write(pol); //Результат: -5/16 + 105/16 x² - 315/16 x⁴ + 231/16 x⁶, alternative: (231 x⁶ - 315 x⁴ + 105 x² - 5) / 16
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			var result = "";

			int den;
			switch (Deg)
			{
				default:
					den = 1;
					break;
				case 2:
				case 3:
					den = 2;
					break;
				case 4:
				case 5:
					den = 8;
					break;
				case 6:
				case 7:
					den = 16;
					break;
				case 8:
				case 9:
					den = 128;
					break;
				case 10:
					den = 256;
					break;

			}

			for (var i = A.Length - 1; i >= 0; i--)
			{

				var val = i == 0 ? "" : "x";
				if (i > 1)
				{
					val += CStatic.GetIndex(i, true);
				}

				CStatic.AddLinearItem(ref result, A[i] * den, val);
			}

			if (den > 1)
			{
				result = "(" + result + ") / " + den;
			}

			return base.ToString() + ", alternative: " + result;
		}
	}
}