using System;
using System.Collections.Generic;
using System.Linq;
using Ruzil3D.Algebra;

namespace Ruzil3D.Curves
{
	/// <summary>
	/// Последовательность кривых Безье из массива структур Point3D.
	/// </summary>
	public class Beziers : ParametricCurves<BezierCurve>
	{
		/// <summary>
		/// Массив структур <see cref="Point3D"/> описывающий последовательность кривых <see cref="BezierCurve"/>.
		/// </summary>
		private Point3D[] _points;

		/// <summary>
		/// Возвращает узловые точки кривой.
		/// </summary>
		/// <returns>Узловые точки кривой</returns>
		public Point3D[] GetPoints()
		{
			var result = new Point3D[_points.Length];
			_points.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Beziers"/> из массива структур.
		/// </summary>
		/// <param name="points">Массив узловых точек кривой.</param>
		public Beziers(Point3D[] points)
		{
			if (points.Length%3 != 1)
			{
				throw new ArgumentException("Кривая должна содержать 3n+1 точек.", nameof(points));
			}

			var pointsCopy = new Point3D[points.Length];
			points.CopyTo(pointsCopy, 0);

			InitFromArray(pointsCopy);
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Beziers"/> из коллекции структур.
		/// </summary>
		/// <param name="points">Коллекция узловых точек кривой.</param>
		public Beziers(ICollection<Point3D> points)
		{
			if (points.Count%3 != 1)
			{
				throw new ArgumentException("Кривая должна содержать 3n+1 точек.", nameof(points));
			}

			var pointsCopy = new Point3D[points.Count];
			points.CopyTo(pointsCopy, 0);

			InitFromArray(pointsCopy);
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Beziers"/> из перечислителя структур.
		/// </summary>
		/// <param name="points">Перечислитель узловых точек кривой.</param>
		public Beziers(IEnumerable<Point3D> points)
		{
			var pointsCopy = points.ToArray();

			if (pointsCopy.Length%3 != 1)
			{
				throw new ArgumentException("Кривая должна содержать 3n+1 точек.", nameof(points));
			}

			InitFromArray(pointsCopy);
		}

		private void InitFromArray(Point3D[] points)
		{
			_points = points;

			Compilers = new ParametricCurveDistanceCompiler<BezierCurve>[_points.Length/3];
			for (var i = 1; i < _points.Length; i += 3)
			{
				var bezier = new BezierCurve(_points[i - 1], _points[i], _points[i + 1], _points[i + 2]);
				Compilers[i/3] = new ParametricCurveDistanceCompiler<BezierCurve>(bezier);
			}
		}
	}
}