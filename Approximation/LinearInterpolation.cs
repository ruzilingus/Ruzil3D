using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Approximation
{
	/// <summary>
	/// Представляет класс линейной интерполяции функций одной переменной.
	/// </summary>
	public class LinearInterpolation : CGridPolynomialApproximation
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CubicInterpolation"/> из перечислителя структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Перечислитель структур <see cref="PointD"/>.</param>
		/// <exception cref="ArgumentException">Нескольким одинаковым аргументам X соответствуют разные значения Y.</exception>
		public LinearInterpolation(IEnumerable<PointD> points) : base(points)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CubicInterpolation"/> из массива структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Массив структур <see cref="PointD"/>.</param>
		/// <param name="check">Условие указывающее на необходимость проверить исходные данные на корректность.</param>
		public LinearInterpolation(PointD[] points, bool check = false) : base(points, check)
		{
		}
		
		/// <summary>
		/// Заполняет массив многочленов не выше 1 степени представленные структурами <see cref="Polynomial"/>, соответствующие кусочно-линейной аппроксимации.
		/// </summary>
		/// <param name="result">Массив многочленов не выше 1 степени, кусочно-аппроксимирующие клетки сетки.</param>
		protected override void InitPolynomials(ref Polynomial[] result)
		{
			for (var i = 1; i < Points.Length; i++)
			{
				var p0 = Points[i - 1];
				var p1 = Points[i];
				var polynom = Polynomial.GetPolynomial(p0, p1);
				result[i - 1] = polynom;
			}
		}

		/// <summary>
		/// Возвращает значение кусочно-аппроксимирующего полинома для соответствующего аргумента <paramref name="x"/>.
		/// </summary>
		/// <param name="index">Индекс клетки сетки.</param>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Значение кусочно-аппроксимирующего полинома соответствующее индексу и аргументу.</returns>
		protected override double GetValue(int index, double x)
		{
			return GetPolynom(index).GetValue(x);
		}
	}
}