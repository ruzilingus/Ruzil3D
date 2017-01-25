namespace Ruzil3D.Curves
{
	/// <summary>
	/// Детальная характеристика кривой в точке.
	/// </summary>
	/// <typeparam name="T">Тип кривой. Наследник от ParametricCurve.</typeparam>
	public struct CurveDetails<T> where T : ParametricCurve
	{
		/// <summary>
		/// Кривая.
		/// </summary>
		public readonly T Curve;

		/// <summary>
		/// Растояние от начальной точки кривой.
		/// </summary>
		public readonly double Distance;

		/// <summary>
		/// Параметр кривой.
		/// </summary>
		public readonly double Parameter;

		/// <summary>
		/// Индекс кривой в последовательности.
		/// </summary>
		public readonly int Index;

		/// <summary>
		/// Инициализирует новый экземпляр.
		/// </summary>
		/// <param name="index">Индекс кривой в последовательности.</param>
		/// <param name="curve">Кривая.</param>
		/// <param name="distance">Растояние от начальной точки кривой.</param>
		/// <param name="parameter">Параметр кривой.</param>
		public CurveDetails(double distance, int index, T curve, double parameter)
		{
			Distance = distance;
			Index = index;
			Parameter = parameter;
			Curve = curve;
		}
	}
}