using Ruzil3D.Algebra;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет круг на плоскости.
	/// </summary>
	public struct Circle
	{
		/// <summary>
		/// Получает или задает центр круга.
		/// </summary>
		public PointD Center;

		/// <summary>
		/// Получает или задает радиус круга.
		/// </summary>
		public double Radius;

		/// <summary>
		/// Инициализирует новый круг с заданным центром и радиусом.
		/// </summary>
		/// <param name="center">Координаты центра круга.</param>
		/// <param name="radius">Радиус круга.</param>
		public Circle(PointD center, double radius)
		{
			Center = center;
			Radius = radius;
		}

		/// <summary>
		/// Получает значение указывающее, что заданная точка находится внутри данного круга.
		/// </summary>
		/// <param name="point">Точка, которая проверяется на нахождене в центре данного круга.</param>
		/// <returns>Значение указывающее, что заданная точка находится внутри данного круга.</returns>
		public bool Contains(PointD point)
		{
			return (point - Center).Length < Radius;
		}
	}
}