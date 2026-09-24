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
		/// <param name="point">Точка, которая проверяется на нахождение внутри данного круга.</param>
		/// <returns>Значение <b>true</b>, если расстояние от точки <paramref name="point"/> до центра меньше радиуса; в противном случае (в том числе для точки на границе круга) — значение <b>false</b>.</returns>
		public bool Contains(PointD point)
		{
			return (point - Center).Length < Radius;
		}
	}
}