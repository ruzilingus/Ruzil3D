using Ruzil3D.Algebra;
using Ruzil3D.Geometry;

namespace Ruzil3D.Curves
{
    /// <summary>
    /// Планарная кривая в трехмерном евклидовом пространстве заданная параметрически.
    /// </summary>
    public abstract class PlanarCurve : ParametricCurve
	{
		/// <summary>
		/// Конгруэнтное преобразование трехмерного евклидова пространства.
		/// </summary>
		public Isometry Isometry { get; protected set; }

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="PlanarCurve"/> с тождественным преобразованием.
		/// </summary>
		protected PlanarCurve()
		{
			Isometry = Isometry.Identity;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="PlanarCurve"/> с произвольным преобразованием.
		/// </summary>
		/// <param name="isometry"></param>
		protected PlanarCurve(Isometry isometry)
		{
			Isometry = isometry;
		}

		/// <summary>
		/// Возвращает координату точки кривой на плоскости соответствующую параметру.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Координаты точки кривой на плоскости представленной структурой <see cref="PointD"/>.</returns>
		public abstract PointD GetPlanarValue(double parameter);

		/// <summary>
		/// Возвращает координату точки на кривой соответствующую параметру.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Координаты точки на кривой представленной структурой <see cref="Point3D"/>.</returns>
		public override Point3D GetValue(double parameter)
		{
			return Isometry*GetPlanarValue(parameter);
		}


		/// <summary>
		/// Возвращает вектор касательной к кривой на плоскости в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение вектора касательной на плоскости представленный структурой <see cref="PointD"/>.</returns>
		public abstract PointD GetPlanarTangent(double parameter);

		/// <summary>
		/// Возвращает вектор касательной к кривой в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение вектора касательной представленный структурой <see cref="Point3D"/>.</returns>
		public override Point3D GetTangent(double parameter)
		{
			return Isometry.Matrix * GetPlanarTangent(parameter);
		}

		/// <summary>
		/// Возвращает значение кривизны кривой на плоскости в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение кривизны на плоскости.</returns>
		/// <remarks>Значение равное Z-состовляющей псевдовектора кривизны направленого перпендикулярно к плоскости.</remarks>
		public abstract double GetPlanarCurvature(double parameter);

		/// <summary>
		/// Возвращает вектор кривизны к кривой в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение вектора кривизны представленный структурой <see cref="Point3D"/>.</returns>
		/// <remarks>Псевдовектор кривизны направлен перпендикулярно к плоскости образованной векторами нормали и касательной.</remarks>
		public override Point3D GetCurvature(double parameter)
		{
			var planarCurvature = GetPlanarCurvature(parameter);
			return new Point3D(
				Isometry.Matrix.Line1.Z*planarCurvature,
				Isometry.Matrix.Line2.Z*planarCurvature,
				Isometry.Matrix.Line3.Z*planarCurvature
				);
		}
	}
}