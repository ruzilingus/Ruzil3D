using Ruzil3D.Algebra;
using static Ruzil3D.Utility.CStatic;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет прямую на плоскости.
	/// </summary>
	public struct Line
	{
		private bool _isNormalized;

		/// <summary>
		/// Получает или задает коэффициент A из общего уравнения прямой.
		/// </summary>
		public double A;

		/// <summary>
		/// Получает или задает коэффициент B из общего уравнения прямой.
		/// </summary>
		public double B;

		/// <summary>
		/// Получает или задает коэффициент C из общего уравнения прямой.
		/// </summary>
		public double C;

		/// <summary>
		/// Инициализирует новую прямую в плоскости проходящую через заданные точки.
		/// </summary>
		/// <param name="point0">Первая заданная точка.</param>
		/// <param name="point1">Вторая заданная точка.</param>
		public Line(PointD point0, PointD point1)
		{
			_isNormalized = false;
			A = point1.Y - point0.Y;
			B = point0.X - point1.X;
			C = -A*point0.X - B*point0.Y;

			Normalize();
		}

		/// <summary>
		/// Инициализирует новую прямую через коэффициенты общего уравнения прямой.
		/// </summary>
		/// <param name="a">Задает коэффициент A.</param>
		/// <param name="b">Задает коэффициент B.</param>
		/// <param name="c">Задает коэффициент C.</param>
		public Line(double a, double b, double c)
		{
			_isNormalized = false;
			A = a;
			B = b;
			C = c;
		}

		#region Overloads

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух прямых.
		/// </summary>
		/// <param name="x">Первая прямая для сравнения.</param>
		/// <param name="y">Вторая прямая для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Line x, Line y)
		{
			var xNorm = x.GetNorm();
			if (double.IsNaN(xNorm) || double.IsInfinity(xNorm) || 0D.Equals(xNorm))
			{
				return false;
			}

			var yNorm = y.GetNorm();
			if (double.IsNaN(yNorm) || double.IsInfinity(yNorm) || 0D.Equals(yNorm))
			{
				return false;
			}

			if (x.C > 0)
			{
				xNorm *= -1;
			}

			if (y.C > 0)
			{
				yNorm *= -1;
			}

			return
				(x.A*yNorm).Equals(y.A*xNorm) &&
				(x.B*yNorm).Equals(y.B*xNorm) &&
				(x.C*yNorm).Equals(y.C*xNorm);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух прямых.
		/// </summary>
		/// <param name="x">Первая прямая для сравнения.</param>
		/// <param name="y">Вторая прямая для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="x"/> и <paramref name="y"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Line x, Line y)
		{
			return !(x == y);
		}

		#endregion

		#region Methods

		/*
		public double Resolve(PointD point)
		{
			return A * point.X + B * point.Y + C;
		}
		*/

		/// <summary>
		/// Приводит уравнение прямой к нормальному виду.
		/// </summary>
		public void Normalize()
		{
			if (_isNormalized)
			{
				return;
			}

			var norm = GetNorm();

			if (C > 0)
			{
				norm *= -1;
			}

			A /= norm;
			B /= norm;
			C /= norm;

			_isNormalized = true;
		}

		private double GetNorm()
		{
			return _isNormalized ? 1 : Math.Sqrt(A*A + B*B);
		}

		/// <summary>
		/// Возвращает значение указывающее на пересечение данной прямой с указанным отрезком.
		/// </summary>
		/// <param name="point0">Первая точка отрезка</param>
		/// <param name="point1">Вторая точка отрезка</param>
		/// <returns></returns>
		public bool IntersectsWith(PointD point0, PointD point1)
		{
			return (A*point0.X + B*point0.Y + C)*(A*point1.X + B*point1.Y + C) <= 0;
		}

		/// <summary>
		/// Получает угол нормали.
		/// </summary>
		public double Theta
		{
			get
			{
				var norm = GetNorm();

				double theta;

				if (norm.Equals(0D) || double.IsNaN(norm))
				{
					theta = double.NaN;
				}
				else
				{
					var sign = double.IsNaN(C) ? 0 : Math.Sign(-C);

					if (sign == 0)
					{
						sign = A.Equals(0D) ? Math.Sign(B) : Math.Sign(A);
					}

					theta = B.Equals(0D) ? Math.Atan2(0D, sign*A) : Math.Atan2(sign*B, sign*A);
				}

				return theta;
			}
		}

		/// <summary>
		/// Получает расстояние от начала координат до прямой.
		/// </summary>
		public double P
		{
			get
			{
				var norm = GetNorm();

				double p;

				if (norm.Equals(0D) || double.IsNaN(norm))
				{
					p = double.NaN;
				}
				else
				{
					var sign = double.IsNaN(C) ? 0 : Math.Sign(-C);

					if (sign == 0)
					{
						sign = A.Equals(0D) ? Math.Sign(B) : Math.Sign(A);
					}

					var theta = Math.Atan2(sign*B, sign*A);
					p = double.IsNaN(theta) ? double.NaN : -sign*C/norm;
				}

				return p;
			}
		}

		/// <summary>
		/// Возвращает строковое представлеине данной прямой.
		/// </summary>
		/// <returns>Строковое представлеине данной прямой.</returns>
		/// <remarks>
		/// <code>
		/// var line = new Line(1,1,3);
		/// Console.Write(line); //Результат: θ = -3/4 π, p = 3/√2
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			/*
			var norm = GetNorm();

			double theta,  p;

			if (norm.Equals(0D) || double.IsNaN(norm))
			{
				theta = p = double.NaN;
			}
			else
			{
				var sign = double.IsNaN(C) ? 0 : Math.Sign(-C);
				
				if (sign == 0)
				{
					sign = A.Equals(0D) ? Math.Sign(B) : Math.Sign(A);
				}

				theta = Math.Atan2(sign * B, sign * A);
				p = double.IsNaN(theta) ? double.NaN : -sign * C / norm;
			}
            */

			return "θ = " + AngleToString(Theta) + EmSp + "p = " + DoubleToString(P);
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другая прямая.</param>
		/// <returns>Значение <b>true</b>, если две прямые совпадают; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Line other)
		{
			return this == other;
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Line"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Line && Equals((Line) obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		public override int GetHashCode()
		{
			return 0;
		}

		#endregion
	}
}