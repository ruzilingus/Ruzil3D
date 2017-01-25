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
		/// <param name="lines">Массив линейных уравнений заданных массивом структур <see cref="Linear"/>.</param>
		/// <returns>Решение системы линейных уравнений.</returns>
		public static double[] Resolve(Linear[] lines)
		{
			#region Прямая итерация (Превращаем в треугольную матрицу c единичной диагональю)

			for (var i = 0; i < lines.Length; i++)
			{
				#region Преобразования над строками. Для гарантии lines[i].A[i] != 0

				//Ищем ближайшую строчку с ненулевым i-ым коэффициентом
				var j = i;
				Linear line;
				do
				{
					line = lines[j];

					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (line.A[i] == 0)
					{
						j++;
					}
					else
					{
						break;
					}
				} while (j < lines.Length);

				if (j == lines.Length)
				{
					//Если не нашли такую строчку
					throw new Exception("Задача вырожденная.");
				}
				else if (j != i)
				{
					//Если текущая строка с нулевым i-ым коэффициентом, то меняем строки местами
					lines[j] = lines[i];
					lines[i] = line;
				}

				#endregion

				//Нормируем строчку i. 
				lines[i] /= lines[i].A[i];

				for (j = i + 1; j < lines.Length; j++)
				{
					lines[j] -= lines[j].A[i]*lines[i];
				}
			}

			#endregion

			#region Обратная итерация (Превращаем в единичную матрицу)

			for (var i = lines.Length - 1; i > 0; i--)
			{
				var y = lines[i].Y;
				for (var j = i - 1; j >= 0; j--)
				{
					if (i < lines[j].A.Length)
					{
						lines[j].Y -= lines[j].A[i]*y;

						//Необязательно, просто для наглядности
						lines[j].A[i] = 0;
					}
				}
			}

			#endregion

			var result = new double[lines.Length];
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = lines[i].Y;
			}
			return result;
		}
	}
}