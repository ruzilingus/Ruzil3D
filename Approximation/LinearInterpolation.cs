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
		/// Инициализирует новый экземпляр класса <see cref="LinearInterpolation"/> из перечислителя структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Перечислитель структур <see cref="PointD"/>.</param>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="points"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Нескольким одинаковым аргументам X соответствуют разные значения Y или задано меньше двух различных узлов.</exception>
		public LinearInterpolation(IEnumerable<PointD> points) : base(points)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="LinearInterpolation"/> из массива структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Массив структур <see cref="PointD"/>. Массив копируется.</param>
		/// <param name="check">Условие указывающее на необходимость проверить исходные данные на корректность: отсортировать узлы по возрастанию X и удалить повторы.
		/// При значении <b>false</b> узлы должны быть уже отсортированы по возрастанию X и иметь различные X, иначе значения интерполяции неверны.</param>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="points"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Задано меньше двух узлов или при проверке (<paramref name="check"/> = <b>true</b>) нескольким одинаковым аргументам X соответствуют разные значения Y.</exception>
		public LinearInterpolation(PointD[] points, bool check = false) : base(points, check)
		{
		}

		/// <summary>
		/// Заполняет массив многочленов не выше 1 степени представленные структурами <see cref="Polynomial"/>, соответствующие кусочно-линейной аппроксимации.
		/// </summary>
		/// <param name="result">Массив многочленов не выше 1 степени от переменной x, кусочно-аппроксимирующие клетки сетки.</param>
		/// <remarks>Многочлены записаны по степеням x, поэтому вдали от нуля их значения теряют точность из-за сокращения больших слагаемых.
		/// Значения интерполяции (<see cref="GetValue(int, double)"/>) вычисляются в локальной координате и точности не теряют.</remarks>
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
		/// <remarks>Значение вычисляется в локальной координате x − xᵢ, где xᵢ — левый узел клетки, а не по многочлену <see cref="CGridPolynomialApproximation.GetPolynom"/>.</remarks>
		protected override double GetValue(int index, double x)
		{
			//Прежде значение вычислялось по многочлену от x: при x порядка 1e12 его свободный член почти сокращался
			//со вторым слагаемым, и в середине отрезка [1e12, 1e12 + 1] получалось 0.0500031 вместо 0.05.
			var p0 = Points[index];
			var p1 = Points[index + 1];
			return p0.Y + (x - p0.X)*((p1.Y - p0.Y)/(p1.X - p0.X));
		}
	}
}