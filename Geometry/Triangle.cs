using Ruzil3D.Algebra;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет треугольник на плоскости.
	/// </summary>
	public struct Triangle
	{
		/// <summary>
		/// Получает первую вершину данного треугольника.
		/// </summary>
		public readonly PointD Point0;

		/// <summary>
		/// Получает вторую вершину данного треугольника.
		/// </summary>
		public readonly PointD Point1;

		/// <summary>
		/// Получает третью вершину данного треугольника.
		/// </summary>
		public readonly PointD Point2;

		/// <summary>
		/// Центр окружности описанной вокруг данного треугольника.
		/// </summary>
		private PointD GetCircumCenter()
		{
			var dx01 = Point0.X - Point1.X;
			var dx12 = Point1.X - Point2.X;
			var dx20 = Point2.X - Point0.X;

			var dy01 = Point0.Y - Point1.Y;
			var dy12 = Point1.Y - Point2.Y;
			var dy20 = Point2.Y - Point0.Y;

			var d0 = Point0.X*Point0.X + Point0.Y*Point0.Y;
			var d1 = Point1.X*Point1.X + Point1.Y*Point1.Y;
			var d2 = Point2.X*Point2.X + Point2.Y*Point2.Y;

			var norm = 2*(dx01*dy20 - dy01*dx20);

			return new PointD(-(dy01*d2 + dy12*d0 + dy20*d1)/norm, (dx01*d2 + dx12*d0 + dx20*d1)/norm);
		}

		/// <summary>
		/// Возвращает окружность описанную вокруг данного треугольника.
		/// </summary>
		/// <returns>Окружность описанная вокруг данного треугольника.</returns>
		public Circle GetCircumscribed()
		{
			var center = GetCircumCenter();
			var radius = (center - Point0).Length;
			return new Circle(center, radius);
		}

		/// <summary>
		/// Инициализирует треугольник с вершинами в заданных точках.
		/// </summary>
		/// <param name="point0">Первая вершина треугольника.</param>
		/// <param name="point1">Вторая вершина треугольника.</param>
		/// <param name="point2">Третья вершина треугольника.</param>
		public Triangle(PointD point0, PointD point1, PointD point2)
		{
			Point0 = point0;
			Point1 = point1;
			Point2 = point2;
		}

		/// <summary>
		/// Возвращает площадь данного треугольника.
		/// </summary>
		public double GetArea() => ((Point1 - Point0)*(Point2 - Point0)).Length/2D;
	}
}