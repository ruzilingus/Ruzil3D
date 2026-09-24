using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;
using Ruzil3D.Curves;
using static Ruzil3D.Math;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// Сглаживатель кривых в трехмерном пространстве: сглаживает ломаную, параметризованную длиной дуги, ядром <see cref="BlurCompiler"/>.
    /// </summary>
    /// <remarks>
    /// <para>Точка сглаженной кривой на расстоянии d (по длине дуги) от начала — это взвешенное среднее точек ломаной на
    /// расстояниях d ± rᵢ. За пределами [0, <see cref="DMax"/>] ломаная продолжается согласно режиму <see cref="ExtrapolationMode"/>:
    /// в режиме <see cref="ExtrapolationMode.Mirror"/> — центральной симметрией относительно концов, поэтому концы сглаженной
    /// незамкнутой кривой совпадают с концами ломаной, а прямая остаётся прямой; в режиме <see cref="ExtrapolationMode.Closed"/> —
    /// периодически.</para>
    /// <para>Ядро задано в абсолютных единицах длины: ядро по умолчанию — гауссово с σ ≈ 0.354 (см. <see cref="BlurCompiler.DefaultFilter"/>).
    /// Для данных в других масштабах следует задать ядро подходящей ширины.</para>
    /// <para>Методы фильтра не изменяют его состояние, поэтому при потокобезопасной интерполяции (например,
    /// <see cref="CurveInterpolationCompiler"/>) их можно вызывать из нескольких потоков.</para>
    /// </remarks>
    public class CurveBlurFilter : ICurveApproximation
    {
        private readonly BlurCompiler _blurCompiler;
        private readonly ICurveInterpolation _linearCompiler;

        #region Search Corners

        /// <summary>
        /// Продолжение кривой за точку излома центральной симметрией относительно этой точки.
        /// </summary>
        /// <remarks>Прежде точка излома хранилась в полях фильтра, и одновременные вызовы
        /// <see cref="SearchCorners(double, double, double)"/> и <see cref="GetCorrection"/> из разных потоков портили друг другу результат.</remarks>
        internal sealed class Corner
        {
            //Шаг приращения на единицу длины дуги (и величины координат, если их нельзя отсчитывать от точки излома)
            private const double RelativeDelta = 0.000000001D;

            //Точка кривой относительно точки излома
            private readonly Func<double, Point3D> _offset;
            private readonly double _breakPoint;
            private readonly double _doubleBreakPoint;

            public Corner(ICurveInterpolation linearCompiler, double breakPoint)
            {
                _breakPoint = breakPoint;
                _doubleBreakPoint = 2 * breakPoint;

                double magnitude;
                _offset = GetOffsetFunction(linearCompiler, linearCompiler.GetValue3D(breakPoint), out magnitude);

                //Прежде шаг был абсолютным (1e-9), а точки кривой — абсолютными координатами. Вдали от начала координат их
                //ошибка округления (порядка 1e-16·|x|) оказывалась сравнимой с приращением: при смещении 1e6 находились ложные
                //изломы, а при 1e7 и 1e8 изломы смещались или пропадали, и получался NaN. Теперь точки отсчитываются от точки
                //излома, а шаг растёт с длиной дуги, чтобы аргументы breakPoint ± Delta различались и для длинных кривых.
                Delta = RelativeDelta * Max(1, Max(Abs(breakPoint), magnitude));
            }

            /// <summary>
            /// Шаг для вычисления приращений сглаженных функций <see cref="GetValue1"/> и <see cref="GetValue2"/> в точке излома.
            /// </summary>
            public double Delta { get; }

            //Кривая до точки излома и её продолжение после неё (относительно точки излома)
            public Point3D GetValue1(double d)
            {
                if (d <= _breakPoint)
                {
                    return _offset(d);
                }
                return -_offset(_doubleBreakPoint - d);
            }

            //Кривая после точки излома и её продолжение до неё (относительно точки излома)
            public Point3D GetValue2(double d)
            {
                if (d >= _breakPoint)
                {
                    return _offset(d);
                }
                return -_offset(_doubleBreakPoint - d);
            }

            /// <summary>
            /// Возвращает функцию, дающую точку кривой относительно точки <paramref name="origin"/>.
            /// </summary>
            /// <param name="linearCompiler">Интерполяция кривой.</param>
            /// <param name="origin">Точка отсчёта.</param>
            /// <param name="magnitude">Величина координат, от которой зависит погрешность разности: 0, если разность вычисляется
            /// без промежуточных абсолютных координат (<see cref="CurveInterpolationCompiler"/>), иначе |<paramref name="origin"/>|.</param>
            public static Func<double, Point3D> GetOffsetFunction(ICurveInterpolation linearCompiler, Point3D origin, out double magnitude)
            {
                var compiler = linearCompiler as CurveInterpolationCompiler;
                if (compiler != null)
                {
                    magnitude = 0;
                    return d => compiler.GetOffset3D(d, origin);
                }

                magnitude = origin.Length;
                return d => linearCompiler.GetValue3D(d) - origin;
            }
        }

        //Прежде структура реализовывала только необобщённый IComparable: List.Sort упаковывал оба операнда при каждом
        //сравнении, а CompareTo(null) и CompareTo для объекта другого типа выбрасывали NullReferenceException и InvalidCastException.
        private struct CIndexValue : IComparable<CIndexValue>
        {
            public readonly int Index;
            // ReSharper disable once MemberCanBePrivate.Local
            public readonly double Value;

            public CIndexValue(int index, double value)
            {
                Index = index;
                Value = value;
            }

            public int CompareTo(CIndexValue other)
            {
                return Value.CompareTo(other.Value);
            }

            public override string ToString()
            {
                return nameof(Index) + ": " + Index + ", "+ nameof(Value) + ": " + Value;
            }
        }

        //Разрешение по углу, радианы: отклонение от прямой или от разворота меньше этого значения неотличимо от погрешности
        private const double AngleResolution = 0.000001D;

        /// <summary>
        /// Возвращает поправку, направленную по внешней биссектрисе угла излома кривой в точке <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Длина дуги от начала кривой до точки излома (обычно <see cref="GetD"/> вершины).</param>
        /// <returns>
        /// Вектор длины (π − α)/α вдоль внешней биссектрисы угла излома, где α — угол (в радианах) между участками кривой
        /// до и после точки, найденный по производным сглаженных продолжений кривой: α = π для прямой, π/2 для прямого угла, 0 для
        /// возврата кривой назад. Для прямого (с точностью 1e-6 радиана) участка возвращается нулевой вектор, а для острия угол
        /// ограничивается снизу значением 1e-6, чтобы длина поправки оставалась конечной.
        /// </returns>
        public Point3D GetCorrection(double value)
        {
            var corner = new Corner(_linearCompiler, value);

            //Вычисляем приращения по разную сторону точки
            var diff1 = -_blurCompiler.GetDifferential(corner.GetValue1, value, corner.Delta);
            var diff2 = _blurCompiler.GetDifferential(corner.GetValue2, value, corner.Delta);

            //Угол функции в данной точке.
            //Из-за округления косинус выходил за пределы [-1, 1], и Acos давал NaN.
            var cos = Max(-1, Min(1, diff1.Cos(diff2)));
            var angle = Acos(cos);

            var sum = diff1 / diff1.Length + diff2 / diff2.Length;

            //На прямом участке сумма единичных векторов нулевая: прежде её нормировка давала NaN (0/0) на отрезках,
            //параллельных осям, и вектор-шум длиной около 6e-8 в случайном направлении на остальных.
            var length = sum.Length;
            if (length <= AngleResolution)
            {
                return Point3D.Empty;
            }

            sum = -sum / length;

            //Для острия угол равен нулю, и прежде множитель (π − angle)/angle был бесконечным
            sum *= (Pi - angle) / Max(angle, AngleResolution);

            return sum;
        }

        /// <summary>
        /// Находит вершины излома кривой.
        /// </summary>
        /// <param name="derivative">Массив, в который для каждой проверенной вершины i записывается косинус угла поворота кривой в ней
        /// (1 — прямая, 0 — прямой угол, −1 — возврат назад), или null. Длина массива должна быть не меньше числа вершин
        /// <see cref="ICurveInterpolation.Length"/>; элементы непроверенных вершин (у концов незамкнутой кривой) не изменяются.</param>
        /// <param name="smoothness">Наименьшее расстояние по длине дуги между найденными изломами, а у незамкнутой кривой — и от её концов.
        /// Из близких изломов остаётся самый острый.</param>
        /// <param name="maxAngle">Наибольший угол излома, градусы: излом — вершина, в которой угол между участками кривой меньше
        /// <paramref name="maxAngle"/>, то есть кривая поворачивает больше чем на 180° − <paramref name="maxAngle"/>.</param>
        /// <param name="scale">Масштаб ядра сглаживания (см. <see cref="BlurCompiler.GetValue3D(Func{double, Point3D}, double, double)"/>).</param>
        /// <returns>Упорядоченные по возрастанию индексы вершин излома.</returns>
        public int[] SearchCorners(ref double[] derivative, double smoothness = 0.5D, double maxAngle = 120, double scale = 1)
        {
            maxAngle = Max(0, Min(Pi, maxAngle * Pi / 180));

            //Предельное значение угла меньше которого не считается изломом
            var maxCos = Cos(Pi - maxAngle);



            var closed = _linearCompiler.IsClosed;


            var maxDistance = _linearCompiler.DMax;

            var bestPoints = new List<CIndexValue>();
            var distanceArray = new double[_linearCompiler.Length];

            var breakPoint = 0D;
            var lastPoint = _linearCompiler[0];

            var len = closed ? _linearCompiler.Length - 1 : _linearCompiler.Length;

            for (var i = 0; i < len; i++)
            {
                var curPoint = _linearCompiler[i];

                //Заполняем вспомогательные значения
                breakPoint += lastPoint.Distance(curPoint);
                lastPoint = curPoint;
                distanceArray[i] = breakPoint;

                if (!closed)
                {
                    if (breakPoint < smoothness)
                    {
                        //Игнорируем начальные точки
                        continue;
                    }
                    if (maxDistance - breakPoint < smoothness)
                    {
                        //Игнорируем конечные точки
                        break;
                    }
                }

                var corner = new Corner(_linearCompiler, breakPoint);

                //Вычисляем приращения по разную сторону точки
                var diff1 = _blurCompiler.GetDifferential(corner.GetValue1, breakPoint, corner.Delta, scale);
                var diff2 = _blurCompiler.GetDifferential(corner.GetValue2, breakPoint, corner.Delta, scale);

                //Угол функции в данной точке
                var value = diff1.Cos(diff2);

                if (derivative != null)
                {
                    derivative[i] = value;
                }

                if (value < maxCos)
                {
                    bestPoints.Add(new CIndexValue(i, value));
                }

            }

            var resultList = new List<CIndexValue>();
            bestPoints.Sort();

            foreach (var curValue in bestPoints)
            {
                var success = true;
                foreach (var value in resultList)
                {
                    var dist1 = distanceArray[curValue.Index];
                    var dist2 = distanceArray[value.Index];

                    double distance;
                    if (closed)
                    {
                        distance = dist2 > dist1
                            ? Min(dist2 - dist1, dist1 + maxDistance - dist2)
                            : Min(dist1 - dist2, dist2 + maxDistance - dist1);
                    }
                    else
                    {
                        distance = Abs(dist2 - dist1);
                    }

                    if (distance < smoothness)
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                {
                    resultList.Add(curValue);
                }
            }

            //Заполняем индексы
            var result = new int[resultList.Count];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = resultList[i].Index;
            }

            //Сортируем индексы
            Array.Sort(result);

            return result;
        }

        /// <summary>
        /// Находит вершины излома кривой.
        /// </summary>
        /// <param name="smoothness">Наименьшее расстояние по длине дуги между найденными изломами, а у незамкнутой кривой — и от её концов.
        /// Из близких изломов остаётся самый острый.</param>
        /// <param name="maxAngle">Наибольший угол излома, градусы: излом — вершина, в которой угол между участками кривой меньше
        /// <paramref name="maxAngle"/>, то есть кривая поворачивает больше чем на 180° − <paramref name="maxAngle"/>.</param>
        /// <param name="scale">Масштаб ядра сглаживания (см. <see cref="BlurCompiler.GetValue3D(Func{double, Point3D}, double, double)"/>).</param>
        /// <returns>Упорядоченные по возрастанию индексы вершин излома.</returns>
        public int[] SearchCorners(double smoothness = 0.5D, double maxAngle = 120, double scale = 1)
        {
            double[] derivative = null;
            return SearchCorners(ref derivative, smoothness, maxAngle, scale);
        }

        #endregion

        #region Temporary Items

        //Пределы числа точек в FilterPoints
        private const int MinFilterPoints = 10;
        private const int MaxFilterPoints = 1000000;

        /// <summary>
        /// Возвращает сглаженную кривую в виде точек, равномерно расставленных по длине дуги от 0 до <see cref="DMax"/> включительно.
        /// </summary>
        /// <returns>Точки сглаженной кривой: 25 точек на единицу длины дуги плюс 2, но не меньше 10 и не больше 1 000 000
        /// (для кривых длиннее ~40 000 точки редеют).
        /// Первая и последняя точки соответствуют началу и концу кривой.</returns>
        public Point3D[] FilterPoints()
        {
            //Прежде число точек не ограничивалось: для кривой длиннее ~8.6e7 приведение к int переполнялось,
            //и создание массива выбрасывало OverflowException, а для кривой длиной 0.02 получалось всего 3 точки.
            var count = (int)Max(MinFilterPoints, Min(MaxFilterPoints, Round(_linearCompiler.DMax * 25) + 2));

            var result = new Point3D[count];
            var distance = _linearCompiler.DMax;

            var divider = count - 1D;

            for (var i = 0; i < count; i++)
            {
                var coef = i / divider;
                var d = coef * distance;

                // result[i] = _points[i];
                //result[i] = GetLinearValue(d);


                result[i] = GetValue3D(d);
                //result[i].Z += 1.5;
            }


            return result;



            /*
			List<Point3D> result = new List<Point3D>();
			double distance = _distance[_distance.Length - 1];

			double max = _points.Length * 10;
			for (int i = 0; i <= max; i++)
			{
			    double d = distance * i / max;

			    Point3D point = GetBlurValue(d);
			    point.Z -= 1;


			    result.Add(point);
			}

			return result.ToArray();
			*/




            /*
			List<Point3D> result = new List<Point3D>();
			double distance = _distance[_distance.Length - 1];

			for (int i = -_points.Length * 10; i < _points.Length * 10; i++)
			{
			    double d = distance * i / (_points.Length - 1);

			    Point3D point = GetLinearValue(d);
			    point = new Point3D(d, point.X, 0);


			    result.Add(point);
			}

			return result.ToArray();
			*/


        }

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CurveBlurFilter"/> для ломаной с гауссовым ядром по умолчанию.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> равна null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public CurveBlurFilter(IEnumerable<Point3D> curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            _blurCompiler = new BlurCompiler();
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CurveBlurFilter"/> с заданными ядром и интерполяцией кривой.
        /// </summary>
        /// <param name="filter">Скомпилированное ядро сглаживания.</param>
        /// <param name="interpolator">Интерполяция кривой по длине дуги.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filter"/> или <paramref name="interpolator"/> равны null.</exception>
        /// <exception cref="ArgumentException">Режим экстраполяции <paramref name="interpolator"/> не является допустимым значением.</exception>
        public CurveBlurFilter(BlurCompiler filter, ICurveInterpolation interpolator)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (interpolator == null)
            {
                throw new ArgumentNullException(nameof(interpolator));
            }

            //Прежде недопустимый режим обнаруживался только в GetValue3D на концах кривой (ArgumentOutOfRangeException без имени параметра)
            if (interpolator.Mode != ExtrapolationMode.Closed && interpolator.Mode != ExtrapolationMode.Mirror)
            {
                throw new ArgumentException("Недопустимый режим экстраполяции интерполяции кривой: " + interpolator.Mode + ".", nameof(interpolator));
            }

            _blurCompiler = filter;
            _linearCompiler = interpolator;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CurveBlurFilter"/> для ломаной с заданным скомпилированным ядром.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки.</param>
        /// <param name="filter">Скомпилированное ядро сглаживания.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> или <paramref name="filter"/> равны null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public CurveBlurFilter(IEnumerable<Point3D> curve, BlurCompiler filter, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _blurCompiler = filter;
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CurveBlurFilter"/> для ломаной с заданным ядром.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки.</param>
        /// <param name="filter">Ядро сглаживания (см. <see cref="BlurCompiler(BlurFunctionHandler, double)"/>).</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> или <paramref name="filter"/> равны null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек либо ядро нельзя скомпилировать.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public CurveBlurFilter(IEnumerable<Point3D> curve, BlurFunctionHandler filter, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            _blurCompiler = new BlurCompiler(filter);
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
        }

        #endregion

        //Шаг производной на единицу длины дуги (и величины координат, если их нельзя отсчитывать от точки кривой)
        private const double RelativeDerivativeDelta = 0.000001D;

        /// <summary>
        /// Возвращает производную сглаженной кривой по длине дуги в точке <paramref name="d"/>.
        /// </summary>
        /// <param name="d">Длина дуги.</param>
        /// <param name="value">Точка сглаженной кривой в <paramref name="d"/>, от которой отсчитываются точки кривой.</param>
        private Point3D GetBlurDerivative(double d, Point3D value)
        {
            //Как и в Corner, точки отсчитываются от точки кривой, а шаг растёт с длиной дуги,
            //чтобы приращение не терялось в ошибке округления
            double magnitude;
            var offset = Corner.GetOffsetFunction(_linearCompiler, value, out magnitude);
            var delta = RelativeDerivativeDelta * Max(1, Max(Abs(d), magnitude));
            return _blurCompiler.GetDerivative(offset, d, delta);
        }

        /// <summary>
        /// Возвращает кубическую кривую Безье, приближающую сглаженную кривую на участке длины дуги от <paramref name="d0"/> до <paramref name="d1"/>.
        /// </summary>
        /// <param name="d0">Длина дуги в начале участка.</param>
        /// <param name="d1">Длина дуги в конце участка.</param>
        /// <returns>
        /// Кривая Безье в трёхмерном пространстве (кубический многочлен Эрмита): её концы P0 и P3 совпадают с точками
        /// <see cref="GetValue3D"/>(<paramref name="d0"/>) и <see cref="GetValue3D"/>(<paramref name="d1"/>), а касательные в концах —
        /// с производными s₀ и s₁ сглаженной кривой по длине дуги: P1 = P0 + s₀·(d1 − d0)/3, P2 = P3 − s₁·(d1 − d0)/3.
        /// </returns>
        public BezierCurve Resolve(double d0, double d1)
        {
            //Прежде кривая строилась устаревшим двумерным методом BezierCurve.ResolveXY по проекциям точек на плоскость xOy,
            //и у трёхмерной кривой терялась координата Z.
            var p0 = GetValue3D(d0);
            var p3 = GetValue3D(d1);

            var s0 = GetBlurDerivative(d0, p0);
            var s1 = GetBlurDerivative(d1, p3);

            var step = (d1 - d0) / 3D;

            return new BezierCurve(p0, p0 + s0 * step, p3 - s1 * step, p3);
        }

        #region IInterpolation Items

        /// <summary>
        /// Создаёт фильтр с тем же ядром для другой ломаной.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <returns>Новый объект <see cref="CurveBlurFilter"/> с ядром этого фильтра.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> равна null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            //Прежде создавался фильтр с гауссовым ядром по умолчанию, и заданное ядро терялось
            return new CurveBlurFilter(curve, _blurCompiler, mode);
        }

        /// <summary>
        /// Возвращает точку сглаженной кривой на расстоянии <paramref name="d"/> по длине дуги от начала ломаной.
        /// </summary>
        /// <param name="d">Длина дуги. Вне отрезка [0, <see cref="DMax"/>] ломаная продолжается согласно режиму экстраполяции.</param>
        /// <returns>Точка сглаженной кривой. В режиме <see cref="ExtrapolationMode.Mirror"/> концы (d = 0 и d = <see cref="DMax"/>)
        /// точно совпадают с концами ломаной, в режиме <see cref="ExtrapolationMode.Closed"/> обоим концам соответствует одна точка.</returns>
        public Point3D GetValue3D(double d)
        {
            //Для обеспечения нулевой точности на границах
            if (d.Equals(0D) || d.Equals( _linearCompiler.DMax))
            {
                switch (_linearCompiler.Mode)
                {
                    case ExtrapolationMode.Mirror:
                        return _linearCompiler.GetValue3D(d);

                    case ExtrapolationMode.Closed:
                        return _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, 0);

                    default:
                        //Режим проверяется в конструкторах
                        throw new InvalidOperationException("Недопустимый режим экстраполяции: " + _linearCompiler.Mode + ".");
                }
            }

            return _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, d);
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

        #endregion
    }
}