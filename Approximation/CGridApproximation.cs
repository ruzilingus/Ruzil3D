using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Approximation
{
	/// <summary>
	/// Представляет абстрактный класс сеточной аппроксимации одномерных функций.
	/// </summary>
	/// <remarks>В каждой отдельной клетке сетки своя кусочно-аппроксимирующая функция.</remarks>
	public abstract class CGridApproximation : IFunctionApproximation
	{
		/// <summary>
		/// Набор аргументов и значений аппроксимируемой функции в узлах представленный массивом структур <see cref="PointD"/>.
		/// </summary>
		protected readonly PointD[] Points;

		#region Private Items

		private class PointXComparer : IComparer<PointD>
		{
			public int Compare(PointD x, PointD y)
			{
				return x.X.CompareTo(y.X);
			}
		}

		private static readonly PointXComparer Comparer = new PointXComparer();

		private static PointD[] Format(IEnumerable<PointD> points)
		{
			var result = new List<PointD>();
			result.AddRange(points);
			result.Sort(Comparer);

			for (var i = result.Count - 1; i > 0; i--)
			{
				var p0 = result[i];
				var p1 = result[i - 1];

				if (p0 == p1)
				{
					result.RemoveAt(i);
				}
				else if (p0.X.Equals(p1.X))
				{
					throw new ArgumentException("Разным значениям Y должны соответствовать разные аргументы X");
				}
			}

			return result.ToArray();
		}

		#endregion

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CGridApproximation"/> из перечислителя структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Перечислитель структур <see cref="PointD"/>.</param>
		/// <exception cref="ArgumentException">Нескольким одинаковым аргументам X соответствуют разные значения Y.</exception>
		protected CGridApproximation(IEnumerable<PointD> points)
		{
			Points = Format(points);

			LArgument = Points[0].X;
			RArgument = Points[Points.Length - 1].X;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CGridApproximation"/> из массива структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Массив структур <see cref="PointD"/>.</param>
		/// <param name="check">Условие указывающее на необходимость проверить исходные данные на корректность.</param>
		protected CGridApproximation(PointD[] points, bool check = false)
		{
			Points = check ? Format(points) : points;

			LArgument = Points[0].X;
			RArgument = Points[Points.Length - 1].X;
		}

		/// <summary>
		/// Получает левую границу области определения аппроксимирующей функции.
		/// </summary>
		/// <value>Левая граница области определения аппроксимирующей функции.</value>
		public double LArgument { get; }

		/// <summary>
		/// Правая граница области определения аппроксимирующей функции.
		/// </summary>
		/// <value>Правая граница области определения аппроксимирующей функции.</value>
		public double RArgument { get; }

		/// <summary>
		/// Возвращает значение аппроксимирующей функции для любого аргумента области определения.
		/// </summary>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Значение аппроксимации соответствующее аргументу.</returns>
		public virtual double GetValue(double x)
		{
			return GetValue(GetIndex(x), x);
		}

		/// <summary>
		/// Возвращает значение кусочно-аппроксимирующей функции для соответствующего аргумента <paramref name="x"/>.
		/// </summary>
		/// <param name="index">Индекс клетки сетки.</param>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Значение аппроксимации соответствующее индексу и аргументу.</returns>
		protected abstract double GetValue(int index, double x);

		/// <summary>
		/// Возвращает индекс клетки сетки соответствующее аргументу.
		/// </summary>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Индекс клетки сетки.</returns>
		public int GetIndex(double x)
		{
			if (x < LArgument || x > RArgument)
			{
				throw new ArgumentOutOfRangeException(nameof(x),
					"Недопустимый аргумент. Аргумент \"" + nameof(x) + "\" находится вне области определения функции.");
			}

			var min = 1;
			var max = Points.Length - 1;

			while (min != max)
			{
				var mid = (min + max)/2;

				if (x < Points[mid].X)
				{
					max = mid;
				}
				else
				{
					min = mid + 1;
				}
			}

			return min - 1;
		}

		/// <summary>
		/// Возращает набор узловых точек аппроксимируемой функции представленных массивом структур <see cref="PointD"/> отсортированные по возрастанию аргумента.
		/// </summary>
		/// <returns>Массив пар аргумент-значение узловых точек аппроксимируемой функции.</returns>
		public PointD[] ToArray()
		{
			return Points.Clone() as PointD[];
		}
	}
}