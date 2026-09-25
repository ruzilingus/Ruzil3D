using System;
using Ruzil3D.Algebra;

namespace Ruzil3D.Calculus
{
	/// <summary>
	/// Представляет систему линейных уравнений над полем вещесвенных чисел.
	/// </summary>
	public static class LinearSystem
	{
		/// <summary>
		/// Решает систему линейных уравнений и возвращает результат.
		/// </summary>
		/// <param name="lines">Массив линейных уравнений заданных массивом структур <see cref="Linear"/>. Массив и уравнения не изменяются.</param>
		/// <returns>Решение системы линейных уравнений: значения неизвестных x₁, …, xₙ, где n — число уравнений.</returns>
		/// <remarks>
		/// Число неизвестных должно быть равно числу уравнений. Уравнение может содержать меньше коэффициентов, чем неизвестных:
		/// недостающие коэффициенты считаются нулевыми. Коэффициенты при неизвестных с номерами больше n допускаются, только если они равны нулю.
		/// Система решается методом Гаусса с выбором главного элемента по столбцу. Вырожденность определяется так же, как прежде:
		/// исключение выбрасывается, если при исключении без перестановок в столбце не нашлось ненулевого ведущего элемента.
		/// Для почти вырожденной системы исключение не выбрасывается: возвращается решение, точность которого определяется
		/// числом обусловленности матрицы.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="lines"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Число неизвестных не равно числу уравнений.</exception>
		/// <exception cref="ArithmeticException">Система вырожденная: решение не существует или не единственно.</exception>
		public static double[] Resolve(Linear[] lines)
		{
			if (lines == null)
			{
				throw new ArgumentNullException(nameof(lines));
			}

			//Прежде размерности не проверялись: для системы из трёх уравнений с двумя неизвестными выбрасывалось
			//IndexOutOfRangeException, а для одного уравнения с двумя неизвестными молча возвращалось одно число.
			var size = lines.Length;
			var unknowns = 0;
			foreach (var line in lines)
			{
				if (line.A != null && line.A.Length > unknowns)
				{
					unknowns = line.A.Length;
				}
			}

			if (unknowns < size)
			{
				throw new ArgumentException(
					"Число уравнений (" + size + ") больше числа неизвестных (" + unknowns + ").", nameof(lines));
			}

			for (var i = 0; i < size; i++)
			{
				var coefficients = lines[i].A ?? new double[0];
				for (var j = size; j < coefficients.Length; j++)
				{
					if (coefficients[j] != 0)
					{
						throw new ArgumentException(
							"Число неизвестных больше числа уравнений (" + size + "): уравнение " + (i + 1) +
							" содержит ненулевой коэффициент при неизвестной x" + (j + 1) + ".", nameof(lines));
					}
				}
			}

			//Решение ищется на рабочих массивах: прежде метод записывал промежуточные уравнения в массив вызывающего кода,
			//а каждое действие над строками создавало несколько новых массивов (для 300 уравнений — 320 МБ).
			var a = new double[size][];
			var y = new double[size][];
			for (var i = 0; i < size; i++)
			{
				a[i] = new double[size];
				y[i] = new double[1];
			}

			Action<double[][], double[][]> fill = (matrix, values) =>
			{
				for (var i = 0; i < size; i++)
				{
					var coefficients = lines[i].A ?? new double[0];
					var row = matrix[i];
					Array.Clear(row, 0, size);
					Array.Copy(coefficients, row, coefficients.Length < size ? coefficients.Length : size);
					values[i][0] = lines[i].Y;
				}
			};

			fill(a, y);

			if (!Solve(a, y, fill))
			{
				//Прежде выбрасывалось исключение общего типа Exception.
				throw new ArithmeticException("Задача вырожденная.");
			}

			var result = new double[size];
			for (var i = 0; i < size; i++)
			{
				result[i] = y[i][0];
			}

			return result;
		}

