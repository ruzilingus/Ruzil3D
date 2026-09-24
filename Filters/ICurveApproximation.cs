using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// Интерфейс аппроксимации кривой, заданной вершинами ломаной и параметризованной длиной дуги.
    /// </summary>
    public interface ICurveApproximation
    {
        /// <summary>
        /// Возвращает точку аппроксимирующей кривой на расстоянии <paramref name="d"/> по длине дуги от начала ломаной.
        /// </summary>
        /// <param name="d">Длина дуги. Вне отрезка [0, <see cref="DMax"/>] кривая продолжается согласно режиму <see cref="ExtrapolationMode"/>.</param>
        /// <returns>Точка кривой.</returns>
        Point3D GetValue3D(double d);

        /// <summary>
        /// Создаёт аппроксимацию того же вида и с теми же настройками для другой ломаной.
        /// </summary>
        /// <param name="curve">Вершины ломаной.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <returns>Новая аппроксимация.</returns>
        ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror);

        /// <summary>
        /// Получает длину ломаной (для замкнутой — вместе с замыкающим отрезком).
        /// </summary>
        double DMax { get; }

        /// <summary>
        /// Возвращает длину дуги от начала ломаной до вершины с индексом <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Индекс вершины.</param>
        /// <returns>Длина дуги до вершины.</returns>
        double GetD(int index);
    }
}