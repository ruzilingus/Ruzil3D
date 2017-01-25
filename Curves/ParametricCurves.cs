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
	/// <typeparam name="T"></typeparam>
	public class ParametricCurves<T> : IEnumerable<T> where T : ParametricCurve
	{
		#region Inner Items

		/// <summary>
		/// Массив компиляторов для кривых.
		/// </summary>
		protected ParametricCurveDistanceCompiler<T>[] Compilers;

		private double[] _lengths;

		/// <summary>
		/// Возвращает массив длин кривых.
		/// </summary>
		protected double[] Lengths => _lengths ?? (_lengths = Compile());

		/// <summary>
		/// Вычисляет массив длин кривых.
		/// </summary>
		/// <returns>Массив структур <see cref="double"/> представляющий длин кривых.</returns>
		private double[] Compile()
		{
			var result = new double[Compilers.Length + 1];

			double sum = 0;
			for (var i = 1; i <= Compilers.Length; i++)
			{
				var compiler = Compilers[i - 1];
				sum += compiler.Curve.Length;
				result[i] = sum;
			}

			return result;
		}

		/// <summary>
		/// Возращает индекс кривой соответствующей параметру.
		/// </summary>
		/// <param name="distance">Параметр кривой.</param>
		/// <returns>Индекс кривой.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Аргумент находится вне области определения функции.</exception>
		private int GetIndex(double distance)
		{
			if (distance < 0 || distance > Length)
			{
				throw new ArgumentOutOfRangeException(nameof(distance),
					"Недопустимый аргумент. Аргумент \"" + nameof(distance) + "\" находится вне области определения функции.");
			}

			var min = 1;
			var max = Lengths.Length - 1;

			while (min != max)
			{
				var mid = (min + max)/2;

				if (distance < Lengths[mid])
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
		public double Length => Lengths[Lengths.Length - 1];

		/// <summary>
		/// Возвращает координату точки на кривой соответствующую дистанции.
		/// </summary>
		/// <param name="distance">Расстояние от начальной точки кривой.</param>
		/// <returns>Координаты точки на кривой представленной структурой <see cref="Point3D"/>.</returns>
		public Point3D GetValue(double distance)
		{
			var idx = GetIndex(distance);

			var curve = Compilers[idx];
			distance -= Lengths[idx];

			return curve.GetValue(Math.Min(distance, curve.Curve.Length));
		}

		/// <summary>
		/// Возвращает детальную характеристику кривой в произвольной точке соответствующую дистанции.
		/// </summary>
		/// <param name="distance">Расстояние от начальной точки кривой.</param>
		/// <returns>Детальная характеристика кривой в точке.</returns>
		public CurveDetails<T> GetDetails(double distance)
		{
			var idx = GetIndex(distance);
			var compiler = Compilers[idx];
			var curve = compiler.Curve;
			distance -= Lengths[idx];
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

		private CurveEnumerator _enumerator;
		private CurveEnumerator Enumerator => _enumerator ?? (_enumerator = new CurveEnumerator(Compilers));

		#endregion


		/// <summary>
		/// Возвращает <see cref="IEnumerator{T}"/>.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return Enumerator;
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