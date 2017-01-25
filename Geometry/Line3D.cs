using System;
using Ruzil3D.Algebra;
using static Ruzil3D.Utility.CStatic;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет прямую в трехмерном евклидовом пространстве.
	/// </summary>
	public struct Line3D
	{
		/// <summary>
		/// Получает направляющий вектор прямой.
		/// </summary>
		public Point3D S { get; private set; }

		/// <summary>
		/// Получает точку лежащюй на прямой.
		/// </summary>
		public Point3D M { get; private set; }

		/// <summary>
		/// Инициализирует новую прямую проходящую через заданные точки в трехмерном евклидовом пространстве.
		/// </summary>
		/// <param name="point0">Первая точка в трехмерном евклидовом пространстве.</param>
		/// <param name="point1">Вторая точка в трехмерном евклидовом пространстве.</param>
		/// <exception cref="ArgumentException"></exception>
		public Line3D(Point3D point0, Point3D point1)
		{
			M = point0;

			var s = point1 - point0;

			var len = s.Length;

			if (len.Equals(0D))
			{
				throw new ArgumentException("Точки \"" + nameof(point0) + "\" и \"" + nameof(point1) + "\" совпадают!");
			}

			if (double.IsNaN(len))
			{
				throw new ArgumentException("Точки должны содержать только числовые координаты!");
			}

			S = s/len;
		}

		/// <summary>
		/// Возвращает расстояние до заданной точки в трехмерном евклидовом пространстве.
		/// </summary>
		/// <param name="point">Точка в трехмерном евклидовом пространстве до которой нужно вычислить расстояние.</param>
		/// <returns>Расстояние до заданной точки <paramref name="point"/>.</returns>
		public double GetDistance(Point3D point)
		{
			return ((M - point)*S).Length;
		}

		/// <summary>
		/// Возвращает основание перпендикуляра опущенной от заданной точки в трехмерном евклидовом пространстве.
		/// </summary>
		/// <param name="point">Точка в трехмерном евклидовом пространстве от которого опущен перпендикуляр.</param>
		/// <returns>Основание перпендикуляра опущенной от точки <paramref name="point"/></returns>
		public Point3D NormalPoint(Point3D point)
		{
			return M - S.DotProduct(M - point)*S;
		}

		/// <summary>
		/// Возвращает точку на прямой соответствующий параметру.
		/// </summary>
		/// <param name="t">Парметр прямой заданный числом с плавающей запятой двойной точности.</param>
		/// <returns>Точку на прямой соответствующий параметру <paramref name="t"/>.</returns>
		public Point3D GetValue(double t)
		{
			return M + S*t;
		}

		/// <summary>
		/// Возвращает линию параллельную заданной и проходящую через указанную точку.
		/// </summary>
		/// <param name="point">Точка, через которую проходит параллельная прямая.</param>
		/// <returns>Линия параллельная заданной и проходящаячерез точку <paramref name="point"/></returns>
		public Line3D GetParallel(Point3D point)
		{
			return new Line3D
			{
				M = point,
				S = S
			};
		}

		/// <summary>
		/// Возвращает точку на заданной прямой координата X которого задается параметром. 
		/// </summary>
		/// <param name="x">Координата X точки.</param>
		/// <returns>Точка на заданной прямой координата X которого задается параметром <paramref name="x"/>.</returns>
		[Obsolete]
		public Point3D ResolveByX(double x)
		{
			return GetValue((x - M.X)/S.X);
		}

		/// <summary>
		/// Возвращает точку на заданной прямой координата Y которого задается параметром. 
		/// </summary>
		/// <param name="y">Координата Y точки.</param>
		/// <returns>Точка на заданной прямой координата Y которого задается параметром <paramref name="y"/>.</returns>
		[Obsolete]
		public Point3D ResolveByY(double y)
		{
			return GetValue((y - M.Y)/S.Y);
		}
		
		#region Overloads

		/// <summary>
		/// Преобразует исходную линию в соответствии с заданным преобразованием.
		/// </summary>
		/// <param name="transform">Преобразование трехмерного евклидово пространства.</param>
		/// <param name="line">Исходная линия.</param>
		/// <returns>Преобразованная линия.</returns>
		public static Line3D operator *(Affinity transform, Line3D line)
		{
			return transform.Matrix*line + transform.Center;
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на прямую в пространстве, преставленный структурой <see cref="Line3D"/>.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="line">Прямая для перемножения.</param>
		/// <returns>Произведение исходной матрицы на прямую <paramref name="line"/>.</returns>
		public static Line3D operator *(Matrix3D matrix, Line3D line)
		{
			var m = matrix*line.M;
			var s = matrix*(line.M + line.S) - m;

			return new Line3D
			{
				M = m,
				S = s
			};
		}

		/// <summary>
		/// Перемещает исходную линию в направлении и расстоянии указанное вектором.
		/// </summary>
		/// <param name="line">Исходная линия.</param>
		/// <param name="vector">Вектор указывающий перемещение.</param>
		/// <returns>Перемещенная линия.</returns>
		public static Line3D operator +(Line3D line, Point3D vector)
		{
			return new Line3D
			{
				M = line.M + vector,
				S = line.S
			};
		}

		/// <summary>
		/// Перемещает исходную линию в обратном направлении и расстоянии указанное вектором.
		/// </summary>
		/// <param name="line">Исходная линия.</param>
		/// <param name="vector">Вектор указывающий перемещение.</param>
		/// <returns>Перемещенная линия.</returns>
		public static Line3D operator -(Line3D line, Point3D vector)
		{
			return new Line3D
			{
				M = line.M - vector,
				S = line.S
			};
		}

		/// <summary>
		/// Перемещает исходную линию в направлении и расстоянии указанное вектором.
		/// </summary>
		/// <param name="vector">Вектор указывающий перемещение.</param>
		/// <param name="line">Исходная линия.</param>
		/// <returns>Перемещенная линия.</returns>
		public static Line3D operator +(Point3D vector, Line3D line)
		{
			return line + vector;
		}

		#endregion

		/// <summary>
		/// Возвращает строковое представление структуры.
		/// </summary>
		/// <returns>
		/// Строковое представление структуры.
		/// </returns>
		public override string ToString()
		{
			//return base.ToString();

			var s = S;

			if (S.X < 0)
			{
				s *= -1;
			}
			else if (s.X.Equals(0D) && s.Y < 0)
			{
				S *= -1;
			}
			else if (s.Y.Equals(0D) && s.Z < 0)
			{
				s *= -1;
			}

			var t = -s.X*M.X - s.Y*M.Y - s.Z*M.Z;
			var m = M + s*t;

			//return "cos" + CStatic.NarrowNbSp + "α = " + CStatic.DoubleToString(cos1) + CStatic.EmSp + "cos" + CStatic.NarrowNbSp + "β = " + CStatic.DoubleToString(cos2) + CStatic.EmSp + "cos" + CStatic.NarrowNbSp + "γ = " + CStatic.DoubleToString(cos3) + CStatic.EmSp + "p = " + CStatic.DoubleToString(p);

			return
				"m" + Macron + " = (" + DoubleToString(m.X) + "; " + DoubleToString(m.Y) + "; " + DoubleToString(m.Z) + ")" +
				EmSp +
				"s" + Macron + " = (" + DoubleToString(s.X) + ", " + DoubleToString(s.Y) + ", " + DoubleToString(s.Z) + ")";

			//return "m = " + m + "; s = " + s;
		}
	}
}