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
		/// Порог выбора ведущего элемента: прежний ведущий элемент (первый ненулевой в столбце) сохраняется, если его модуль,
		/// отнесённый к наибольшему модулю в его строке, не меньше этой доли от наибольшего такого отношения в столбце.
		/// </summary>
		private const double PivotThreshold = 0.1;

		/// <summary>
		/// Решает систему A·X = B с несколькими правыми частями.
		/// </summary>
		/// <param name="a">Квадратная матрица коэффициентов, заданная строками длины n. Содержимое изменяется.</param>
		/// <param name="b">Правые части, заданные строками: по одной строке на уравнение. На выходе строка i содержит значения неизвестной xᵢ.</param>
		/// <param name="fill">Заполняет строки массивов (строка i — уравнение i) исходными значениями. Вызывается, только если нужно
		/// проверить вырожденность или повторить решение прежним методом.</param>
		/// <returns>Значение <b>false</b>, если матрица вырожденная; в противном случае — значение <b>true</b>.</returns>
		/// <remarks>
		/// Используется прежний метод Гаусса с той же арифметикой: ведущим берётся первый ненулевой элемент столбца, его строка
		/// умножается на обратное к нему число и вычитается из следующих строк. Поэтому результаты совпадают с прежними побитово.
		/// Прежний ведущий элемент заменяется, только если он опасно мал: его модуль, отнесённый к наибольшему модулю в строке,
		/// меньше <see cref="PivotThreshold"/> от наибольшего такого отношения в столбце (масштабированный выбор главного элемента
		/// с порогом). Прежде в этом случае терялась точность: [[1e-20, 1], [1, 1]]·x = [1, 2] давало [0, 1] вместо [1, 1].
		/// Полный выбор главного элемента по столбцу без порога здесь не используется: он менял результаты и там, где прежний
		/// порядок был точнее (например, у матриц Вандермонда и при сильно различающемся масштабе строк).
		/// Вырожденность определяется так же, как прежде: если ведущий элемент был заменён, прежний метод повторяется на исходных
		/// данных, и исключение выбрасывается ровно в тех же случаях, что и раньше.
		/// </remarks>
		internal static bool Solve(double[][] a, double[][] b, Action<double[][], double[][]> fill)
		{
			var size = a.Length;

			var scales = new double[size];
			for (var i = 0; i < size; i++)
			{
				var row = a[i];
				double max = 0;
				for (var j = 0; j < size; j++)
				{
					var abs = System.Math.Abs(row[j]);
					if (abs > max)
					{
						max = abs;
					}
				}

				scales[i] = max;
			}

			var replaced = false;

			for (var k = 0; k < size; k++)
			{
				//Прежний ведущий элемент — первый ненулевой элемент столбца (NaN тоже считается ненулевым).
				var pivotRow = k;
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				while (pivotRow < size && a[pivotRow][k] == 0)
				{
					pivotRow++;
				}

				if (pivotRow == size)
				{
					//Если ведущий элемент не заменялся, вычисления совпадали с прежними, и прежде здесь выбрасывалось исключение.
					return replaced && SolveAsBefore(a, b, fill);
				}

				var pivotScaled = System.Math.Abs(a[pivotRow][k])/scales[pivotRow];
				var best = pivotRow;
				var bestScaled = pivotScaled;
				for (var i = pivotRow + 1; i < size; i++)
				{
					var scaled = System.Math.Abs(a[i][k])/scales[i];
					if (scaled > bestScaled)
					{
						best = i;
						bestScaled = scaled;
					}
				}

				if (pivotScaled < PivotThreshold*bestScaled)
				{
					pivotRow = best;
					replaced = true;
				}

				if (pivotRow != k)
				{
					Swap(a, k, pivotRow);
					Swap(b, k, pivotRow);
					var scale = scales[k];
					scales[k] = scales[pivotRow];
					scales[pivotRow] = scale;
				}

				EliminateColumn(a, b, k);
			}

			if (replaced)
			{
				//Прежний метод мог встретить нулевой ведущий элемент, и тогда прежде выбрасывалось исключение.
				var originalA = new double[size][];
				var originalB = new double[size][];
				for (var i = 0; i < size; i++)
				{
					originalA[i] = new double[size];
					originalB[i] = new double[b[i].Length];
				}

				fill(originalA, originalB);
				if (!EliminateWithoutPivoting(originalA, null))
				{
					return false;
				}
			}

			SubstituteBackward(a, b);
			return true;
		}

		/// <summary>
		/// Решает систему прежним методом на исходных данных, которые записывает <paramref name="fill"/>.
		/// </summary>
		private static bool SolveAsBefore(double[][] a, double[][] b, Action<double[][], double[][]> fill)
		{
			fill(a, b);
			if (!EliminateWithoutPivoting(a, b))
			{
				return false;
			}

			SubstituteBackward(a, b);
			return true;
		}

		/// <summary>
		/// Приводит матрицу к верхней треугольной с единичной диагональю прежним методом: ведущим берётся первый ненулевой элемент
		/// столбца, строка умножается на обратное к нему число, а затем вычитается из следующих строк.
		/// </summary>
		/// <param name="a">Квадратная матрица коэффициентов. Содержимое изменяется.</param>
		/// <param name="b">Правые части или <b>null</b>. Содержимое изменяется.</param>
		/// <returns>Значение <b>false</b>, если в каком-либо столбце не нашлось ненулевого ведущего элемента; в противном случае — значение <b>true</b>.</returns>
		private static bool EliminateWithoutPivoting(double[][] a, double[][] b)
		{
			var size = a.Length;

			for (var k = 0; k < size; k++)
			{
				var pivotRow = k;
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				while (pivotRow < size && a[pivotRow][k] == 0)
				{
					pivotRow++;
				}

				if (pivotRow == size)
				{
					return false;
				}

				if (pivotRow != k)
				{
					Swap(a, k, pivotRow);
					if (b != null)
					{
						Swap(b, k, pivotRow);
					}
				}

				EliminateColumn(a, b, k);
			}

			return true;
		}

		/// <summary>
		/// Умножает строку <paramref name="k"/> на обратное к её ведущему элементу и вычитает её из следующих строк, как прежние
		/// Matrix.GetInverse и LinearSystem.Resolve.
		/// </summary>
		private static void EliminateColumn(double[][] a, double[][] b, int k)
		{
			var size = a.Length;
			var line = a[k];
			var inverse = 1/line[k];
			for (var t = k; t < size; t++)
			{
				line[t] *= inverse;
			}

			var lineB = b?[k];
			if (lineB != null)
			{
				for (var t = 0; t < lineB.Length; t++)
				{
					lineB[t] *= inverse;
				}
			}

			for (var j = k + 1; j < size; j++)
			{
				var row = a[j];
				var factor = row[k];
				for (var t = k; t < size; t++)
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

		/// <summary>
		/// Обратный ход для матрицы с единичной диагональю, как в прежних Matrix.GetInverse и LinearSystem.Resolve.
		/// </summary>
		private static void SubstituteBackward(double[][] a, double[][] b)
		{
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
		}

		private static void Swap(double[][] array, int i, int j)
		{
			var save = array[i];
			array[i] = array[j];
			array[j] = save;
		}
	}
}