using System;
using System.Collections.Generic;
using Ruzil3D.Utility;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет многомерную матрицу.
	/// </summary>
	public struct Matrix : ICloneable
	{
		#region Fields

		/// <summary>
		/// Строки матрицы представленные в виде структур <see cref="Vector"/>.
		/// </summary>
		private readonly Vector[] _lines;

		#endregion

		#region Properties

		/// <summary>
		/// Получает или задает строку матрицы по индексу.
		/// </summary>
		/// <param name="index">Индекс строки матрицы.</param>
		/// <returns>Строка матрицы под номером <paramref name="index"/> представленная структурой <see cref="Vector"/>.</returns>
		public Vector this[int index]
		{
			get { return _lines.Length > index ? _lines[index] : Vector.Empty; }
			set
			{
				/*
				if (A.Length <= index)
				{
				    Array.Resize(ref A, index + 1);
				    //double[] aCopy = A;
				    //A = new double[index+1];
				    //aCopy.CopyTo(A, 0);
				}
				 */
				_lines[index] = value;

			}
		}

		/// <summary>
		/// Получает элемент матрицы соответствующий мультииндексу строка-столбец.
		/// </summary>
		/// <param name="i">Индекс строки.</param>
		/// <param name="j">Индекс столбца</param>
		/// <returns>Элемент матрицы соответствующий мультииндексу строка-столбец.</returns>
		public double this[int i, int j]
		{
			get
			{
				var line = this[i];
				return line[j];
			}
			set
			{
				var line = this[i];
				line[j] = value;
			}
		}

		/// <summary>
		/// Получает количесво строк матрицы.
		/// </summary>
		public int Length => _lines.Length;

		/// <summary>
		/// Возвращает значение, показывающее, является ли данная матрица нулевой.
		/// </summary>
		/// <param name="x">Матрица.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="x"/> равняется <see cref="Empty"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsEmpty(Matrix x) => x == Empty;


		#endregion

		#region Static Methods

		/// <summary>
		/// Представляет пустую матрицу.
		/// </summary>
		public static readonly Matrix Empty = new Matrix(new Vector[0]);

		/// <summary>
		/// Возвращает единичную матрицу указанной размерности.
		/// </summary>
		/// <param name="size">Размерность матрицы.</param>
		/// <returns>Единичная матрица указанной размерности.</returns>
		public static Matrix GetIdentity(int size)
		{
			var lines = new List<Vector>();
			for (var i = 0; i < size; i++)
			{
				var line = new Vector(new double[i + 1]) {[i] = 1};
				lines.Add(line);
			}

			return new Matrix(lines.ToArray());
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр <see cref="Matrix"/> с указанными строками.
		/// </summary>
		/// <param name="lines">Строки матрицы.</param>
		public Matrix(Vector[] lines)
		{
			_lines = lines;
		}

		#endregion

		#region Overloads

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух матриц.
		/// </summary>
		/// <param name="x">Первая матрица для сравнения.</param>
		/// <param name="y">Вторая матрица для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Matrix x, Matrix y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух матриц.
		/// </summary>
		/// <param name="x">Первая матрица для сравнения.</param>
		/// <param name="y">Вторая матрица для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Matrix x, Matrix y)
		{
			Matrix xArray, yArray;
			if (x.Length < y.Length)
			{
				xArray = x;
				yArray = y;
			}
			else
			{
				xArray = y;
				yArray = x;
			}

			for (var i = xArray.Length; i < yArray.Length; i++)
			{
				if (yArray[i] != Vector.Empty)
				{
					return false;
				}
			}


			for (var i = 0; i < xArray.Length; i++)
			{
				if (xArray[i] != yArray[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Возвращает аддитивную инверсию матрицы заданного параметром <paramref name="x"/>.
		/// </summary>
		/// <param name="x">Инвертируемое значение.</param>
		/// <returns>Результат умножения исходной матрицы на -1.</returns>
		public static Matrix operator -(Matrix x)
		{
			var result = new Matrix(new Vector[x.Length]);

			for (var i = 0; i < x.Length; i++)
			{
				result[i] = -x[i];
			}

			return result;
		}

		/// <summary>
		/// Возвращает исходную матрицу.
		/// </summary>
		/// <param name="x">Исходная матрица.</param>
		/// <returns>Исходная матрица.</returns>
		public static Matrix operator +(Matrix x)
		{
			return x;
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Исходная матрица.</param>
		/// <param name="y">Множитель с плавающей запятой двойной точности.</param>
		/// <returns>Произведение <paramref name="y"/> и <paramref name="x"/>.</returns>
		public static Matrix operator *(Matrix x, double y)
		{
			var result = new Matrix(new Vector[x.Length]);

			for (var i = 0; i < x.Length; i++)
			{
				result[i] = y*x[i];
			}

			return result;
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <param name="y">Исходная матрица.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Matrix operator *(double x, Matrix y)
		{
			return y*x;
		}

		/// <summary>
		/// Делит исходную матрицу на число с плавающей запятой двойной точности и возвращает результат.
		/// </summary>
		/// <param name="x">Исходная матрица.</param>
		/// <param name="y">Знаменатель с плавающей запятой двойной точности.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Matrix operator /(Matrix x, double y)
		{
			return x*(1D/y);
		}

		/// <summary>
		/// Делит число с плавающей запятой двойной точности на исходную матрицу и возвращает результат.
		/// </summary>
		/// <param name="x">Числитель с плавающей запятой двойной точности.</param>
		/// <param name="y">Исходная матрица.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Matrix operator /(double x, Matrix y)
		{
			return x*y.GetInverse();
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на вектор, преставленный структурой <see cref="Vector"/>.
		/// </summary>
		/// <param name="x">Исходная матрица.</param>
		/// <param name="y">Вектор для перемножения.</param>
		/// <returns>Произведение исходной матрицы на вектор <paramref name="y"/>.</returns>
		public static Vector operator *(Matrix x, Vector y)
		{
			var result = new double[y.Length];

			for (var i = 0; i < x.Length; i++)
			{
				result[i] = x[i].DotProduct(y);
			}

			return new Vector(result);
		}

		/// <summary>
		/// Складывает две матрицы представленные структурой <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">Первое из складываемых матриц.</param>
		/// <param name="y">Второе из складываемых матриц.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Matrix operator +(Matrix x, Matrix y)
		{
			Matrix xArray, yArray;
			if (x.Length < y.Length)
			{
				xArray = x;
				yArray = y;
			}
			else
			{
				xArray = y;
				yArray = x;
			}

			var result = new Matrix(new Vector[y.Length]);

			for (var i = 0; i < xArray.Length; i++)
			{
				result[i] = xArray[i] + yArray[i];
			}

			for (var i = xArray.Length; i < yArray.Length; i++)
			{
				result[i] = yArray[i];
			}

			return result;
		}

		/// <summary>
		/// Вычитает две матрицы представленные структурой <see cref="Matrix"/>.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Matrix operator -(Matrix x, Matrix y)
		{
			return x + -y;
		}

		/// <summary>
		/// Возвращает произведение двух матриц.
		/// </summary>
		/// <param name="x">Первая матрица для перемножения.</param>
		/// <param name="y">Вторая матрица для перемножения.</param>
		/// <returns>Произведение двух матриц.</returns>
		public static Matrix operator *(Matrix x, Matrix y)
		{
			var result = new List<Vector>();

			var jyMax = 0;
			for (var i = 0; i < y.Length; i++)
			{
				jyMax = Math.Max(jyMax, y[i].Length);
			}

			for (var ix = 0; ix < x.Length; ix++)
			{
				var line = new Vector(new double[jyMax]);
				for (var jy = 0; jy < jyMax; jy++)
				{
					//Скалярное произведение строки x[i] со столбцом y[j]
					double sum = 0;
					for (var iy = 0; iy < y.Length; iy++)
					{
						sum += x[ix, iy]*y[iy, jy];
						//sum += 1000000000000 * x[ix, iy] * y[iy, jy];
					}
					line[jy] = sum;
					//line[jy] = sum / 1000000000000;
				}
				result.Add(line);
			}

			return new Matrix(result.ToArray());
		}

		/// <summary>
		/// Делит одну матрицу на другую и возвращает результат.
		/// </summary>
		/// <param name="x">Матрица-числитель.</param>
		/// <param name="y">Матрица - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="y"/>.</returns>
		public static Matrix operator /(Matrix x, Matrix y)
		{
			return x*y.GetInverse();
		}


		#endregion

		#region Methods

		/// <summary>
		/// Возвращает матрицу, обратную текущей.
		/// </summary>
		/// <returns>Матрица, обратная текущей.</returns>
		public Matrix GetInverse()
		{
			var inverse = GetIdentity(Length);
			var current = (Matrix) Clone();

			#region Прямая итерация (Превращаем в треугольную матрицу c единичной диагональю)

			for (var i = 0; i < current.Length; i++)
			{
				#region Преобразования над строками. Для гарантии lines[i].A[i] != 0

				//Ищем ближайшую строчку с ненулевым i-ым коэффициентом
				var j = i;
				Vector line;
				do
				{
					line = current[j];

					// ReSharper disable once CompareOfFloatsByEqualityOperator
					if (line[i] == 0)
					{
						j++;
					}
					else
					{
						break;
					}
				} while (j < current.Length);

				if (j == current.Length)
				{
					//Если не нашли такую строчку
					throw new DivideByZeroException("Матрица вырожденная");
				}
				else if (j != i)
				{
					//Если текущая строка с нулевым i-ым коэффициентом, то меняем строки местами
					current[j] = current[i];
					current[i] = line;

					//То же самое проделываем со второй матрицей
					var save = inverse[j];
					inverse[j] = inverse[i];
					inverse[i] = save;
				}

				#endregion

				//Нормируем строчку i. 
				var norm = current[i, i];
				current[i] /= norm;
				inverse[i] /= norm;

				for (j = i + 1; j < current.Length; j++)
				{
					norm = -current[j, i];
					current[j] += norm*current[i];
					inverse[j] += norm*inverse[i];
				}
			}

			#endregion

			#region Обратная итерация (Превращаем в единичную матрицу)

			for (var i = current.Length - 1; i > 0; i--)
			{
				for (var j = i - 1; j >= 0; j--)
				{
					var norm = -current[j, i];
					inverse[j] += norm*inverse[i];

					//Необязательно, просто для наглядности
					//current[j,i] = 0;
				}
			}

			#endregion


			return inverse;
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Возвращает строковое представлеине данной матрицы.
		/// </summary>
		/// <returns>Строковое представлеине данной матрицы.</returns>
		/// <remarks>
		/// <code>
		/// var matrix = new Matrix(new[] {new Vector(1, 2, 3), new Vector(3, 4)});
		/// Console.Write(matrix); //Результат: l̄₁ = x̄₁ + 2 x̄₂ + 3 x̄₃ l̄₂ = 3 x̄₁ + 4 x̄₂
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			var result = "";

			var isEmpty = true;

			//Знак вектора
			var line = "l" + CStatic.Macron;

			for (var i = 0; i < Length; i++)
			{
				if (i > 0)
				{
					result += CStatic.EmSp;
				}

				var vector = this[i];
				isEmpty &= vector == Vector.Empty;
				result += line + CStatic.GetIndex(i + 1) + " = " + vector;
			}

			return isEmpty ? "Ø" : result;
		}


		#endregion

		#region Equality members


		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другая матрица.</param>
		/// <returns>Значение <b>true</b>, если две матрицы совпадают; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Matrix other)
		{
			return this == other;
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Matrix"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Matrix && Equals((Matrix) obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		public override int GetHashCode() => 0;

		#endregion

		#region IClonable Items

		/// <summary>
		/// Создает новый объект, который является копией текущего экземпляра.
		/// </summary>
		/// <returns>
		/// Новый объект, являющийся копией этого экземпляра.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public object Clone() => new Matrix(_lines.Clone() as Vector[]);

		#endregion
	}
}