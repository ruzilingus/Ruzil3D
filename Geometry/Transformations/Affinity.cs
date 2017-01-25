using Ruzil3D.Algebra;

namespace Ruzil3D.Geometry
{
	/// <summary>
	/// Представляет аффинное преобразование системы координат трехмерного евклидова пространства.
	/// </summary>
	public class Affinity
	{
		#region Properties

		/// <summary>
		/// Представляет новый экземпляр класса <see cref="Affinity"/> с тождественным преобразованием.
		/// </summary>
		public static readonly Affinity Identity = new Affinity();

		/// <summary>
		/// Получает матрицу преобразования пространства.
		/// </summary>
		/// <value>Матрица преобразования пространства.</value>
		/// <remarks>Чтобы преобразование было биективным, необходимо чтобы матрица была обратимой, то есть быть невырожденной. По умолчанию это поле приравнено тождественной матрице.</remarks>
		public Matrix3D Matrix { get; protected set; }

		/// <summary>
		/// Получает точку на которую смещается центр координат при преобразовании.
		/// </summary>
		/// <value>Точка на которую смещается центр координат при преобразовании.</value>
		public Point3D Center { get; protected set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Affinity"/> с тождественным преобразованием системы координат.
		/// </summary>
		protected Affinity()
		{
			Matrix = Matrix3D.Identity;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Affinity"/> с указанными параметрами.
		/// </summary>
		/// <param name="matrix">Матрица преобразования базисных векторов.</param>
		/// <param name="center">Смещение центра координат.</param>
		public Affinity(Matrix3D matrix, Point3D center) : this(matrix)
		{
			Center = center;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Affinity"/> с указанным преобразованием базисных векторов.
		/// </summary>
		/// <param name="matrix">Матрица преобразования базисных векторов.</param>
		public Affinity(Matrix3D matrix)
		{
			Matrix = matrix;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="Affinity"/> с указанным смещением центра координат.
		/// </summary>
		/// <param name="center">Смещение центра координат.</param>
		public Affinity(Point3D center) : this()
		{
			Center = center;
		}

		#endregion

		#region Operators

		/// <summary>
		/// Возвращает значение, указывающее на равенство двух преобразований.
		/// </summary>
		/// <param name="x">Первое сравниваемое преобразование.</param>
		/// <param name="y">Второе сравниваемое преобразование.</param>
		/// <returns>Значение <b>true</b>, если два преобразования равны; в противном случае — значение <b>false</b>.</returns>
		/// <remarks>Два преобразования считаются равными, если равны их матрицы и смещения.</remarks>
		public static bool operator ==(Affinity x, Affinity y)
		{
			if (ReferenceEquals(x, null))
			{
				return ReferenceEquals(y, null);
			}

			if (ReferenceEquals(y, null))
			{
				return false;
			}

			return x.Equals(y);
		}

		/// <summary>
		/// Возвращает значение, указывающее на неравенство двух преобразований.
		/// </summary>
		/// <param name="x">Первое сравниваемое преобразование.</param>
		/// <param name="y">Второе сравниваемое преобразование.</param>
		/// <returns>Значение <b>true</b>, если два преобразования не равны; в противном случае — значение <b>false</b>.</returns>
		public static bool operator !=(Affinity x, Affinity y)
		{
			return !(x == y);
		}

		/// <summary>
		/// Возвращает преобразование, являющийся результатом перемножения (суперпозиции) двух преобразований.
		/// </summary>
		/// <param name="x">Первое преобразование.</param>
		/// <param name="y">Второе преобразование.</param>
		/// <returns>Преобразование произведения (суперпозиции).</returns>
		public static Affinity operator *(Affinity x, Affinity y)
		{
			var m = x.Matrix*y.Matrix;

			var v = new Point3D(
				x.Matrix.Line1.DotProduct(y.Center) + x.Center.X,
				x.Matrix.Line2.DotProduct(y.Center) + x.Center.Y,
				x.Matrix.Line3.DotProduct(y.Center) + x.Center.Z
				);

			return new Affinity(m, v);
		}

		/// <summary>
		/// Возвращает преобразование, получаемый в результате масштабирования заданного преобразования на скалярный множитель.
		/// </summary>
		/// <param name="x">Скалярное значение.</param>
		/// <param name="y">Исходное преобразование.</param>        
		/// <returns>Масштабированное преобразование.</returns>
		public static Affinity operator *(double x, Affinity y)
		{
			return new Affinity(x*y.Matrix, x*y.Center);
		}

		/// <summary>
		/// Возвращает преобразование, получаемый в результате масштабирования заданного преобразования на скалярный множитель.
		/// </summary>
		/// <param name="x">Исходное преобразование.</param>
		/// <param name="y">Скалярное значение.</param>
		/// <returns>Масштабированное преобразование.</returns>
		public static Affinity operator *(Affinity x, double y)
		{
			return y*x;
		}

		/// <summary>
		/// Преобразовывает структуру <see cref="Point3D"/>.
		/// </summary>
		/// <param name="x">Преобразование.</param>
		/// <param name="y">Структура <see cref="Point3D"/>.</param>
		/// <returns>Преобразованная структура <see cref="Point3D"/>.</returns>
		public static Point3D operator *(Affinity x, Point3D y)
		{
			return x.Matrix*y + x.Center;
		}

		/// <summary>
		/// Преобразовывает структуру <see cref="PointD"/>.
		/// </summary>
		/// <param name="x">Преобразование.</param>
		/// <param name="y">Структура <see cref="PointD"/>.</param>
		/// <returns>Преобразованная структура <see cref="PointD"/>.</returns>
		public static Point3D operator *(Affinity x, PointD y)
		{
			return x.Matrix*y + x.Center;
		}

		/// <summary>
		/// Преобразовывает массив структур <see cref="Point3D"/>.
		/// </summary>
		/// <param name="x">Преобразование.</param>
		/// <param name="y">Массив структур <see cref="Point3D"/>.</param>
		/// <returns>Преобразованный массив структур <see cref="Point3D"/>.</returns>
		public static Point3D[] operator *(Affinity x, Point3D[] y)
		{
			var result = new Point3D[y.Length];

			for (var i = 0; i < y.Length; i++)
			{
				result[i] = x*y[i];
			}
			return result;
		}

		/// <summary>
		/// Преобразовывает массив структур <see cref="PointD"/>.
		/// </summary>
		/// <param name="x">Преобразование.</param>
		/// <param name="y">Массив структур <see cref="PointD"/>.</param>
		/// <returns>Преобразованный массив структур <see cref="Point3D"/>.</returns>
		public static Point3D[] operator *(Affinity x, PointD[] y)
		{
			var result = new Point3D[y.Length];

			for (var i = 0; i < y.Length; i++)
			{
				result[i] = x*y[i];
			}
			return result;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Возвращает обратное преобразование.
		/// </summary>
		/// <returns>Обратное преобразование</returns>
		public Affinity GetInvert()
		{
			var m = Matrix.GetInvert();
			return new Affinity(m, -m*Center);
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр другому преобразованию.
		/// </summary>
		/// <param name="other">Другое преобразование.</param>
		/// <returns>Значение <b>true</b>, если два преобразования равны; в противном случае — значение <b>false</b>.</returns>
		protected bool Equals(Affinity other)
		{
			return Matrix.Equals(other.Matrix) && Center.Equals(other.Center);
		}

		/// <summary>
		/// Возвращает значение, указывающее, равен ли данный экземпляр указанному объекту.
		/// </summary>
		/// <param name="obj">Объект для сравнения с текущим экземпляром.</param>
		/// <returns>Значение <b>true</b>, если <paramref name="obj"/> относится к типу <see cref="Affinity"/> не является <b>null</b> и представляет одинаковые значения с исходным объектом; в противном случае — значение <b>false</b>.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Affinity) obj);
		}

		/// <summary>
		/// Возвращает хэш-код данного экземпляра.
		/// </summary>
		/// <returns>Хэш-код.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (Matrix.GetHashCode()*397) ^ Center.GetHashCode();
			}
		}

		#endregion
	}
}