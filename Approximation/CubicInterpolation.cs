using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Approximation
{
	/// <summary>
	/// Представляет класс кубической интерполяции функций одной переменной.
	/// </summary>
	public class CubicInterpolation : CGridPolynomialApproximation
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CubicInterpolation"/> из перечислителя структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Перечислитель структур <see cref="PointD"/>.</param>
		/// <exception cref="ArgumentException">Нескольким одинаковым аргументам X соответствуют разные значения Y.</exception>
		public CubicInterpolation(IEnumerable<PointD> points) : base(points)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CubicInterpolation"/> из массива структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Массив структур <see cref="PointD"/>.</param>
		/// <param name="check">Условие указывающее на необходимость проверить исходные данные на корректность.</param>
		public CubicInterpolation(PointD[] points, bool check = false) : base(points, check)
		{
		}

		/// <summary>
		/// Заполняет массив многочленов не выше 3 степени представленные структурами <see cref="Polynomial"/>, соответствующие кусочно-кубической аппроксимации.
		/// </summary>
		/// <returns>Массив многочленов не выше 3 степени, кусочно-аппроксимирующие клетки сетки.</returns>
		protected override void InitPolynomials(ref Polynomial[] result)
		{
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

				var d = (c - prevC)/dx;
				var b = dx*(2D*c + prevC)/6D + dy/dx;

				//var pDx = new Polynomial(-x, 1);
				//result[i] = a + pDx*(b + pDx*(c/2D + pDx*d/6D));

				result[i] = new Polynomial(
					a0: a - x*(b - x*(c/2D - x*d/6D)),
					a1: b - x*(c - x*d/2D),
					a2: (c - x*d)/2D,
					a3: d/6D
					);
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