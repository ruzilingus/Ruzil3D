namespace Ruzil3D.Curves
{
	/// <summary>
	/// Детальная характеристика кривой в точке.
	/// </summary>
	/// <typeparam name="T">Тип кривой. Наследник от ParametricCurve.</typeparam>
	/// <remarks>Описывает точку последовательности кривых (<see cref="ParametricCurves{T}.GetDetails"/>) относительно той
	/// кривой последовательности, на которой она лежит.</remarks>
	public struct CurveDetails<T> where T : ParametricCurve
	{
		/// <summary>
		/// Кривая последовательности, на которой лежит точка.
		/// </summary>
		public readonly T Curve;

		/// <summary>
		/// Расстояние от начальной точки кривой <see cref="Curve"/> до точки.
		/// </summary>
		/// <remarks>Расстояние отсчитывается от начала кривой с индексом <see cref="Index"/>, а не от начала
		/// последовательности: например, для последовательности из двух кривых длиной 3 точке на расстоянии 4.5 от начала
		/// последовательности соответствует расстояние 1.5.</remarks>
		public readonly double Distance;

		/// <summary>
		/// Параметр кривой <see cref="Curve"/>, соответствующий точке.
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
		/// <param name="distance">Расстояние от начальной точки кривой <paramref name="curve"/>.</param>
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