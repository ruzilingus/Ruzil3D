using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ruzil3D.Algebra;

namespace Ruzil3D.Curves
{
	/// <summary>
	/// Универсальный класс из последовательности кривых <see cref="ParametricCurve"/>.
	/// </summary>
	/// <typeparam name="T">Тип кривых последовательности.</typeparam>
	public class ParametricCurves<T> : IEnumerable<T> where T : ParametricCurve
	{
		#region Inner Items

		/// <summary>
		/// Массив компиляторов для кривых.
		/// </summary>
		protected ParametricCurveDistanceCompiler<T>[] Compilers;

		/// <summary>
		/// Сохранённые длины кривых вместе с данными, по которым проверяется их актуальность.
		/// </summary>
		private sealed class LengthsCache
		{
			public readonly ParametricCurveDistanceCompiler<T>[] Compilers;
			public readonly T[] Curves;
			public readonly double[] Lengths;
			public readonly int Replacements;

			public LengthsCache(ParametricCurveDistanceCompiler<T>[] compilers, T[] curves, double[] lengths, int replacements)
			{
				Compilers = compilers;
				Curves = curves;
				Lengths = lengths;
				Replacements = replacements;
			}
		}

		private LengthsCache _lengthsCache;

		/// <summary>
		/// Возвращает массив длин кривых.
		/// </summary>
		/// <value>Массив нарастающих сумм длин кривых: элемент с индексом i равен суммарной длине первых i кривых.</value>
		protected double[] Lengths
		{
			get
			{
				//Кривую компилятора можно заменить через его свойство Curve, поэтому сохранённые длины проверяются.
				//Прежде они сохранялись навсегда: после замены кривой длина последовательности и расстояния оставались прежними.
				//Счётчик замен читается до кривых, поэтому замена во время вычисления приведёт к повторной проверке.
				var replacements = ParametricCurveDistanceCompiler<T>.CurveReplacements;
				var compilers = Compilers;
				var cache = _lengthsCache;

				if (cache != null && ReferenceEquals(cache.Compilers, compilers))
				{
					if (cache.Replacements == replacements)
					{
						return cache.Lengths;
					}

					if (IsActual(cache, compilers))
					{
						_lengthsCache = new LengthsCache(compilers, cache.Curves, cache.Lengths, replacements);
						return cache.Lengths;
					}
				}

				var curves = new T[compilers.Length];
				for (var i = 0; i < curves.Length; i++)
				{
					curves[i] = compilers[i].Curve;
				}

				var lengths = Compile(curves);
				_lengthsCache = new LengthsCache(compilers, curves, lengths, replacements);

				return lengths;
			}
		}

