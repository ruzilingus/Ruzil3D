namespace Ruzil3D.Filters
{
    /// <summary>
    /// - Режимы экстраполяции кривой вне области определения.
    /// </summary>
    public enum ExtrapolationMode
    {
        /// <summary>
        /// Замкнутая в себя кривая (Первый и последние точки замыкаются друг в друга).
        /// </summary>
        Closed,

        /// <summary>
        /// Кривая продолжается зеркально.
        /// </summary>
        Mirror
    }
}