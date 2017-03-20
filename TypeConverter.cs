using System;

namespace Ruzil3D
{
	/// <summary>
	/// Преобразует базовые типы чисел в массив байтов и массив байтов в базовые типы чисел.
	/// </summary>
	public static class TypeConverter
	{
		private static long GetLong(byte[] buffer, int from, int step, int length)
		{
			var result = 0L;

			for (var i = 0; i < length; i++)
			{
				result = result * 256 + buffer[from];
				from += step;
			}

			return result;
		}

		private static long GetLong(byte[] buffer, int startIndex, int length)
		{
			if (startIndex >= 0 && buffer.Length - startIndex >= length)
			{
				return GetLong(buffer, startIndex + length - 1, -1, length);
			}

			if (startIndex < 0 && startIndex <= 1 - length)
			{
				return GetLong(buffer, startIndex + length - 1, 1, length);
			}

			throw new ArgumentOutOfRangeException();
		}

		/// <summary>
		/// Возвращает 8-битовое целое число со знаком, преобразованное из четырех байтов с указанной позицией в массиве байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов.</param>
		/// <param name="startIndex">Позиция начала в <paramref name="buffer"/>. Если <paramref name="startIndex"/> &lt; 0 то порядок байт считается в обратном направлении.</param>
		/// <returns>8-битовое целое число со знаком, преобразованное из четырех байтов с указанной позицией в массиве байтов.</returns>
		public static sbyte ToSByte(byte[] buffer, int startIndex)
		{
			return (sbyte)GetLong(buffer, startIndex, 1);
		}

		/// <summary>
		/// Возвращает 16-битовое целое число со знаком, преобразованное из четырех байтов с указанной позицией в массиве байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов.</param>
		/// <param name="startIndex">Позиция начала в <paramref name="buffer"/>. Если <paramref name="startIndex"/> &lt; 0 то порядок байт считается в обратном направлении.</param>
		/// <returns>16-битовое целое число со знаком, преобразованное из четырех байтов с указанной позицией в массиве байтов.</returns>
		public static short ToInt16(byte[] buffer, int startIndex)
		{
			return (short)GetLong(buffer, startIndex, 2);
		}

		/// <summary>
		/// Возвращает 32-битовое целое число со знаком, преобразованное из четырех байтов с указанной позицией в массиве байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов.</param>
		/// <param name="startIndex">Позиция начала в <paramref name="buffer"/>. Если <paramref name="startIndex"/> &lt; 0 то порядок байт считается в обратном направлении.</param>
		/// <returns>32-битовое целое число со знаком, преобразованное из четырех байтов с указанной позицией в массиве байтов.</returns>
		public static int ToInt32(byte[] buffer, int startIndex)
		{
			return (int)GetLong(buffer, startIndex, 4);
		}

		/// <summary>
		/// Возвращает 64-битовое целое число со знаком, преобразованное из четырех байтов с указанной позицией в массиве байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов.</param>
		/// <param name="startIndex">Позиция начала в <paramref name="buffer"/>. Если <paramref name="startIndex"/> &lt; 0 то порядок байт считается в обратном направлении.</param>
		/// <returns>64-битовое целое число со знаком, преобразованное из четырех байтов с указанной позицией в массиве байтов.</returns>
		public static long ToInt64(byte[] buffer, int startIndex)
		{
			return GetLong(buffer, startIndex, 8);
		}

		/// <summary>
		/// Возвращает 8-битовое целое число без знака, преобразованное из четырех байтов с указанной позицией в массиве байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов.</param>
		/// <param name="startIndex">Позиция начала в <paramref name="buffer"/>. Если <paramref name="startIndex"/> &lt; 0 то порядок байт считается в обратном направлении.</param>
		/// <returns>8-битовое целое число без знака, преобразованное из четырех байтов с указанной позицией в массиве байтов.</returns>
		public static byte ToByte(byte[] buffer, int startIndex)
		{
			return (byte)GetLong(buffer, startIndex, 1);
		}

		/// <summary>
		/// Возвращает 16-битовое целое число без знака, преобразованное из четырех байтов с указанной позицией в массиве байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов.</param>
		/// <param name="startIndex">Позиция начала в <paramref name="buffer"/>. Если <paramref name="startIndex"/> &lt; 0 то порядок байт считается в обратном направлении.</param>
		/// <returns>16-битовое целое число без знака, преобразованное из четырех байтов с указанной позицией в массиве байтов.</returns>
		public static ushort ToUInt16(byte[] buffer, int startIndex)
		{
			return (ushort)GetLong(buffer, startIndex, 2);
		}

		/// <summary>
		/// Возвращает 32-битовое целое число без знака, преобразованное из четырех байтов с указанной позицией в массиве байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов.</param>
		/// <param name="startIndex">Позиция начала в <paramref name="buffer"/>. Если <paramref name="startIndex"/> &lt; 0 то порядок байт считается в обратном направлении.</param>
		/// <returns>32-битовое целое число без знака, преобразованное из четырех байтов с указанной позицией в массиве байтов.</returns>
		public static uint ToUInt32(byte[] buffer, int startIndex)
		{
			return (uint)GetLong(buffer, startIndex, 4);
		}

		/// <summary>
		/// Возвращает 64-битовое целое число без знака, преобразованное из четырех байтов с указанной позицией в массиве байтов.
		/// </summary>
		/// <param name="buffer">Массив байтов.</param>
		/// <param name="startIndex">Позиция начала в <paramref name="buffer"/>. Если <paramref name="startIndex"/> &lt; 0 то порядок байт считается в обратном направлении.</param>
		/// <returns>64-битовое целое число без знака, преобразованное из четырех байтов с указанной позицией в массиве байтов.</returns>
		public static ulong ToUInt64(byte[] buffer, int startIndex)
		{
			return (ulong)GetLong(buffer, startIndex, 8);
		}
	}
}