using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// - Интерфейс интерполирующего класса.
    /// </summary>
    public interface ICurveInterpolation : ICurveApproximation
    {
        Point3D this[int index] { get; }

        ExtrapolationMode Mode { get; }

        int Length { get; }

        bool IsClosed { get; }
    }
}