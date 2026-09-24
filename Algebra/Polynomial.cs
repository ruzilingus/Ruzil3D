using System;
using System.Collections;
using System.Collections.Generic;
using Ruzil3D.Calculus;
using Ruzil3D.Utility;
using static Ruzil3D.Math;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет многочлен одной переменной над полем вещественных чисел.
	/// </summary>
	public class Polynomial : IEnumerable<double>, IConvertible
	{
		/// <summary>
		/// Массив структур <see cref="double"/> представляющий коэффициенты членов.
		/// </summary>
		/// <remarks>Индекс члена соответствует его степени.</remarks>
		protected double[] A;

		#region Static fields

		/// <summary>
		/// Представляет многочлен равный 0.
		/// </summary>
		public static readonly Polynomial Empty = new Polynomial { A = new[] { 0D } };

		/// <summary>
		/// Представляет многочлен равный 1.
		/// </summary>
		public static readonly Polynomial Identity = new Polynomial { A = new[] { 1D } };

		/// <summary>
		/// Представляет полином <i>ρ</i>(<i>x</i>) = <i>x</i>.
		/// </summary>
		public static readonly Polynomial Up = new Polynomial(0, 1);

		/// <summary>
		/// Представляет полином <i>ρ</i>(<i>x</i>) = 1 - <i>x</i>.
		/// </summary>
		public static readonly Polynomial Dn = new Polynomial(1, -1);

		#endregion

		#region Properties

		/// <summary>
		/// Получает коэффициент члена соответствующий индексу.
		/// </summary>
		/// <param name="index">Индекс члена.</param>
		/// <returns>Коэффициент члена соответствующий индексу.</returns>
		/// <remarks>Индекс члена соответствует его степени.</remarks>
		public double this[int index] => index < A.Length ? A[index] : 0;

		/// <summary>
		/// Получает значение указывающее равняется ли данный многочлен 0.
		/// </summary>
		/// <param name="p">Многочлен.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="p"/> равняется <see cref="Empty"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsEmpty(Polynomial p) => p == Empty;

		/// <summary>
		/// Получает значение указывающее равняется ли данный многочлен 1.
		/// </summary>
		/// <param name="p">Многочлен.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="p"/> равняется <see cref="Identity"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsIdentity(Polynomial p) => p == Identity;

		private int _degree = -1;

		/// <summary>
		/// Получает степень многочлена.
		/// </summary>
		private int Degree
		{
			get
			{
				if (_degree == -1)
				{
					_degree = A.Length - 1;
					while (_degree > 0 && A[_degree].Equals(0D))
					{
						_degree--;
					}
				}

				return _degree;
			}
		}

		#endregion

		#region Statics

		/// <summary>
		/// Возвращает полином <i>x<sup><paramref name="n"/></sup></i>.
		/// </summary>
		/// <param name="n">Показатель степени.</param>
		public static Polynomial GetPolynomial(int n)
		{
			if (n < 0)
			{
				throw new ArgumentException("Степень многочлена должно быть не меньше 0", nameof(n));
			}

			var a = new double[n + 1];
			a[n] = 1;
			
			return new Polynomial { A = a };
		}

		/// <summary>
		/// Строит интерполяционный полином минимальной степени, проходящий через заданные точки.
		/// </summary>
		/// <param name="point0">Первая точка.</param>
		/// <param name="point1">Вторая точка.</param>
		/// <returns>Интерполяционный полином.</returns>
		/// <exception cref="ArgumentException">Точки имеют одинаковую координату X.</exception>
		public static Polynomial GetPolynomial(PointD point0, PointD point1)
		{
			//Прежде деление на ноль давало многочлен с бесконечными коэффициентами или NaN,
			//хотя перегрузка для массива точек в этом случае выбрасывает исключение.
			var d = point1.X - point0.X;
			if (d.Equals(0D))
			{
				throw new ArgumentException("Точки интерполяции должны иметь разные координаты X.", nameof(point1));
			}

			var k = (point1.Y - point0.Y) / d;
			return new Polynomial(point0.Y - point0.X * k, k);
		}

		/// <summary>
		/// Строит интерполяционный полином минимальной степени, проходящий через заданные точки <paramref name="points"/>.
		/// </summary>
		/// <param name="points">Массив точек представляющий значения полинома в узлах.</param>
		/// <returns>Интерполяционный полином.</returns>
		/// <exception cref="ArgumentException">Две точки имеют одинаковую координату X.</exception>
		public static Polynomial GetPolynomial(params PointD[] points)
		{
			//В форме Лагранжа

			var result = Empty;
			for (var i = 0; i < points.Length; i++)
			{
				var point = points[i];
				var y = point.Y;

				//Для одной точки произведение пусто и базисный многочлен — константа y: прежде он оставался равным null,
				//и метод возвращал null вместо многочлена.
				Polynomial l = null;
				for (var j = 0; j < points.Length; j++)
				{
					if (j == i) continue;

					var p = points[j];
					var d = point.X - p.X;

					//Прежде деление на ноль давало многочлен с бесконечными коэффициентами.
					if (d.Equals(0D))
					{
						throw new ArgumentException("Точки интерполяции должны иметь разные координаты X.", nameof(points));
					}

					if (l == null)
					{
						l = new Polynomial(-p.X * y / d, y / d);
					}
					else
					{
						l *= new Polynomial(-p.X / d, 1D / d);
					}
				}

				result += l ?? y;
			}

			return result;
		}

		/// <summary>
		/// Строит интерполяционный полином проходящий через заданные точки <paramref name="points"/> и а производная проходящая через точки <paramref name="derivatives"/>.
		/// </summary>
		/// <param name="points">Массив точек представляющий значения полинома в узлах.</param>
		/// <param name="derivatives">Массив точек представляющий значения производной в узлах.</param>
		/// <returns>Интерполяционный полином.</returns>
		public static Polynomial GetPolynomial(PointD[] points, PointD[] derivatives)
		{
			var size = points.Length + derivatives.Length;

			var lines = new List<Linear>();

			for (var i = 0; i < points.Length; i++)
			{
				var a = new double[size];

				var x = points[i].X;
				double coefficient = 1;
				for (var j = 0; j < a.Length; j++)
				{
					a[j] = coefficient;
					coefficient *= x;
				}

				lines.Add(new Linear(a, points[i].Y));
			}

			for (var i = 0; i < derivatives.Length; i++)
			{
				var a = new double[size];

				var x = derivatives[i].X;
				double coefficient = 1;
				for (var j = 1; j < a.Length; j++)
				{
					a[j] = j * coefficient;
					coefficient *= x;
				}

				lines.Add(new Linear(a, derivatives[i].Y));
			}



			var resolve = LinearSystem.Resolve(lines.ToArray());
			return new Polynomial { A = resolve };
		}

		#region Bernstein polynomials

		private const int BernsteinsCashSize = 32;

		/// <summary>
		/// Кэш полиномов Бернштейна.
		/// </summary>
		private static readonly Polynomial[][] Bernsteins = new Polynomial[BernsteinsCashSize][];

		/// <summary>
		/// Вычисляет и возвращает базисный полином Бернштейна.
		/// </summary>
		/// <param name="k"></param>
		/// <param name="n"></param>
		/// <returns></returns>
		private static Polynomial CalculateBernstein(int k, int n)
		{
			//Биномиальный коэффициент C(n, k) вычисляется в double: каждое промежуточное значение
			//C(n, i + 1) = C(n, i)·(n - i)/(i + 1) — целое число, поэтому при n ≤ 51 результат точный.
			//Прежде числитель и знаменатель накапливались в int и переполнялись, начиная с n = 13
			//(B(9, 17)(0,5) получался равным 0,0049 вместо 0,1855), а неверные многочлены попадали в кэш.
			var binomial = 1D;

			var p1 = Identity;
			for (var i = 0; i < k; i++)
			{
				binomial = binomial * (n - i) / (i + 1);
				p1 *= Up;
			}

			var p2 = Identity;
			for (var i = k; i < n; i++)
			{
				p2 *= Dn;
			}

			return p1 * p2 * binomial;
		}

		/// <summary>
		/// Возвращает базисный полином Бернштейна степени <paramref name="n"/> соответствующий индексу <paramref name="k"/>: <i>b<sub><paramref name="k"/>,<paramref name="n"/></sub></i>(<i>x</i>).
		/// </summary>
		/// <param name="k">Индекс полинома.</param>
		/// <param name="n">Степень полинома.</param>
		/// <returns>Базисный полином Бернштейна <i>b<sub><paramref name="k"/>,<paramref name="n"/></sub></i>(<i>x</i>)</returns>
		public static Polynomial GetBernstein(int k, int n)
		{
			if (n >= BernsteinsCashSize)
			{
				return CalculateBernstein(k, n);
			}

			//Кэш заполняется под блокировкой: прежде одновременная ленивая инициализация из разных потоков
			//могла заменить уже заполненный массив и привести к NullReferenceException.
			lock (Bernsteins)
			{
				var polynomials = Bernsteins[n] ?? (Bernsteins[n] = new Polynomial[n + 1]);
				return polynomials[k] ?? (polynomials[k] = CalculateBernstein(k, n));
			}
		}

		#endregion

		#region Resolve methods

		/// <summary>
		/// Машинный эпсилон: разность между 1 и следующим за ним числом двойной точности (2⁻⁵²).
		/// </summary>
		private const double MachineEpsilon = 2.220446049250313E-16;

		/// <summary>
		/// Наибольшее число итераций уточнения простого корня. Бисекции достаточно около 2100 шагов, чтобы сжать любой
		/// конечный промежуток до соседних чисел двойной точности; обычно метод Ньютона сходится за несколько шагов.
		/// </summary>
		private const int MaxRootIterations = 2200;

		/// <summary>
		/// Возвращает массив чисел являющихся корнями кубического уравнения <i>a x³ + b x² + c x + d</i> = 0.
		/// </summary>
		/// <param name="a">Коэффициент при x³.</param>
		/// <param name="b">Коэффициент при x².</param>
		/// <param name="c">Коэффициент при x.</param>
		/// <param name="d">Свободный член.</param>
		/// <param name="multiple">Параметр указывающий на необходимость учитывать кратность корней.</param>
		/// <returns>Массив чисел являющихся вещественными корнями кубического уравнения, упорядоченных по возрастанию. Если <paramref name="a"/> равно 0, возвращаются корни уравнения меньшей степени.</returns>
		public static double[] ResolveCubicReal(double a, double b, double c, double d, bool multiple = true)
		{
			//Уравнение решается общим методом Resolve. Прежде использовались формулы Кардано: проверка трёхкратного корня
			//срабатывала и при двукратном (у (x + 1,5)²(x - 3) терялся корень 3), точное сравнение дискриминанта с нулём
			//теряло двукратные корни из-за округления, а промежуточные величины переполнялись при коэффициентах порядка 1e52.
			return new Polynomial(d, c, b, a).Resolve(multiple);
		}

		/// <summary>
		/// Находит различные вещественные корни многочлена и их кратности.
		/// </summary>
		/// <param name="a">Коэффициенты многочлена степени не ниже первой, старший коэффициент не равен 0.</param>
		/// <param name="roots">Список, в который добавляются различные корни в порядке возрастания.</param>
		/// <param name="multiplicities">Список, в который добавляются кратности корней.</param>
		private static void SolveReal(double[] a, List<double> roots, List<int> multiplicities)
		{
			if (a.Length == 2)
			{
				roots.Add(-a[0] / a[1]);
				multiplicities.Add(1);
				return;
			}

			//Корни каждой производной разбивают прямую на промежутки монотонности предыдущего многочлена цепочки,
			//поэтому корни вычисляются снизу вверх, начиная с квадратного трёхчлена, и каждая производная решается один раз.
			var chain = new List<double[]> { a };
			while (chain[chain.Count - 1].Length > 3)
			{
				chain.Add(Differentiate(chain[chain.Count - 1]));
			}

			SolveQuadratic(chain[chain.Count - 1], roots, multiplicities);

			for (var k = chain.Count - 2; k >= 0; k--)
			{
				//Критическая точка, вычисленная как бесконечность (возможно лишь при переполнении), не разбивает прямую.
				var critical = new List<double>();
				var criticalMultiplicities = new List<int>();
				for (var i = 0; i < roots.Count; i++)
				{
					if (!double.IsNaN(roots[i]) && !double.IsInfinity(roots[i]))
					{
						critical.Add(roots[i]);
						criticalMultiplicities.Add(multiplicities[i]);
					}
				}

				roots.Clear();
				multiplicities.Clear();
				SolveByCriticalPoints(chain[k], critical.ToArray(), criticalMultiplicities.ToArray(), roots, multiplicities);
			}
		}

		/// <summary>
		/// Находит вещественные корни квадратного трёхчлена <i>a₂x² + a₁x + a₀</i>.
		/// </summary>
		/// <param name="a">Коэффициенты трёхчлена, старший коэффициент не равен 0.</param>
		/// <param name="roots">Список, в который добавляются различные корни в порядке возрастания.</param>
		/// <param name="multiplicities">Список, в который добавляются кратности корней.</param>
		private static void SolveQuadratic(double[] a, List<double> roots, List<int> multiplicities)
		{
			var c = a[0];
			var b = a[1];
			var major = a[2];

			var discriminant = b * b - 4D * major * c;

			//Дискриминант, равный нулю в пределах погрешности вычислений и представления коэффициентов, означает двукратный корень.
			//Прежде дискриминант сравнивался с нулём точно, и двукратный корень терялся, когда округление делало его отрицательным
			//(например, у x² - 0,42x + 0,0441). Порог совпадает с проверкой значения в вершине параболы в SolveByCriticalPoints.
			var tolerance = 3D * MachineEpsilon * (3D * b * b + 4D * System.Math.Abs(major * c));

			if (discriminant < -tolerance)
			{
				return;
			}

			if (discriminant <= tolerance)
			{
				roots.Add(-b / (2D * major));
				multiplicities.Add(2);
				return;
			}

			//Устойчивая форма: q и корни q/a₂, a₀/q вычисляются без вычитания близких чисел. Прежде по школьной формуле
			//малый корень x² - 1e8·x + 1 получался равным 7,45e-9 вместо 1e-8.
			var sqrt = System.Math.Sqrt(discriminant);
			var q = -0.5 * (b < 0D ? b - sqrt : b + sqrt);
			var x1 = q / major;
			var x2 = c / q;

			roots.Add(System.Math.Min(x1, x2));
			roots.Add(System.Math.Max(x1, x2));
			multiplicities.Add(1);
			multiplicities.Add(1);
		}

		/// <summary>
		/// Находит вещественные корни многочлена по корням его производной (критическим точкам).
		/// </summary>
		/// <param name="a">Коэффициенты многочлена степени не ниже второй, старший коэффициент не равен 0.</param>
		/// <param name="critical">Различные вещественные корни производной в порядке возрастания.</param>
		/// <param name="criticalMultiplicities">Кратности корней производной.</param>
		/// <param name="roots">Список, в который добавляются различные корни в порядке возрастания.</param>
		/// <param name="multiplicities">Список, в который добавляются кратности корней.</param>
		/// <remarks>
		/// Между соседними критическими точками многочлен строго монотонен, поэтому на промежутке со строго разными знаками
		/// на концах лежит ровно один простой корень, а на остальных промежутках корней нет. Критическая точка, значение
		/// в которой не превышает погрешности вычислений, — кратный корень.
		/// </remarks>
		private static void SolveByCriticalPoints(double[] a, double[] critical, int[] criticalMultiplicities, List<double> roots, List<int> multiplicities)
		{
			var n = a.Length - 1;
			var count = critical.Length;

			var values = new double[count];
			var zeros = new bool[count];
			for (var j = 0; j < count; j++)
			{
				double derivative;
				values[j] = Evaluate(a, critical[j], out derivative);

				//Прежде кратные корни в критических точках пропускались: у (x - 1)²(x - 2)(x - 3)(x - 4) не находился корень 1.
				zeros[j] = !double.IsInfinity(values[j]) && System.Math.Abs(values[j]) <= GetErrorBound(a, critical[j]);
			}

			var majorSign = Sign(a[n]);
			var previousPosition = double.NegativeInfinity;
			var previousSign = n % 2 == 0 ? majorSign : -majorSign;
			var bound = 0D;

			var index = 0;
			while (true)
			{
				double position;
				int sign;
				var rootMultiplicity = 0;

				if (index < count && zeros[index])
				{
					//Подряд идущие нулевые критические точки неразличимы в пределах погрешности: между ними многочлен монотонен,
					//поэтому это один кратный корень. Его кратность на 1 больше суммарной кратности корней производной
					//(так число корней не превышает степени), а положение — среднее с учётом кратностей.
					var first = critical[index];
					var offset = 0D;
					var sum = 0;

					for (; index < count && zeros[index]; index++)
					{
						offset += (critical[index] - first) * criticalMultiplicities[index];
						sum += criticalMultiplicities[index];
					}

					position = first + offset / sum;
					sign = 0;
					rootMultiplicity = sum + 1;
				}
				else if (index < count)
				{
					position = critical[index];
					sign = Sign(values[index]);
					index++;
				}
				else
				{
					position = double.PositiveInfinity;
					sign = majorSign;
				}

				//Знаки сравниваются без перемножения значений, чтобы не терять их при переполнении и потере значимости.
				if (previousSign != 0 && sign != 0 && previousSign != sign)
				{
					if (bound.Equals(0D) && (double.IsInfinity(previousPosition) || double.IsInfinity(position)))
					{
						bound = GetRootBound(a);
					}

					roots.Add(FindRoot(a, previousPosition, position, previousSign, bound));
					multiplicities.Add(1);
				}

				if (rootMultiplicity > 0)
				{
					roots.Add(position);
					multiplicities.Add(rootMultiplicity);
				}

				if (double.IsPositiveInfinity(position))
				{
					return;
				}

				previousPosition = position;
				previousSign = sign;
			}
		}

		/// <summary>
		/// Находит простой корень многочлена на промежутке монотонности, на концах которого многочлен имеет разные знаки.
		/// </summary>
		/// <param name="a">Коэффициенты многочлена.</param>
		/// <param name="lo">Левый конец промежутка или минус бесконечность.</param>
		/// <param name="hi">Правый конец промежутка или плюс бесконечность.</param>
		/// <param name="loSign">Знак многочлена на левом конце промежутка.</param>
		/// <param name="bound">Граница модулей корней многочлена; используется вместо бесконечного конца.</param>
		/// <returns>Корень многочлена.</returns>
		/// <remarks>
		/// Гибрид метода Ньютона и бисекции (как rtsafe из Numerical Recipes): промежуток, содержащий корень, сохраняется
		/// на каждом шаге, а шаг Ньютона заменяется бисекцией, если выходит за промежуток или сокращается медленнее,
		/// чем вдвое за два шага. Прежде метод Ньютона не удерживал промежуток и останавливался, как только модуль
		/// значения переставал убывать, поэтому для многочленов степени выше 4 возвращались точки, не являющиеся корнями.
		/// </remarks>
		private static double FindRoot(double[] a, double lo, double hi, int loSign, double bound)
		{
			//За границей модулей корней знак многочлена совпадает со знаком старшего члена.
			if (double.IsNegativeInfinity(lo))
			{
				lo = System.Math.Min(-2D * bound, hi - bound);
			}

			if (double.IsPositiveInfinity(hi))
			{
				hi = System.Math.Max(2D * bound, lo + bound);
			}

			var x = 0.5 * lo + 0.5 * hi;
			var best = x;
			var bestAbs = double.PositiveInfinity;
			var step = hi - lo;
			var previousStep = step;

			for (var iteration = 0; iteration < MaxRootIterations; iteration++)
			{
				double derivative;
				var value = Evaluate(a, x, out derivative);

				var abs = System.Math.Abs(value);
				if (abs <= bestAbs)
				{
					best = x;
					bestAbs = abs;
				}

				if (value.Equals(0D))
				{
					break;
				}

				if (Sign(value) == loSign)
				{
					lo = x;
				}
				else
				{
					hi = x;
				}

				var newton = x - value / derivative;

				//Шаг Ньютона меньше точности представления: корень найден.
				if (newton.Equals(x))
				{
					break;
				}

				var next = newton > lo && newton < hi && 2D * System.Math.Abs(newton - x) <= System.Math.Abs(previousStep)
					? newton
					: 0.5 * lo + 0.5 * hi;

				//Промежуток сжался до соседних чисел двойной точности.
				if (!(next > lo && next < hi))
				{
					break;
				}

				previousStep = step;
				step = next - x;
				x = next;
			}

			return best;
		}

		/// <summary>
		/// Возвращает коэффициенты производной многочлена, нормированные функцией <see cref="Normalize"/>.
		/// </summary>
		/// <param name="a">Коэффициенты многочлена.</param>
		/// <returns>Коэффициенты производной.</returns>
		private static double[] Differentiate(double[] a)
		{
			var result = new double[a.Length - 1];
			for (var i = 1; i < a.Length; i++)
			{
				result[i - 1] = i * a[i];
			}

			Normalize(result);
			return result;
		}

		/// <summary>
		/// Умножает коэффициенты многочлена на степень двойки так, чтобы наибольший из них по модулю был порядка 1.
		/// </summary>
		/// <param name="a">Коэффициенты многочлена.</param>
		/// <remarks>
		/// Умножение на степень двойки выполняется точно и не меняет корней, но исключает переполнение и потерю значимости
		/// в промежуточных вычислениях: прежде у 1e52·(x - 1)(x - 2)(x - 3) получались корни -∞, NaN и ∞.
		/// </remarks>
		private static void Normalize(double[] a)
		{
			var max = 0D;
			foreach (var coefficient in a)
			{
				max = System.Math.Max(max, System.Math.Abs(coefficient));
			}

			if (!(max > 0D) || double.IsInfinity(max))
			{
				return;
			}

			var exponent = -GetExponent(max);
			for (var i = 0; i < a.Length; i++)
			{
				a[i] = ScaleByPowerOfTwo(a[i], exponent);
			}
		}

		/// <summary>
		/// Выполняет замену переменной <i>x</i> = 2<sup><i>k</i></sup><i>y</i>, после которой модули корней в среднем близки к 1,
		/// и нормирует коэффициенты многочлена от <i>y</i>.
		/// </summary>
		/// <param name="a">Коэффициенты многочлена; старший и свободный коэффициенты не равны 0.</param>
		/// <returns>Показатель <i>k</i>: корни исходного многочлена в 2<sup><i>k</i></sup> раз больше корней нового.</returns>
		/// <remarks>
		/// Среднее геометрическое модулей корней равно |a₀/aₙ|^(1/n), поэтому после замены старший и свободный коэффициенты
		/// одного порядка и ни один из них не теряется при нормировке, даже если исходные коэффициенты различаются
		/// на сотни порядков (например, у 1e-300·x³ - 1e300). Умножения на степени двойки выполняются точно, а показатели
		/// вычисляются по двоичным порядкам коэффициентов, поэтому умножение многочлена на степень двойки не меняет корней даже в последнем бите.
		/// </remarks>
		private static int Balance(double[] a)
		{
			var n = a.Length - 1;
			var k = (int)System.Math.Round((double)(GetExponent(a[0]) - GetExponent(a[n])) / n);

			var shift = int.MinValue;
			for (var i = 0; i <= n; i++)
			{
				if (!a[i].Equals(0D))
				{
					shift = System.Math.Max(shift, GetExponent(a[i]) + k * i);
				}
			}

			for (var i = 0; i <= n; i++)
			{
				a[i] = ScaleByPowerOfTwo(a[i], k * i - shift);
			}

			return k;
		}

		/// <summary>
		/// Возвращает двоичный порядок числа, то есть целую часть log₂|<paramref name="x"/>|, вычисленную точно по двоичному представлению.
		/// </summary>
		/// <param name="x">Конечное число, не равное 0.</param>
		/// <returns>Двоичный порядок числа.</returns>
		private static int GetExponent(double x)
		{
			var exponent = (int)((BitConverter.DoubleToInt64Bits(x) >> 52) & 0x7FF);

			//Денормализованное число после умножения на 2⁵⁴ становится нормализованным.
			return exponent == 0 ? GetExponent(x * 18014398509481984D) - 54 : exponent - 1023;
		}

		/// <summary>
		/// Умножает число на 2<sup><paramref name="exponent"/></sup>.
		/// </summary>
		/// <param name="value">Исходное число.</param>
		/// <param name="exponent">Показатель степени двойки.</param>
		/// <returns>Произведение, вычисленное точно, если оно не выходит за пределы нормализованных чисел двойной точности.</returns>
		private static double ScaleByPowerOfTwo(double value, int exponent)
		{
			//Множитель 2^exponent может не поместиться в double, поэтому умножение выполняется по частям,
			//каждая из которых — точное число 2^part, собранное из двоичного представления.
			while (exponent != 0)
			{
				var part = System.Math.Max(-1000, System.Math.Min(1000, exponent));
				value *= BitConverter.Int64BitsToDouble((long)(part + 1023) << 52);
				exponent -= part;
			}

			return value;
		}

		/// <summary>
		/// Вычисляет по схеме Горнера значение многочлена и его производной.
		/// </summary>
		/// <param name="a">Коэффициенты многочлена.</param>
		/// <param name="x">Значение аргумента.</param>
		/// <param name="derivative">Значение производной.</param>
		/// <returns>Значение многочлена.</returns>
		private static double Evaluate(double[] a, double x, out double derivative)
		{
			var n = a.Length - 1;
			var value = a[n];
			derivative = 0D;

			for (var i = n - 1; i >= 0; i--)
			{
				derivative = derivative * x + value;
				value = value * x + a[i];

				if (double.IsInfinity(value))
				{
					//При переполнении знак значения определяется старшими членами, как в методе GetValue.
					derivative = double.NaN;
					return i % 2 == 0 ? value : value * Sign(x);
				}
			}

			return value;
		}

		/// <summary>
		/// Возвращает оценку погрешности значения многочлена, вычисленного по схеме Горнера, с учётом погрешности
		/// представления коэффициентов: <i>(n + 1)·ε·Σ|aᵢ||x|ⁱ</i>.
		/// </summary>
		/// <param name="a">Коэффициенты многочлена.</param>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Оценка погрешности значения многочлена.</returns>
		private static double GetErrorBound(double[] a, double x)
		{
			var n = a.Length - 1;
			var abs = System.Math.Abs(x);
			var sum = System.Math.Abs(a[n]);

			for (var i = n - 1; i >= 0; i--)
			{
				sum = sum * abs + System.Math.Abs(a[i]);
			}

			return (n + 1) * MachineEpsilon * sum;
		}

		/// <summary>
		/// Возвращает степень двойки, не меньшую границы модулей корней многочлена по Фудзиваре:
		/// <i>2·max(|aᵢ/aₙ|^(1/(n - i)), |a₀/(2aₙ)|^(1/n))</i>.
		/// </summary>
		/// <param name="a">Коэффициенты многочлена.</param>
		/// <returns>Положительное число, не меньшее модуля любого корня.</returns>
		private static double GetRootBound(double[] a)
		{
			var n = a.Length - 1;
			var major = GetExponent(a[n]);
			var max = int.MinValue;

			//Отношения коэффициентов оцениваются сверху по их двоичным порядкам: |aᵢ/aₙ| < 2^(eᵢ + 1 - eₙ).
			//Это исключает переполнение, а граница точно масштабируется вместе с коэффициентами.
			for (var i = 0; i < n; i++)
			{
				if (a[i].Equals(0D))
				{
					continue;
				}

				var exponent = GetExponent(a[i]) + 1 - major - (i == 0 ? 1 : 0);
				max = System.Math.Max(max, (int)System.Math.Ceiling((double)exponent / (n - i)));
			}

			return ScaleByPowerOfTwo(2D, System.Math.Max(-1070, System.Math.Min(1020, max)));
		}

		#endregion

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Polynomial"/> из массива структур <see cref="double"/> представляющий коэффициенты членов.
		/// </summary>
		/// <param name="a">Коэффициенты членов.</param>
		/// <remarks>Индекс члена соответствует его степени.</remarks>
		public Polynomial(params double[] a)
		{
			if (a == null)
			{
				throw new ArgumentException("Степень многочлена должно быть не меньше 0", nameof(a));
			}

			if (a.Length == 0)
			{
				A = new[] { 0D };
			}
			else
			{
				A = new double[a.Length];
				a.CopyTo(A, 0);
			}
		}

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Polynomial"/> не выше первой степени: <i><paramref name="a0"/> + <paramref name="a1"/> x</i>.
		/// </summary>
		/// <param name="a0">Коэффициент при нулевом члене.</param>
		/// <param name="a1">Коэффициент при первом члене.</param>
		public Polynomial(double a0, double a1)
		{
			A = new[] { a0, a1 };
		}

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Polynomial"/> не выше второй степени: <i><paramref name="a0"/> + <paramref name="a1"/> x + <paramref name="a2"/> x²</i>.
		/// </summary>
		/// <param name="a0">Коэффициент при нулевом члене.</param>
		/// <param name="a1">Коэффициент при первом члене.</param>
		/// <param name="a2">Коэффициент при втором члене.</param>
		public Polynomial(double a0, double a1, double a2)
		{
			A = new[] { a0, a1, a2 };
		}

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Polynomial"/> не выше третей степени: <i><paramref name="a0"/> + <paramref name="a1"/> x + <paramref name="a2"/> x² + <paramref name="a3"/> x³</i>.
		/// </summary>
		/// <param name="a0">Коэффициент при нулевом члене.</param>
		/// <param name="a1">Коэффициент при первом члене.</param>
		/// <param name="a2">Коэффициент при втором члене.</param>
		/// <param name="a3">Коэффициент при третьем члене.</param>
		public Polynomial(double a0, double a1, double a2, double a3)
		{
			A = new[] { a0, a1, a2, a3 };
		}

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Polynomial"/> не выше четвертой степени: <i><paramref name="a0"/> + <paramref name="a1"/> x + <paramref name="a2"/> x² + <paramref name="a3"/> x³ + <paramref name="a4"/> x⁴</i>.
		/// </summary>
		/// <param name="a0">Коэффициент при нулевом члене.</param>
		/// <param name="a1">Коэффициент при первом члене.</param>
		/// <param name="a2">Коэффициент при втором члене.</param>
		/// <param name="a3">Коэффициент при третьем члене.</param>
		/// <param name="a4">Коэффициент при четвертом члене.</param>
		public Polynomial(double a0, double a1, double a2, double a3, double a4)
		{
			A = new[] { a0, a1, a2, a3, a4 };
		}

		/// <summary>
		/// Возвращает полином с заданными корнями и с единичным старшим коэффициентом.
		/// </summary>
		/// <param name="roots">Корни полинома.</param>
		/// <returns>Полином с заданными корнями и с единичным старшим коэффициентом.</returns>
		public static Polynomial GetPolynomialByRoots(params double[] roots)
		{
			var result = Identity;

			foreach (var root in roots)
			{
				result *= new Polynomial(-root, 1);
			}

			return result;
		}

		#endregion

		#region Overloads

		/// <summary>
		/// Определяет неявное преобразование числа с плавающей запятой двойной точности в многочлен.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в многочлен.</param>
		/// <returns>Многочлен состоящий из одного нулевого члена с коэффициентом <paramref name="value"/>.</returns>
		public static implicit operator Polynomial(double value)
		{
			return new Polynomial { A = new[] { value } };
		}

		/// <summary>
		/// Определяет неявное преобразование дробного числа в многочлен.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в многочлен.</param>
		/// <returns>Многочлен состоящий из одного нулевого члена с коэффициентом <paramref name="value"/>.</returns>
		public static implicit operator Polynomial(Fraction value)
		{
			return new Polynomial { A = new double[] { value } };
		}

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух многочленов.
		/// </summary>
		/// <param name="x">Первый многочлен для сравнения.</param>
		/// <param name="y">Вторый многочлен для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Polynomial x, Polynomial y)
		{
			if (ReferenceEquals(x, null))
			{
				return ReferenceEquals(y, null);
			}
			if (ReferenceEquals(y, null))
			{
				return false;
			}

			double[] xArray, yArray;
			if (x.A.Length < y.A.Length)
			{
				xArray = x.A;
				yArray = y.A;
			}
			else
			{
				xArray = y.A;
				yArray = x.A;
			}

			for (var i = xArray.Length; i < yArray.Length; i++)
			{
				if (!yArray[i].Equals(0D))
				{
					return false;
				}
			}


			for (var i = 0; i < xArray.Length; i++)
			{
				if (!xArray[i].Equals(yArray[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух многочленов.
		/// </summary>
		/// <param name="x">Первый многочлен для сравнения.</param>
		/// <param name="y">Вторый многочлен для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Polynomial x, Polynomial y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Возвращает аддитивную инверсию многочлена заданного параметром <paramref name="x"/>.
		/// </summary>
		/// <param name="x">Инвертируемое значение.</param>
		/// <returns>Результат умножения многочлена <paramref name="x"/> на -1.</returns>
		public static Polynomial operator -(Polynomial x)
		{
			var result = new double[x.A.Length];

			for (var i = 0; i < x.A.Length; i++)
			{
				result[i] = -x.A[i];
			}

			return new Polynomial { A = result };
		}

		/// <summary>
		/// Возвращает исходный многочлен заданный параметром <paramref name="x"/>.
		/// </summary>
		/// <param name="x">Исходный многочлен.</param>
		/// <returns>Исходный многочлен.</returns>
		public static Polynomial operator +(Polynomial x)
		{
			return x;
		}

		/// <summary>
		/// Складывает два многочлена.
		/// </summary>
		/// <param name="x">Первое из складываемых многочленов.</param>
		/// <param name="y">Второе из складываемых многочленов.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator +(Polynomial x, Polynomial y)
		{
			if (ReferenceEquals(x, Empty)) return y;
			if (ReferenceEquals(y, Empty)) return x;

			double[] array1, array2;
			if (x.A.Length < y.A.Length)
			{
				array1 = x.A;
				array2 = y.A;
			}
			else
			{
				array1 = y.A;
				array2 = x.A;
			}


			var result = new double[array2.Length];

			for (var i = 0; i < array1.Length; i++)
			{
				result[i] = array1[i] + array2[i];
			}

			for (var i = array1.Length; i < array2.Length; i++)
			{
				result[i] = array2[i];
			}

			return new Polynomial { A = result };
		}

		/// <summary>
		/// Вычитает многочлен из другого многочлена.
		/// </summary>
		/// <param name="x">Многочлен, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Многочлен для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator -(Polynomial x, Polynomial y)
		{
			//return x + -y;

			if (ReferenceEquals(x, Empty)) return -y;
			if (ReferenceEquals(y, Empty)) return x;


			if (x.A.Length < y.A.Length)
			{
				var result = new double[y.A.Length];
				for (var i = 0; i < x.A.Length; i++)
				{
					result[i] = x.A[i] - y.A[i];
				}

				for (var i = x.A.Length; i < y.A.Length; i++)
				{
					result[i] = -y.A[i];
				}

				return new Polynomial { A = result };
			}
			else
			{
				var result = new double[x.A.Length];

				for (var i = 0; i < y.A.Length; i++)
				{
					result[i] = x.A[i] - y.A[i];
				}

				for (var i = y.A.Length; i < x.A.Length; i++)
				{
					result[i] = x.A[i];
				}

				return new Polynomial { A = result };
			}
		}

		/// <summary>
		/// Возвращает произведение двух многочленов.
		/// </summary>
		/// <param name="x">Первый многочлен для перемножения.</param>
		/// <param name="y">Второй многочлен для перемножения.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator *(Polynomial x, Polynomial y)
		{
			if (ReferenceEquals(x, Identity)) return y;
			if (ReferenceEquals(y, Identity)) return x;
			if (ReferenceEquals(x, Empty)) return Empty;
			if (ReferenceEquals(y, Empty)) return Empty;

			var deg = x.A.Length + y.A.Length - 2;

			var result = new double[deg + 1];

			for (var i = 0; i < x.A.Length; i++)
				for (var j = 0; j < y.A.Length; j++)
				{
					result[i + j] += x.A[i] * y.A[j];
				}

			return new Polynomial { A = result };
		}

		/// <summary>
		/// Вычисляет частное и остаток от деления двух многочленов.
		/// </summary>
		/// <param name="x">Значение содержащее делимое.</param>
		/// <param name="y">Значение содержащее делитель.</param>
		/// <param name="rem">Значение, представляющее полученный остаток.</param>
		/// <returns>Значение, содержащее частное указанных многочленов.</returns>
		/// <exception cref="DivideByZeroException">Значение параметра <paramref name="y"/> равно нулю</exception>
		public static Polynomial DivRem(Polynomial x, Polynomial y, out Polynomial rem)
		{
			var yDeg = y.Degree;

			if (yDeg == 0)
			{
				var num = y.A[0];
				if (num.Equals(0D))
				{
					throw new DivideByZeroException("Значение параметра " + nameof(y) + " равно нулю.");
				}

				rem = 0;
				return x / num;
			}

			var xDeg = x.Degree;

			if (xDeg < yDeg)
			{
				rem = x;
				return 0D;
			}

			double[] remArray;
			var result = Div(x.A, y.A, xDeg, yDeg, out remArray);

			rem = new Polynomial {A = remArray};
			return result;
		}

		private static Polynomial Div(IList<double> x, IList<double> y, int xDeg, int yDeg, out double[] rem)
		{
			//Копируем только значимые коэффициенты: массив делимого может содержать нулевые старшие
			//коэффициенты, и прежде копирование всего массива выбрасывало исключение.
			rem = new double[xDeg + 1];
			for (var i = 0; i <= xDeg; i++)
			{
				rem[i] = x[i];
			}

			var degree = xDeg - yDeg;
			var result = new double[degree + 1];
			var major = y[yDeg];
			
			while (degree >= 0)
			{
				var coef = rem[degree + yDeg]/major;
				result[degree] = coef;

				for (var i = 0; i < yDeg; i++)
				{
					rem[i + degree] -= coef*y[i];
				}

				degree--;
			}

			Array.Resize(ref rem, yDeg);
			return new Polynomial {A = result};
		}


		/// <summary>
		/// Делит один многочлен на другой и возвращает результат.
		/// </summary>
		/// <param name="x">Многочлен-числитель.</param>
		/// <param name="y">Многочлен - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Polynomial operator /(Polynomial x, Polynomial y)
		{
			Polynomial rem;
			return DivRem(x, y, out rem);
		}

		/// <summary>
		/// Делит один многочлен на другой и возвращает остаток.
		/// </summary>
		/// <param name="x">Многочлен-числитель.</param>
		/// <param name="y">Многочлен - знаменатель.</param>
		/// <returns>Остаток от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Polynomial operator %(Polynomial x, Polynomial y)
		{
			Polynomial rem;
			DivRem(x, y, out rem);
			return rem;
		}

		/// <summary>
		/// Делит многочлен на число двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="x">Многочлен-числитель.</param>
		/// <param name="y">Число двойной точности с плавающей запятой.</param>
		/// <returns>Многочлен представляющий частное от деления многочлена <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Polynomial operator /(Polynomial x, double y)
		{
			if (y.Equals(0D))
			{
				throw new DivideByZeroException("Значение знаменателя равно нулю.");
			}

			if (y.Equals(1D))
			{
				return x;
			}

			var result = new double[x.A.Length];
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = x.A[i] / y;
			}
			return new Polynomial { A = result };
		}


		/// <summary>
		/// Возвращает произведение многочлена на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель.</param>
		/// <param name="y">Многочлен.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator *(double x, Polynomial y)
		{
			return y * x;
		}

		/// <summary>
		/// Возвращает произведение многочлена на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Многочлен.</param>
		/// <param name="y">Множитель.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator *(Polynomial x, double y)
		{
			if (y.Equals(0D))
			{
				return Empty;
			}

			if (y.Equals(1D))
			{
				return x;
			}

			var result = new double[x.A.Length];

			for (var i = 0; i < result.Length; i++)
			{
				result[i] = x.A[i] * y;
			}

			return new Polynomial { A = result };
		}

		/// <summary>
		/// Складывает число с плавающей запятой двойной точности и многочлен.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator +(double x, Polynomial y)
		{
			return y + x;
		}

		/// <summary>
		/// Складывает многочлен и число с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator +(Polynomial x, double y)
		{
			if (y.Equals(0D))
			{
				return x;
			}

			var result = x.A.Clone() as double[];

			// ReSharper disable once PossibleNullReferenceException
			result[0] += y;
			return new Polynomial { A = result };
		}

		/// <summary>
		/// Вычитает многочлен из числа с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator -(double y, Polynomial x)
		{
			//return y + -x;

			var result = new double[x.A.Length];

			result[0] = y - x.A[0];
			for (var i = 1; i < x.A.Length; i++)
			{
				result[i] = -x.A[i];
			}

			return new Polynomial { A = result };
		}

		/// <summary>
		/// Вычитает число с плавающей запятой двойной точности из многочлена.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Polynomial operator -(Polynomial x, double y)
		{
			if (y.Equals(0D))
			{
				return x;
			}

			var result = x.A.Clone() as double[];
			// ReSharper disable once PossibleNullReferenceException
			result[0] -= y;
			return new Polynomial { A = result };
		}


		#endregion

		#region Methods

		private static readonly double[] EmptyArray = new double[0];

		/// <summary>
		/// Возвращает вещественные корни многочлена в массиве структур <see cref="double"/>, упорядоченные по возрастанию.
		/// </summary>
		/// <param name="multiple">Параметр указывающий на необходимость учитывать кратность корней: если <b>true</b>, кратный корень повторяется столько раз, какова его кратность; если <b>false</b>, возвращаются только различные корни.</param>
		/// <returns>Корни многочлена. Для многочлена нулевой степени, в том числе нулевого, и для многочлена с коэффициентами <see cref="double.NaN"/> или бесконечностью возвращается пустой массив.</returns>
		/// <remarks>
		/// Корни ищутся на промежутках монотонности, которые задают корни производной, найденные тем же методом.
		/// На промежутке с разными знаками на концах простой корень уточняется гибридом метода Ньютона и бисекции,
		/// а критическая точка, значение в которой не превышает погрешности вычислений, считается кратным корнем.
		/// Поэтому корни, неразличимые в пределах точности чисел двойной точности, возвращаются как один кратный корень.
		/// Умножение многочлена на число не меняет результата, если коэффициенты при этом не округляются (например, при умножении на степень двойки).
		/// </remarks>
		public double[] Resolve(bool multiple = true)
		{
			var degree = Degree;

			for (var i = 0; i <= degree; i++)
			{
				if (double.IsNaN(A[i]) || double.IsInfinity(A[i]))
				{
					return EmptyArray;
				}
			}

			var roots = new List<double>();
			var multiplicities = new List<int>();

			if (degree > 0)
			{
				//Нулевые младшие коэффициенты дают точный корень 0 соответствующей кратности:
				//прежде x⁵.Resolve() возвращал восемь «корней» -0 и 0.
				var zeros = 0;
				while (A[zeros].Equals(0D))
				{
					zeros++;
				}

				if (zeros < degree)
				{
					var a = new double[degree - zeros + 1];
					Array.Copy(A, zeros, a, 0, a.Length);

					var exponent = Balance(a);
					SolveReal(a, roots, multiplicities);

					for (var i = 0; i < roots.Count; i++)
					{
						roots[i] = ScaleByPowerOfTwo(roots[i], exponent);
					}
				}

				if (zeros > 0)
				{
					var index = 0;
					while (index < roots.Count && roots[index] < 0D)
					{
						index++;
					}

					roots.Insert(index, 0D);
					multiplicities.Insert(index, zeros);
				}
			}

			var result = new List<double>();
			for (var i = 0; i < roots.Count; i++)
			{
				//Прибавление нуля заменяет -0 на 0.
				var root = roots[i] + 0D;

				//Различные корни, совпавшие после обратной замены переменной (при переполнении или потере значимости), объединяются.
				if (!multiple && result.Count > 0 && result[result.Count - 1].Equals(root))
				{
					continue;
				}

				for (var k = multiple ? multiplicities[i] : 1; k > 0; k--)
				{
					result.Add(root);
				}
			}

			return result.Count == 0 ? EmptyArray : result.ToArray();
		}

		/*

		/// <summary>
		/// Возвращает значение многочлена от десятичного числа.
		/// </summary>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Значение многочлена.</returns>
		public decimal GetValue(decimal x)
		{
			var result = 0M;
			var powX = 1M;
			foreach (var coef in A)
			{
				result += powX * (decimal) coef;
				powX *= x;
			}

			return result;
		}
		*/

		/// <summary>
		/// Возвращает значение многочлена в комплексной точке <paramref name="x"/>.
		/// </summary>
		/// <param name="x">Комплексное значение аргумента.</param>
		/// <returns>Значение многочлена.</returns>
		public Complex GetValue(Complex x)
		{
			var result = Complex.Empty;
			var powX = Complex.Identity;
			foreach (var coef in A)
			{
				result += powX * coef;
				powX *= x;
			}
			return result;
		}

		/// <summary>
		/// Возвращает значение многочлена в точке x вычисленный методом Горнера.
		/// </summary>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Значение многочлена.</returns>
		public double GetValue(double x)
		{
			if (double.IsNaN(x))
			{
				return double.NaN;
			}

			var i = A.Length - 1;

			double result = 0;

			while (i >= 0 && result.Equals(0D))
			{
				result = A[i];
				i--;
			}

			if (i < 0)
			{
				return result;
			}

			while (i >= 0)
			{
				result = result * x + A[i];

				if (double.IsInfinity(result))
				{
					return i%2 == 0 ? result : result*Sign(x);
				}

				i--;
			}
			return result;
		}

		/// <summary>
		/// Возвращает определённый интеграл исходного многочлена от <paramref name="x0"/> до <paramref name="x1"/>.
		/// </summary>
		/// <param name="x0">Начальная точка.</param>
		/// <param name="x1">Конечная точка.</param>
		/// <returns>Определённый интеграл исходного многочлена, то есть площадь со знаком: части фигуры под осью абсцисс учитываются со знаком минус (например, для <i>x</i> на отрезке [-1, 1] результат равен 0).</returns>
		public double GetArea(double x0, double x1)
		{
			double result = 0;
			var powX0 = 1D;
			var powX1 = 1D;

			//Первообразная
			for (var i = 0; i < A.Length; i++)
			{
				powX0 *= x0;
				powX1 *= x1;
				result += (powX1 - powX0) * A[i] / (i + 1D);
			}

			return result;
		}

		/*
		public static implicit operator Polynomial(string value)
		{
			return new Polynomial(1,-1);
		}
		*/

		/// <summary>
		/// Возвращает многочлен представляющий производную от исходного.
		/// </summary>
		/// <param name="order">Степень производной.</param>
		/// <returns>Многочлен представляющий производную от исходного.</returns>
		public Polynomial GetDerivative(int order = 1)
		{
			if (order < 0)
			{
				throw new ArgumentException("Порядок производной не может быть отрицательным.", nameof(order));
			}

			//Производная порядка выше степени многочлена равна нулю (прежде создавался массив отрицательной длины).
			if (order >= A.Length)
			{
				return Empty;
			}

			if (order == 0)
			{
				//return new Polynomial { A = A.Clone() as double[] };
				return this;
			}

			var result = new double[A.Length - order];

			for (var i = order; i < A.Length; i++)
			{
				//Убывающий факториал i·(i - 1)·…·(i - order + 1) вычисляется в double: прежде он вычислялся в int
				//и переполнялся, начиная с 13! (x¹³ после 13 дифференцирований давал 1932053504 вместо 6227020800).
				double coef = i;
				for (var j = 1; j < order; j++)
				{
					coef *= i - j;
				}

				result[i - order] = coef * A[i];
			}

			return new Polynomial { A = result };
		}

		/// <summary>
		/// Возвращает многочлен соответствующий замене переменной заданный параметром <paramref name="u"/>.
		/// </summary>
		/// <param name="u">Многочлен подстановки: ρ(x) => ρ(u(x)).</param>
		/// <returns>Многочлен соответствующий замене переменной.</returns>
		public Polynomial Substitution(Polynomial u)
		{
			Polynomial result = A[0];
			var pow = u;
			for (var i = 1; i < A.Length; i++)
			{
				result += A[i] * pow;
				pow *= u;
			}

			return result;
		}

		/// <summary>
		/// Возвращает исходный многочлен, возведенный в степень, заданную 32-битовым целым числом со знаком.
		/// </summary>
		/// <param name="y">32-битовое целое число со знаком, задающее степень.</param>
		/// <returns>Многочлен, возведенный в степень <paramref name="y"/>.</returns>
		public Polynomial Pow(int y)
		{
			//return y == 0 ? Identity : this * Pow(y - 1);

			if (y < 0)
			{
				throw new ArgumentException("Значение " + nameof(y) + " должно быть не меньше 0");
			}

			//Нулевая степень любого многочлена, в том числе нулевого, равна 1: прежде проверка на Empty выполнялась
			//раньше, и Empty.Pow(0) возвращал 0, хотя new Polynomial(0).Pow(0) возвращал 1.
			if (y == 0) return Identity;
			if (ReferenceEquals(this, Identity)) return Identity;
			if (ReferenceEquals(this, Empty)) return Empty;

			switch (y)
			{
				case 1:
					//return new Polynomial {A = A.Clone() as double[]};
					return this;

				case 2:
					return this * this;

				default:

					var pow = this;
					var result = Identity;
					while (true)
					{
						if ((y & 1) != 0)
						{
							result *= pow;
						}

						y >>= 1;
						if (y == 0) break;
						pow *= pow;
					}
					return result;
			}
		}

		#endregion

		#region Inherited Items

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другой многочлен.</param>
		/// <returns>Значение <b>true</b>, если два многочлена равны; в противном случае — значение <b>false</b>.</returns>
		protected bool Equals(Polynomial other)
		{
			return this == other;
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Polynomial"/> (в том числе к производному типу) и представляет одинаковые значения с исходным объектом; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			//Сравнение по значению, как у оператора ==. Прежде требовалось совпадение типов, и многочлен Лежандра
			//LegendrePolynomial(1) был равен Polynomial.Up по оператору ==, но не по Equals.
			var other = obj as Polynomial;
			return !ReferenceEquals(other, null) && this == other;
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		public override int GetHashCode()
		{
			//Прежде хэш-код всегда был равен 0, и хэш-таблицы многочленов вырождались в списки.
			//Хэш-код согласован с оператором ==: старшие нулевые коэффициенты не учитываются, -0 и 0 (как и все NaN) неразличимы.
			unchecked
			{
				var hashCode = 0;
				for (var i = 0; i <= Degree; i++)
				{
					hashCode = (hashCode * 397) ^ GetCoefficientHashCode(A[i]);
				}

				return hashCode;
			}
		}

		/// <summary>
		/// Возвращает хэш-код коэффициента, одинаковый для равных по <see cref="double.Equals(double)"/> значений.
		/// </summary>
		/// <param name="value">Коэффициент.</param>
		/// <returns>Хэш-код коэффициента.</returns>
		private static int GetCoefficientHashCode(double value)
		{
			if (value.Equals(0D))
			{
				return 0;
			}

			if (double.IsNaN(value))
			{
				return int.MinValue;
			}

			//Биты перемешиваются финализатором MurmurHash3: у небольших целых и коротких десятичных чисел младшие биты
			//двоичного представления нулевые, и без перемешивания хэш-коды многочленов часто совпадают.
			unchecked
			{
				var bits = (ulong)BitConverter.DoubleToInt64Bits(value);
				bits ^= bits >> 33;
				bits *= 0xFF51AFD7ED558CCDUL;
				bits ^= bits >> 33;
				bits *= 0xC4CEB9FE1A85EC53UL;
				bits ^= bits >> 33;
				return (int)bits ^ (int)(bits >> 32);
			}
		}

		#endregion

		#region  IEnumerable items

		/// <summary>
		/// Возвращает перечислитель, выполняющий перебор коэффициентов членов.
		/// </summary>
		/// <returns>
		/// Интерфейс, который может использоваться для перебора коэффициентов членов.
		/// </returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<double> GetEnumerator()
		{
			return ((IEnumerable<double>)A).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return A.GetEnumerator();
		}

		#endregion

		/// <summary>
		/// Возвращает строковое представление исходного многочлена.
		/// </summary>
		/// <returns>Строковое представление исходного многочлена.</returns>
		/// <remarks>
		/// <code>
		/// var pol = new Polynomial(0.5, -3, 4, -System.Math.PI);
		/// Console.Write(pol); //Результат: -π x³ + 4 x² - 3 x + 1/2
		/// Console.Write(pol/3); //Результат: -π/3 x³ + 4/3 x² - x + 1/6
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			return ToString("");
		}

		/// <summary>
		/// Возвращает строковое представление исходного многочлена.
		/// </summary>
		/// <param name="format">Сведения об особенностях форматирования.</param>
		/// <returns>Строковое представление исходного многочлена.</returns>
		public string ToString(string format)
		{
			format = string.IsNullOrEmpty(format) ? "x" : format;

			//Ключ кэша строится по коэффициентам: прежде он строился по хэш-коду массива, и многочлены
			//с совпавшими хэш-кодами получали чужое строковое представление.
			var key = CStatic.GetToStringHashKey(nameof(Polynomial), format, A);

			var result = CStatic.GetToStringHashValue(key);
			if (result != null) return result;
			result = "";

			string part = null;
			var count = 0;

			//for (var i = 0; i < A.Length; i++)
			for (var i = A.Length - 1; i >= 0; i--)
			{
				var coef = A[i];
				if (double.IsNaN(coef))
				{
					result = "NaN";
					break;
				}

				var sym = i == 0 ? "" : i == 1 ? format : format + CStatic.GetIndex(i, true);

				if (CStatic.AddLinearItem(ref result, coef, sym))
				{
					count++;

					if (count == 21)
					{
						//Запоминаем часть
						part = result;
					}

					if (count == 25)
					{
						//Досрочное завершение
						result = part + " ...";
						break;
					}
				}
			}

			result = result == "" ? "0" : result;

			CStatic.AddToStringHashValue(key, result);
			return result;
		}

		#endregion

		/// <summary>
		/// Получает старший коэффициент.
		/// </summary>
		public double Major => this[Degree];

		/// <summary>
		/// Получает строковое представление разложения многочлена с выделением старшего коэффициента и если метод <see cref="Resolve(bool)"/> находит корни, то расскладывает на линейные множители.
		/// </summary>
		public string ResolveString
		{
			get
			{
				var result = "";

				var rem = this / Major;

				//Метод Resolve решает уравнения любой степени, поэтому прежний перехват NotImplementedException не нужен.
				foreach (var x in Resolve())
				{
					var part = GetPolynomialByRoots(x);
					rem /= part;
					result += "(" + part + ")" + CStatic.NarrowNbSp;
				}

				if (rem.Degree > 0)
				{
					result += "(" + rem + ")";
				}

				if (Major.Equals(-1D))
				{
					result = "-" + result;
				}
				else if (!Major.Equals(1D))
				{
					result = CStatic.DoubleToString(Major) + CStatic.NarrowNbSp + result.Trim();
				}

				return result;
			}
		}


		#region IConvertible members

		/// <summary>
		/// Возвращает <see cref="T:System.TypeCode"/> для этого экземпляра.
		/// </summary>
		/// <returns>
		/// Перечислимая константа, которая является <see cref="T:System.TypeCode"/> данного класса или типа значения, реализующего этот интерфейс.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему логическое значение с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Логическое значение, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public bool ToBoolean(IFormatProvider provider)
		{
			return !IsEmpty(this);
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентный символ Юникода с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Символ Юникода, эквивалентный значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public char ToChar(IFormatProvider provider)
		{
			return Convert.ToChar(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 8-битовое целое число со знаком с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 8-битовое целое число со знаком, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public sbyte ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 8-битовое целое число без знака с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 8-битовое целое число без знака, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public byte ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 16-битовое целое число со знаком с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 16-битовое целое число со знаком, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public short ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 16-битовое целое число без знака с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 16-битовое целое число без знака, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public ushort ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 32-битовое целое число со знаком с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 32-битовое целое число со знаком, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public int ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 32-битовое целое число без знака с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 32-битовое целое число без знака, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public uint ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 64-битовое целое число со знаком с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 64-битовое целое число со знаком, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public long ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное ему 64-битовое целое число без знака с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// 64-битовое целое число без знака, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public ulong ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное число одинарной точности с плавающей запятой с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Число одинарной точности с плавающей запятой, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public float ToSingle(IFormatProvider provider)
		{
			return Convert.ToSingle(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное число двойной точности с плавающей запятой с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Число двойной точности с плавающей запятой, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public double ToDouble(IFormatProvider provider)
		{
			if (Degree == 0)
			{
				return A[0];
			}

			throw new InvalidCastException("Степень многочлена больше 0");
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентное число типа <see cref="T:System.Decimal"/> с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Число типа <see cref="T:System.Decimal"/>, эквивалентное значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public decimal ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентную строку <see cref="T:System.DateTime"/> с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Экземпляр <see cref="T:System.DateTime"/>, эквивалентный значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public DateTime ToDateTime(IFormatProvider provider)
		{
			return Convert.ToDateTime(ToDouble(provider));
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в эквивалентную строку <see cref="T:System.String"/> с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Экземпляр <see cref="T:System.String"/>, эквивалентный значению данного экземпляра.
		/// </returns>
		/// <param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public string ToString(IFormatProvider provider)
		{
			return ToString();
		}

		/// <summary>
		/// Преобразует значение этого экземпляра в объект <see cref="T:System.Object"/> указанного типа <see cref="T:System.Type"/>, имеющий эквивалентное значение, с использованием указанных сведений об особенностях форматирования, связанных с языком и региональными параметрами.
		/// </summary>
		/// <returns>
		/// Экземпляр <see cref="T:System.Object"/> типа <paramref name="conversionType"/>, значение которого эквивалентно значению данного экземпляра.
		/// </returns>
		/// <param name="conversionType"><see cref="T:System.Type"/>, в который преобразуется значение данного экземпляра. </param><param name="provider">Реализация интерфейса <see cref="T:System.IFormatProvider"/>, предоставляющая сведения об особенностях форматирования, связанных с языком и региональными параметрами. </param><filterpriority>2</filterpriority>
		public object ToType(Type conversionType, IFormatProvider provider)
		{
			//Прежде преобразование в строку выбрасывало InvalidCastException для многочлена ненулевой степени
			//(хотя Convert.ToString работал), а преобразование в фактический тип экземпляра, например в LegendrePolynomial,
			//пыталось получить число.
			if (conversionType == typeof(string))
			{
				return ToString(provider);
			}

			if (conversionType != null && conversionType.IsInstanceOfType(this))
			{
				return this;
			}

			// ReSharper disable once AssignNullToNotNullAttribute
			return Convert.ChangeType(ToDouble(provider), conversionType);
		}

		#endregion
	}
}