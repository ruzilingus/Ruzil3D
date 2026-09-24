using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// Интерфейс интерполяции ломаной: кривая проходит через вершины ломаной, доступные по индексу.
    /// </summary>
    public interface ICurveInterpolation : ICurveApproximation
    {
        /// <summary>
        /// Получает вершину ломаной с индексом <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Индекс вершины от 0 до <see cref="Length"/> − 1.</param>
        Point3D this[int index] { get; }

        /// <summary>
        /// Получает режим продолжения ломаной за её концы.
        /// </summary>
        ExtrapolationMode Mode { get; }

        /// <summary>
        /// Получает число вершин ломаной (у замкнутой ломаной последней вершиной может повторяться первая).
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Получает значение, показывающее, замкнута ли ломаная (режим <see cref="ExtrapolationMode.Closed"/>).
        /// </summary>
        bool IsClosed { get; }
    }
}