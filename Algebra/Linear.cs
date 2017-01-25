using System;
using System.Linq;
using Ruzil3D.Utility;

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет линейное уравнение над полем вещественных чисел.
	/// </summary>
	public struct Linear: IEquatable<Linear>
	{
		/// <summary>
		/// Коээффциенты линейной комбинации.
		/// </summary>
		public readonly double[] A;
		
		/// <summary>
		/// Значение линейной комбинации.
		/// </summary>
		public double Y;

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Linear"/> представленный коэффициентами линейной комбинации <paramref name="a"/> и значением <paramref name="y"/>.
		/// </summary>
		/// <param name="a">Коээффциенты линейной комбинации.</param>
		/// <param name="y">Значение линейной комбинации.</param>
		public Linear(double[] a, double y)
		{
			A = a;
			Y = y;
		}

		/// <summary>
		/// Возвращает аддитивную инверсию линейного уравнения заданного параметром <paramref name="linear"/>.
		/// </summary>
		/// <param name="linear">Инвертируемое значение.</param>
		/// <returns>Результат умножения линейного уравнения <paramref name="linear"/> на -1.</returns>
		public static Linear operator -(Linear linear)
		{
			return new Linear(linear.A.Select(item => -item).ToArray(), -linear.Y);
		}

		/// <summary>
		/// Возвращает исходное линейное уравнение.
		/// </summary>
		/// <param name="linear">Исходное линейное уравнение.</param>
		/// <returns>Исходное линейное уравнение.</returns>
		public static Linear operator +(Linear linear)
		{
			return linear;
		}


		/// <summary>
		/// Возвращает произведение линейного уравнения на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="linear">Линейное уравнение.</param>
		/// <param name="x">Множитель.</param>
		/// <returns>Произведение <paramref name="linear"/> и <paramref name="x"/>.</returns>
		public static Linear operator *(Linear linear, double x)
		{
			return new Linear(linear.A.Select(item => x*item).ToArray(), x*linear.Y);
		}

		/// <summary>
		/// Возвращает произведение линейного уравнения на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель.</param>
		/// <param name="linear">Линейное уравнение.</param>
		/// <returns>Произведение <paramref name="linear"/> и <paramref name="x"/>.</returns>
		public static Linear operator *(double x, Linear linear)
		{
			return linear*x;
		}

		/// <summary>
		/// Делит линейное уравнение на число с плавающей запятой двойной точности и возвращает результат.
		/// </summary>
		/// <param name="linear">Линейное уравнение число-числитель.</param>
		/// <param name="x">Знаменатель с плавающей запятой двойной точности.</param>
		/// <returns>Частное от деления <paramref name="linear"/> на <paramref name="x"/>.</returns>
		public static Linear operator /(Linear linear, double x)
		{
			return linear*(1/x);
		}

		/// <summary>
		/// Складывает два линейных уравнения.
		/// </summary>
		/// <param name="x">Первое из складываемых значений.</param>
		/// <param name="y">Второе из складываемых значений.</param>
		/// <returns>Сумма <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Linear operator +(Linear x, Linear y)
		{
			double[] xArray, yArray;
			if (x.A.Length < y.A.Length)
			{
				xArray = x.A;
				yArray = y.A;
			}
			else
			{
				xArray = y.A;
				yArray = x.A;
			}

			var result = new double[yArray.Length];

			for (var i = 0; i < xArray.Length; i++)
			{
				result[i] = xArray[i] + yArray[i];
			}

			for (var i = xArray.Length; i < yArray.Length; i++)
			{
				result[i] = yArray[i];
			}

			return new Linear(result, x.Y + y.Y);
		}

		/// <summary>
		/// Вычитает линейное уравнение из другого линейного уравнения.
		/// </summary>
		/// <param name="x">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="y">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="x"/> и <paramref name="y"/>.</returns>
		public static Linear operator -(Linear x, Linear y)
		{
			return x + -y;
		}

		#region Equality members

		/// <summary>
		/// Возвращает значение указывающее на равенство двух уравнений.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд равен второму, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator ==(Linear x, Linear y)
		{
			return x.Equals(y);
		}

		/// <summary>
		/// Возвращает значение указывающее на неравенство уравнений.
		/// </summary>
		/// <param name="x">Первый операнд.</param>
		/// <param name="y">Второй операнд.</param>
		/// <returns>Значение <b>true</b>, если первый операнд не равен второму, в противном случае возвращается значение <b>false</b>.</returns>
		public static bool operator !=(Linear x, Linear y)
		{
			return !x.Equals(y);
		}

		/// <summary>
		/// Указывает, равен ли текущий объект другому объекту того же типа.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с данным объектом.</param>
		/// <returns>true, если текущий объект равен параметру <paramref name="other"/>, в противном случае — false.</returns>
		public bool Equals(Linear other)
		{
			if (!Y.Equals(other.Y) || A.Length != other.A.Length)
			{
				return false;
			}

			for (var i = 0; i < A.Length; i++)
			{
				if (!A[i].Equals(other.A[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение true, если <paramref name="obj"/> и данный экземпляр относятся к одному типу и представляют одинаковые значения; в противном случае — значение false.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Linear && Equals((Linear)obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			return 0;
		}

		#endregion

		/// <summary>
		/// Возвращает строковое представлеине данного линейного уравнения.
		/// </summary>
		/// <returns>Строковое представление данного линейного уравнения.</returns>
		/// <remarks>
		/// <code>
		/// var linear = new Linear(new double[] {3, 1, -1}, -4);
		/// Console.Write(linear); //Результат: 3 x₁ + x₂ - x₃ = -4
		/// Console.Write(-linear/3); //Результат: -x₁ - 1/3 x₂ + 1/3 x₃ = 4/3
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			var result = CStatic.GetToStringHashValue(this);
			if (result != null) return result;
			result = "";


			for (var i = 0; i < A.Length; i++)
			{
				var sym = "x" + CStatic.GetIndex(i + 1);
				CStatic.AddLinearItem(ref result, A[i], sym);
			}
			result = result == "" ? "0" : result;
			result += " = " + CStatic.DoubleToString(Y);


			CStatic.AddToStringHashValue(this, result);
			return result;
		}
	}
}