		/// <summary>
		/// Решает систему A·X = B с несколькими правыми частями.
		/// </summary>
		/// <param name="a">Квадратная матрица коэффициентов, заданная строками длины n. Содержимое изменяется.</param>
		/// <param name="b">Правые части, заданные строками: по одной строке на уравнение. На выходе строка i содержит значения неизвестной xᵢ.</param>
		/// <param name="fill">Заполняет строки массивов <paramref name="a"/> и <paramref name="b"/> исходными значениями
		/// (строка i — уравнение i). Вызывается, только если нужно повторить решение прежним методом.</param>
		/// <returns>Значение <b>false</b>, если матрица вырожденная; в противном случае — значение <b>true</b>.</returns>
		/// <remarks>
		/// Решение вычисляется методом Гаусса с выбором главного элемента по столбцу. Прежде ведущим брался первый ненулевой элемент
		/// столбца, из-за чего, например, для [[1e-20, 1], [1, 1]] получался неверный результат.
		/// Вырожденность же определяется прежним методом — исключением без перестановок с той же арифметикой (см.
		/// <see cref="EliminateWithoutPivoting"/>), поэтому исключение выбрасывается ровно в тех же случаях, что и прежде. Метод с выбором
		/// главного элемента из-за другого порядка округлений не распознаёт, например, вырожденность целочисленной матрицы
		/// [[1, 2, 3], [4, 5, 6], [7, 8, 9]] (ведущий элемент получается порядка 1e-16, а не нулём), а любой порог «численной
		/// вырожденности» отвергал бы и плохо обусловленные системы с полезным решением, которые прежде решались.
		/// </remarks>
		internal static bool Solve(double[][] a, double[][] b, Action<double[][], double[][]> fill)
		{
			if (!EliminateWithoutPivoting(Copy(a), null))
			{
				return false;
			}

			if (SolveWithPivoting(a, b))
			{
				return true;
			}

			//Нулевой ведущий элемент встретился только при выборе главного элемента (так бывает лишь у матриц, вырожденных
			//с точностью до округления). Как и прежде, возвращается решение метода без перестановок.
			fill(a, b);
			EliminateWithoutPivoting(a, b);
			for (var i = a.Length - 1; i > 0; i--)
			{
				var solution = b[i];
				for (var j = i - 1; j >= 0; j--)
				{
					var factor = a[j][i];
					var lineB = b[j];
					for (var t = 0; t < lineB.Length; t++)
					{
						lineB[t] -= factor*solution[t];
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Приводит матрицу к верхней треугольной с единичной диагональю прежним методом: ведущим берётся первый ненулевой элемент
		/// столбца, строка умножается на обратное к нему число, а затем вычитается из следующих строк.
		/// </summary>
		/// <param name="a">Квадратная матрица коэффициентов. Содержимое изменяется.</param>
		/// <param name="b">Правые части или <b>null</b>. Содержимое изменяется.</param>
		/// <returns>Значение <b>false</b>, если в каком-либо столбце не нашлось ненулевого ведущего элемента; в противном случае — значение <b>true</b>.</returns>
		/// <remarks>Арифметика повторяет прежние Matrix.GetInverse и LinearSystem.Resolve, поэтому и нули получаются в тех же случаях.</remarks>
		private static bool EliminateWithoutPivoting(double[][] a, double[][] b)
		{
			var size = a.Length;

			for (var i = 0; i < size; i++)
			{
				var pivotRow = i;
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				while (pivotRow < size && a[pivotRow][i] == 0)
				{
					pivotRow++;
				}

				if (pivotRow == size)
				{
					return false;
				}

				if (pivotRow != i)
				{
					Swap(a, i, pivotRow);
					if (b != null)
					{
						Swap(b, i, pivotRow);
					}
				}

				var line = a[i];
				var inverse = 1/line[i];
				for (var t = i; t < size; t++)
				{
					line[t] *= inverse;
				}

				var lineB = b?[i];
				if (lineB != null)
				{
					for (var t = 0; t < lineB.Length; t++)
					{
						lineB[t] *= inverse;
					}
				}

				for (var j = i + 1; j < size; j++)
				{
					var row = a[j];
					var factor = row[i];
					for (var t = i; t < size; t++)
					{
						row[t] -= factor*line[t];
					}

					if (lineB != null)
					{
						var rowB = b[j];
						for (var t = 0; t < rowB.Length; t++)
						{
							rowB[t] -= factor*lineB[t];
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Решает систему A·X = B методом Гаусса с выбором главного элемента по столбцу.
		/// </summary>
		/// <param name="a">Квадратная матрица коэффициентов. Содержимое изменяется.</param>
		/// <param name="b">Правые части. На выходе строка i содержит значения неизвестной xᵢ.</param>
		/// <returns>Значение <b>false</b>, если ведущий элемент оказался равен нулю; в противном случае — значение <b>true</b>.</returns>
		private static bool SolveWithPivoting(double[][] a, double[][] b)
		{
			var size = a.Length;

			#region Прямой ход (приведение к верхней треугольной матрице)

			for (var k = 0; k < size; k++)
			{
				//Выбор главного элемента: наибольший по модулю элемент столбца.
				//NaN выбирается сразу, чтобы он перешёл в результат, а не превратился в ложное сообщение о вырожденности.
				var pivotRow = k;
				var pivotAbs = System.Math.Abs(a[k][k]);
				for (var i = k + 1; i < size && !double.IsNaN(pivotAbs); i++)
				{
					var abs = System.Math.Abs(a[i][k]);
					if (abs > pivotAbs || double.IsNaN(abs))
					{
						pivotRow = i;
						pivotAbs = abs;
					}
				}

				if (pivotRow != k)
				{
					Swap(a, k, pivotRow);
					Swap(b, k, pivotRow);
				}

				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (pivotAbs == 0)
				{
					return false;
				}

				var pivot = a[k][k];
				var pivotLine = a[k];
				var pivotLineB = b[k];

				for (var i = k + 1; i < size; i++)
				{
					var line = a[i];
					var factor = line[k]/pivot;

					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (factor == 0)
					{
						continue;
					}

					line[k] = 0;
					for (var j = k + 1; j < size; j++)
					{
						line[j] -= factor*pivotLine[j];
					}

					var lineB = b[i];
					for (var j = 0; j < lineB.Length; j++)
					{
						lineB[j] -= factor*pivotLineB[j];
					}
				}
			}

			#endregion

			#region Обратный ход

			for (var k = size - 1; k >= 0; k--)
			{
				var line = a[k];
				var lineB = b[k];

				for (var j = k + 1; j < size; j++)
				{
					var coefficient = line[j];

					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (coefficient == 0)
					{
						continue;
					}

					var solution = b[j];
					for (var t = 0; t < lineB.Length; t++)
					{
						lineB[t] -= coefficient*solution[t];
					}
				}

				var pivot = line[k];
				for (var t = 0; t < lineB.Length; t++)
				{
					lineB[t] /= pivot;
				}
			}

			#endregion

			return true;
		}

		private static double[][] Copy(double[][] array)
		{
			var result = new double[array.Length][];
			for (var i = 0; i < array.Length; i++)
			{
				result[i] = (double[])array[i].Clone();
			}

			return result;
		}

		private static void Swap(double[][] array, int i, int j)
		{
			var save = array[i];
			array[i] = array[j];
			array[j] = save;
		}
	}
}