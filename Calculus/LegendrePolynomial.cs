using System;
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

		/// <summary>
		/// Корни и веса правила Гаусса для одной степени.
		/// </summary>
		private sealed class GaussNodes
		{
			/// <summary>
			/// Корни с индексами от 0 до Deg/2 - 1 (остальные симметричны им, а средний корень нечетной степени равен нулю).
			/// </summary>
			public readonly double[] Roots;

			/// <summary>
			/// Веса с индексами от 0 до (Deg + 1)/2 - 1 (остальные симметричны им).
			/// </summary>
			public readonly double[] Weights;

			public GaussNodes(double[] roots, double[] weights)
			{
				Roots = roots;
				Weights = weights;
			}
		}

		//Корни и веса вычисляются для степени целиком при первом обращении и публикуются одной записью ссылки на
		//неизменяемый объект, поэтому чтение обходится без блокировок. Прежде корни и веса хранились в общих списках:
		//одновременное первое обращение из разных потоков (в том числе при инициализации правил Гаусса в Calculus)
		//портило их до конца работы процесса, а после исправления все обращения шли под общей блокировкой.
		//Одновременные вычисления дают одни и те же значения. Индекс — степень (поддерживаются степени до 10).
		private static readonly object[] NodesCache = new object[11];

		private GaussNodes GetNodes()
		{
			var nodes = (GaussNodes) System.Threading.Thread.VolatileRead(ref NodesCache[Deg]);
			if (nodes == null)
			{
				nodes = ComputeNodes();
				System.Threading.Thread.VolatileWrite(ref NodesCache[Deg], nodes);
			}

			return nodes;
		}

		private GaussNodes ComputeNodes()
		{
			var derivative = GetDerivative();

			var roots = new double[Deg/2];
			for (var i = 0; i < roots.Length; i++)
			{
				roots[i] = ResolveRoot(i, derivative);
			}

			var weights = new double[(Deg + 1)/2];
			for (var i = 0; i < weights.Length; i++)
			{
				//Вес w = 2/((1 - x²)·P′(x)²) вычисляется по значению производной в корне. Прежде вычислялось значение
				//раскрытого многочлена (1 - x²)·P′(x)² с коэффициентами до 3,5e7, и из-за вычитания близких чисел
				//относительная погрешность весов при n = 10 достигала 2e-11, а их сумма отличалась от 2 на 5,6e-12.
				//Индекс, которому не соответствует элемент массива корней, — средний корень нечетной степени, он равен нулю.
				var root = i < roots.Length ? roots[i] : 0;
				var value = derivative.GetValue(root);
				weights[i] = 2 / ((1 - root * root) * value * value);
			}

			return new GaussNodes(roots, weights);
		}

		/// <summary>
		/// Находит корень полинома Лежандра по индексу.
		/// </summary>
		/// <param name="index">Индекс корня.</param>
		/// <param name="derivative">Производная полинома.</param>
		/// <returns>Корень полинома.</returns>
		private double ResolveRoot(int index, Polynomial derivative)
		{
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

			return GetNodes().Roots[index];
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

			return GetNodes().Weights[index];
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
					//P₂(x) = (3x² - 1)/2. Прежде свободный член был равен -1, и P₂(1) = 0,5, а корни и веса
					//двухточечной квадратуры Гаусса были неверными.
					A = new[] { -1 / 2D, 0, 3 / 2D };
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
		/// Возвращает строковое представление исходного многочлена Лежандра.
		/// </summary>
		/// <returns>Строковое представление исходного многочлена Лежандра.</returns>
		/// <remarks>
		/// <code>
		/// var pol = new LegendrePolynomial(6);
		/// Console.Write(pol); //Результат: 231/16 x⁶ - 315/16 x⁴ + 105/16 x² - 5/16, alternative: (231 x⁶ - 315 x⁴ + 105 x² - 5) / 16
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