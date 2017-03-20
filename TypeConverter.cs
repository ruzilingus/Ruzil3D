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

		public static byte ToByte(byte[] buffer, int startIndex)
		{
			return (byte)GetLong(buffer, startIndex, 1);
		}

		public static short ToInt16(byte[] buffer, int startIndex)
		{
			return (short)GetLong(buffer, startIndex, 2);
		}

		public static int ToInt32(byte[] buffer, int startIndex)
		{
			return (int)GetLong(buffer, startIndex, 4);
		}

		public static long ToInt64(byte[] buffer, int startIndex)
		{
			return GetLong(buffer, startIndex, 8);
		}

		public static sbyte ToSByte(byte[] buffer, int startIndex)
		{
			return (sbyte)GetLong(buffer, startIndex, 1);
		}

		public static ushort ToUInt16(byte[] buffer, int startIndex)
		{
			return (ushort)GetLong(buffer, startIndex, 2);
		}

		public static uint ToUInt32(byte[] buffer, int startIndex)
		{
			return (uint)GetLong(buffer, startIndex, 4);
		}

		public static ulong ToUInt64(byte[] buffer, int startIndex)
		{
			return (ulong)GetLong(buffer, startIndex, 8);
		}
	}
}