namespace Ruzil3D.Approximation
{
	/// <summary>
	/// Представляет интерфейс аппроксимирующей функции для вещественной монотонной функции одной переменной.
	/// </summary>
	public interface IMonotonicFunctionApproximation : IFunctionApproximation
	{
		/// <summary>
		/// Получает значение функции на левой границе области определения.
		/// </summary>
		double LValue { get; }

		/// <summary>
		/// Получает значение функции на правой границе области определения.
		/// </summary>
		double RValue { get; }

		/// <summary>
		/// Получает минимальное значение функции в области определения.
		/// </summary>
		double MinValue { get; }

		/// <summary>
		/// Получает максимальное значение функции области определения.
		/// </summary>
		double MaxValue { get; }

		/// <summary>
		/// Возвращает аргумент соответствующий значению функции из области значений.
		/// </summary>
		/// <param name="y">Значение функции.</param>
		/// <returns>Аргумент соответствующий значению функции.</returns>
		/// <remarks>Функция соответствует аппроксимации обратной функции.</remarks>
		double GetArgument(double y);
	}
}