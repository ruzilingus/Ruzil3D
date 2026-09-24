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
		/// Смещение центра окружности, описанной вокруг данного треугольника, относительно вершины <see cref="Point0"/>.
		/// </summary>
		private PointD GetCircumCenterOffset()
		{
			//Вычисления ведутся относительно вершины Point0. Прежде использовались квадраты абсолютных координат, и вдали
			//от начала координат погрешность росла с их квадратом: при смещении 1e7 центр сдвигался на ~0.016, а для
			//прямоугольного треугольника в точке (1e8, 1e8) радиус получался нулевым.
			var bx = Point1.X - Point0.X;
			var by = Point1.Y - Point0.Y;
			var cx = Point2.X - Point0.X;
			var cy = Point2.Y - Point0.Y;

			var b2 = bx*bx + by*by;
			var c2 = cx*cx + cy*cy;

			var norm = 2*(bx*cy - by*cx);

			return new PointD((cy*b2 - by*c2)/norm, (bx*c2 - cx*b2)/norm);
		}

		/// <summary>
		/// Возвращает окружность описанную вокруг данного треугольника.
		/// </summary>
		/// <returns>Окружность описанная вокруг данного треугольника.</returns>
		/// <remarks>Если вершины треугольника лежат на одной прямой, центр и радиус окружности бесконечны или не являются числами (<see cref="double.NaN"/>).</remarks>
		public Circle GetCircumscribed()
		{
			var offset = GetCircumCenterOffset();
			return new Circle(Point0 + offset, offset.Length);
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