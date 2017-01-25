using Ruzil3D.Algebra;

namespace Ruzil3D.Curves
{
	/// <summary>
	/// Отрезок прямой в трехмерном евклидовом пространстве.
	/// </summary>
	public class LineCurve : BernsteinCurve
	{
		#region Main

		/// <summary>
		///  Получает координату P0 кривой <see cref="LineCurve"/> представленную структурой <see cref="Point3D"/>.
		/// </summary>
		public Point3D P0 => Points[0];

		/// <summary>
		///  Получает координату P1 кривой <see cref="LineCurve"/> представленную структурой <see cref="Point3D"/>.
		/// </summary>
		public Point3D P1 => Points[1];

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="LineCurve"/> с указанными узловыми точками.
		/// </summary>
		/// <param name="p0">Первая узловая точка.</param>
		/// <param name="p1">Вторая узловая точка.</param>
		public LineCurve(Point3D p0, Point3D p1) : base(new[] {p0, p1})
		{
		}

		private LineCurve(Point3D[] points) : base(points)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Разбивиет кривую на две в заданной произвольным параметром точке и возвращает массив кривых <see cref="LineCurve"/>.
		/// </summary>
		/// <param name="parameter">Параметр точки разбиения кривой.</param>
		/// <returns>Массив кривых.</returns>
		public LineCurve[] Split(double parameter)
		{
			var points1 = new Point3D[Points.Length];
			var points2 = new Point3D[Points.Length];
			SplitPoints(parameter, points1, points2);
			return new[] { new LineCurve(points1), new LineCurve(points2) };
		}

		/// <summary>
		/// Разбивиет кривую на две в точке соответствующей значению параметра 0.5 и возвращает массив кривых <see cref="LineCurve"/>.
		/// </summary>
		/// <returns>Массив кривых.</returns>
		public LineCurve[] Split()
		{
			return Split(0.5);
		}

		/// <summary>
		/// Возвращает участок отрезка между двумя точками заданных параметрически.
		/// </summary>
		/// <param name="t0">Параметр начала участка кривой.</param>
		/// <param name="t1">Параметр конца участка кривой.</param>
		/// <returns>Участок кривой.</returns>
		public LineCurve GetPart(double t0, double t1)
		{
			return new LineCurve(GetPartPoints(t0, t1));
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Вычисляет и возвращает длину кривой.
		/// </summary>
		/// <returns>Длина кривой.</returns>
		protected override double GetLength() => P0.Distance(P1);

		/// <summary>
		/// Вычисляет и возвращают длину отрезка между параметрами t0 до t1.
		/// </summary>
		/// <param name="t0">Параметр начала участка.</param>
		/// <param name="t1">Параметр конца участка.</param>
		/// <returns>Длина участка кривой.</returns>
		public override double GetDistance(double t0, double t1) => Length*Math.Abs(t1 - t0);

		/// <summary>
		/// Приводит отрезок к кривой Безье.
		/// </summary>
		/// <returns>Возвращают кривую Безье близкую к заданной.</returns>
		public override BezierCurve RecastToBezierCurve() => new BezierCurve(P0, P0, P1, P1);

		#endregion
	}
}