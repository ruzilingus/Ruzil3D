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
		/// Машинный эпсилон: расстояние от 1 до следующего числа двойной точности.
		/// </summary>
		private const double MachineEpsilon = 2.220446049250313E-16;

		/// <summary>
		/// Решает систему линейных уравнений и возвращает результат.
		/// </summary>
		/// <param name="lines">Массив линейных уравнений заданных массивом структур <see cref="Linear"/>. Массив и уравнения не изменяются.</param>
		/// <returns>Решение системы линейных уравнений: значения неизвестных x₁, …, xₙ, где n — число уравнений.</returns>
		/// <remarks>
		/// Число неизвестных должно быть равно числу уравнений. Уравнение может содержать меньше коэффициентов, чем неизвестных:
		/// недостающие коэффициенты считаются нулевыми. Коэффициенты при неизвестных с номерами больше n допускаются, только если они равны нулю.
		/// Система решается методом Гаусса с выбором главного элемента по столбцу. Система считается вырожденной, если ведущий элемент
		/// не превосходит оценки накопленной в нём погрешности округления (так бывает у матриц с числом обусловленности порядка 1/(n·ε) и больше).
		/// </remarks>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="lines"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Число неизвестных не равно числу уравнений.</exception>
		/// <exception cref="ArithmeticException">Система вырожденная, в том числе численно: решение не существует или не единственно.</exception>
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

			//Решение ищется на рабочих массивах: прежде метод записывал промежуточные уравнения в массив вызывающего кода,
			//а каждое действие над строками создавало несколько новых массивов (для 300 уравнений — 320 МБ).
			var a = new double[size][];
			var y = new double[size][];
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

				var row = new double[size];
				Array.Copy(coefficients, row, coefficients.Length < size ? coefficients.Length : size);
				a[i] = row;
				y[i] = new[] {lines[i].Y};
			}

			if (!Solve(a, y))
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
		/// Решает систему A·X = B с несколькими правыми частями методом Гаусса с выбором главного элемента по столбцу.
		/// </summary>
		/// <param name="a">Квадратная матрица коэффициентов, заданная строками длины n. Содержимое изменяется.</param>
		/// <param name="b">Правые части, заданные строками: по одной строке на уравнение. На выходе строка i содержит значения неизвестной xᵢ.</param>
		/// <returns>Значение <b>false</b>, если матрица вырожденная, в том числе численно; в противном случае — значение <b>true</b>.</returns>
		/// <remarks>
		/// Прежде ведущим элементом брался первый ненулевой элемент столбца, а вырожденность проверялась точным сравнением с нулём.
		/// Из-за этого для [[1e-20, 1], [1, 1]] получался неверный результат, а для численно вырожденной матрицы вместо исключения
		/// возвращались числа порядка 1e16. Теперь ведущим берётся наибольший по модулю элемент столбца, а нулём он считается, если
		/// не превосходит n·ε·S, где S — сумма модулей слагаемых, из которых он получен при исключении. Такая оценка погрешности
		/// округления не зависит от масштаба строк и столбцов, поэтому, например, diag(1e-300, 1) по-прежнему обращается.
		/// </remarks>
		internal static bool Solve(double[][] a, double[][] b)
		{
			var size = a.Length;

			//Оценки модулей слагаемых, из которых получен каждый элемент матрицы.
			var bounds = new double[size][];
			for (var i = 0; i < size; i++)
			{
				var row = a[i];
				var bound = new double[size];
				for (var j = 0; j < size; j++)
				{
					bound[j] = System.Math.Abs(row[j]);
				}

				bounds[i] = bound;
			}

			var tolerance = size*MachineEpsilon;

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
					Swap(bounds, k, pivotRow);
				}

				var pivotBound = bounds[k][k];
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (pivotAbs == 0 || pivotAbs <= tolerance*pivotBound && !double.IsInfinity(pivotBound))
				{
					return false;
				}

				var pivot = a[k][k];
				var pivotLine = a[k];
				var pivotLineBounds = bounds[k];
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

					var absFactor = System.Math.Abs(factor);
					var lineBounds = bounds[i];

					line[k] = 0;
					for (var j = k + 1; j < size; j++)
					{
						line[j] -= factor*pivotLine[j];
						lineBounds[j] += absFactor*pivotLineBounds[j];
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

		private static void Swap(double[][] array, int i, int j)
		{
			var save = array[i];
			array[i] = array[j];
			array[j] = save;
		}
	}
}