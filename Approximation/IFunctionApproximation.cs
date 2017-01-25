namespace Ruzil3D.Approximation
{
	/// <summary>
	/// Представляет интерфейс аппроксимирующей функции для вещественной функции одной переменной.
	/// </summary>
	public interface IFunctionApproximation
	{
		/// <summary>
		/// Левая граница области определения аппроксимирующей функции.
		/// </summary>
		double LArgument { get; }

		/// <summary>
		/// Правая граница области определения аппроксимирующей функции.
		/// </summary>
		double RArgument { get; }

		/// <summary>
		/// Возвращает значение аппроксимирующей функции для любого аргумента области определения.
		/// </summary>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Значение аппроксимации соответствующее аргументу.</returns>
		double GetValue(double x);
	}
}