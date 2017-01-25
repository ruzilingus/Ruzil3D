using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		public static Polynomial GetPolynomial(PointD point0, PointD point1)
		{
			var k = (point1.Y - point0.Y) / (point1.X - point0.X);
			return new Polynomial(point0.Y - point0.X * k, k);
		}

		/// <summary>
		/// Строит интерполяционный полином минимальной степени, проходящий через заданные точки <paramref name="points"/>.
		/// </summary>
		/// <param name="points">Массив точек представляющий значения полинома в узлах.</param>
		/// <returns>Интерполяционный полином.</returns>
		public static Polynomial GetPolynomial(params PointD[] points)
		{
			//В форме Лагранжа

			var result = Empty;
			for (var i = 0; i < points.Length; i++)
			{
				var point = points[i];
				var y = point.Y;

				Polynomial l = null;
				for (var j = 0; j < points.Length; j++)
				{
					if (j == i) continue;

					var p = points[j];
					var d = point.X - p.X;

					if (l == null)
					{
						l = new Polynomial(-p.X * y / d, y / d);
					}
					else
					{
						l *= new Polynomial(-p.X / d, 1D / d);
					}
				}

				result += l;
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
		private static Polynomial[][] _bernsteins;

		/// <summary>
		/// Вычисляет и возвращает базисный полином Бернштейна.
		/// </summary>
		/// <param name="k"></param>
		/// <param name="n"></param>
		/// <returns></returns>
		private static Polynomial CalculateBernstein(int k, int n)
		{
			var a = 1;
			var b = 1;

			var p1 = Identity;
			for (var i = 0; i < k; i++)
			{
				a *= n - i;
				b *= i + 1;
				p1 *= Up;
			}

			var p2 = Identity;
			for (var i = k; i < n; i++)
			{
				p2 *= Dn;
			}

			// ReSharper disable once PossibleLossOfFraction
			return p1 * p2 * (a / b);
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

			if (_bernsteins == null)
			{
				_bernsteins = new Polynomial[BernsteinsCashSize][];
			}

			if (_bernsteins[n] == null)
			{
				_bernsteins[n] = new Polynomial[n + 1];
			}

			return _bernsteins[n][k] ?? (_bernsteins[n][k] = CalculateBernstein(k, n));
		}

		#endregion

		#region Resolve methods

		private static Complex[] ResolveSquare(Complex a, Complex b, Complex c, bool multiple = true)
		{
			//Приведение к каноническому виду
			var k = b / 2D;

			//Дискриминант
			var discr = k * k - a * c;

			if (multiple || discr != Complex.Empty)
			{
				var discrSqrt = discr.Pow(0.5D);
				return a.R > 0
					? new[] {(-k - discrSqrt)/a, (-k + discrSqrt)/a}
					: new[] {(-k + discrSqrt)/a, (-k - discrSqrt)/a};
			}
			else
			{
				return new[] { -k / a };
			}
		}

		private static double[] ResolveSquareReal(double a, double b, double c, bool multiple = true)
		{
			//Приведение к каноническому виду
			var k = b / 2D;

			//Дискриминант
			var discr = k * k - a * c;

			if (discr < 0)
			{
				return EmptyArray;
			}

			if (multiple || !discr.Equals(0D))
			{
				var discrSqrt = Sqrt(discr);
				return a > 0 ? new[] { (-k - discrSqrt) / a, (-k + discrSqrt) / a } : new[] { (-k + discrSqrt) / a, (-k - discrSqrt) / a };
			}
			else
			{
				return new[] { -k / a };
			}
		}

		private static Complex[] ResolveCubic(Complex a, Complex b, Complex c, Complex d)
		{
			var p = (3 * a * c - b * b) / (3 * a * a);
			var q = (2 * b * b * b - 9 * a * b * c + 27 * a * a * d) / (27 * a * a * a);

			var result2 = ResolveSquare(1, q, -p * p * p / 27);
			var w = result2[0].GetRoots(3);

			var dx = -b / (3 * a);
			return new[]
			{
				w[0] - p/(3*w[0]) + dx,
				w[1] - p/(3*w[1]) + dx,
				w[2] - p/(3*w[2]) + dx
			};
		}

		private static Complex[] ResolveCubic2(Complex a, Complex b, Complex c, Complex d)
		{
			//Метод Рузиля
			var a3 = 3 * a;

			var u = a3 * c - b * b;
			var s = 2 * b * b * b - 9 * a * b * c + 27 * a * a * d;
			var square = (s * s + 4 * u * u * u).Pow(0.5D) / 2 - s / 2;

			var w = square.GetRoots(3);

			return new[]
			{
				(w[0] - u/w[0] - b)/a3,
				(w[1] - u/w[1] - b)/a3,
				(w[2] - u/w[2] - b)/a3
			};
		}

		
		private static Complex[] ResolveCubic2(double a, double b, double c, double d)
		{
			//Метод Рузиля
			var a3 = 3 * a;

			var u = a3 * c - b * b;
			var s = 2 * b * b * b - 9 * a * b * c + 27 * a * a * d;
			var square = ((Complex)(s * s + 4 * u * u * u)).Pow(0.5D) / 2 - s / 2;

			var w = square.GetRoots(3);

			return new[]
			{
				(w[0] - u/w[0] - b)/a3,
				(w[1] - u/w[1] - b)/a3,
				(w[2] - u/w[2] - b)/a3
			};
		}

		/// <summary>
		/// Возвращает массив чисел являющихся корнями кубического уравнения.
		/// </summary>
		/// <param name="a">Коэффициент при x³.</param>
		/// <param name="b">Коэффициент при x².</param>
		/// <param name="c">Коэффициент при x.</param>
		/// <param name="d">Свободный член.</param>
		/// <param name="multiple">Параметр указывающий на необходимость учитывать кратность корней.</param>
		/// <returns>Массив чисел являющихся корнями кубического уравнения.</returns>
		public static double[] ResolveCubicReal(double a, double b, double c, double d, bool multiple = true)
		{
			//Метод Рузиля
			var u = 3D * a * c - b * b;
			var s = 2D * b * b * b - 9D * a * b * c + 27D * a * a * d;

			//Дискриминант Рузиля
			// ReSharper disable once InconsistentNaming
			var qSquare = 4D * u * u * u + s * s;

			var divisor = 3D * a;

			if (qSquare < 0D)
			{
				//Неприводимый случай

				//var sQ = new Complex(0D, Sqrt(-Q));
				//var alpha = ((sQ - s) / 2D).Pow(Third);// ^ Third;

				var cplx = new Complex(-s / 2D, System.Math.Sqrt(-qSquare) / 2D);
				var alpha = cplx.Pow(Third);

				var r = -alpha.R;
				var i = Sqrt3 * alpha.I;

				//Три разных вещественных корня
				return a > 0
					? new[] { (r - i - b) / divisor, (r + i - b) / divisor, (2D * alpha.R - b) / divisor }
					: new[] { (2D * alpha.R - b) / divisor, (r + i - b) / divisor, (r - i - b) / divisor };
			}
			else
			{
				var sQ = System.Math.Sqrt(qSquare);

				var t1 = (-sQ - s) / 2D;
				t1 = t1 >= 0D ? System.Math.Pow(t1, Third) : -System.Math.Pow(-t1, Third);

				if (!0D.Equals(qSquare))
				{
					var t2 = (sQ - s) / 2D;
					t2 = t2 >= 0D ? System.Math.Pow(t2, Third) : -System.Math.Pow(-t2, Third);

					//Один вещественный и два комплексных сопряженных корня
					return new[] { (t1 + t2 - b) / divisor };
				}

				var x = (-b - t1) / divisor;

				if (s.Equals(9D * a * u))
				{
					//Один трехкратный корень
					return multiple ? new[] { x, x, x } : new[] { x };
				}

				//Один двухкратный и один однократный корень
				if (t1 * a > 0D)
				{
					return multiple ? new[] { x, x, (2D * t1 - b) / divisor } : new[] { x, (2D * t1 - b) / divisor };
				}

				//Другой порядок тех же корней
				return multiple ? new[] { (2D * t1 - b) / divisor, x, x } : new[] { (2D * t1 - b) / divisor, x };
			}
		}

		private static double[] ResolveCubicR(double a, double b, double c, double d, bool multiple = true)
		{
			//Замена переменных
			var dx = -b / (3D * a);

			//Приведение к каноническому виду
			var p = (3 * a * c - b * b) / (3 * a * a);
			var q = (2 * b * b * b - 9 * a * b * c + 27 * a * a * d) / (27 * a * a * a);

			//Дискриминант
			// ReSharper disable once InconsistentNaming
			var Q = p * p * p / 27D + q * q / 4D;

			if (Q < 0)
			{
				//Неприводимый случай
				var sQ = new Complex(0, Sqrt(-Q));
				var alpha = (sQ - q / 2D).Pow(Third); // ^ Third;

				var r = -alpha.R;
				var i = Sqrt3 * alpha.I;

				//Три разных вещественных корня
				return new[] { r - i + dx, r + i + dx, 2 * alpha.R + dx };
			}
			else
			{
				var sQ = Sqrt(Q);

				var t1 = -sQ - q / 2D;
				t1 = t1 >= 0 ? System.Math.Pow(t1, Third) : -System.Math.Pow(-t1, Third);


				if (!Q.Equals(0D))
				{
					var t2 = sQ - q / 2D;
					t2 = t2 >= 0 ? System.Math.Pow(t2, Third) : -System.Math.Pow(-t2, Third);

					//Один вещественный и два комплексных сопряженных корня
					return new[] { t1 + t2 + dx };
				}

				if (p.Equals(q))
				{
					//Один трехкратный корень
					var x = dx - t1;

					if (multiple)
					{
						return new[] { x, x, x };
					}

					return new[] { x };
				}
					
				//Два вещественных корня. Один из корней двухкратный
				return new[] { 2 * t1 + dx, dx - t1 };
			}
		}

		private static Complex[] ResolveTesseract(Complex a, Complex b, Complex c, Complex d, Complex e)
		{
			var p = (8 * a * c - 3 * b * b) / (8 * a * a);
			var q = (8 * a * a * d + b * b * b - 4 * a * b * c) / (8 * a * a * a);
			var r = (16 * a * b * b * c - 64 * a * a * b * d - 3 * b * b * b * b + 256 * a * a * a * e) / (256 * a * a * a * a);

			var dx = -b / (4 * a);

			if (q.Equals(Complex.Empty))
			{
				var resolve2 = ResolveSquare(1D, p / 2, (p * p - 4 * r) / 16);
				return new[]
				{
					-resolve2[0].Pow(0.5D) - resolve2[1].Pow(0.5D) + dx,
					resolve2[0].Pow(0.5D) - resolve2[1].Pow(0.5D) + dx,
					-resolve2[0].Pow(0.5D) + resolve2[1].Pow(0.5D) + dx,
					resolve2[0].Pow(0.5D) + resolve2[1].Pow(0.5D) + dx,
				};
			}

			var resolve3 = ResolveCubic2(1D, p / 2, (p * p - 4 * r) / 16, -q * q / 64);
			var z1 = resolve3[0].Pow(0.5D);
			var z2 = resolve3[1].Pow(0.5D);
			var z3 = resolve3[2].Pow(0.5D);

			double sign = (z1 * z2 * z3 / q).R < 0 ? 1 : -1;

			return new[]
			{
					sign*(z1 + z2 + z3) + dx,
					sign*(z1 - z2 - z3) + dx,
					sign*(-z1 + z2 - z3) + dx,
					sign*(-z1 - z2 + z3) + dx,
				};
		}

		private static Complex[] ResolveTesseract(double a, double b, double c, double d, double e)
		{
			var p = (8*a*c - 3*b*b)/(8*a*a);
			var q = (8*a*a*d + b*b*b - 4*a*b*c)/(8*a*a*a);
			var r = (16*a*b*b*c - 64*a*a*b*d - 3*b*b*b*b + 256*a*a*a*e)/(256*a*a*a*a);

			var dx = -b/(4*a);

			if (q.Equals(0D))
			{
				var resolve2 = ResolveSquare(1D, p/2, (p*p - 4*r)/16);
				return new[]
				{
					-resolve2[0].Pow(0.5D) - resolve2[1].Pow(0.5D) + dx,
					resolve2[0].Pow(0.5D) - resolve2[1].Pow(0.5D) + dx,
					-resolve2[0].Pow(0.5D) + resolve2[1].Pow(0.5D) + dx,
					resolve2[0].Pow(0.5D) + resolve2[1].Pow(0.5D) + dx,
				};
			}
			
			var resolve3 = ResolveCubic2(1D, p/2, (p*p - 4*r)/16, -q*q/64);
			var z1 = resolve3[0].Pow(0.5D);
			var z2 = resolve3[1].Pow(0.5D);
			var z3 = resolve3[2].Pow(0.5D);
			double sign = (z1*z2*z3/q).R < 0 ? 1 : -1;
			
			/*
			var resolve3 = ResolveCubicReal(1D, p / 2, (p * p - 4 * r) / 16, -q * q / 64);
			var z1 = System.Math.Sqrt(resolve3[0]);
			var z2 = System.Math.Sqrt(resolve3[1]);
			var z3 = System.Math.Sqrt(resolve3[2]);
			double sign = z1 * z2 * z3 / q < 0 ? 1 : -1;
			*/

			return new []
			{
				sign*(z1 + z2 + z3) + dx,
				sign*(z1 - z2 - z3) + dx,
				sign*(-z1 + z2 - z3) + dx,
				sign*(-z1 - z2 + z3) + dx,
			};
		}
		
		private static double[] ResolveTesseractR(double a, double b, double c, double d, double e, bool multiple = true)
		{
			var resolve = ResolveTesseract(a, b, c, d, e);
			
			var eps = 1E-12;
			var result = new List<double>();

			if (System.Math.Abs(resolve[0].I) < eps) result.Add(resolve[0].R);
			if (System.Math.Abs(resolve[1].I) < eps) result.Add(resolve[1].R);
			if (System.Math.Abs(resolve[2].I) < eps) result.Add(resolve[2].R);
			if (System.Math.Abs(resolve[3].I) < eps) result.Add(resolve[3].R);

			if (result.Count < 1)
			{
				return EmptyArray;
			}

			result.Sort();
			return result.ToArray();

			//throw new NotImplementedException("Решение для уравнений " + 4 + " степени не реализовано");
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
		/// Вычисляет частное двух многочленов и возвращает остаток.
		/// </summary>
		/// <param name="x">Значение содержащее делимое.</param>
		/// <param name="y">Значение содержащее делитель.</param>
		/// <param name="rem">Значение, представляющее полученный остаток.</param>
		/// <returns> Значение, содержащее частное указанных многочленов.</returns>
		/// <exception cref="DivideByZeroException">Значение параметра <paramref name="y"/> равно нулю</exception>
		[Obsolete]
		private static Polynomial DivRem_OBSOLETE(Polynomial x, Polynomial y, out Polynomial rem)
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
			var yCoef = y.A[yDeg];

			//todo Метод можно ускорить.

			var result = Empty;

			rem = x;
			while (xDeg >= yDeg)
			{
				var div = GetPolynomial(xDeg - yDeg) * rem.A[xDeg] / yCoef;
				result += div;
				rem -= div * y;
				xDeg--;
			}

			return result;
		}

		/// <summary>
		/// Вычисляет частное двух многочленов и возвращает остаток.
		/// </summary>
		/// <param name="x">Значение содержащее делимое.</param>
		/// <param name="y">Значение содержащее делитель.</param>
		/// <param name="rem">Значение, представляющее полученный остаток.</param>
		/// <returns> Значение, содержащее частное указанных многочленов.</returns>
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

		private static Polynomial Div(ICollection<double> x, IList<double> y, int xDeg, int yDeg, out double[] rem)
		{
			rem = new double[xDeg + 1];
			x.CopyTo(rem,0);

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
		/// Метод Ньютона для нахождения корней многочлена.
		/// </summary>
		/// <param name="polynom">Исходный многочлен.</param>
		/// <param name="a">Начало области монотонности.</param>
		/// <param name="b">Конец области монотонности.</param>
		/// <param name="derivative">Производная многочлена.</param>
		/// <returns>Возвращает корень многочлена, на отрезке [<paramref name="a"/>, <paramref name="b"/>] если существует. <see cref="double.NaN"/> - в противном случае.</returns>
		private static double FindRoot(Polynomial polynom, double a, double b, Polynomial derivative = null)
		{
			//Проверка начальных условий
			if (a > b) return double.NaN;
			var aValue = polynom.GetValue(a);
			if (aValue.Equals(0D)) return a;
			if (a.Equals(b)) return double.NaN;
			var bValue = polynom.GetValue(b);
			if (bValue.Equals(0D)) return b;
			if (aValue * bValue > 0) return double.NaN;

			/*
			const double eps = 1E-12;
			if (aValue > eps && bValue > eps || aValue < -eps && bValue < -eps)
			{
				return double.NaN;
			}
			*/

			double x;

			if (a.Equals(double.NegativeInfinity))
			{
				x = System.Math.Min(1, b) - 1;
			}
			else if (b.Equals(double.PositiveInfinity))
			{
				x = System.Math.Max(-1, a) + 1;
			}
			else
			{
				x = (a + b)/2D;
			}

			var y = polynom.GetValue(x);
			var abs = System.Math.Abs(y);

			derivative = derivative ?? polynom.GetDerivative();
			
			do
			{
				var xNext = x - y / derivative.GetValue(x);
				var yNext = polynom.GetValue(xNext);
				var absNext = System.Math.Abs(yNext);

				if (absNext >= abs)
				{
					return x;
				}

				x = xNext;
				y = yNext;
				abs = absNext;
			}
			while (true);
		}



		/// <summary>
		/// Возвращает вещественные корни многочлена в массиве структур <see cref="double"/>.
		/// </summary>
		/// <param name="multiple">Параметр указывающий на необходимость учитывать кратность корней.</param>
		/// <returns>Корни многочлена</returns>
		/// <exception cref="NotImplementedException">Решение для уравнений соответствующей степени не реализовано.</exception>
		public double[] Resolve(bool multiple = true)
		{
			switch (Degree)
			{
				case 0:
					return EmptyArray;

				case 1:
					return new[] {-A[0]/A[1]};

				case 2:
					return ResolveSquareReal(A[2], A[1], A[0], multiple);

				case 3:
					return ResolveCubicReal(A[3], A[2], A[1], A[0], multiple);

				case 4:
					//todo Метод нужно протестировать
					return ResolveTesseractR(A[4], A[3], A[2], A[1], A[0], multiple);
			}
			
			//Находим критические точки и точки перегиба функции.
			var derivative1 = GetDerivative();
			var derivative2 = derivative1.GetDerivative();
			var roots1 = derivative1.Resolve();
			var roots2 = derivative2.Resolve();

			var list = new List<double> { double.NegativeInfinity, double.PositiveInfinity };
			list.AddRange(roots1);
			list.AddRange(roots2);
			list.Sort();

			var result = new List<double>();

			for (var i = 1; i < list.Count; i++)
			{
				var a = list[i - 1];
				var b = list[i];

				//Ищем решение уравнения методом Ньютона между критическими точками и точками перегиба.
				var root = FindRoot(this, a, b, derivative1);
				
				if (double.IsNaN(root))
				{
					continue;
				}

				result.Add(root);


				/*
				double[] rem;
				var pol = Div(A, new[] { -root, 1 }, Degree, 1, out rem);
				result.AddRange(pol.Resolve());
				result.Sort();
				break;
				*/

				if (root.Equals(b))
				{
					if (multiple && roots1.Contains(b))
					{
						result.Add(root);
					}

					i++;
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// Возвращает значение многочлена от числа двойной точности с плавающей запятой.
		/// </summary>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Значение многочлена.</returns>
		private double GetValue_OBSOLETE(double x)
		{
			if (double.IsNaN(x))
			{
				return double.NaN;
			}

			var result = 0D;
			var powX = 1D;
			foreach (var coef in A)
			{
				result += powX * coef;
				powX *= x;
			}

			if (double.IsNaN(result))
			{
				//if (Degree)


				//return x*Major > 0 ? double.PositiveInfinity : double.NegativeInfinity;
			}


			return result;
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
		/// Возвращает площадь фигуры ограниченой графиком исходного многочлена.
		/// </summary>
		/// <param name="x0">Начальная точка.</param>
		/// <param name="x1">Конечная точка.</param>
		/// <returns>Площадь фигуры ограниченой графиком исходного многочлена.</returns>
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
		/// <returns>Многочлен производной.</returns>
		public Polynomial GetDerivative(int order = 1)
		{
			if (A.Length == order)
			{
				return Empty;
			}

			if (order < 0)
			{
				throw new ArgumentException("Порядок производной не может быть отрицательным.", nameof(order));
			}

			if (order == 0)
			{
				//return new Polynomial { A = A.Clone() as double[] };
				return this;
			}

			var result = new double[A.Length - order];

			for (var i = order; i < A.Length; i++)
			{
				var coef = i;
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

		/*
        private static Polynomial Multiply(Polynomial p0, Polynomial p1, Polynomial p2)
        {
            var deg = p0.A.Length + p1.A.Length + p2.A.Length - 3;

            var result = new double[deg + 1];

            for (var i = 0; i < p0.A.Length; i++)
                for (var j = 0; j < p1.A.Length; j++)
                    for (var k = 0; k < p2.A.Length; k++)
                    {
                        result[i + j + k] += p0.A[i] * p1.A[j] * p2.A[k];
                    }

            return new Polynomial { A = result };
        }


        private static Polynomial Square(Polynomial polynomial)
        {
            var deg = polynomial.A.Length*2 - 2;

            var result = new double[deg + 1];

            for (var i = 0; i < polynomial.A.Length; i++)
                for (var j = 0; j < polynomial.A.Length; j++)
                    {
                        result[i + j] += polynomial.A[i] * polynomial.A[j];
                    }

            return new Polynomial { A = result };
        }
        */

		private static double[] Multiply(double[] x, double[] y)
		{
			var deg = x.Length + y.Length - 2;

			var result = new double[deg + 1];
			for (var i = 0; i < x.Length; i++)
				for (var j = 0; j < y.Length; j++)
				{
					result[i + j] += x[i] * y[j];
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

			if (ReferenceEquals(this, Identity)) return Identity;
			if (ReferenceEquals(this, Empty)) return Empty;

			switch (y)
			{
				case 0:
					return Identity;

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


					/*
			    var pow = A;
			    double[] result = null;
			    while (true)
			    {
				if ((y & 1) != 0)
				{
				    result = result == null ? pow : Multiply(result, pow);
				}

				y >>= 1;
				if (y == 0) break;
				pow = Multiply(pow, pow);
			    }
			    return new Polynomial { A = result };
			    */



			}

			/*
			*/

		}

		#region Limit

		//Не реализовано
		private static double GetLimitValue(Polynomial numerator, Polynomial denominator, double limit,
			ELimitSide side = ELimitSide.Both)
		{
			//Если числитель нулевой
			if (numerator == Empty)
			{
				//Либо нет придела либо 0
				return denominator == Empty ? double.NaN : 0;
			}

			//Если придел на бесконечности
			if (double.IsInfinity(limit))
			{
				var numDeg = numerator.Degree;
				var denDeg = denominator.Degree;

				if (numDeg > denDeg)
				{
					return System.Math.Pow(limit, numDeg - denDeg) * numerator[numDeg] / denominator[denDeg];
				}

				if (numDeg == denDeg)
				{
					return numerator[numDeg] / denominator[denDeg];
				}

				return 0;
			}

			var pol = new Polynomial(limit, 1);
			numerator = numerator.Substitution(pol);
			denominator = denominator.Substitution(pol);




			var numValue = numerator.GetValue(limit);
			var denValue = denominator.GetValue(limit);

			if (!denValue.Equals(0D))
			{
				return numValue / denValue;
			}

			if (IsEmpty(denominator))
			{

			}

			//todo Не реализовано
			return 111111111111;

			var numCounter = 0;

			while (!IsEmpty(numerator) && !IsEmpty(denominator) && numerator.GetValue(limit).Equals(0) &&
			       denominator.GetValue(limit).Equals(0))
			{
				numerator /= pol;
				denominator /= pol;
			}


			do
			{

				if (!denValue.Equals(0))
				{
					return numValue / denValue;
				}

				if (!numValue.Equals(0))
				{

				}


				//Polynomial.DivRem(numerator, pol);

				if (!numValue.Equals(0))
				{
					//Плюс или минус бесконечность (неопределенность)
					return double.NaN;
				}

				numerator /= pol;
				denominator /= pol;
			} while (true);
		}

		#endregion

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
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Polynomial"/> и представляет одинаковые значения с исходным объектом; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Polynomial)obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		public override int GetHashCode()
		{
			return 0;
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
		/// Возвращает строковое представлеине исходного многочлена.
		/// </summary>
		/// <returns>Строковое представлеине исходного многочлена.</returns>
		/// <remarks>
		/// <code>
		/// var pol = new Polynomial(0.5, -3, 4, -System.Math.PI);
		/// Console.Write(pol); //Результат: 1/2 - 3 x + 4 x² - π x³
		/// Console.Write(pol/3); //Результат: 1/6 - x + 4/3 x² - π/3 x³
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			return ToString("");
		}

		/// <summary>
		/// Возвращает строковое представлеине исходного многочлена.
		/// </summary>
		/// <param name="format">Сведения об особенностях форматирования.</param>
		/// <returns>Строковое представлеине исходного многочлена.</returns>
		public string ToString(string format)
		{
			format = string.IsNullOrEmpty(format) ? "x" : format;
			var key = A.GetHashCode() + "_" + format;

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
		/// Получает строковое представление разложения многочлена с выделением старшего коэффициента и если метод <see cref="Resolve(bool)"/> находить корни, то расскладывает на линейные множители.
		/// </summary>
		/// <exception cref="NotImplementedException">Решение многочленов заданной степени не реализовано.</exception>
		public string ResolveString
		{
			get
			{
				var result = "";

				var rem = this / Major;
				try
				{
					var resolve = Resolve();
					foreach (var x in resolve)
					{
						var part = GetPolynomialByRoots(x);
						rem /= part;
						result += "(" + part + ")" + CStatic.NarrowNbSp;
					}
				}
				catch (NotImplementedException)
				{
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
			if (conversionType == typeof(Polynomial))
			{
				return this;
			}

			// ReSharper disable once AssignNullToNotNullAttribute
			return Convert.ChangeType(ToDouble(provider), conversionType);
		}

		#endregion
	}
}