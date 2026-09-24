using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет треугольник в трехмерном евклидовом пространстве.
	/// </summary>
	public struct Triangle3D
	{
		/// <summary>
		/// Получает первую вершину данного треугольника.
		/// </summary>
		public readonly Point3D Point0;
		
		/// <summary>
		/// Получает вторую вершину данного треугольника.
		/// </summary>
		public readonly Point3D Point1;

		/// <summary>
		/// Получает третью вершину данного треугольника.
		/// </summary>
		public readonly Point3D Point2;

		/*
		public Point3D CircumCenter
		{
		    get
		    {
			 Point3D d01 = Point0 - Point1;
		    Point3D d12 = Point1 - Point2;
		    Point3D d20 = Point2 - Point0;

		    double d0 = Point0.Length;
		    double d1 = Point1.Length;
		    double d2 = Point2.Length;

		    double norm = 2 * (dx01 * dy20 - dy01 * dx20);

		    return new PointD(-(dy01 * d2 + dy12 * d0 + dy20 * d1) / norm, (dx01 * d2 + dx12 * d0 + dx20 * d1) / norm);
		    }
		}
		*/
		
		private class CVertexComparer : IComparer<Point3D>
		{
			public int Compare(Point3D x, Point3D y)
			{
				if (x.X < y.X)
				{
					return -1;
				}
				else if (x.X > y.X)
				{
					return 1;
				}
				else if (x.Y < y.Y)
				{
					return -1;
				}
				else if (x.Y > y.Y)
				{
					return 1;
				}
				else if (x.Z < y.Z)
				{
					return -1;
				}
				else if (x.Z > y.Z)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}

		/// <summary>
		/// Представляет лексикографическое сравнение вершин: сначала по координате X, затем по Y и по Z.
		/// </summary>
		public static readonly IComparer<Point3D> VertexComparer = new CVertexComparer();

		#region Contains Items

		private class CTriangles3DComparer : IComparer<Triangle3D>
		{
			public int Compare(Triangle3D x, Triangle3D y)
			{
				var result = VertexComparer.Compare(x.Point0, y.Point0);

				if (result == 0)
				{
					result = VertexComparer.Compare(x.Point1, y.Point1);

					if (result == 0)
					{
						result = VertexComparer.Compare(x.Point2, y.Point2);
					}

				}

				return result;
			}
		}

		/// <summary>
		/// Представляет лексикографическое сравнение треугольников: сначала по вершине <see cref="Point0"/>, затем по <see cref="Point1"/> и по <see cref="Point2"/>
		/// (вершины сравниваются с помощью <see cref="VertexComparer"/>).
		/// </summary>
		public static readonly IComparer<Triangle3D> Triangles3DComparer = new CTriangles3DComparer();

		/// <summary>
		/// Выполняет двоичный поиск треугольника в списке, упорядоченном с помощью <see cref="Triangles3DComparer"/>.
		/// </summary>
		/// <param name="sortedItems">Список треугольников, упорядоченный по возрастанию с помощью <see cref="Triangles3DComparer"/>.</param>
		/// <param name="item">Искомый треугольник.</param>
		/// <param name="index">Индекс найденного треугольника или, если он не найден, индекс, по которому его нужно вставить, чтобы сохранить порядок.</param>
		/// <returns>Значение <b>true</b>, если треугольник найден; в противном случае — значение <b>false</b>.</returns>
		[Obsolete("Метод устарел. Используйте List<Triangle3D>.BinarySearch с компаратором " + nameof(Triangles3DComparer) + ".")]
		public static bool Contains(List<Triangle3D> sortedItems, Triangle3D item, out int index)
		{
			if (sortedItems == null || sortedItems.Count == 0)
			{
				index = 0;
				return false;
			}

			var begIndex = 0;
			var beg = sortedItems[begIndex];

			if (sortedItems.Count == 1)
			{
				var comparerValue = Triangles3DComparer.Compare(item, beg);
				if (comparerValue <= 0)
				{
					index = begIndex;
				}
				else
				{
					index = begIndex + 1;
				}
				return comparerValue == 0;
			}

			var endIndex = sortedItems.Count - 1;
			var end = sortedItems[endIndex];

			if (Triangles3DComparer.Compare(item, beg) == -1)
			{
				index = 0;
				return false;
			}

			if (Triangles3DComparer.Compare(end, item) == -1)
			{
				index = endIndex + 1;
				return false;
			}

			int midIndex;
			do
			{
				midIndex = (begIndex + endIndex)/2;
				var midItem = sortedItems[midIndex];

				var comparerValue = Triangles3DComparer.Compare(item, midItem);
				if (comparerValue == 0)
				{
					index = midIndex;
					return true;
				}
				else if (comparerValue == -1)
				{
					endIndex = midIndex;
				}
				else
				{
					begIndex = midIndex;
				}
			} while (begIndex != endIndex - 1);

			if (begIndex == midIndex)
			{
				var comparerValue = Triangles3DComparer.Compare(item, sortedItems[endIndex]);
				if (comparerValue <= 0)
				{
					index = endIndex;
				}
				else
				{
					index = endIndex + 1;
				}
				return comparerValue == 0;
			}
			else
			{
				var comparerValue = Triangles3DComparer.Compare(item, sortedItems[begIndex]);
				if (comparerValue <= 0)
				{
					index = begIndex;
				}
				else
				{
					index = begIndex + 1;
				}
				return comparerValue == 0;
			}

		}

		#endregion

		/// <summary>
		/// Инициализирует треугольник с вершинами в заданных точках.
		/// </summary>
		/// <param name="point0">Первая вершина треугольника.</param>
		/// <param name="point1">Вторая вершина треугольника.</param>
		/// <param name="point2">Третья вершина треугольника.</param>
		/// <remarks>
		/// Вершины упорядочиваются и могут не совпадать с порядком параметров: <see cref="Point0"/> — наименьшая вершина в смысле <see cref="VertexComparer"/>,
		/// а <see cref="Point1"/> и <see cref="Point2"/> выбираются так, чтобы обход <see cref="Point0"/> → <see cref="Point1"/> → <see cref="Point2"/> в проекции на плоскость XY
		/// шел против часовой стрелки (если проекция вырождается в отрезок, <see cref="Point1"/> — большая, а <see cref="Point2"/> — меньшая из двух оставшихся вершин).
		/// Поэтому порядок обхода вершин и направление нормали исходного треугольника могут измениться на противоположные.
		/// </remarks>
		public Triangle3D(Point3D point0, Point3D point1, Point3D point2)
		{
			Point3D[] vertex = {point0, point1, point2};

			//Упорядочиваем вершины
			Array.Sort(vertex, VertexComparer);

			Point0 = vertex[0];

			var v1 = vertex[1] - vertex[0];
			var v2 = vertex[2] - vertex[0];

			//Если векторное произведение направлено вверх
			if (v1.X*v2.Y - v1.Y*v2.X > 0)
			{
				Point1 = vertex[1];
				Point2 = vertex[2];
			}
			else
			{
				Point1 = vertex[2];
				Point2 = vertex[1];
			}
		}

		/// <summary>
		/// Возвращает площадь данного треугольника.
		/// </summary>
		/// <returns>Площадь данного треугольника</returns>
		public double GetArea() => ((Point1 - Point0)*(Point2 - Point0)).Length/2D;
	}
}