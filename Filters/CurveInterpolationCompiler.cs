using System;
using System.Collections.Generic;
using System.Linq;
using Ruzil3D.Algebra;
using static Ruzil3D.Math;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// Кусочно-линейная интерполяция ломаной в трёхмерном пространстве, параметризованной длиной дуги.
    /// </summary>
    /// <remarks>
    /// Точка на расстоянии d по длине дуги от первой вершины лежит на соответствующем отрезке ломаной. За пределами [0, <see cref="DMax"/>]
    /// ломаная продолжается согласно <see cref="ExtrapolationMode"/>: в режиме <see cref="ExtrapolationMode.Mirror"/> — центральной
    /// симметрией относительно концевых точек, в режиме <see cref="ExtrapolationMode.Closed"/> ломаная замыкается отрезком от последней
    /// вершины к первой и продолжается периодически. Объект неизменяем, и его можно использовать из нескольких потоков.
    /// </remarks>
    public class CurveInterpolationCompiler : ICurveInterpolation
    {
        private Point3D[] _points;
        private double[] _distance;
        private ExtrapolationMode _mode;

        //Вспомогательные поля
        private double _buildDMax;
        private Point3D _buildFBeg;
        private Point3D _buildFEnd;
        private Point3D _buildDiff;

        //Массив curve принадлежит объекту: конструкторы передают копию
        private void Compile(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            //Прежде недопустимый режим принимался и обнаруживался только в CurveBlurFilter.GetValue3D на концах кривой
            if (mode != ExtrapolationMode.Closed && mode != ExtrapolationMode.Mirror)
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Недопустимый режим экстраполяции.");
            }

            if (curve.Length == 0)
            {
                throw new ArgumentException("Кривая должна содержать хотя бы две различные точки.", nameof(curve));
            }

            _mode = mode;
            _points = curve;

            //Если функция замкнутая соединяем первую и последние точки
            if (mode == ExtrapolationMode.Closed)
            {
                var points = new Point3D[_points.Length + 1];
                _points.CopyTo(points, 0);
                points[_points.Length] = points[0];
                _points = points;
            }


            //Заполняем массив расстояний
            _distance = new double[_points.Length];
            _distance[0] = 0;
            for (var i = 1; i < _distance.Length; i++)
            {
                _distance[i] = _distance[i - 1] + _points[i].Distance(_points[i - 1]);
            }

            //Заполняем вспомогательные поля
            _buildDMax = _distance[_distance.Length - 1];

            if (!(_buildDMax > 0))
            {
                throw new ArgumentException("Кривая должна содержать хотя бы две различные точки.", nameof(curve));
            }

            _buildFBeg = _points[0];
            _buildFEnd = _points[_points.Length - 1];
            _buildDiff = _buildFEnd - _buildFBeg;
        }

        #region Constructors

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CurveInterpolationCompiler"/> по вершинам ломаной.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки. Массив копируется.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> равен null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public CurveInterpolationCompiler(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            //Массив копируется: прежде в режиме Mirror сохранялась ссылка на массив вызывающего кода, и его последующее
            //изменение меняло точки кривой, но не длины дуг и не DMax.
            Compile(curve == null ? null : (Point3D[])curve.Clone(), mode);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CurveInterpolationCompiler"/> по вершинам ломаной.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> равна null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public CurveInterpolationCompiler(IEnumerable<Point3D> curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            Compile(curve.ToArray(), mode);
        }

        #endregion

        private Point3D _mean;

        //Прежде поле было readonly и всегда равнялось false, поэтому среднее вычислялось заново при каждом вызове.
        //Среднее сначала полностью вычисляется, а затем публикуется записью volatile-флага, поэтому поток,
        //увидевший флаг, видит и готовое значение.
        private volatile bool _meanCalculated;

        /// <summary>
        /// Возвращает среднюю точку ломаной, то есть центр масс равномерно распределённой по её длине массы
        /// (для замкнутой ломаной — вместе с замыкающим отрезком).
        /// </summary>
        /// <returns>Средняя точка ломаной. Значение вычисляется при первом вызове и запоминается.</returns>
        public Point3D GetMean()
        {
            if (!_meanCalculated)
            {
                var mean = Point3D.Empty;


                for (var i = 1; i < _points.Length; i++)
                {
                    var dist = _distance[i] - _distance[i - 1];
                    mean += dist * (_points[i] + _points[i - 1]);
                }

                mean /= 2 * _distance[_distance.Length - 1];

                _mean = mean;
                _meanCalculated = true;
            }


            return _mean;
        }

        #region IInterpolation Items

        /// <summary>
        /// Возвращает точку ломаной на расстоянии <paramref name="d"/> по длине дуги от первой вершины.
        /// </summary>
        /// <param name="d">Длина дуги. Вне отрезка [0, <see cref="DMax"/>] ломаная продолжается согласно режиму экстраполяции.</param>
        /// <returns>Точка ломаной; для бесконечного аргумента и NaN — точка с координатами NaN.</returns>
        public Point3D GetValue3D(double d)
        {
            return GetOffset3D(d, Point3D.Empty);
        }

        /// <summary>
        /// Возвращает разность точки ломаной на расстоянии <paramref name="d"/> по длине дуги и точки <paramref name="origin"/>.
        /// </summary>
        /// <remarks>Координаты вершин вычитаются из <paramref name="origin"/> до интерполяции, поэтому для близких к ломаной точек
        /// <paramref name="origin"/> погрешность определяется величиной разности, а не координат. У GetValue3D(d) − origin вдали от
        /// начала координат погрешность порядка 1e-16·|origin|, и при численном дифференцировании она сравнивалась с приращением.
        /// При <paramref name="origin"/> = <see cref="Point3D.Empty"/> результат в точности равен <see cref="GetValue3D"/>.</remarks>
        internal Point3D GetOffset3D(double d, Point3D origin)
        {
            //Определяем функцию (непрерывно и гладко) на всей числовой прямой.
            //Номер периода div вычисляется в double: приведение к int переполнялось для далёких и бесконечных
            //аргументов, и взаимная рекурсия переполняла стек. Для бесконечного аргумента результат — NaN.
            if (d < 0)
            {
                if (IsClosed)
                {
                    d = d - _buildDMax * Floor(d / _buildDMax);
                    return GetOffset3D(d, origin);
                }
                var div = Truncate(d / _buildDMax);
                d = d - div * _buildDMax;

                if (div % 2 == 0)
                {
                    return 2 * (_buildFBeg - origin) - GetOffset3D(-d, origin) + div * _buildDiff;
                }
                return GetOffset3D(_buildDMax + d, origin) + (div - 1) * _buildDiff;
            }
            if (d > _buildDMax)
            {
                if (IsClosed)
                {
                    d = d - _buildDMax * Floor(d / _buildDMax);
                    return GetOffset3D(d, origin);
                }
                var div = Truncate(d / _buildDMax);
                d = d - div * _buildDMax;

                if (div % 2 == 0)
                {
                    return 2 * (_buildFBeg - origin) - GetOffset3D(-d, origin) + div * _buildDiff;
                }
                return 2 * (_buildFEnd - origin) - GetOffset3D(_buildDMax - d, origin) + (div - 1) * _buildDiff;
            }

            var beg = 0;
            var end = _distance.Length - 1;

            do
            {
                var mid = (beg + end) / 2;
                var distance = _distance[mid];
                if (d < distance)
                {
                    end = mid;
                }
                else
                {
                    beg = mid;
                }
            }
            while (beg != end - 1);


            var curDiff = _distance[end] - _distance[beg];

            if (curDiff.Equals(0D))
            {
                return _points[beg] - origin;
            }
            //return (_points[beg] - _points[end]) * ((_distance[end] - d) / curDiff) + _points[end];
            /*
				if (d == _distance[beg])
				{
				    return _points[beg];
				}
				else if (d == _distance[end])
				{
				    return _points[end];
				}
				*/

            var coef = (_distance[end] - d) / curDiff;
            return (_points[beg] - _points[end]) * coef + (_points[end] - origin);
        }

        /// <summary>
        /// Получает длину ломаной (для замкнутой — вместе с замыкающим отрезком).
        /// </summary>
        public double DMax => _buildDMax;

        /// <summary>
        /// Возвращает длину дуги от первой вершины до вершины с индексом <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Индекс вершины от 0 до <see cref="Length"/> − 1.</param>
        /// <returns>Длина дуги до вершины.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> вне допустимого диапазона.</exception>
        public double GetD(int index)
        {
            return _distance[index];
        }

        /// <summary>
        /// Получает число вершин ломаной; у замкнутой ломаной оно на единицу больше числа переданных точек,
        /// так как последней вершиной повторяется первая.
        /// </summary>
        public int Length => _points.Length;

        /// <summary>
        /// Создаёт интерполяцию другой ломаной.
        /// </summary>
        /// <param name="curve">Вершины ломаной; должны содержать хотя бы две различные точки. Массив копируется.</param>
        /// <param name="mode">Режим продолжения ломаной за её концы.</param>
        /// <returns>Новый объект <see cref="CurveInterpolationCompiler"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="curve"/> равен null.</exception>
        /// <exception cref="ArgumentException">Ломаная не содержит двух различных точек.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            return new CurveInterpolationCompiler(curve, mode);
        }

        /// <summary>
        /// Получает вершину ломаной с индексом <paramref name="index"/> (у замкнутой ломаной последняя вершина совпадает с первой).
        /// </summary>
        /// <param name="index">Индекс вершины от 0 до <see cref="Length"/> − 1.</param>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> вне допустимого диапазона.</exception>
        public Point3D this[int index] => _points[index];

        /// <summary>
        /// Получает значение, показывающее, замкнута ли ломаная (режим <see cref="ExtrapolationMode.Closed"/>).
        /// </summary>
        public bool IsClosed => _mode == ExtrapolationMode.Closed;

        /// <summary>
        /// Получает режим продолжения ломаной за её концы.
        /// </summary>
        public ExtrapolationMode Mode => _mode;

        #endregion


    }
}