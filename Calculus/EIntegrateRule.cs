namespace Ruzil3D.Calculus
{
	/// <summary>
	/// Перечисление методов одномерного числинного интегрирования.
	/// </summary>
	public enum EIntegrateRule
	{
		/// <summary>
		/// Установленный по умолчанию разработчиком метод.
		/// </summary>
		Default,
		
		/// <summary>
		/// Метод центральных прямоугольников.
		/// </summary>
		Rectangle,

		/// <summary>
		/// Метод трапеций.
		/// </summary>
		Trapezoidal,

		/// <summary>
		/// Метод парабол Ньютона-Симпсона.
		/// </summary>
		Simpson,

		/// <summary>
		/// Квадратурный метод Гаусса-2.
		/// </summary>
		Gauss2,

		/// <summary>
		/// Квадратурный метод Гаусса-3.
		/// </summary>
		Gauss3,

		/// <summary>
		/// Квадратурный метод Гаусса-5.
		/// </summary>
		Gauss5,

		/// <summary>
		/// Квадратурный метод Гаусса-6.
		/// </summary>
		Gauss6,

		/// <summary>
		/// Квадратурный метод Гаусса-10.
		/// </summary>
		Gauss10
	}
}