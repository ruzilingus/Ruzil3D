using System;
using Ruzil3D.Algebra;

namespace Ruzil3D.Curves
{
	/// <summary>
	/// Параметрические кривые в трехмерном евклидовом пространстве.
	/// </summary>
	public abstract class ParametricCurve
	{
		#region Fields

		private double _length = -1D;
		private Func<double, double> _rectificationDerivativeFunction;

		#endregion

		/// <summary>
		/// Получает значение указывающее, что эквивалентная репараметризация <i>p = <see cref="Length"/>*t</i> данной кривой является натуральной.
		/// </summary>
		/// <remarks>Натуральность означает, что длина любого участка от <i>t₀</i> до <i>t₁</i> (0 ≤ <i>t₀</i> ≤ <i>t₁</i> ≤ 1) кривой равняется <i><see cref="Length"/>*(t₁ - t₀)</i>.</remarks>
		public virtual bool IsNatural { get; } = false;

		/// <summary>
		/// Возвращает длину кривой.
		/// </summary>
		public double Length
		{
			get
			{
				if (_length < 0)
				{
					_length = GetLength();
				}
				return _length;
			}
		}

		/// <summary>
		/// Возвращает координату точки на кривой соответствующую параметру.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Координаты точки на кривой представленной структурой <see cref="Point3D"/>.</returns>
		/// <exception cref="NotImplementedException">Метод не реализован.</exception>
		public virtual Point3D GetValue(double parameter)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Возвращает вектор касательной к кривой в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение вектора касательной представленный структурой <see cref="Point3D"/>.</returns>
		/// <exception cref="NotImplementedException">Метод не реализован.</exception>
		public virtual Point3D GetTangent(double parameter)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Возвращает вектор кривизны к кривой в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение вектора кривизны представленный структурой <see cref="Point3D"/>.</returns>
		/// <exception cref="NotImplementedException">Метод не реализован.</exception>
		/// <remarks>Псевдовектор кривизны направлен перпендикулярно к плоскости образованной векторами нормали и касательной.</remarks>
		public virtual Point3D GetCurvature(double parameter)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Приводит кривую к кривой Безье.
		/// </summary>
		/// <returns>Возвращают кривую Безье близкую к заданной.</returns>
		/// <exception cref="NotImplementedException">Метод не реализован.</exception>
		public virtual BezierCurve RecastToBezierCurve()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Вычисляет и возвращает длину кривой.
		/// </summary>
		/// <returns>Длина кривой.</returns>
		protected virtual double GetLength()
		{
			return GetDistance(0D, 1D);
		}

		/// <summary>
		/// Вычисляет и возвращают длину участка кривой между параметрами t0 до t1.
		/// </summary>
		/// <param name="t0">Параметр начала участка.</param>
		/// <param name="t1">Параметр конца участка.</param>
		/// <returns>Длина участка кривой.</returns>
		public virtual double GetDistance(double t0, double t1)
		{
			if (RectificationFunction != null)
			{
				return RectificationFunction(t1) - RectificationFunction(t0);
			}

			return Calculus.Calculus.Integrate(RectificationDerivativeFunction, t0, t1);
		}

		#region Rectification Items

		/// <summary>
		/// Возвращает функцию выпрямления кривой.
		/// </summary>
		/// <value>Функция выпрямления кривой - функция которая возвращает длину участка кривой от нулевого до указанного параметра.</value>
		public virtual Func<double, double> RectificationFunction { get; } = null;

		/// <summary>
		/// Возвращает функцию обратную функции выпрямления кривой – <see cref="RectificationFunction"/>.
		/// </summary>
		public virtual Func<double, double> GetInvertRectificationFunction()
		{
			if (IsNatural)
			{
				return (distance) => distance/Length;
			}

			return null;
		}

		/// <summary>
		/// Возвращает производную от функции выпрямления кривой.
		/// </summary>
		/// <returns>Производная от функции выпрямления.</returns>
		/// <exception cref="NotImplementedException">Ни один из виртуальных методов или свойств: <see cref="GetRectificationDerivative"/>, <see cref="RectificationFunction"/>, <see cref="GetDistance"/> не реализован.</exception>
		protected virtual Func<double, double> GetRectificationDerivative()
		{
			throw new NotImplementedException(
				"Нужно реализовать хотя бы один из виртуальных методов или свойств: " +
				nameof(GetRectificationDerivative) + ", " +
				nameof(RectificationFunction) + ", " +
				nameof(GetDistance) + "."
				);
		}


		/// <summary>
		/// Получает производную от функции выпрямления кривой.
		/// </summary>
		public Func<double, double> RectificationDerivativeFunction
			=> _rectificationDerivativeFunction ?? (_rectificationDerivativeFunction = GetRectificationDerivative());

		#endregion

		/// <summary>
		/// Возвращает строку, представляющую текущий объект.
		/// </summary>
		/// <returns> Строка, представляющая текущий объект. </returns>
		public override string ToString() => GetType().Name + "; " + nameof(Length) + ": " + Length;

		/*
        internal void CloneFieldsTo(ParametricCurve curve)
	    {
	        curve._length = _length;
	        curve._rectificationDerivativeFunction = _rectificationDerivativeFunction;
	    }
        */
	}
}