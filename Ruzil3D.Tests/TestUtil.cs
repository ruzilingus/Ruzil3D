using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Threading;
using Ruzil3D.Algebra;
using Xunit;

namespace Ruzil3D.Tests
{
	/// <summary>
	/// Вспомогательные методы для тестов.
	/// </summary>
	internal static class TestUtil
	{
		/// <summary>
		/// Выполняет функцию в отдельном потоке и завершает тест с ошибкой, если она не уложилась в отведённое время.
		/// Нужен для регрессионных тестов на зависания: без него зависший тест останавливает весь прогон.
		/// </summary>
		public static T CompletesWithin<T>(Func<T> func, int milliseconds = 10000)
		{
			var result = default(T);
			ExceptionDispatchInfo error = null;

			var thread = new Thread(() =>
			{
				try
				{
					result = func();
				}
				catch (Exception e)
				{
					error = ExceptionDispatchInfo.Capture(e);
				}
			}) {IsBackground = true};

			thread.Start();

			if (!thread.Join(milliseconds))
			{
				Assert.Fail("Операция не завершилась за " + milliseconds + " мс: похоже на зависание.");
			}

			error?.Throw();
			return result;
		}

		/// <summary>
		/// Выполняет действие в отдельном потоке и завершает тест с ошибкой, если оно не уложилось в отведённое время.
		/// </summary>
		public static void CompletesWithin(Action action, int milliseconds = 10000)
		{
			CompletesWithin<object>(() =>
			{
				action();
				return null;
			}, milliseconds);
		}

		/// <summary>
		/// Одновременно запускает действие в нескольких потоках и возвращает все возникшие исключения.
		/// </summary>
		public static List<Exception> RunConcurrently(int threadCount, Action<int> action)
		{
			var errors = new List<Exception>();
			var threads = new Thread[threadCount];

			using (var start = new ManualResetEventSlim(false))
			{
				for (var i = 0; i < threadCount; i++)
				{
					var index = i;
					threads[i] = new Thread(() =>
					{
						start.Wait();
						try
						{
							action(index);
						}
						catch (Exception e)
						{
							lock (errors)
							{
								errors.Add(e);
							}
						}
					}) {IsBackground = true};
					threads[i].Start();
				}

				start.Set();

				foreach (var thread in threads)
				{
					Assert.True(thread.Join(60000), "Поток не завершился за минуту.");
				}
			}

			return errors;
		}

		/// <summary>
		/// Выполняет функцию с указанной текущей культурой потока и восстанавливает прежнюю культуру.
		/// </summary>
		public static T WithCulture<T>(string name, Func<T> func)
		{
			var saved = CultureInfo.CurrentCulture;
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
			try
			{
				return func();
			}
			finally
			{
				CultureInfo.CurrentCulture = saved;
			}
		}

		/// <summary>
		/// Проверяет, что две точки совпадают с заданной абсолютной точностью.
		/// </summary>
		public static void Near(Point3D expected, Point3D actual, double tolerance = 1e-12)
		{
			Assert.True(expected.Distance(actual) <= tolerance,
				"Ожидалось (" + expected.X + ", " + expected.Y + ", " + expected.Z + "), получено (" +
				actual.X + ", " + actual.Y + ", " + actual.Z + ").");
		}
	}
}
