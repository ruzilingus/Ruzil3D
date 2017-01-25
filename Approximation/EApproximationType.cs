namespace Ruzil3D.Approximation
{
	/// <summary>
	/// Перечисление способ аппроксимации.
	/// </summary>
	public enum EApproximationType
	{
		/// <summary>
		/// Установленный по умолчанию разработчиком способ аппроксимации.
		/// </summary>
		Default,

		/// <summary>
		/// Самый быстро вычисляемый способ аппроксимации из данного списка.
		/// </summary>
		HighSpeed,

		/// <summary>
		/// Самый точный способ аппроксимации из данного списка.
		/// </summary>
		HighQuality,

		/// <summary>
		/// Линейная аппроксимация.
		/// </summary>
		Linear,

		/// <summary>
		/// Аппроксимация кубическими сплайнами.
		/// </summary>
		Cubic
	}
}