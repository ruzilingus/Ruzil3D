using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Approximation
{
	/// <summary>
	/// Представляет абстрактный класс кусочно-полиномиальной аппроксимации одномерных функций.
	/// </summary>
	/// <remarks>В каждой отдельной клетке сетки свой кусочно-аппроксимирующий многочлен.</remarks>
	public abstract class CGridPolynomialApproximation : CGridApproximation
	{
		private Polynomial[] _polynom;

		private Polynomial[] Polynoms => _polynom ?? (_polynom = GetPolynomials());

		/// <summary>
		/// Возвращает полином соответствующий к клетке сетки заданной параметром <paramref name="index"/>.
		/// </summary>
		/// <param name="index">Индекс клетки сетки.</param>
		/// <returns>Соответствующий полином.</returns>
		public Polynomial GetPolynom(int index)
		{
			return Polynoms[index];
		}

		private Polynomial[] GetPolynomials()
		{
			if (Points.Length < 2)
			{
				throw new ArgumentException("Недостаточное количество точек.", nameof(Points));
			}

			var result = new Polynomial[Points.Length - 1];

			InitPolynomials(ref result);

			return result;
		}
		
		/// <summary>
		/// Заполняет массив структур <see cref="Polynomial"/> соответствующий кусочно-полиномиальной аппроксимации.
		/// </summary>
		/// <param name="result">Массив многочленов кусочно-аппроксимирующие клетки сетки.</param>
		protected abstract void InitPolynomials(ref Polynomial[] result);

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CGridPolynomialApproximation"/> из перечислителя структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Перечислитель структур <see cref="PointD"/>.</param>
		/// <exception cref="ArgumentException">Нескольким одинаковым аргументам X соответствуют разные значения Y.</exception>
		protected CGridPolynomialApproximation(IEnumerable<PointD> points) : base(points)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CGridPolynomialApproximation"/> из массива структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Массив структур <see cref="PointD"/>.</param>
		/// <param name="check">Условие указывающее на необходимость проверить исходные данные на корректность.</param>
		protected CGridPolynomialApproximation(PointD[] points, bool check = false) : base(points, check)
		{
		}
	}
}