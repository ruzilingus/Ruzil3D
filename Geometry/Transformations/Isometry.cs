using Ruzil3D.Algebra;
using static Ruzil3D.Math;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет конгруэнтное преобразование трехмерного евклидова пространства.
	/// </summary>
	public class Isometry : Affinity
	{
		/// <summary>
		/// Получает кватернион поворота пространства соответствующий преобразованию.
		/// </summary>
		/// <value>Кватернион поворота пространства соответствующий преобразованию. Кватернионы q и -q задают один поворот; возвращается кватернион с неотрицательной вещественной частью.</value>
		public Quaternion Quaternion
		{
			get
			{
				//Метод Шеппарда: сначала находится наибольшая по модулю компонента кватерниона (по наибольшему из следа
				//и диагональных элементов матрицы), остальные — делением недиагональных сумм и разностей на нее.
				//Прежде все компоненты вычислялись через квадратные корни диагональных комбинаций со знаками
				//недиагональных разностей: для поворотов на π знаки обращались в ноль (кватернион (0; 0, 0, 0) или NaN),
				//округление делало подкоренное выражение отрицательным (NaN), а малые углы терялись (1e-9 давал x = 0).
				//Кватернион вычисляется при каждом обращении: прежний кэш в поле-структуре мог быть прочитан другим
				//потоком частично записанным.
				var matrix = Matrix;

				var m11 = matrix.Line1.X;
				var m12 = matrix.Line1.Y;
				var m13 = matrix.Line1.Z;
				var m21 = matrix.Line2.X;
				var m22 = matrix.Line2.Y;
				var m23 = matrix.Line2.Z;
				var m31 = matrix.Line3.X;
				var m32 = matrix.Line3.Y;
				var m33 = matrix.Line3.Z;

				var trace = m11 + m22 + m33;

				double w, x, y, z;

				if (trace >= m11 && trace >= m22 && trace >= m33)
				{
					//r = 2|w|
					var r = Sqrt(1 + trace);
					var f = 0.5/r;
					w = r/2;
					x = (m32 - m23)*f;
					y = (m13 - m31)*f;
					z = (m21 - m12)*f;
				}
				else if (m11 >= m22 && m11 >= m33)
				{
					//r = 2|x|
					var r = Sqrt(1 + m11 - m22 - m33);
					var f = 0.5/r;
					w = (m32 - m23)*f;
					x = r/2;
					y = (m12 + m21)*f;
					z = (m13 + m31)*f;
				}
				else if (m22 >= m33)
				{
					//r = 2|y|
					var r = Sqrt(1 - m11 + m22 - m33);
					var f = 0.5/r;
					w = (m13 - m31)*f;
					x = (m12 + m21)*f;
					y = r/2;
					z = (m23 + m32)*f;
				}
				else
				{
					//r = 2|z|
					var r = Sqrt(1 - m11 - m22 + m33);
					var f = 0.5/r;
					w = (m21 - m12)*f;
					x = (m13 + m31)*f;
					y = (m23 + m32)*f;
					z = r/2;
				}

				return w < 0 ? new Quaternion(-w, -x, -y, -z) : new Quaternion(w, x, y, z);
			}
		}


		/// <summary>
		/// Представляет тождественное преобразование.
		/// </summary>
		public new static readonly  Isometry Identity = new Isometry();

		private Isometry(Matrix3D matrix, Point3D center) : base(matrix, center)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса с тождественным преобразованием.
		/// </summary>
		protected Isometry()
		{
		}
		
		/// <summary>
		/// Инициализирует новый экземпляр класса со смещением.
		/// </summary>
		/// <param name="offset">Вектор задающий смещение пространства.</param>
		public Isometry(Point3D offset) : base(offset)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса с поворотом вокруг произвольной оси.
		/// </summary>
		/// <param name="angle">Число задающий угол поворота в радианах.</param>
		/// <param name="axis">Вектор, вокруг которого происходит поворот пространства.</param>
		public Isometry(double angle, Point3D axis) : base(Matrix3D.GetRotation(angle, axis))
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса с поворотом вокруг произвольной оси и смещением.
		/// </summary>
		/// <param name="angle">Число задающий угол поворота в радианах.</param>
		/// <param name="axis">Вектор, вокруг которого происходит поворот пространства.</param>
		/// <param name="offset">Вектор задающий смещение пространства.</param>
		public Isometry(double angle, Point3D axis, Point3D offset) : base(Matrix3D.GetRotation(angle, axis), offset)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса с поворотом вокруг произвольной оси и смещением.
		/// </summary>
		/// <param name="quaternion">Кватернион, задающий поворот.</param>
		/// <param name="offset">Вектор задающий смещение пространства.</param>
		public Isometry(Quaternion quaternion, Point3D offset): base (Matrix3D.GetRotation(quaternion), offset)
		{
			
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса с поворотом вокруг произвольной оси.
		/// </summary>
		/// <param name="quaternion">Кватернион, задающий поворот.</param>
		public Isometry(Quaternion quaternion) : base(Matrix3D.GetRotation(quaternion))
		{

		}

		/// <summary>
		/// Получает преобразование обратное заданному.
		/// </summary>
		/// <returns>Обратное преобразование.</returns>
		public new Isometry GetInvert()
		{
			var m = Matrix.GetInvert();
			return new Isometry(m, -m * Center);
		}

		/// <summary>
		/// Возвращает обратное преобразование, которое также является изометрией.
		/// </summary>
		/// <returns>Обратное преобразование типа <see cref="Isometry"/>.</returns>
		protected override Affinity GetInvertCore()
		{
			//Благодаря переопределению Affinity.GetInvert возвращает Isometry и при вызове через ссылку на Affinity.
			return GetInvert();
		}

		/// <summary>
		/// Возвращает преобразование, являющийся результатом перемножения (суперпозиции) двух преобразований.
		/// </summary>
		/// <param name="x">Первое преобразование.</param>
		/// <param name="y">Второе преобразование.</param>
		/// <returns>Преобразование произведения (суперпозиции).</returns>
		public static Isometry operator *(Isometry x, Isometry y)
		{
			var m = x.Matrix * y.Matrix;

			var v = new Point3D(
				x.Matrix.Line1.DotProduct(y.Center) + x.Center.X,
				x.Matrix.Line2.DotProduct(y.Center) + x.Center.Y,
				x.Matrix.Line3.DotProduct(y.Center) + x.Center.Z
				);

			return new Isometry(m, v);
		}

	}
}