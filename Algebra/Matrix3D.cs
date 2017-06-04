using Ruzil3D.Utility;
using static Ruzil3D.Math;

// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace Ruzil3D.Algebra
{
	/// <summary>
	/// Представляет квадратную матрицу размерности 3x3.
	/// </summary>
	public struct Matrix3D
	{
		/// <summary>
		/// Получает или задает первую строчку матрицы представленнную структурой <see cref="Point3D"/>.
		/// </summary>
		public Point3D Line1;

		/// <summary>
		/// Получает или задает вторую строчку матрицы представленнную структурой <see cref="Point3D"/>.
		/// </summary>
		public Point3D Line2;

		/// <summary>
		/// Получает или задает третью строчку матрицы представленнную структурой <see cref="Point3D"/>.
		/// </summary>
		public Point3D Line3;

		#region Statics

		/// <summary>
		/// Представляет новый экземпляр класса <see cref="Matrix3D"/> с неинициализированными данными членов.
		/// </summary>
		public static readonly Matrix3D Empty = new Matrix3D(Point3D.Empty, Point3D.Empty, Point3D.Empty);

		/// <summary>
		/// Представляет единичную матрицу.
		/// </summary>
		public static readonly Matrix3D Identity = new Matrix3D(Point3D.UnitX, Point3D.UnitY, Point3D.UnitZ);

		#endregion

		#region Properties

		/// <summary>
		/// Возвращает значение, показывающее, являтся ли данная матрица нулевой.
		/// </summary>
		/// <param name="x">Матрица.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="x"/> равняется <see cref="Empty"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsEmpty(Matrix3D x) => x == Empty;

		/// <summary>
		/// Возвращает значение, показывающее, равняется ли данная матрица единичной.
		/// </summary>
		/// <param name="x">Матрица.</param>
		/// <returns>Значение <b>true</b>, если параметр <paramref name="x"/> равняется <see cref="Identity"/>; в противном случае — значение <b>false</b>.</returns>
		public static bool IsIdentity(Matrix3D x) => x == Identity;

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Matrix3D"/> с указанными элементами.
		/// </summary>
		/// <param name="a11">Первый элемент первого столбца матрицы: <i>a<sub>11</sub></i>.</param>
		/// <param name="a12">Второй элемент первого столбца матрицы: <i>a<sub>12</sub></i>.</param>
		/// <param name="a13">Третий элемент первого столбца матрицы: <i>a<sub>13</sub></i>.</param>
		/// <param name="a21">Первый элемент второго столбца матрицы: <i>a<sub>21</sub></i>.</param>
		/// <param name="a22">Второй элемент второго столбца матрицы: <i>a<sub>22</sub></i>.</param>
		/// <param name="a23">Третий элемент второго столбца матрицы: <i>a<sub>23</sub></i>.</param>
		/// <param name="a31">Первый элемент третьего столбца матрицы: <i>a<sub>31</sub></i>.</param>
		/// <param name="a32">Второй элемент третьего столбца матрицы: <i>a<sub>32</sub></i>.</param>
		/// <param name="a33">Третий элемент третьего столбца матрицы: <i>a<sub>33</sub></i>.</param>
		public Matrix3D(double a11, double a12, double a13, double a21, double a22, double a23, double a31, double a32,
			double a33)
		{
			Line1 = new Point3D(a11, a12, a13);
			Line2 = new Point3D(a21, a22, a23);
			Line3 = new Point3D(a31, a32, a33);
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Matrix3D"/> со строками указанными структурами <see cref="Point3D"/>.
		/// </summary>
		/// <param name="line1">Первая строка матрицы.</param>
		/// <param name="line2">Вторая строка матрицы.</param>
		/// <param name="line3">Третья строка матрицы.</param>
		public Matrix3D(Point3D line1, Point3D line2, Point3D line3)
		{
			Line1 = line1;
			Line2 = line2;
			Line3 = line3;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Matrix3D"/> соответствующий повороту, указанному кватерионом.
		/// </summary>
		/// <param name="quaternion">Матрица, указывающий на поворот пространства.</param>
		public Matrix3D(Quaternion quaternion)
		{
			var abs = quaternion.Abs;

			var w = quaternion.W/abs;
			var x = quaternion.U.X/abs;
			var y = quaternion.U.Y/abs;
			var z = quaternion.U.Z/abs;

			Line1 = new Point3D(1 - 2*y*y - 2*z*z, 2*x*y - 2*z*w, 2*x*z + 2*y*w);
			Line2 = new Point3D(2*x*y + 2*z*w, 1 - 2*x*x - 2*z*z, 2*y*z - 2*x*w);
			Line3 = new Point3D(2*x*z - 2*y*w, 2*y*z + 2*x*w, 1 - 2*x*x - 2*y*y);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Возвращает определитель матрицы.
		/// </summary>
		/// <returns>Определитель матрицы.</returns>
		public double GetDeterminant()
		{
			return
				Line1.X*Line2.Y*Line3.Z +
				Line1.Y*Line2.Z*Line3.X +
				Line1.Z*Line2.X*Line3.Y -
				Line1.Z*Line2.Y*Line3.X -
				Line1.Y*Line2.X*Line3.Z -
				Line1.X*Line2.Z*Line3.Y;
		}

		/// <summary>
		/// Возвращает союзную матрицу.
		/// </summary>
		/// <returns>Союзная матрица.</returns>
		public Matrix3D GetAdjugate()
		{
			return new Matrix3D(
				Line2.Y*Line3.Z - Line2.Z*Line3.Y, Line1.Z*Line3.Y - Line1.Y*Line3.Z, Line1.Y*Line2.Z - Line1.Z*Line2.Y,
				Line2.Z*Line3.X - Line2.X*Line3.Z, Line1.X*Line3.Z - Line1.Z*Line3.X, Line1.Z*Line2.X - Line1.X*Line2.Z,
				Line2.X*Line3.Y - Line2.Y*Line3.X, Line1.Y*Line3.X - Line1.X*Line3.Y, Line1.X*Line2.Y - Line1.Y*Line2.X
				);
		}


		private Matrix3D GetAdjugate(double coef)
		{
			return new Matrix3D(
				coef*(Line2.Y*Line3.Z - Line2.Z*Line3.Y),
				coef*(Line1.Z*Line3.Y - Line1.Y*Line3.Z),
				coef*(Line1.Y*Line2.Z - Line1.Z*Line2.Y),

				coef*(Line2.Z*Line3.X - Line2.X*Line3.Z),
				coef*(Line1.X*Line3.Z - Line1.Z*Line3.X),
				coef*(Line1.Z*Line2.X - Line1.X*Line2.Z),

				coef*(Line2.X*Line3.Y - Line2.Y*Line3.X),
				coef*(Line1.Y*Line3.X - Line1.X*Line3.Y),
				coef*(Line1.X*Line2.Y - Line1.Y*Line2.X)
				);
		}

		/// <summary>
		/// Возвращает обратную матрицу.
		/// </summary>
		/// <returns>Обратная матрица.</returns>
		public Matrix3D GetInvert()
		{
			return GetAdjugate(1/GetDeterminant());
			//return GetAdjugate()/ GetDeterminant();
		}


		/// <summary>
		/// Получает решение системы из трех уравнений A <i>x</i> = <paramref name="y"/>.
		/// </summary>
		/// <param name="y">Правая часть системы.</param>
		/// <returns>Решение системы из трех уравнений A <i>x</i> = <paramref name="y"/>.</returns>
		public Point3D Resolve(Point3D y)
		{
			return GetInvert()*y;
		}

		/*
		public Point3D Resolve(double y1, double y2, double y3)
		{
			var coef = 1 / GetDeterminant();

			return new Point3D(
				coef * (
					(Line2.Y * Line3.Z - Line2.Z * Line3.Y) * y1 +
					(Line1.Z * Line3.Y - Line1.Y * Line3.Z) * y2 +
					(Line1.Y * Line2.Z - Line1.Z * Line2.Y) * y3
					),

				coef * (
					(Line2.Z * Line3.X - Line2.X * Line3.Z) * y1 +
					(Line1.X * Line3.Z - Line1.Z * Line3.X) * y2 +
					(Line1.Z * Line2.X - Line1.X * Line2.Z) * y3
					),

				coef * (
					(Line2.X * Line3.Y - Line2.Y * Line3.X) * y1 +
					(Line1.Y * Line3.X - Line1.X * Line3.Y) * y2 +
					(Line1.X * Line2.Y - Line1.Y * Line2.X) * y3
					)
				);
		}
		*/


		/// <summary>
		/// Возвращает транспонированную матрицу.
		/// </summary>
		/// <returns>Транспонированная матрица.</returns>
		public Matrix3D GetTranspose()
		{
			return new Matrix3D(
				Line1.X, Line2.X, Line3.X,
				Line1.Y, Line2.Y, Line3.Y,
				Line1.Z, Line2.Z, Line3.Z
				);
		}

		/// <summary>
		/// Возвращает матрицу вращения из кватерниона.
		/// </summary>
		/// <param name="quaternion">Кватернион, указывающий на поворот пространства.</param>
		/// <returns>Матрица вращения эквивалентное кватерниону <paramref name="quaternion"/>.</returns>
		public static Matrix3D GetRotation(Quaternion quaternion)
		{
			return new Matrix3D(quaternion);
		}

		/// <summary>
		/// Возвращает матрицу вращения на заданный угол по произвольной оси.
		/// </summary>
		/// <param name="angle">Угол вращения в радианах заданный числом с плавающей запятой двойной точности.</param>
		/// <param name="axis">Ось вращения заданная структурой <see cref="Point3D"/>.</param>
		/// <returns>Матрица вращения на угол <paramref name="angle"/> по оси <paramref name="axis"/>.</returns>
		public static Matrix3D GetRotation(double angle, Point3D axis)
		{
			axis /= axis.Length;
			var cos = Cos(angle);
			var sin = Sin(angle);

			var n = 1 - cos;

			var x = axis.X;
			var y = axis.Y;
			var z = axis.Z;

			return new Matrix3D(
				cos + n*x*x, n*x*y - sin*z, n*x*z + sin*y,
				n*y*x + sin*z, cos + n*y*y, n*y*z - sin*x,
				n*z*x - sin*y, n*z*y + sin*x, cos + n*z*z
				);
		}

		/// <summary>
		/// Возвращает матрицу вращения на заданный угол по оси X.
		/// </summary>
		/// <param name="angle">Угол вращения по оси X в радианах, заданный числом с плавающей запятой двойной точности.</param>
		/// <returns>Матрица вращения на угол <paramref name="angle"/> по оси X.</returns>
		public static Matrix3D GetRotationX(double angle)
		{
			var sin = Sin(angle);
			var cos = Cos(angle);
			return new Matrix3D(
				1, 0, 0,
				0, cos, -sin,
				0, sin, cos
				);
		}

		/// <summary>
		/// Возвращает матрицу вращения на заданный угол по оси Y.
		/// </summary>
		/// <param name="angle">Угол вращения по оси Y в радианах, заданный числом с плавающей запятой двойной точности.</param>
		/// <returns>Матрица вращения на угол <paramref name="angle"/> по оси Y.</returns>
		public static Matrix3D GetRotationY(double angle)
		{
			var sin = Sin(angle);
			var cos = Cos(angle);
			return new Matrix3D(
				cos, 0, sin,
				0, 1, 0,
				-sin, 0, cos
				);
		}

		/// <summary>
		/// Возвращает матрицу вращения на заданный угол по оси Z.
		/// </summary>
		/// <param name="angle">Угол вращения по оси Z в радианах, заданный числом с плавающей запятой двойной точности.</param>
		/// <returns>Матрица вращения на угол <paramref name="angle"/> по оси Z.</returns>
		public static Matrix3D GetRotationZ(double angle)
		{
			var sin = Sin(angle);
			var cos = Cos(angle);
			return new Matrix3D(
				cos, -sin, 0,
				sin, cos, 0,
				0, 0, 1
				);
		}

		#endregion

		#region Overloads

		/// <summary>
		/// Определяет неявное преобразование числа с плавающей запятой двойной точности в структуру <see cref="Matrix3D"/>.
		/// </summary>
		/// <param name="value">Значение, преобразуемое в структуру <see cref="Matrix3D"/>.</param>
		/// <returns>Диагональная матрица, элементами равными <paramref name="value"/>.</returns>
		public static implicit operator Matrix3D(double value)
		{
			return new Matrix3D(value, 0, 0, 0, value, 0, 0, 0, value);
		}


		/// <summary>
		/// Складывает две матрицы представленные структурой <see cref="Matrix3D"/>.
		/// </summary>
		/// <param name="matrix1">Первое из складываемых матриц.</param>
		/// <param name="matrix2">Второе из складываемых матриц.</param>
		/// <returns>Сумма <paramref name="matrix1"/> и <paramref name="matrix2"/>.</returns>
		public static Matrix3D operator +(Matrix3D matrix1, Matrix3D matrix2)
		{
			return new Matrix3D(
				matrix1.Line1 + matrix2.Line1,
				matrix1.Line2 + matrix2.Line2,
				matrix1.Line3 + matrix2.Line3
				);
		}


		/// <summary>
		/// Вычитает две матрицы представленные структурой <see cref="Matrix3D"/>.
		/// </summary>
		/// <param name="matrix1">Значение, из которого следует вычитать (уменьшаемое).</param>
		/// <param name="matrix2">Значение для вычитания (вычитаемое).</param>
		/// <returns>Разность <paramref name="matrix1"/> и <paramref name="matrix2"/>.</returns>
		public static Matrix3D operator -(Matrix3D matrix1, Matrix3D matrix2)
		{
			return new Matrix3D(
				matrix1.Line1 - matrix2.Line1,
				matrix1.Line2 - matrix2.Line2,
				matrix1.Line3 - matrix2.Line3
				);
		}

		/// <summary>
		/// Возвращает исходную матрицу.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <returns>Исходная матрица.</returns>
		public static Matrix3D operator +(Matrix3D matrix)
		{
			return matrix;
		}

		/// <summary>
		/// Возвращает аддитивную инверсию матрицы заданного параметром <paramref name="matrix"/>.
		/// </summary>
		/// <param name="matrix">Инвертируемое значение.</param>
		/// <returns>Результат умножения исходной матрицы на -1.</returns>
		public static Matrix3D operator -(Matrix3D matrix)
		{
			return new Matrix3D(-matrix.Line1, -matrix.Line2, -matrix.Line3);
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="matrix"/>.</returns>
		public static Matrix3D operator *(Matrix3D matrix, double x)
		{
			return new Matrix3D(matrix.Line1*x, matrix.Line2*x, matrix.Line3*x);
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на множитель с плавающей запятой двойной точности.
		/// </summary>
		/// <param name="x">Множитель с плавающей запятой двойной точности.</param>
		/// <param name="matrix">Исходная матрица.</param>
		/// <returns>Произведение <paramref name="x"/> и <paramref name="matrix"/>.</returns>
		public static Matrix3D operator *(double x, Matrix3D matrix)
		{
			return matrix*x;
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на вектор, преставленный структурой <see cref="Point3D"/>.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="vector">Вектор для перемножения.</param>
		/// <returns>Произведение исходной матрицы на вектор <paramref name="vector"/>.</returns>
		public static Point3D operator *(Matrix3D matrix, Point3D vector)
		{
			return new Point3D(
				matrix.Line1.DotProduct(vector),
				matrix.Line2.DotProduct(vector),
				matrix.Line3.DotProduct(vector)
				);
		}

		/// <summary>
		/// Возвращает произведение исходной матрицы на вектор, преставленный структурой <see cref="PointD"/>.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="vector">Вектор для перемножения.</param>
		/// <returns>Произведение исходной матрицы на вектор <paramref name="vector"/>.</returns>
		public static Point3D operator *(Matrix3D matrix, PointD vector)
		{
			return new Point3D(
				matrix.Line1.DotProduct(vector),
				matrix.Line2.DotProduct(vector),
				matrix.Line3.DotProduct(vector)
				);
		}

		/// <summary>
		/// Возвращает произведение двух матриц.
		/// </summary>
		/// <param name="matrix1">Первая матрица для перемножения.</param>
		/// <param name="matrix2">Вторая матрица для перемножения.</param>
		/// <returns>Произведение двух матриц.</returns>
		public static Matrix3D operator *(Matrix3D matrix1, Matrix3D matrix2)
		{
			return new Matrix3D(
				matrix1.Line1.X*matrix2.Line1.X + matrix1.Line1.Y*matrix2.Line2.X + matrix1.Line1.Z*matrix2.Line3.X,
				matrix1.Line1.X*matrix2.Line1.Y + matrix1.Line1.Y*matrix2.Line2.Y + matrix1.Line1.Z*matrix2.Line3.Y,
				matrix1.Line1.X*matrix2.Line1.Z + matrix1.Line1.Y*matrix2.Line2.Z + matrix1.Line1.Z*matrix2.Line3.Z,

				matrix1.Line2.X*matrix2.Line1.X + matrix1.Line2.Y*matrix2.Line2.X + matrix1.Line2.Z*matrix2.Line3.X,
				matrix1.Line2.X*matrix2.Line1.Y + matrix1.Line2.Y*matrix2.Line2.Y + matrix1.Line2.Z*matrix2.Line3.Y,
				matrix1.Line2.X*matrix2.Line1.Z + matrix1.Line2.Y*matrix2.Line2.Z + matrix1.Line2.Z*matrix2.Line3.Z,

				matrix1.Line3.X*matrix2.Line1.X + matrix1.Line3.Y*matrix2.Line2.X + matrix1.Line3.Z*matrix2.Line3.X,
				matrix1.Line3.X*matrix2.Line1.Y + matrix1.Line3.Y*matrix2.Line2.Y + matrix1.Line3.Z*matrix2.Line3.Y,
				matrix1.Line3.X*matrix2.Line1.Z + matrix1.Line3.Y*matrix2.Line2.Z + matrix1.Line3.Z*matrix2.Line3.Z
				);
		}

		/// <summary>
		/// Возвращает массив структур <see cref="Point3D"/> содержащий результат покомпонентного произведения исходной матрицы с заданным массивом векторов представленных структурой <see cref="Point3D"/>.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="points">Массив векторов.</param>
		/// <returns>Массив структур <see cref="Point3D"/> содержащий результат покомпонентного произведения исходной матрицы с заданным массивом векторов представленных структурой <see cref="Point3D"/>.</returns>
		public static Point3D[] operator *(Matrix3D matrix, Point3D[] points)
		{
			var result = new Point3D[points.Length];

			for (var i = 0; i < points.Length; i++)
			{
				result[i] = matrix*points[i];
			}

			return result;
		}

		/// <summary>
		/// Возвращает массив структур <see cref="Point3D"/> содержащий результат покомпонентного произведения исходной матрицы с заданным массивом векторов представленных структурой <see cref="PointD"/>.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="points">Массив векторов.</param>
		/// <returns>Массив структур <see cref="Point3D"/> содержащий результат покомпонентного произведения исходной матрицы с заданным массивом векторов представленных структурой <see cref="PointD"/>.</returns>
		public static Point3D[] operator *(Matrix3D matrix, PointD[] points)
		{
			var result = new Point3D[points.Length];

			for (var i = 0; i < points.Length; i++)
			{
				result[i] = matrix*points[i];
			}

			return result;
		}

		/// <summary>
		/// Делит исходную матрицу на число с плавающей запятой двойной точности и возвращает результат.
		/// </summary>
		/// <param name="matrix">Исходная матрица.</param>
		/// <param name="x">Знаменатель с плавающей запятой двойной точности.</param>
		/// <returns>Частное от деления <paramref name="matrix"/> на <paramref name="x"/>.</returns>
		public static Matrix3D operator /(Matrix3D matrix, double x)
		{
			return new Matrix3D(matrix.Line1/x, matrix.Line2/x, matrix.Line3/x);
		}

		/// <summary>
		/// Делит число с плавающей запятой двойной точности на исходную матрицу и возвращает результат.
		/// </summary>
		/// <param name="x">Числитель с плавающей запятой двойной точности.</param>
		/// <param name="matrix">Исходная матрица.</param>
		/// <returns>Частное от деления <paramref name="x"/> на <paramref name="matrix"/>.</returns>
		public static Matrix3D operator /(double x, Matrix3D matrix)
		{
			return x*matrix.GetInvert();
		}

		/// <summary>
		/// Делит одну матрицу на другую и возвращает результат.
		/// </summary>
		/// <param name="matrix1">Матрица-числитель.</param>
		/// <param name="matrix2">Матрица - знаменатель.</param>
		/// <returns>Частное от деления <paramref name="matrix1"/> на <paramref name="matrix2"/>.</returns>
		public static Matrix3D operator /(Matrix3D matrix1, Matrix3D matrix2)
		{
			return matrix1*matrix2.GetInvert();
		}

		/// <summary>
		/// Возвращает значение, указывающее, на неравенство двух матриц.
		/// </summary>
		/// <param name="matrix1">Первая матрица для сравнения.</param>
		/// <param name="matrix2">Вторая матрица для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="matrix1"/> и <paramref name="matrix2"/> имеют разные значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Matrix3D matrix1, Matrix3D matrix2)
		{
			return !(matrix1 == matrix2);
		}

		/// <summary>
		/// Возвращает значение, указывающее, на равенство двух матриц.
		/// </summary>
		/// <param name="matrix1">Первая матрица для сравнения.</param>
		/// <param name="matrix2">Вторая матрица для сравнения.</param>
		/// <returns>Значение <b>true</b>, если параметры <paramref name="matrix1"/> и <paramref name="matrix2"/> имеют одинаковые значения; в противном случае — значение <b>false</b>.</returns>
		public static bool operator ==(Matrix3D matrix1, Matrix3D matrix2)
		{
			return matrix1.Equals(matrix2);
		}

		#endregion

		/// <summary>
		/// Возвращает условие показывающее, что хотя бы один из компонентов X, Y или Z не является числом.
		/// </summary>
		public bool IsNaN => Line1.IsNaN || Line2.IsNaN || Line3.IsNaN;

		/// <summary>
		/// Возвращает строковое представлеине данной матрицы.
		/// </summary>
		/// <returns>Строковое представлеине данной матрицы.</returns>
		/// <remarks>
		/// <code>
		/// var matrix = Matrix3D.GetRotation(Math.PI, new Point3D(1, 1, 1));
		/// Console.Write(matrix); //Результат: -1/3 2/3 2/3 | 2/3 -1/3 2/3 | 2/3 2/3 -1/3
		/// </code>
		/// </remarks>
		public override string ToString()
		{
			if (IsNaN)
			{
				return "NaN";
			}

			return
				CStatic.DoubleToString(Line1.X) + CStatic.EmSp + 
				CStatic.DoubleToString(Line1.Y) + CStatic.EmSp +
				CStatic.DoubleToString(Line1.Z) + CStatic.EmSp +
				"|" + CStatic.EmSp +
				CStatic.DoubleToString(Line2.X) + CStatic.EmSp + 
				CStatic.DoubleToString(Line2.Y) + CStatic.EmSp +
				CStatic.DoubleToString(Line2.Z) + CStatic.EmSp +
				"|" + CStatic.EmSp +
				CStatic.DoubleToString(Line3.X) + CStatic.EmSp + 
				CStatic.DoubleToString(Line3.Y) + CStatic.EmSp +
				CStatic.DoubleToString(Line3.Z);
		}

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому.
		/// </summary>
		/// <param name="other">Другая матрица.</param>
		/// <returns>Значение <b>true</b>, если две матрицы совпадают; в противном случае — значение <b>false</b>.</returns>
		public bool Equals(Matrix3D other)
		{
			return Line1.Equals(other.Line1) && Line2.Equals(other.Line2) && Line3.Equals(other.Line3);
		}

		/// <summary>
		/// Показывает, равен ли этот экземпляр заданному объекту.
		/// </summary>
		/// <returns>
		/// Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Matrix3D"/> и представляет одинаковые значения с исходной структурой; в противном случае — значение <b>false</b>.
		/// </returns>
		/// <param name="obj">Другой объект, подлежащий сравнению.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Matrix3D && Equals((Matrix3D) obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>
		/// 32-разрядное целое число со знаком, являющееся хэш-кодом для данного экземпляра.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Line1.GetHashCode();
				hashCode = (hashCode*401) ^ Line2.GetHashCode();
				hashCode = (hashCode*409) ^ Line3.GetHashCode();
				return hashCode;
			}
		}

		#endregion
	}
}