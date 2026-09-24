using System;
using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
	/// <summary>
	/// Кусочно-линейная интерполяция функции одной переменной, заданной значениями в узлах.
	/// </summary>
	/// <remarks>
	/// Функция определена на отрезке [X₀, Xₙ] между аргументами первой и последней точек, а за его пределами продолжается
	/// согласно <see cref="ExtrapolationMode"/>: в режиме <see cref="ExtrapolationMode.Mirror"/> — центральной симметрией
	/// относительно концевых точек (f(X₀ − t) = 2f(X₀) − f(X₀ + t)), в режиме <see cref="ExtrapolationMode.Closed"/> — периодически
	/// с периодом Xₙ − X₀.
	/// </remarks>
    public class InterpolationCompiler
    {
        private PointD[] _points;
        private ExtrapolationMode _mode;

        //Вспомогательные поля
        private double _buildXBeg;
        private double _buildXEnd;
        private double _buildDMax;
        private double _buildFBeg;
        private double _buildFEnd;
        private double _buildDiff;

        /// <summary>
        /// Получает значение, показывающее, продолжается ли функция периодически (режим <see cref="ExtrapolationMode.Closed"/>).
        /// </summary>
        public bool IsClosed => _mode == ExtrapolationMode.Closed;

        /// <summary>
        /// Возвращает значение функции в точке <paramref name="d"/>.
        /// </summary>
        /// <param name="d">Аргумент. Вне отрезка между аргументами первой и последней точек функция продолжается согласно режиму
        /// экстраполяции.</param>
        /// <returns>Значение линейной интерполяции между соседними узлами; NaN для бесконечного аргумента и NaN.</returns>
        public double GetValue(double d)
        {
            //Определяем функцию (непрерывно и гладко) на всей числовой прямой.
            //Номер периода div вычисляется в double: приведение к int переполнялось для далёких и бесконечных
            //аргументов, и взаимная рекурсия переполняла стек. Для бесконечного аргумента результат — NaN.
            //Прежде функция молча предполагала, что аргумент первой точки равен нулю: отражение шло относительно нуля,
            //а период считался равным аргументу последней точки. Теперь отсчёт ведётся от аргумента первой точки.
            if (d < _buildXBeg)
            {
                var offset = d - _buildXBeg;
                if (IsClosed)
                {
                    return GetPeriodicValue(offset);
                }
                var div = Math.Truncate(offset / _buildDMax);
                offset = offset - div * _buildDMax;

                if (div % 2 == 0)
                {
                    return 2 * _buildFBeg - GetValue(_buildXBeg - offset) + div * _buildDiff;
                }
                return GetValue(_buildXEnd + offset) + (div - 1) * _buildDiff;
            }
            if (d > _buildXEnd)
            {
                var offset = d - _buildXBeg;
                if (IsClosed)
                {
                    return GetPeriodicValue(offset);
                }
                var div = Math.Truncate(offset / _buildDMax);
                offset = offset - div * _buildDMax;

                if (div % 2 == 0)
                {
                    return 2 * _buildFBeg - GetValue(_buildXBeg - offset) + div * _buildDiff;
                }
                return 2 * _buildFEnd - GetValue(_buildXEnd - offset) + (div - 1) * _buildDiff;
            }

            return Interpolate(d);
        }

        /// <summary>
        /// Возвращает значение периодической функции по смещению <paramref name="offset"/> от аргумента первой точки.
        /// </summary>
        private double GetPeriodicValue(double offset)
        {
            offset = offset - _buildDMax * Math.Floor(offset / _buildDMax);

            //Из-за округления остаток может немного выйти за пределы периода
            if (offset < 0)
            {
                offset += _buildDMax;
            }
            else if (offset > _buildDMax)
            {
                offset -= _buildDMax;
            }

            return Interpolate(Math.Min(_buildXEnd, Math.Max(_buildXBeg, _buildXBeg + offset)));
        }

        /// <summary>
        /// Возвращает значение линейной интерполяции для аргумента в пределах отрезка между первой и последней точками.
        /// </summary>
        private double Interpolate(double d)
        {
            var beg = 0;
            var end = _points.Length - 1;

            do
            {
                var mid = (beg + end) / 2;
                var distance = _points[mid].X;
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


            var curDiff = _points[end].X - _points[beg].X;

            if (curDiff.Equals(0D))
            {
                return _points[beg].Y;
            }
            //return (_points[beg] - _points[end]) * ((_distance[end] - d) / curDiff) + _points[end];

            var coef = (_points[end].X - d) / curDiff;
            return (_points[beg].Y - _points[end].Y) * coef + _points[end].Y;
        }

        private void Compile(PointD[] points, ExtrapolationMode mode)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            if (mode != ExtrapolationMode.Closed && mode != ExtrapolationMode.Mirror)
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Недопустимый режим экстраполяции.");
            }

            //С одной точкой поиск отрезка не завершался
            if (points.Length < 2)
            {
                throw new ArgumentException("Функция должна быть задана не менее чем двумя точками.", nameof(points));
            }

            //Прежде порядок аргументов не проверялся, и для неупорядоченных точек двоичный поиск молча давал неверные значения.
            for (var i = 0; i < points.Length; i++)
            {
                var x = points[i].X;
                if (double.IsNaN(x) || double.IsInfinity(x) || (i > 0 && x < points[i - 1].X))
                {
                    throw new ArgumentException("Аргументы точек должны быть конечными числами, упорядоченными по неубыванию.", nameof(points));
                }
            }

            //При нулевой длине области определения продолжение функции за её пределы делило на ноль.
            //Прежде требовалось, чтобы аргумент последней точки был больше нуля, так как отсчёт шёл от нуля.
            var range = points[points.Length - 1].X - points[0].X;
            if (!(range > 0) || double.IsInfinity(range))
            {
                throw new ArgumentException("Аргумент последней точки должен быть больше аргумента первой, а длина области определения — конечной.", nameof(points));
            }

            //Точки копируются: прежде хранилась ссылка на массив вызывающего кода, и его последующее изменение
            //ломало упорядоченность узлов.
            _points = (PointD[])points.Clone();
            _mode = mode;

            _buildXBeg = points[0].X;
            _buildXEnd = points[points.Length - 1].X;
            _buildDMax = range;
            _buildFBeg = points[0].Y;
            _buildFEnd = points[points.Length - 1].Y;
            _buildDiff = _buildFEnd - _buildFBeg;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InterpolationCompiler"/> по узлам функции.
        /// </summary>
        /// <param name="points">Узлы функции (X — аргумент, Y — значение), не менее двух, упорядоченные по неубыванию аргумента.
        /// Аргумент последней точки должен быть больше аргумента первой; равные аргументы соседних точек допустимы (разрыв функции).
        /// Массив копируется.</param>
        /// <param name="mode">Режим продолжения функции за пределы отрезка между первой и последней точками.</param>
        /// <exception cref="ArgumentNullException"><paramref name="points"/> равен null.</exception>
        /// <exception cref="ArgumentException">Точек меньше двух, аргументы не упорядочены по неубыванию, не конечны либо аргумент
        /// последней точки не больше аргумента первой.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> не является допустимым значением.</exception>
        public InterpolationCompiler(PointD[] points, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            Compile(points, mode);
        }
    }
}