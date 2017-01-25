namespace Ruzil3D.Filters
{
    /// <summary>
    /// Представляет делегат фильтра стремящийся к нулю когда radius стремится к бесконечности.
    /// </summary>
    /// <param name="radius">Парамет фильтра radius >= 0</param>
    /// <returns>Значение фильтра.</returns>
    public delegate double BlurFunctionHandler(double radius);
}