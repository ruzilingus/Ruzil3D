using System.Diagnostics;

namespace Ruzil3D.Utility
{
	/// <summary>
	/// Представляет сверхточный счетчик времени.
	/// </summary>
	/// <remarks>Основан на <see cref="Stopwatch"/>: в Windows он использует тот же QueryPerformanceCounter,
	/// а в отличие от прямого вызова kernel32.dll работает и на других платформах.</remarks>
	public static class TickCounter
	{
		//Время выполнения такта (в миллисекундах)
		private static readonly double Scale = 1000D/Stopwatch.Frequency;

		/// <summary>
		/// Получает истекшее время в миллисекундах.
		/// </summary>
		public static double TickCount => Stopwatch.GetTimestamp()*Scale;
	}
}
