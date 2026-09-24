using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
	/// <summary>
	/// Сглаживатель кривых с переменной шириной ядра: вблизи изломов кривая сглаживается слабее, чем на прямых участках.
	/// </summary>
	/// <remarks>
	/// <para>При создании в каждой вершине ломаной вычисляется значение угла излома — угол между участками кривой до и после вершины,
	/// делённый на π: 1 соответствует прямой, 0.5 — прямому углу, 0 — возврату кривой назад. Эти значения интерполируются
	/// (<see cref="InterpolationCompiler"/>) и сглаживаются тем же ядром; сглаженное значение угла в точке служит масштабом ядра
	/// при сглаживании самой кривой (см. <see cref="BlurCompiler.GetValue3D(Func{double, Point3D}, double, double)"/>).</para>
	/// <para>Методы фильтра не изменяют его состояние, поэтому их можно вызывать из нескольких потоков.</para>
	/// </remarks>
    public class CurveBlurFilter2 : ICurveApproximation
    {
        private readonly BlurCompiler _blurCompiler;
        private readonly ICurveInterpolation _linearCompiler;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CurveBlurFilter2"/> для ломаной с заданным ядром.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки.</param>
        /// <param name="filter">Ядро сглаживания (см. <see cref="BlurCompiler(BlurFunctionHandler, double)"/>).</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> или <paramref name="filter"/> равны null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек либо ядро нельзя скомпилировать.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public CurveBlurFilter2(Point3D[] curve, BlurFunctionHandler filter, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            _blurCompiler = new BlurCompiler(filter);
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
            _cornerInterpolation = BuildCorners();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CurveBlurFilter2"/> для ломаной с заданным скомпилированным ядром.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки.</param>
        /// <param name="filter">Скомпилированное ядро сглаживания.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> или <paramref name="filter"/> равны null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public CurveBlurFilter2(Point3D[] curve, BlurCompiler filter, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _blurCompiler = filter;
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
            _cornerInterpolation = BuildCorners();
        }

        #region Build Corners

        private readonly InterpolationCompiler _cornerInterpolation;

        private InterpolationCompiler BuildCorners()
        {
            var derPonts = new List<PointD>();

            var breakPoint = 0D;
            var lastPoint = _linearCompiler[0];
            for (var i = 0; i < _linearCompiler.Length; i++)
            {
                var curPoint = _linearCompiler[i];

                //Заполняем вспомогательные значения
                breakPoint += lastPoint.Distance(curPoint);
                lastPoint = curPoint;

                //Продолжение кривой за точку излома и шаг приращения берутся из CurveBlurFilter: прежде шаг был абсолютным (1e-9),
                //и вдали от начала координат приращение терялось в ошибке округления координат.
                var corner = new CurveBlurFilter.Corner(_linearCompiler, breakPoint);

                //Вычисляем приращения по разную сторону точки
                var diff1 = -_blurCompiler.GetDifferential(corner.GetValue1, breakPoint, corner.Delta);
                var diff2 = _blurCompiler.GetDifferential(corner.GetValue2, breakPoint, corner.Delta);

                //Угол функции в данной точке.
                //Косинус ограничивается отрезком [-1, 1]: на прямых участках из-за округления он бывал меньше −1, Acos давал NaN,
                //и через сглаживание значения угла NaN распространялся на все точки кривой.
                var value = Math.Max(-1, Math.Min(1, diff1.Cos(diff2)));

                //Значение 1 соответствует углу π (прямая), 0 — возврату кривой назад
                derPonts.Add(new PointD(breakPoint, Math.Acos(value)/ Math.Pi));
            }

            return new InterpolationCompiler(derPonts.ToArray(), _linearCompiler.Mode);
        }

        #endregion

        #region Temporary Items

        /// <summary>
        /// Возвращает сглаженную кривую в виде точек, равномерно расставленных по длине дуги от 0 до <see cref="DMax"/> включительно.
        /// </summary>
        /// <returns>Точки сглаженной кривой; их столько же, сколько вершин у ломаной (для замкнутой — вместе с повторённой первой вершиной).</returns>
        public Point3D[] FilterPoints()
        {
            var count = _linearCompiler.Length;
            var result = new Point3D[count];
            var distance = _linearCompiler.DMax;


            for (var i = 0; i < count; i++)
            {
                var d = distance*i/(count - 1);
                result[i] = GetValue3D(d);
            }


            return result;
        }

        #endregion

        /// <summary>
        /// Создаёт фильтр с тем же ядром для другой ломаной.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <returns>Новый объект <see cref="CurveBlurFilter2"/> с ядром этого фильтра.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> равна null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            //Прежде метод выбрасывал NotImplementedException, хотя входит в интерфейс ICurveApproximation
            return new CurveBlurFilter2(curve, _blurCompiler, mode);
        }

        /// <summary>
        /// Возвращает точку сглаженной кривой на расстоянии <paramref name="d"/> по длине дуги от начала ломаной.
        /// Ядро растягивается в <see cref="GetCornerValue"/>(<paramref name="d"/>) раз, поэтому у изломов сглаживание слабее.
        /// </summary>
        /// <param name="d">Длина дуги. Вне отрезка [0, <see cref="DMax"/>] ломаная продолжается согласно режиму экстраполяции.</param>
        /// <returns>Точка сглаженной кривой.</returns>
        public Point3D GetValue3D(double d)
        {
            //Для вычисления значения в точке нужно знать угол излома в данной точке
            //Зная угол излома, будем знать какой фильтр применять

            //Вычисляем сглаженное значение угла в точке d (значение 1 соответствует углу π, то есть прямой)
            var corner = _blurCompiler.GetValue(_cornerInterpolation.GetValue, d);
            var scale = corner;

            //corner = GetCornerDerValue(d);
            //scale = Math.Max(0, 1 - corner);

            return _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, d, scale);
        }


        /// <summary>
        /// Получает длину ломаной (для замкнутой — вместе с замыкающим отрезком).
        /// </summary>
        public double DMax => _linearCompiler.DMax;

        /// <summary>
        /// Возвращает длину дуги от начала ломаной до вершины с индексом <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Индекс вершины от 0 до <see cref="ICurveInterpolation.Length"/> − 1.</param>
        /// <returns>Длина дуги до вершины.</returns>
        public double GetD(int index)
        {
            return _linearCompiler.GetD(index);
        }

        /// <summary>
        /// Возвращает сглаженное значение угла излома в точке <paramref name="d"/>: угол между участками кривой, делённый на π.
        /// </summary>
        /// <param name="d">Длина дуги.</param>
        /// <param name="scale">Масштаб ядра, не меньше нуля (см. <see cref="BlurCompiler.GetValue(Func{double, double}, double, double)"/>).</param>
        /// <returns>Значение около 1 на прямых участках, 0.5 у прямого угла, около 0 у возврата кривой назад.</returns>
        /// <exception cref="ArgumentException"><paramref name="scale"/> меньше нуля.</exception>
        public double GetCornerValue(double d, double scale = 1)
        {
            //Вычисляем сглаженное значение угла в точке d (значение 1 соответствует углу π, то есть прямой)
            return _blurCompiler.GetValue(_cornerInterpolation.GetValue, d, scale);
        }

        /// <summary>
        /// Возвращает модуль скорости изменения сглаженного значения угла излома <see cref="GetCornerValue"/> по длине дуги.
        /// </summary>
        /// <param name="d">Длина дуги.</param>
        /// <returns>Модуль центральной разности |G(d + 0.01) − G(d − 0.01)| / 0.02, где G — <see cref="GetCornerValue"/>,
        /// то есть оценка модуля производной, а не сама производная.</returns>
        public double GetCornerDerValue(double d)
        {
            //Модуль производной, оценённой центральной разностью с шагом eps

            const double eps = 0.01;
            const double eps2 = 2*eps;

            return Math.Abs(GetCornerValue(d + eps) - GetCornerValue(d - eps))/eps2;
        }
    }
}