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
		/// <returns>Соответствующий полином от переменной x.</returns>
		/// <remarks>Многочлен записан по степеням x, поэтому вдали от нуля его значения могут терять точность из-за сокращения больших слагаемых;
		/// для вычисления значений аппроксимации используйте <see cref="CGridApproximation.GetValue(double)"/>.</remarks>
		public Polynomial GetPolynom(int index)
		{
			return Polynoms[index];
		}

		private Polynomial[] GetPolynomials()
		{
			//Сетка из менее чем двух узлов отклоняется конструктором CGridApproximation, поэтому проверка лишь защитная.
			//Прежде здесь выбрасывалось ArgumentException с именем поля Points вместо имени параметра.
			if (Points.Length < 2)
			{
				throw new InvalidOperationException("Недостаточное количество точек.");
			}

			var result = new Polynomial[Points.Length - 1];

			InitPolynomials(ref result);

			return result;
		}
		
		/// <summary>
		/// Заполняет массив структур <see cref="Polynomial"/> соответствующий кусочно-полиномиальной аппроксимации.
		/// </summary>
		/// <param name="result">Массив многочленов от переменной x, кусочно-аппроксимирующие клетки сетки.</param>
		protected abstract void InitPolynomials(ref Polynomial[] result);

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CGridPolynomialApproximation"/> из перечислителя структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Перечислитель структур <see cref="PointD"/>.</param>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="points"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Нескольким одинаковым аргументам X соответствуют разные значения Y или задано меньше двух различных узлов.</exception>
		protected CGridPolynomialApproximation(IEnumerable<PointD> points) : base(points)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="CGridPolynomialApproximation"/> из массива структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="points">Массив структур <see cref="PointD"/>. Массив копируется.</param>
		/// <param name="check">Условие указывающее на необходимость проверить исходные данные на корректность: отсортировать узлы по возрастанию X и удалить повторы.
		/// При значении <b>false</b> узлы должны быть уже отсортированы по возрастанию X и иметь различные X, иначе значения аппроксимации неверны.</param>
		/// <exception cref="ArgumentNullException">Значение параметра <paramref name="points"/> равно <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Задано меньше двух узлов или при проверке (<paramref name="check"/> = <b>true</b>) нескольким одинаковым аргументам X соответствуют разные значения Y.</exception>
		protected CGridPolynomialApproximation(PointD[] points, bool check = false) : base(points, check)
		{
		}
	}
}