		/// <summary>
		/// Проверяет, что компиляторы содержат те же кривые, для которых вычислены сохранённые длины.
		/// </summary>
		private static bool IsActual(LengthsCache cache, ParametricCurveDistanceCompiler<T>[] compilers)
		{
			for (var i = 0; i < compilers.Length; i++)
			{
				if (!ReferenceEquals(cache.Curves[i], compilers[i].Curve))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Вычисляет массив длин кривых.
		/// </summary>
		/// <param name="curves">Кривые последовательности.</param>
		/// <returns>Массив структур <see cref="double"/> представляющий длин кривых.</returns>
		private static double[] Compile(T[] curves)
		{
			var result = new double[curves.Length + 1];

			double sum = 0;
			for (var i = 1; i <= curves.Length; i++)
			{
				sum += curves[i - 1].Length;
				result[i] = sum;
			}

			return result;
		}

		/// <summary>
		/// Возвращает индекс кривой соответствующей параметру.
		/// </summary>
		/// <param name="lengths">Массив нарастающих сумм длин кривых.</param>
		/// <param name="distance">Параметр кривой.</param>
		/// <returns>Индекс кривой.</returns>
		/// <exception cref="InvalidOperationException">Последовательность не содержит кривых.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Аргумент находится вне области определения функции.</exception>
		private int GetIndex(double[] lengths, double distance)
		{
			if (Compilers.Length == 0)
			{
				//Без этой проверки поиск по пустой последовательности не завершался.
				throw new InvalidOperationException("Последовательность не содержит кривых.");
			}

			if (distance < 0 || distance > lengths[lengths.Length - 1])
			{
				throw new ArgumentOutOfRangeException(nameof(distance),
					"Недопустимый аргумент. Аргумент \"" + nameof(distance) + "\" находится вне области определения функции.");
			}

			var min = 1;
			var max = lengths.Length - 1;

			while (min < max)
			{
				var mid = (min + max)/2;

				if (distance < lengths[mid])
				{
					max = mid;
				}
				else
				{
					min = mid + 1;
				}
			}

			return min - 1;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ParametricCurves{T}"/>.
		/// </summary>
		protected ParametricCurves()
		{
			
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ParametricCurves{T}"/> из перечислителя объектов <see cref="ParametricCurveDistanceCompiler{T}"/>.
		/// </summary>
		/// <param name="compilers">Перечислитель компилированных кривых.</param>
		public ParametricCurves(IEnumerable<ParametricCurveDistanceCompiler<T>> compilers)
		{
			Compilers = compilers.ToArray();
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ParametricCurves{T}"/> из коллекции объектов <see cref="ParametricCurveDistanceCompiler{T}"/>.
		/// </summary>
		/// <param name="compilers">Коллекция компилированных кривых.</param>
		public ParametricCurves(ICollection<ParametricCurveDistanceCompiler<T>> compilers)
		{
			var compilersCopy = new ParametricCurveDistanceCompiler<T>[compilers.Count];
			compilers.CopyTo(compilersCopy, 0);
			Compilers = compilersCopy;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ParametricCurves{T}"/> из массива объектов <see cref="ParametricCurveDistanceCompiler{T}"/>.
		/// </summary>
		/// <param name="compilers">Массив компилированных кривых.</param>
		public ParametricCurves(ParametricCurveDistanceCompiler<T>[] compilers)
		{
			var compilersCopy = new ParametricCurveDistanceCompiler<T>[compilers.Length];
			compilers.CopyTo(compilersCopy, 0);
			Compilers = compilersCopy;
		}

		#endregion

		/// <summary>
		/// Возвращает длину всей последовательности.
		/// </summary>
		/// <remarks>Длина учитывает замену кривых в компиляторах последовательности (свойство <see cref="ParametricCurveDistanceCompiler{T}.Curve"/>).</remarks>
		public double Length
		{
			get
			{
				var lengths = Lengths;
				return lengths[lengths.Length - 1];
			}
		}

		/// <summary>
		/// Возвращает координату точки на кривой соответствующую дистанции.
		/// </summary>
		/// <param name="distance">Расстояние от начальной точки кривой.</param>
		/// <returns>Координаты точки на кривой представленной структурой <see cref="Point3D"/>.</returns>
		/// <exception cref="InvalidOperationException">Последовательность не содержит кривых.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Расстояние находится вне отрезка [0, <see cref="Length"/>].</exception>
		public Point3D GetValue(double distance)
		{
			var lengths = Lengths;
			var idx = GetIndex(lengths, distance);

			var curve = Compilers[idx];
			distance -= lengths[idx];

			return curve.GetValue(Math.Min(distance, curve.Curve.Length));
		}

		/// <summary>
		/// Возвращает детальную характеристику кривой в произвольной точке соответствующую дистанции.
		/// </summary>
		/// <param name="distance">Расстояние от начальной точки кривой.</param>
		/// <returns>Детальная характеристика кривой в точке. Её расстояние <see cref="CurveDetails{T}.Distance"/> отсчитывается от начала кривой с индексом <see cref="CurveDetails{T}.Index"/>, а не от начала последовательности.</returns>
		/// <exception cref="InvalidOperationException">Последовательность не содержит кривых.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Расстояние находится вне отрезка [0, <see cref="Length"/>].</exception>
		public CurveDetails<T> GetDetails(double distance)
		{
			var lengths = Lengths;
			var idx = GetIndex(lengths, distance);
			var compiler = Compilers[idx];
			var curve = compiler.Curve;
			distance -= lengths[idx];
			distance = Math.Min(distance, curve.Length);
			
			return new CurveDetails<T>(distance, idx, curve, compiler.GetParameter(distance));
		}


		/// <summary>
		/// Возвращает кривую из последовательности соответствующую указанному индексу.
		/// </summary>
		/// <param name="index">Индекс кривой в последовательности.</param>
		/// <value>Кривая из последовательности соответствующая указанному индексу.</value>
		public T this[int index] => Compilers[index].Curve;
		

		/// <summary>
		/// Возвращает количество кривых в последовательности.
		/// </summary>
		public int Count => Compilers.Length;

		#region IEnumerable Items

		#region Private

		private class CurveEnumerator : IEnumerator<T>
		{
			private readonly ParametricCurveDistanceCompiler<T>[] _curves;
			private int _index;

			public CurveEnumerator(ParametricCurveDistanceCompiler<T>[] curves)
			{
				_curves = curves;
				Reset();
			}

			public void Dispose()
			{
				Reset();
			}

			public bool MoveNext()
			{
				_index++;
				return _index < _curves.Length;
			}

			public void Reset()
			{
				_index = -1;
			}

			public T Current => _curves[_index].Curve as T;

			object IEnumerator.Current => Current;
		}

		#endregion


		/// <summary>
		/// Возвращает <see cref="IEnumerator{T}"/>.
		/// </summary>
		/// <returns>Новый перечислитель кривых последовательности.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			//Каждый вызов возвращает собственный перечислитель: общий перечислитель ломал вложенные
			//и одновременные обходы коллекции (вложенный foreach не завершался).
			return new CurveEnumerator(Compilers);
		}

		/// <summary>
		/// Возвращает <see cref="IEnumerator"/>.
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}