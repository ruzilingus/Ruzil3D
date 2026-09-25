using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Approximation
{
	/// <summary>
	/// Представляет класс кубической интерполяции функций одной переменной.
	/// </summary>
	/// <remarks>
	/// Строится естественный кубический сплайн: дважды непрерывно дифференцируемая кусочно-кубическая функция, которая проходит
	/// через все узлы и имеет нулевую вторую производную в крайних узлах. Сплайн не сохраняет монотонность: даже для монотонных
	/// данных значения между узлами могут выходить за пределы значений в соседних узлах.
	/// </remarks>
	public class CubicInterpolation : CGridPolynomialApproximation
	{
		/// <summary>
		/// Многочлен сплайна в клетке сетки, записанный по степеням (x − xᵢ), где xᵢ — правый узел клетки:
		/// S(x) = a + b·(x − xᵢ) + c/2·(x − xᵢ)² + d/6·(x − xᵢ)³.
		/// </summary>
		private struct Cell
		{
			/// <summary>
			/// Правый узел клетки.
			/// </summary>
			public double X;

			/// <summary>
			/// Значение сплайна в правом узле.
			/// </summary>
			public double A;

			/// <summary>
			/// Первая производная сплайна в правом узле.
			/// </summary>
			public double B;

			/// <summary>
			/// Вторая производная сплайна в правом узле.
			/// </summary>
			public double C;

			/// <summary>
			/// Третья производная сплайна в клетке.
			/// </summary>
			public double D;

			/// <summary>
			/// Возвращает значение многочлена клетки, вычисленное в локальной координате x − xᵢ.
			/// </summary>
			public double GetValue(double x)
			{
				var dx = x - X;
				return A + dx*(B + dx*(C/2D + dx*D/6D));
			}

			/// <summary>
			/// Возвращает многочлен клетки от глобальной переменной x.
			/// </summary>
			public Polynomial ToPolynomial()
			{
				return new Polynomial(
					a0: A - X*(B - X*(C/2D - X*D/6D)),
					a1: B - X*(C - X*D/2D),
					a2: (C - X*D)/2D,
					a3: D/6D
					);
			}
		}

		private Cell[] _cells;

		private Cell[] Cells => _cells ?? (_cells = GetCells());

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CubicInterpolation"/> из перечислителя структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Перечислитель структур <see cref="PointD"/>.</param>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="points"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Нескольким одинаковым аргументам X соответствуют разные значения Y или задано меньше двух различных узлов.</exception>
		public CubicInterpolation(IEnumerable<PointD> points) : base(points)
		{
			_polynomialValues = OverridesInitPolynomials(GetType(), typeof(CubicInterpolation));
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CubicInterpolation"/> из массива структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Массив структур <see cref="PointD"/>. Массив копируется.</param>
		/// <param name="check">Условие указывающее на необходимость проверить исходные данные на корректность: отсортировать узлы по возрастанию X и удалить повторы.
		/// При значении <b>false</b> узлы должны быть уже отсортированы по возрастанию X и иметь различные X, иначе значения интерполяции неверны.</param>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="points"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Задано меньше двух узлов или при проверке (<paramref name="check"/> = <b>true</b>) нескольким одинаковым аргументам X соответствуют разные значения Y.</exception>
		public CubicInterpolation(PointD[] points, bool check = false) : base(points, check)
		{
			_polynomialValues = OverridesInitPolynomials(GetType(), typeof(CubicInterpolation));
		}

		/// <summary>
		/// Значения вычисляются по многочленам <see cref="CGridPolynomialApproximation.GetPolynom"/>, как прежде: наследник переопределил
		/// <see cref="InitPolynomials"/>.
		/// </summary>
		private readonly bool _polynomialValues;

		/// <summary>
		/// Вычисляет коэффициенты естественного кубического сплайна во всех клетках сетки.
		/// </summary>
		/// <returns>Массив многочленов клеток в локальной координате.</returns>
		private Cell[] GetCells()
		{
			var result = new Cell[Points.Length - 1];

			//Решение СЛАУ относительно коэффициентов сплайнов cᵢ методом прогонки для трехдиагональных матриц

			//Вычисление прогоночных коэффициентов - прямой ход метода прогонки
			var alpha = new double[result.Length];
			var beta = new double[result.Length + 1];

			for (var i = 1; i < result.Length; i++)
			{
				var dx1 = Points[i].X - Points[i - 1].X;
				var dx2 = Points[i + 1].X - Points[i].X;

				var dy1 = Points[i].Y - Points[i - 1].Y;
				var dy2 = Points[i + 1].Y - Points[i].Y;

				var c = 2D*(dx1 + dx2);
				var f = 6D*(dy2/dx2 - dy1/dx1);
				var z = dx1*alpha[i - 1] + c;

				alpha[i] = -dx2/z;
				beta[i] = (f - dx1*beta[i - 1])/z;
			}

			//Нахождение решения - обратный ход метода прогонки
			for (var i = beta.Length - 2; i > 0; i--)
			{
				beta[i] += alpha[i]*beta[i + 1];
			}

			//По известным коэффициентам cᵢ находим значения bᵢ и dᵢ
			for (var i = result.Length - 1; i >= 0; i--)
			{
				var x = Points[i + 1].X;
				var a = Points[i + 1].Y;
				var c = beta[i + 1];
				var prevC = beta[i];

				var dx = x - Points[i].X;
				var dy = a - Points[i].Y;

				result[i] = new Cell
				{
					X = x,
					A = a,
					B = dx*(2D*c + prevC)/6D + dy/dx,
					C = c,
					D = (c - prevC)/dx
				};
			}

			return result;
		}

		/// <summary>
		/// Заполняет массив многочленов не выше 3 степени представленные структурами <see cref="Polynomial"/>, соответствующие кусочно-кубической аппроксимации.
		/// </summary>
		/// <param name="result">Массив многочленов не выше 3 степени от переменной x, кусочно-аппроксимирующие клетки сетки.</param>
		/// <remarks>Многочлены записаны по степеням x, поэтому вдали от нуля их значения теряют точность из-за сокращения больших слагаемых.
		/// Значения интерполяции (<see cref="GetValue(int, double)"/>) вычисляются по тем же коэффициентам сплайна в локальной координате и точности не теряют.</remarks>
		protected override void InitPolynomials(ref Polynomial[] result)
		{
			var cells = Cells;
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = cells[i].ToPolynomial();
			}
		}

		/// <summary>
		/// Возвращает значение кусочно-аппроксимирующего полинома для соответствующего аргумента <paramref name="x"/>.
		/// </summary>
		/// <param name="index">Индекс клетки сетки.</param>
		/// <param name="x">Значение аргумента.</param>
		/// <returns>Значение кусочно-аппроксимирующего полинома соответствующее индексу и аргументу.</returns>
		/// <remarks>Значение вычисляется по коэффициентам сплайна в локальной координате x − xᵢ, а не по многочлену <see cref="CGridPolynomialApproximation.GetPolynom"/>.
		/// Если наследник переопределил <see cref="InitPolynomials"/>, значение, как и прежде, вычисляется по многочлену.</remarks>
		protected override double GetValue(int index, double x)
		{
			//Прежде значение вычислялось по многочлену от x: при x порядка 1e6 его коэффициенты порядка 1e18 почти
			//взаимно сокращались, и ошибка в самих узлах достигала 79.
			return _polynomialValues ? GetPolynom(index).GetValue(x) : Cells[index].GetValue(x);
		}
	}
}