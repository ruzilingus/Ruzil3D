using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// - Интерфейс аппроксимирующего класса.
    /// </summary>
    public interface ICurveApproximation
    {
        Point3D GetValue3D(double d);

        ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror);

        double DMax { get; }

        double GetD(int index);
    }
}