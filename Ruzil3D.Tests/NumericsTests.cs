using System;
using System.Collections.Generic;
using System.Linq;
using Ruzil3D.Algebra;
using Ruzil3D.Approximation;
using Ruzil3D.Calculus;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Тесты численных методов: обращение матриц, системы линейных уравнений, поиск корня, дифференцирование,
	/// интегрирование, интерполяция, а также равенство, хэш-коды и значения по умолчанию векторов, матриц и уравнений.
	/// </summary>
	public class NumericsTests
	{
		#region Helpers

		private static Matrix Rows(params double[][] rows)
		{
			var lines = new Vector[rows.Length];
			for (var i = 0; i < rows.Length; i++)
			{
				lines[i] = new Vector(rows[i]);
			}

			return new Matrix(lines);
		}

		/// <summary>
		/// Проверяет, что число совпадает с ожидаемым с заданной относительной точностью.
		/// </summary>
		private static void Close(double expected, double actual, double relativeTolerance = 1e-15)
		{
			var tolerance = relativeTolerance*System.Math.Max(1e-300, System.Math.Abs(expected));
			Assert.True(System.Math.Abs(expected - actual) <= tolerance,
				"Ожидалось " + expected.ToString("R") + ", получено " + actual.ToString("R") + ".");
		}

		/// <summary>
		/// Возвращает наибольшее отклонение произведения матриц от единичной матрицы.
		/// </summary>
		private static double IdentityError(Matrix a, Matrix inverse, int size)
		{
			var x = new double[size, size];
			var y = new double[size, size];
			for (var i = 0; i < size; i++)
			{
				for (var j = 0; j < size; j++)
				{
					x[i, j] = a[i, j];
					y[i, j] = inverse[i, j];
				}
			}

			var error = 0D;
			for (var i = 0; i < size; i++)
			{
				for (var j = 0; j < size; j++)
				{
					var sum = 0D;
					for (var k = 0; k < size; k++)
					{
						sum += x[i, k]*y[k, j];
					}

					error = System.Math.Max(error, System.Math.Abs(sum - (i == j ? 1 : 0)));
				}
			}

			return error;
		}

		private static Matrix RandomMatrix(int size, int seed)
		{
			var random = new Random(seed);
			var lines = new Vector[size];
			for (var i = 0; i < size; i++)
			{
				var row = new double[size];
				for (var j = 0; j < size; j++)
				{
					row[j] = random.NextDouble() - 0.5 + (i == j ? size : 0);
				}

				lines[i] = new Vector(row);
			}

			return new Matrix(lines);
		}

		#endregion

		#region Matrix.GetInverse

		[Fact]
		public void Matrix_Inverse_UsesPartialPivoting()
		{
			// Прежде ведущим брался первый ненулевой элемент столбца: получалось [(0, 1); (1, −1e-20)], а A·A⁻¹ = [(1, 0); (1, 1)].
			var a = Rows(new[] {1e-20, 1}, new[] {1D, 1});
			var inverse = a.GetInverse();

			Close(-1, inverse[0, 0]);
			Close(1, inverse[0, 1]);
			Close(1, inverse[1, 0]);
			Close(-1e-20, inverse[1, 1]);
			Assert.True(IdentityError(a, inverse, 2) <= 1e-15);

			// Для обусловленной матрицы результат прежний.
			var regular = Rows(new[] {4D, 7}, new[] {2D, 6}).GetInverse();
			Close(0.6, regular[0, 0], 1e-12);
			Close(-0.7, regular[0, 1], 1e-12);
			Close(-0.2, regular[1, 0], 1e-12);
			Close(0.4, regular[1, 1], 1e-12);

			// Недостающие элементы строк считаются нулевыми, как и прежде: [[2, 0], [1, 4]].
			var jagged = Rows(new[] {2D}, new[] {1D, 4}).GetInverse();
			Close(0.5, jagged[0, 0]);
			Assert.Equal(0, jagged[0, 1]);
			Close(-0.125, jagged[1, 0]);
			Close(0.25, jagged[1, 1]);

			// Плохо масштабированная, но невырожденная матрица по-прежнему обращается.
			var scaled = Rows(new[] {1e-300, 1}, new[] {0D, 1}).GetInverse();
			Close(1e300, scaled[0, 0]);
			Close(-1e300, scaled[0, 1]);
			Close(1, scaled[1, 1]);
		}

		[Fact]
		public void Matrix_Inverse_SmallPivot_KeepsAccuracy()
		{
			// Прежде деление на ведущий элемент 1e-8 теряло около 8 знаков: элемент [0, 0] получался равным −1.
			var inverse = Rows(new[] {1e-8, 1}, new[] {1D, 1}).GetInverse();
			const double det = 1e-8 - 1;

			Close(1/det, inverse[0, 0]);
			Close(-1/det, inverse[0, 1]);
			Close(-1/det, inverse[1, 0]);
			Close(1e-8/det, inverse[1, 1]);
		}

		[Fact]
		public void Matrix_Inverse_Singular_Throws()
		{
			// Исключение выбрасывается в тех же случаях, что и прежде: вырожденность определяется исключением без перестановок.
			var error = Assert.Throws<DivideByZeroException>(() => Rows(new[] {1D, 2}, new[] {2D, 4}).GetInverse());
			Assert.Equal("Матрица вырожденная", error.Message);

			Assert.Throws<DivideByZeroException>(() => Rows(new[] {0.5, 1.5}, new[] {0.25, 0.75}).GetInverse());
			Assert.Throws<DivideByZeroException>(() => Rows(new[] {1D, 2, 3}, new[] {2D, 4, 6}, new[] {1D, 0, 1}).GetInverse());

			// С выбором главного элемента ведущий элемент этой матрицы из-за округления получается порядка 1e-16, а не нулём.
			Assert.Throws<DivideByZeroException>(() => Rows(new[] {1D, 2, 3}, new[] {4D, 5, 6}, new[] {7D, 8, 9}).GetInverse());
		}

		[Fact]
		public void Matrix_Inverse_NearlySingular_ReturnsResult()
		{
			// Как и прежде, почти вырожденная матрица обращается без исключения. В двоичном представлении [[0.1, 0.3], [0.7, 2.1]]
			// невырожденная, и элементы её обратной порядка 1e16. Порог «численной вырожденности» здесь не используется: он отвергал
			// бы и плохо обусловленные системы с полезным решением, которые прежде решались.
			var inverse = Rows(new[] {0.1, 0.3}, new[] {0.7, 2.1}).GetInverse();
			Assert.True(System.Math.Abs(inverse[0, 0]) > 1e15);

			var a = Rows(new[] {1D, 1}, new[] {1D, 1 + 4.440892098500626E-16});
			var identityError = IdentityError(a, a.GetInverse(), 2);
			Assert.True(identityError <= 1e-15, identityError.ToString("R"));
		}

		[Fact]
		public void Matrix_Inverse_ZeroPivotOnlyWithPivoting_ReturnsPreviousResult()
		{
			// При выборе главного элемента у этой матрицы получается точный ноль (x = fl(fl(1/3)·7)), а при исключении
			// без перестановок — нет. Как и прежде, исключение не выбрасывается и возвращается прежний результат.
			var x = (1D/3)*7;
			var inverse = Rows(new[] {1D, x}, new[] {3D, 7}).GetInverse();
			Assert.Equal(BitConverter.Int64BitsToDouble(0x433C000000000000), inverse[0, 0]);
			Assert.Equal(BitConverter.Int64BitsToDouble(unchecked((long)0xC322AAAAAAAAAAAA)), inverse[0, 1]);
			Assert.Equal(BitConverter.Int64BitsToDouble(unchecked((long)0xC328000000000000)), inverse[1, 0]);
			Assert.Equal(BitConverter.Int64BitsToDouble(0x4310000000000000), inverse[1, 1]);

			var solution = LinearSystem.Resolve(new[] {new Linear(new[] {1, x}, 1), new Linear(new[] {3D, 7}, 2)});
			Assert.Equal(new[] {2627099782632790D, -1125899906842624D}, solution);
		}

		[Fact]
		public void Matrix_Inverse_DenseRandomMatrices()
		{
			// Хорошо обусловленные плотные матрицы без диагонального преобладания обращаются при любом размере
			// (промежуточная версия ошибочно считала вырожденными матрицы порядка 100 и больше).
			var random = new Random(7);
			foreach (var size in new[] {10, 100, 150, 200})
			{
				var lines = new Vector[size];
				for (var i = 0; i < size; i++)
				{
					var row = new double[size];
					for (var j = 0; j < size; j++)
					{
						row[j] = random.NextDouble() - 0.5;
					}

					lines[i] = new Vector(row);
				}

				var a = new Matrix(lines);
				var error = IdentityError(a, a.GetInverse(), size);
				Assert.True(error <= 1e-10, size + ": " + error.ToString("R"));
			}
		}

		[Fact]
		public void Matrix_Inverse_LargeMatrix_AllocatesLittle()
		{
			// Прежде каждое действие над строками создавало новые векторы: обращение матрицы 200×200 выделяло 155 МБ.
			const int size = 200;
			var a = RandomMatrix(size, 1);

			var before = GC.GetAllocatedBytesForCurrentThread();
			var inverse = a.GetInverse();
			var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

			Assert.True(allocated < 10000000, "Выделено " + allocated + " байт.");
			Assert.True(IdentityError(a, inverse, size) <= 1e-13);
		}

		#endregion

		#region LinearSystem.Resolve

		[Fact]
		public void LinearSystem_UsesPartialPivoting()
		{
			// Прежде выбора главного элемента не было: [[1e-20, 1], [1, 1]]·x = [1, 2] давало [0, 1] вместо ≈ [1, 1].
			var x = LinearSystem.Resolve(new[] {new Linear(new[] {1e-20, 1}, 1), new Linear(new[] {1D, 1}, 2)});

			Close(1, x[0]);
			Close(1, x[1]);

			// Прежде получалось [0, 2.0000000222, −2.2e-8].
			var y = LinearSystem.Resolve(new[]
			{
				new Linear(new[] {1e-8, 1, 1}, 2),
				new Linear(new[] {1D, 1, 0}, 2),
				new Linear(new[] {0D, 1, 1}, 2)
			});

			Assert.True(System.Math.Abs(y[0]) <= 1e-15);
			Close(2, y[1]);
			Assert.True(System.Math.Abs(y[2]) <= 1e-15);
		}

		[Fact]
		public void LinearSystem_Singular_ThrowsArithmeticException()
		{
			// Прежде для точно вырожденной системы выбрасывалось исключение общего типа Exception.
			var error = Assert.Throws<ArithmeticException>(() => LinearSystem.Resolve(new[]
			{
				new Linear(new[] {0.5, 1.5}, 1),
				new Linear(new[] {0.25, 0.75}, 2)
			}));
			Assert.Equal("Задача вырожденная.", error.Message);

			Assert.Throws<ArithmeticException>(() => LinearSystem.Resolve(new[]
			{
				new Linear(new[] {1D, 2, 3}, 1),
				new Linear(new[] {2D, 4, 6}, 2),
				new Linear(new[] {1D, 0, 1}, 4)
			}));

			Assert.Throws<ArithmeticException>(() => LinearSystem.Resolve(new[]
			{
				new Linear(new[] {1D, 2, 3}, 1),
				new Linear(new[] {4D, 5, 6}, 2),
				new Linear(new[] {7D, 8, 9}, 4)
			}));

			Assert.Throws<ArithmeticException>(() => LinearSystem.Resolve(new[]
			{
				new Linear(new[] {1D, 2}, 1),
				new Linear(new[] {2D, 4}, 2)
			}));

			// Уравнения разной длины: прежде здесь выбрасывалось IndexOutOfRangeException.
			Assert.Throws<ArithmeticException>(() => LinearSystem.Resolve(new[]
			{
				new Linear(new[] {1D}, 1),
				new Linear(new[] {1D}, 2),
				new Linear(new[] {1D, 1, 1}, 3)
			}));
		}

		[Fact]
		public void LinearSystem_IllConditioned_ReturnsSolution()
		{
			// Как и прежде, плохо обусловленная, но невырожденная система решается без исключения.
			var x = LinearSystem.Resolve(new[]
			{
				new Linear(new[] {1D, 1}, 2),
				new Linear(new[] {1D, 1 + 4.440892098500626E-16}, 2 + 4.440892098500626E-16)
			});
			Assert.Equal(new[] {1D, 1}, x);

			// Матрица Гильберта 10×10 (число обусловленности 1.6e13): прежняя версия давала погрешность 2.8e-4.
			const int size = 10;
			var lines = new Linear[size];
			for (var i = 0; i < size; i++)
			{
				var row = new double[size];
				double sum = 0;
				for (var j = 0; j < size; j++)
				{
					row[j] = 1D/(i + j + 1);
					sum += row[j];
				}

				lines[i] = new Linear(row, sum);
			}

			foreach (var value in LinearSystem.Resolve(lines))
			{
				Assert.True(System.Math.Abs(value - 1) < 1e-2, value.ToString("R"));
			}

			// Интерполяция Эрмита по восьми узлам 10…17 (система 16×16 по степеням x): промежуточная версия считала её вырожденной.
			var points = new PointD[8];
			var derivatives = new PointD[8];
			for (var i = 0; i < points.Length; i++)
			{
				points[i] = new PointD(10 + i, System.Math.Sin(10 + i));
				derivatives[i] = new PointD(10 + i, System.Math.Cos(10 + i));
			}

			var polynomial = Polynomial.GetPolynomial(points, derivatives);
			foreach (var point in points)
			{
				Assert.True(System.Math.Abs(polynomial.GetValue(point.X) - point.Y) < 1e-6);
			}
		}

		[Fact]
		public void LinearSystem_DoesNotModifyInput()
		{
			// Прежде метод записывал промежуточные уравнения в массив вызывающего кода: [2x₂ = 4, 5x₁ = 10] превращался в [x₁ = 2, x₂ = 2].
			var first = new[] {0D, 2};
			var second = new[] {5D, 0};
			var lines = new[] {new Linear(first, 4), new Linear(second, 10)};

			var x = LinearSystem.Resolve(lines);

			Assert.Equal(new[] {2D, 2}, x);
			Assert.Same(first, lines[0].A);
			Assert.Same(second, lines[1].A);
			Assert.Equal(new[] {0D, 2}, first);
			Assert.Equal(new[] {5D, 0}, second);
			Assert.Equal(4, lines[0].Y);
			Assert.Equal(10, lines[1].Y);
		}

		[Fact]
		public void LinearSystem_ChecksDimensions()
		{
			// Прежде три уравнения с двумя неизвестными давали IndexOutOfRangeException, а одно уравнение с двумя неизвестными
			// молча решалось относительно первой неизвестной ([3]).
			Assert.Throws<ArgumentException>(() => LinearSystem.Resolve(new[]
			{
				new Linear(new[] {1D, 0}, 1),
				new Linear(new[] {0D, 1}, 2),
				new Linear(new[] {1D, 1}, 3)
			}));
			Assert.Throws<ArgumentException>(() => LinearSystem.Resolve(new[] {new Linear(new[] {1D, 2}, 3)}));
			Assert.Throws<ArgumentNullException>(() => LinearSystem.Resolve(null));
			Assert.Empty(LinearSystem.Resolve(new Linear[0]));

			// Недостающие коэффициенты уравнения считаются нулевыми: так BezierCurve подбирает точки Безье для многочлена t.
			var bezier = LinearSystem.Resolve(new[]
			{
				new Linear(new[] {1D}, 0),
				new Linear(new[] {-3D, 3}, 1),
				new Linear(new[] {3D, -6, 3}, 0),
				new Linear(new[] {-1D, 3, -3, 1}, 0)
			});
			Assert.Equal(0, bezier[0]);
			Close(1/3D, bezier[1]);
			Close(2/3D, bezier[2]);
			Close(1, bezier[3]);

			// Нулевые коэффициенты при лишних неизвестных допускаются, как и прежде.
			Assert.Equal(new[] {1D, 2}, LinearSystem.Resolve(new[] {new Linear(new[] {1D, 0, 0}, 1), new Linear(new[] {0D, 1}, 2)}));
		}

		[Fact]
		public void LinearSystem_LargeSystem_AllocatesLittle()
		{
			// Прежде каждое действие над строками создавало несколько массивов через LINQ: для 300 уравнений — 320 МБ.
			const int size = 300;
			var a = RandomMatrix(size, 2);
			var lines = new Linear[size];
			for (var i = 0; i < size; i++)
			{
				var row = new double[size];
				for (var j = 0; j < size; j++)
				{
					row[j] = a[i, j];
				}

				lines[i] = new Linear(row, i);
			}

			var before = GC.GetAllocatedBytesForCurrentThread();
			var x = LinearSystem.Resolve(lines);
			var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

			Assert.True(allocated < 20000000, "Выделено " + allocated + " байт.");
			for (var i = 0; i < size; i++)
			{
				var sum = 0D;
				for (var j = 0; j < size; j++)
				{
					sum += lines[i].A[j]*x[j];
				}

				Assert.True(System.Math.Abs(sum - i) <= 1e-11);
			}
		}

		#endregion

		#region Calculus.FindRoot

		private static double FindRoot(Func<double, double> function, double a, double b, out bool result)
		{
			var found = false;
			var root = TestUtil.CompletesWithin(() => Calculus.Calculus.FindRoot(function, a, b, out found));
			result = found;
			return root;
		}

		[Fact]
		public void FindRoot_RequiresSignChange()
		{
			// Прежде при |f| < 1e-12 на границах проверка смены знака пропускалась: x + 1e-13 на [0, 5] давал «корень» 5,
			// а у 1e-14·(x² + 1), не имеющей корней, находился корень 1.
			bool result;

			Assert.True(double.IsNaN(FindRoot(x => x + 1e-13, 0, 5, out result)));
			Assert.False(result);

			Assert.True(double.IsNaN(FindRoot(x => 1e-14*(x*x + 1), -1, 1, out result)));
			Assert.False(result);
		}

		[Fact]
		public void FindRoot_ComparesSignsWithoutMultiplying()
		{
			// Прежде произведение f(a)·f(m) малых значений обращалось в нуль, и вместо корня 0.3 возвращалась граница 1.
			bool result;
			var root = FindRoot(x => 1e-200*(x - 0.3), 0, 1, out result);

			Assert.True(result);
			Close(0.3, root);
		}

		[Fact]
		public void FindRoot_RejectsNaN()
		{
			// Прежде значение NaN на границе и граница NaN принимались: sqrt(x) − 1 на [−1, 4] давал «корень» 4.
			bool result;

			Assert.True(double.IsNaN(FindRoot(x => System.Math.Sqrt(x) - 1, -1, 4, out result)));
			Assert.False(result);

			Assert.True(double.IsNaN(FindRoot(x => x, double.NaN, 1, out result)));
			Assert.False(result);

			Assert.True(double.IsNaN(FindRoot(x => x, -1, double.NaN, out result)));
			Assert.False(result);
		}

		[Fact]
		public void FindRoot_BoundsInAnyOrder()
		{
			// Прежде считалось, что a < b: x − 1 на [+∞, 0] и x + 100 на [−10, −∞] давали бесконечность.
			bool result;

			Assert.Equal(1, FindRoot(x => x - 1, double.PositiveInfinity, 0, out result));
			Assert.True(result);

			Assert.Equal(-100, FindRoot(x => x + 100, -10, double.NegativeInfinity, out result));
			Assert.True(result);

			Assert.Equal(-100, FindRoot(x => x + 100, double.NegativeInfinity, -10, out result));
			Assert.True(result);

			Assert.Equal(5, FindRoot(x => x - 5, double.NegativeInfinity, double.PositiveInfinity, out result));
			Assert.True(result);

			// Корень, найденный при поиске конечной границы: прежде возвращалось 2.
			Assert.Equal(-2, FindRoot(x => x + 2, double.NegativeInfinity, double.PositiveInfinity, out result));
			Assert.True(result);

			var exponent = FindRoot(x => System.Math.Exp(x) - 1e300, 0, double.PositiveInfinity, out result);
			Assert.True(result);
			Close(300*System.Math.Log(10), exponent, 1e-14);

			var root = FindRoot(x => x*x - 2, 2, 0, out result);
			Assert.True(result);
			Close(System.Math.Sqrt(2), root);
		}

		[Fact]
		public void FindRoot_HugeBounds_MidpointDoesNotOverflow()
		{
			// Прежде середина (a + b)/2 переполнялась, и возвращалась +∞ с признаком успеха.
			bool result;
			var root = FindRoot(x => x - 1.5e308, 1e308, 1.7e308, out result);

			Assert.True(result);
			Close(1.5e308, root);
		}

		[Fact]
		public void FindRoot_Pole_IsNotRoot()
		{
			// Прежде полюс функции 1/x на [−1, 1] выдавался за корень 0.
			bool result;

			Assert.True(double.IsNaN(FindRoot(x => 1/x, -1, 1, out result)));
			Assert.False(result);

			Assert.True(double.IsNaN(FindRoot(System.Math.Tan, 1, 2, out result)));
			Assert.False(result);

			// Настоящие корни по-прежнему находятся, в том числе точные корни на границах.
			Assert.Equal(1, FindRoot(x => x - 1, 1, 3, out result));
			Assert.True(result);
			Assert.Equal(3, FindRoot(x => x - 3, 1, 3, out result));
			Assert.True(result);

			var root = FindRoot(System.Math.Cos, 0, 3, out result);
			Assert.True(result);
			Close(System.Math.PI/2, root);
		}

		#endregion

		#region Calculus.Differentiate

		[Fact]
		public void Differentiate_DividesByActualStep()
		{
			// Прежде разность делилась на номинальный eps, а узел 1e8 ± 1e-8 округлялся: производная x получалась равной 1.49.
			Assert.Equal(1, Calculus.Calculus.Differentiate(x => x, 1e8, 1e-8));
			Assert.Equal(1, Calculus.Calculus.Differentiate(x => x, 1e8, 1e-8, ELimitSide.Left));
			Assert.Equal(1, Calculus.Calculus.Differentiate(x => x, 1e8, 1e-8, ELimitSide.Right));
			Assert.Equal(0, Calculus.Calculus.Differentiate(x => x, 1e8, 1e-8, 2));

			// Если узлы не округляются, результат прежний.
			Assert.Equal(1.25, Calculus.Calculus.Differentiate(x => x*x, 0.5, 0.25, ELimitSide.Right));
			Assert.Equal(0.75, Calculus.Calculus.Differentiate(x => x*x, 0.5, 0.25, ELimitSide.Left));
			Assert.Equal(1, Calculus.Calculus.Differentiate(x => x*x, 0.5, 0.25));
			Assert.Equal(2, Calculus.Calculus.Differentiate(x => x*x, 0.5, 0.25, 2));
			Assert.InRange(Calculus.Calculus.Differentiate(System.Math.Sin, 1, 1e-5) - System.Math.Cos(1), -1e-9, 1e-9);
			Assert.InRange(Calculus.Calculus.Differentiate(System.Math.Sin, 1, 1e-4, 2) + System.Math.Sin(1), -1e-6, 1e-6);

			// Шаг меньше точности представления аргумента: прежде возвращался 0.
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Differentiate(x => x, 1e8, 1e-9));
		}

		[Fact]
		public void Differentiate_ValidatesArguments()
		{
			// Прежде при eps = 0 возвращался NaN, отрицательный eps молча менял сторону (правая производная |x| в нуле равнялась −1),
			// отрицательный порядок давал NotImplementedException, а неизвестная сторона — центральную производную.
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Differentiate(x => x, 1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				Calculus.Calculus.Differentiate(System.Math.Abs, 0, -1e-3, ELimitSide.Right));
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Differentiate(x => x, 1, double.NaN));
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Differentiate(x => x, 1, double.PositiveInfinity));
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Differentiate(x => x, 1, 1e-3, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				Calculus.Calculus.Differentiate(x => x, 1, 1e-3, ELimitSide.Left, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Differentiate(x => x*x, 1, 1e-3, (ELimitSide) 7));
			Assert.Throws<ArgumentNullException>(() => Calculus.Calculus.Differentiate(null, 1, 1e-3));
			Assert.Throws<NotImplementedException>(() => Calculus.Calculus.Differentiate(x => x, 1, 1e-3, 3));
		}

		[Fact]
		public void Differentiate_OrderZero_ReturnsValue()
		{
			// Прежде порядок 0 давал значение функции только в перегрузке со стороной, а в перегрузке без неё — NotImplementedException.
			Assert.Equal(4, Calculus.Calculus.Differentiate(x => x + 3, 1, 1e-3, 0));
			Assert.Equal(4, Calculus.Calculus.Differentiate(x => x + 3, 1, 1e-3, ELimitSide.Both, 0));
			Assert.Equal(4, Calculus.Calculus.Differentiate(x => x + 3, 1, 1e-3, ELimitSide.Left, 0));
		}

		#endregion

		#region Calculus.Integrate

		public static TheoryData<EIntegrateRule> AllRules => new TheoryData<EIntegrateRule>
		{
			EIntegrateRule.Default,
			EIntegrateRule.Rectangle,
			EIntegrateRule.Trapezoidal,
			EIntegrateRule.Simpson,
			EIntegrateRule.Gauss2,
			EIntegrateRule.Gauss3,
			EIntegrateRule.Gauss5,
			EIntegrateRule.Gauss6,
			EIntegrateRule.Gauss10
		};

		[Theory]
		[MemberData(nameof(AllRules))]
		public void Integrate_InfiniteOrNaNBounds_Throw(EIntegrateRule rule)
		{
			// Прежде ∫₀^∞ e^−x давал NaN (правила Гаусса и прямоугольников) или −∞ (трапеции и Симпсон).
			Func<double, double> f = x => System.Math.Exp(-x);

			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Integrate(f, 0, double.PositiveInfinity, rule));
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Integrate(f, double.NegativeInfinity, 0, rule));
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Integrate(f, double.NaN, 1, rule));
			Assert.Throws<ArgumentOutOfRangeException>(() => Calculus.Calculus.Integrate(f, 1, double.NaN, rule));
			Assert.Throws<ArgumentNullException>(() => Calculus.Calculus.Integrate(null, 0, 1, rule));
		}

		#endregion

		#region Interpolation

		[Fact]
		public void CubicInterpolation_FarFromZero_KeepsAccuracy()
		{
			// Прежде многочлены клеток хранились по степеням x: при x ≈ 1e6 ошибка в самих узлах достигала 79.
			var reference = new CubicInterpolation(Enumerable.Range(0, 11).Select(i => new PointD(i, System.Math.Sin(i))).ToArray());

			foreach (var x0 in new[] {1e3, 1e4, 1e5, 1e6})
			{
				var nodes = Enumerable.Range(0, 11).Select(i => new PointD(x0 + i, System.Math.Sin(i))).ToArray();
				var f = new CubicInterpolation(nodes);

				foreach (var node in nodes)
				{
					Assert.InRange(f.GetValue(node.X) - node.Y, -1e-13, 1e-13);
				}

				// Сплайн не зависит от сдвига аргумента.
				for (var t = 0.25; t < 10; t += 0.5)
				{
					Assert.InRange(f.GetValue(x0 + t) - reference.GetValue(t), -1e-12, 1e-12);
				}
			}

			// Многочлены клеток по-прежнему записаны по степеням x: S(x) = 1.5x − 0.5x³ на [0, 1].
			var spline = new CubicInterpolation(new[] {new PointD(0, 0), new PointD(1, 1), new PointD(2, 0)});
			var polynomial = spline.GetPolynom(0);
			Assert.Equal(0, polynomial[0], 12);
			Assert.Equal(1.5, polynomial[1], 12);
			Assert.Equal(0, polynomial[2], 12);
			Assert.Equal(-0.5, polynomial[3], 12);
			Assert.Equal(0.6875, spline.GetPolynom(1).GetValue(1.5), 12);
			Assert.Equal(0.6875, spline.GetValue(1.5), 12);
		}

		[Fact]
		public void LinearInterpolation_FarFromZero_KeepsAccuracy()
		{
			// Прежде в середине отрезка [1e12, 1e12 + 1] получалось 0.0500031 вместо 0.05.
			var f = new LinearInterpolation(new[] {new PointD(1e12, 0), new PointD(1e12 + 1, 0.1)});

			Assert.Equal(0.05, f.GetValue(1e12 + 0.5));
			Assert.Equal(0, f.GetValue(1e12));

			// Многочлены клеток по-прежнему записаны по степеням x.
			var g = new LinearInterpolation(new[] {new PointD(0, 0), new PointD(1, 2), new PointD(3, 4)});
			Assert.Equal(3, g.GetPolynom(1).GetValue(2), 12);
			Assert.Equal(1, g.GetPolynom(1)[1], 12);
		}

		[Fact]
		public void GridApproximation_CopiesCallerArray()
		{
			// Прежде массив хранился по ссылке: после его изменения вызывающим кодом менялись значения, а границы — нет.
			var points = new[] {new PointD(0, 0), new PointD(1, 1), new PointD(2, 4), new PointD(3, 9)};
			var cubic = new CubicInterpolation(points);
			var linear = new LinearInterpolation(points);

			points[1] = new PointD(1, 50);
			points[3] = new PointD(3, 100);

			Assert.Equal(1, cubic.GetValue(1), 12);
			Assert.Equal(9, cubic.GetValue(3), 12);
			Assert.Equal(1, linear.GetValue(1), 12);
			Assert.Equal(9, linear.GetValue(3), 12);
			Assert.Equal(9, cubic.ToArray()[3].Y);
		}

		#endregion

		#region Vector, Matrix, Linear

		[Fact]
		public void Matrix_Clone_IsDeep()
		{
			// Прежде копировался только массив строк, и изменение копии меняло исходную матрицу.
			var original = Rows(new[] {1D, 2}, new[] {3D, 4});
			var clone = (Matrix) original.Clone();

			clone[0, 0] = 100;

			Assert.Equal(1, original[0, 0]);
			Assert.Equal(100, clone[0, 0]);
			Assert.True(clone != original);
		}

		[Fact]
		public void Division_DividesEachElement()
		{
			// Прежде деление заменялось умножением на 1/y: 3/5 = 0.6000000000000001, а 0/1e-310 = NaN, 1e-310/1e-310 = ∞.
			Assert.Equal(0.6, (new Vector(3)/5)[0]);
			Assert.Equal(0.3, (new Vector(3)/10)[0]);

			var tiny = new Vector(0, 1e-310)/1e-310;
			Assert.Equal(0, tiny[0]);
			Assert.Equal(1, tiny[1]);

			Assert.Equal(0.6, (Rows(new[] {3D})/5)[0, 0]);
			var matrix = Rows(new[] {0D, 1e-310})/1e-310;
			Assert.Equal(0, matrix[0, 0]);
			Assert.Equal(1, matrix[0, 1]);

			var linear = new Linear(new[] {3D, 0}, 3)/10;
			Assert.Equal(0.3, linear.A[0]);
			Assert.Equal(0, linear.A[1]);
			Assert.Equal(0.3, linear.Y);
		}

		[Fact]
		public void Vector_HashCode_ConsistentWithEquality()
		{
			// Прежде хэш-код всегда был равен 0.
			var otherNaN = BitConverter.Int64BitsToDouble(0x7FF8000000000001);
			var pairs = new[]
			{
				new[] {new Vector(1, 2), new Vector(1, 2, 0)},
				new[] {new Vector(0D), new Vector(-0D)},
				new[] {new Vector(0, 0, 0), Vector.Empty},
				new[] {new Vector(double.NaN, 1), new Vector(otherNaN, 1)}
			};

			foreach (var pair in pairs)
			{
				Assert.True(pair[0] == pair[1]);
				Assert.Equal(pair[0].GetHashCode(), pair[1].GetHashCode());
			}

			Assert.Equal(3, new[] {new Vector(0, 0, 1), new Vector(0, 1), new Vector(1)}.Select(v => v.GetHashCode()).Distinct().Count());

			var vectors = Enumerable.Range(0, 1000).Select(i => new Vector(i%10, i/10D, 1)).ToList();
			Assert.True(vectors.Select(v => v.GetHashCode()).Distinct().Count() > 900);
			Assert.Equal(1000, new HashSet<Vector>(vectors).Count);
		}

		[Fact]
		public void Matrix_HashCode_ConsistentWithEquality()
		{
			// Прежде хэш-код всегда был равен 0.
			var a = Rows(new[] {1D, 2});
			var b = Rows(new[] {1D, 2, 0}, new[] {0D, 0});

			Assert.True(a == b);
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
			Assert.Equal(Matrix.Empty.GetHashCode(), Rows(new[] {0D}, new double[0]).GetHashCode());

			var matrices = Enumerable.Range(0, 200).Select(i => Rows(new[] {i%10, 1D}, new[] {i/10D})).ToList();
			Assert.True(matrices.Select(m => m.GetHashCode()).Distinct().Count() > 180);
		}

		[Fact]
		public void Linear_HashCode_ConsistentWithEquality()
		{
			// Прежде хэш-код всегда был равен 0.
			var a = new Linear(new[] {1D, 2}, 3);
			var b = new Linear(new[] {1D, 2}, 3);

			Assert.True(a == b);
			Assert.Equal(a.GetHashCode(), b.GetHashCode());

			var c = new Linear(new[] {0D}, double.NaN);
			var d = new Linear(new[] {-0D}, BitConverter.Int64BitsToDouble(0x7FF8000000000001));
			Assert.True(c == d);
			Assert.Equal(c.GetHashCode(), d.GetHashCode());

			var linears = Enumerable.Range(0, 200).Select(i => new Linear(new double[] {i%10, 1}, i/10D)).ToList();
			Assert.True(linears.Select(l => l.GetHashCode()).Distinct().Count() > 180);
		}

		[Fact]
		public void DefaultValues_BehaveAsEmpty()
		{
			// Прежде у default(Vector), default(Matrix), default(Linear) и элементов новых массивов любое обращение
			// выбрасывало NullReferenceException.
			var vector = default(Vector);
			Assert.Equal(0, vector.Length);
			Assert.True(vector == Vector.Empty);
			Assert.True(Vector.IsEmpty(vector));
			Assert.Equal("Ø", vector.ToString());
			Assert.Equal(0, vector[5]);
			Assert.Equal(0, vector.X);
			Assert.Equal(0, Vector.Empty.X);
			Assert.Equal(Vector.Empty.GetHashCode(), vector.GetHashCode());
			Assert.True(vector + new Vector(1, 2) == new Vector(1, 2));
			Assert.True(-vector*2/3 == Vector.Empty);
			Assert.Equal(0, vector.DotProduct(new Vector(1, 2)));
			Assert.True((Vector) vector.Clone() == Vector.Empty);

			var rows = new Matrix(new Vector[2]);
			Assert.True(rows*2 == Matrix.Empty);
			Assert.Equal(0, rows[0, 1]);
			Assert.Equal("Ø", rows.ToString());
			Assert.True(rows + Rows(new[] {1D}) == Rows(new[] {1D}));

			var matrix = default(Matrix);
			Assert.Equal(0, matrix.Length);
			Assert.True(matrix == Matrix.Empty);
			Assert.Equal("Ø", matrix.ToString());
			Assert.Equal(Matrix.Empty.GetHashCode(), matrix.GetHashCode());
			Assert.Equal(0, matrix.GetInverse().Length);
			Assert.Equal(0, ((Matrix) matrix.Clone()).Length);

			var linear = default(Linear);
			Assert.True(linear == default(Linear));
			Assert.True(linear == new Linear(new double[0], 0));
			Assert.Equal(new Linear(new double[0], 0).GetHashCode(), linear.GetHashCode());
			Assert.Equal("0 = 0", linear.ToString());
			Assert.True(linear + new Linear(new[] {1D}, 2) == new Linear(new[] {1D}, 2));
			Assert.True(-linear*2/3 == new Linear(new double[0], 0));
			Assert.Throws<ArithmeticException>(() => LinearSystem.Resolve(new[] {linear, new Linear(new[] {0D, 1}, 2)}));
		}

		#endregion
	}
}
