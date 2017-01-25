using Ruzil3D.Algebra;
using static Ruzil3D.Math;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет конгруэнтное преобразование трехмерного евклидова пространства.
	/// </summary>
	public class Isometry : Affinity
	{
		private Quaternion _quaternion = Quaternion.Empty;

		/// <summary>
		/// Получает кватернион поворота пространства соответствующий преобразованию.
		/// </summary>
		/// <value>Кватернион поворота пространства соответствующий преобразованию.</value>
		public Quaternion Quaternion
		{
			get
			{
				if (_quaternion == Quaternion.Empty)
				{
					var w = Sqrt(Matrix.Line1.X + Matrix.Line2.Y + Matrix.Line3.Z + 1)/2;
					var x = Sqrt(Matrix.Line1.X - Matrix.Line2.Y - Matrix.Line3.Z + 1)/2*Sign(Matrix.Line3.Y - Matrix.Line2.Z);
					var y = Sqrt(-Matrix.Line1.X + Matrix.Line2.Y - Matrix.Line3.Z + 1)/2*Sign(Matrix.Line1.Z - Matrix.Line3.X);
					var z = Sqrt(-Matrix.Line1.X - Matrix.Line2.Y + Matrix.Line3.Z + 1)/2*Sign(Matrix.Line2.X - Matrix.Line1.Y);

					_quaternion = new Quaternion(w,x,y,z);
				}

				return _quaternion;
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
		/// <returns></returns>
		public new Isometry GetInvert()
		{
			var m = Matrix.GetInvert();
			return new Isometry(m, -m * Center);
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