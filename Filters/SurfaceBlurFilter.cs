using System;
using System.Collections.Generic;
using static Ruzil3D.Math;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// Сглаживатель поверхностей: сглаживает функцию двух переменных радиальным ядром <see cref="BlurFunctionHandler"/>.
    /// </summary>
    /// <remarks>
    /// <para>Плоскость вокруг точки делится на центральный круг и кольца, каждое следующее кольцо в 1.5 раза шире предыдущего
    /// (ширина центрального круга — 0.01). Вес кольца равен значению ядра на средней окружности, умноженному на площадь кольца,
    /// и делится поровну между точками, равномерно расставленными по средней окружности; веса нормируются так, чтобы их сумма
    /// была равна единице.</para>
    /// <para>Кольца добавляются, пока ядро не станет пренебрежимо малым (модуль веса кольца меньше 1e-20 наибольшего) на участке,
    /// где радиус вырастает в 10 раз; радиус ядра ограничен значением в 10⁶ раз больше внешнего радиуса кольца с наибольшим весом.
    /// Если ядро настолько узкое, что наибольший вес приходится на центральный круг или первые два кольца, ширина центрального
    /// круга уменьшается в 10 раз (не более 30 раз).</para>
    /// <para>Объект неизменяем, и его можно использовать из нескольких потоков.</para>
    /// </remarks>
    public class SurfaceBlurFilter
    {
        #region Fields

        //Функция к которой нужно применить фильтр
        private readonly Func<double, double, double> _function;

        //Массивы с параметрами фильтра
        private readonly double[] _arrayRadius;
        private readonly double[] _arrayVolumes;
        private readonly int[] _arraySectors;

        #endregion

        #region Methods

        //Кольцо считается пренебрежимо малым, если модуль его объёма меньше этой доли наибольшего модуля объёма кольца.
        //Вклад таких колец не виден в double, а для гауссова ядра по умолчанию набор колец остаётся прежним.
        private const double RelativeLimit = 1E-20D;

        //Ядро считается закончившимся, если после последнего значимого кольца радиус вырос во столько раз
        private const double SupportGap = 10D;

        //Радиус ядра ограничен этим множителем относительно внешнего радиуса кольца с наибольшим объёмом
        private const double MaxSpread = 1E6D;

        //Предельное число колец
        private const int MaxRings = 1000;

        //Наименьший допустимый номер кольца с наибольшим объёмом (0 — центральный круг)
        private const int MinPeakRing = 3;

        //Предельное число уменьшений ширины центрального круга
        private const int MaxRefinements = 30;

        //Сумма весов считается нулевой, если она по модулю меньше этой доли суммы модулей весов: погрешность квадратуры
        //по кольцам — порядка 0.1 % суммы модулей, и при меньшей сумме нормировка теряет смысл (например, у лапласиана гауссианы)
        private const double ZeroWeightTolerance = 1E-2D;

        /// <summary>
        /// Заполняет параметрами фильтра.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="arrayRadius"></param>
        /// <param name="arrayVolumes"></param>
        /// <param name="arraySectors"></param>
        private static void PrepareBlur(BlurFunctionHandler filter, out double[] arrayRadius, out double[] arrayVolumes, out int[] arraySectors)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            //Начальное значение ширины сектора
            var drStart = 0.01D;

            List<double> listRadius;
            List<double> listVolumes;
            List<int> listSectors;
            double vSum;
            double absSum;

            for (var refinement = 0; ; refinement++)
            {
                var peakRing = BuildRings(filter, drStart, out listRadius, out listVolumes, out listSectors, out vSum, out absSum);

                //Прежде ширина центрального круга 0.01 не зависела от ядра: ядро уже нескольких таких шагов почти целиком
                //попадало в центральный круг, и фильтр молча переставал сглаживать.
                if (peakRing >= MinPeakRing || refinement == MaxRefinements)
                {
                    break;
                }

                drStart /= 10D;
            }

            //Прежде нулевое ядро давало 0/0 = NaN во всех весах
            if (!(Abs(vSum) > ZeroWeightTolerance * absSum))
            {
                throw new ArgumentException("Сумма весов ядра равна нулю или пренебрежимо мала по сравнению с суммой их модулей, поэтому ядро нельзя нормировать.", nameof(filter));
            }

            //Нормируем объемы
            for (var i = 0; i < listVolumes.Count; i++)
            {
                listVolumes[i] /= vSum;
            }

            arrayRadius = listRadius.ToArray();
            arraySectors = listSectors.ToArray();
            arrayVolumes = listVolumes.ToArray();

        }

        /// <summary>
        /// Разбивает ядро на кольца, начиная с центрального круга ширины <paramref name="drStart"/>.
        /// </summary>
        /// <returns>Номер кольца с наибольшим модулем объёма (0 — центральный круг).</returns>
        private static int BuildRings(BlurFunctionHandler filter, double drStart, out List<double> listRadius, out List<double> listVolumes,
            out List<int> listSectors, out double vSum, out double absSum)
        {
            //Мультипликатор для dr
            const double drMultiplier = 1.5D;

            listRadius = new List<double>();
            listVolumes = new List<double>();
            listSectors = new List<int>();

            //Полные объёмы колец (в listVolumes хранится объём, приходящийся на одну точку кольца)
            var listRingVolumes = new List<double>();

            //Начальные значение радиуса
            double r = 0;

            //Начальное значение ширины сектора
            var dr = drStart;

            var volume = filter(0) * Pi * dr * dr / 4D;
            CheckVolume(volume);

            //Сохраняем параметры текущей итерации
            listRadius.Add(0);
            listVolumes.Add(volume);
            listSectors.Add(1);
            listRingVolumes.Add(volume);

            //Прежде кольца строились до радиуса 3 независимо от ядра: для гауссова ядра с σ = 2 сглаживание x² + y²
            //в нуле давало 5.35 вместо 8. Теперь, как и в BlurCompiler, протяжённость определяется по самому ядру.

            //Наибольший модуль объёма кольца, его номер и внешний радиус
            var maxVolume = Abs(volume);
            var peakRing = 0;
            var peakEdge = dr / 2D;

            //Внешний радиус последнего значимого кольца
            var lastEdge = dr / 2D;

            do
            {
                //Предыдущее значение
                var saveDr = dr;

                //Текущее значение
                dr *= drMultiplier;

                r += (saveDr + dr) / 2D;

                //Количество делений угла в зависимости от радиуса и от шага радиуса
                var aCount = (int)(Tau * r / dr + 1);

                //Гауссовский объем

                //Первый способ (метод прямоугольников)
                volume = filter(r) * Tau * r * dr;
                CheckVolume(volume);

                //Второй способ (метод трапеций)
                //volume = (filter(r - dr / 2) + filter(r + dr / 2)) * Math.Pi * r * dr;
                //vSum += volume;

                //Сохраняем параметры текущей итерации
                listRadius.Add(r);
                listVolumes.Add(volume / aCount);
                listSectors.Add(aCount);
                listRingVolumes.Add(volume);

                var edge = r + dr / 2D;
                var absVolume = Abs(volume);

                if (absVolume > maxVolume)
                {
                    maxVolume = absVolume;
                    peakRing = listRadius.Count - 1;
                    peakEdge = edge;
                }

                if (absVolume > RelativeLimit * maxVolume)
                {
                    lastEdge = edge;
                }

                //Ядро закончилось: после последнего значимого кольца радиус вырос в SupportGap раз
                if (maxVolume > 0 && edge >= SupportGap * lastEdge)
                {
                    break;
                }

                //Тяжёлый хвост либо нулевое ядро
                if (edge >= MaxSpread * peakEdge)
                {
                    break;
                }

                if (listRadius.Count > MaxRings)
                {
                    throw new ArgumentException("Не удалось построить ядро: функция не убывает к нулю с ростом радиуса.", nameof(filter));
                }
            }
            while (true);

            //Отбрасываем пренебрежимо малые кольца в конце
            var count = 1;
            for (var i = listRingVolumes.Count - 1; i > 0; i--)
            {
                if (Abs(listRingVolumes[i]) > RelativeLimit * maxVolume)
                {
                    count = i + 1;
                    break;
                }
            }

            listRadius.RemoveRange(count, listRadius.Count - count);
            listVolumes.RemoveRange(count, listVolumes.Count - count);
            listSectors.RemoveRange(count, listSectors.Count - count);

            vSum = 0;
            absSum = 0;
            for (var i = 0; i < count; i++)
            {
                vSum += listRingVolumes[i];
                absSum += Abs(listRingVolumes[i]);
            }

            return peakRing;
        }

        /// <summary>
        /// Проверяет, что объём кольца — конечное число.
        /// </summary>
        private static void CheckVolume(double volume)
        {
            if (double.IsNaN(volume) || double.IsInfinity(volume))
            {
                throw new ArgumentException("Функция ядра должна возвращать конечные значения при radius ≥ 0.", "filter");
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SurfaceBlurFilter"/> с заданным ядром без сглаживаемой функции:
        /// функцию нужно передавать в <see cref="GetValue(double, double, Func{double, double, double})"/>.
        /// </summary>
        /// <param name="filter">Радиальное ядро: вес в зависимости от расстояния radius ≥ 0 до центра. Нормировать ядро не нужно,
        /// но его значения должны быть конечными, а сумма весов — отличной от нуля.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filter"/> равен null.</exception>
        /// <exception cref="ArgumentException">Ядро возвращает NaN или бесконечность, не убывает к нулю с ростом радиуса
        /// либо сумма его весов равна нулю.</exception>
        public SurfaceBlurFilter(BlurFunctionHandler filter)
        {
            PrepareBlur(filter, out _arrayRadius, out _arrayVolumes, out _arraySectors);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SurfaceBlurFilter"/> с заданными ядром и сглаживаемой функцией.
        /// </summary>
        /// <param name="filter">Радиальное ядро: вес в зависимости от расстояния radius ≥ 0 до центра. Нормировать ядро не нужно,
        /// но его значения должны быть конечными, а сумма весов — отличной от нуля.</param>
        /// <param name="function">Сглаживаемая функция двух переменных, используемая в <see cref="GetValue(double, double)"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filter"/> равен null.</exception>
        /// <exception cref="ArgumentException">Ядро возвращает NaN или бесконечность, не убывает к нулю с ростом радиуса
        /// либо сумма его весов равна нулю.</exception>
        public SurfaceBlurFilter(BlurFunctionHandler filter, Func<double, double, double> function)
        {
            _function = function;
            PrepareBlur(filter, out _arrayRadius, out _arrayVolumes, out _arraySectors);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Возвращает сглаженное значение функции, заданной в конструкторе, в точке (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">Координата X точки.</param>
        /// <param name="y">Координата Y точки.</param>
        /// <returns>Взвешенное среднее значений функции в точках колец вокруг (<paramref name="x"/>, <paramref name="y"/>).</returns>
        /// <exception cref="InvalidOperationException">Функция не задана в конструкторе.</exception>
        public double GetValue(double x, double y)
        {
            //Прежде при создании фильтра без функции здесь выбрасывалось NullReferenceException
            if (_function == null)
            {
                throw new InvalidOperationException("Сглаживаемая функция не задана: передайте её в конструктор или в GetValue(x, y, function).");
            }

            return GetValue(x, y, _function);
        }

        /// <summary>
        /// Возвращает сглаженное значение функции <paramref name="function"/> в точке (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">Координата X точки.</param>
        /// <param name="y">Координата Y точки.</param>
        /// <param name="function">Сглаживаемая функция двух переменных, определённая на всей плоскости.</param>
        /// <returns>Взвешенное среднее значений функции в точках колец вокруг (<paramref name="x"/>, <paramref name="y"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> равна null.</exception>
        public double GetValue(double x, double y, Func<double, double, double> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            var result = function(x, y) * _arrayVolumes[0];

            for (var i = 1; i < _arrayRadius.Length; i++)
            {
                //Текущее расстояние до средней линии бублика
                var radius = _arrayRadius[i];

                //Количество делений угла
                var aCount = _arraySectors[i];

                //Шаг деления угла
                var angleStep = Tau / aCount;

                //Суммируем значения функции по текущему бублику
                double preResult = 0;

                for (var j = 0; j < aCount; j++)
                {
                    var angle = j * angleStep;
                    preResult += function(x + radius * Cos(angle), y + radius * Sin(angle));
                }

                result += preResult * _arrayVolumes[i];
            }

            return result;
        }

        #endregion
    }
}