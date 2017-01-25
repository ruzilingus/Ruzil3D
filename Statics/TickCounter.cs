namespace Ruzil3D.Utility
{
	/// <summary>
	/// Представляет сверхточный счетчик времени.
	/// </summary>
	public static class TickCounter
	{
		private static class NativeMethods
		{
			[System.Runtime.InteropServices.DllImport("kernel32.dll")]
			public static extern bool QueryPerformanceCounter(out long x);

			[System.Runtime.InteropServices.DllImport("kernel32.dll")]
			public static extern bool QueryPerformanceFrequency(out long x);
		}

		private static readonly double Scale;

		static TickCounter()
		{
			long frequency;
			NativeMethods.QueryPerformanceFrequency(out frequency);
			//Время выполнения такта (в миллисекундах)
			Scale = 1000D/frequency;
		}


		/// <summary>
		/// Получает истекшее время в миллисекундах.
		/// </summary>
		public static double TickCount
		{
			get
			{
				long current;
				NativeMethods.QueryPerformanceCounter(out current);
				return current*Scale;
			}
		}
	}
}