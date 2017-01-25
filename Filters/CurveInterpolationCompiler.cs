using System;
using System.Collections.Generic;
using System.Linq;
using Ruzil3D.Algebra;
using static Ruzil3D.Math;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// - Компилированный объект линейной интерполяции.
    /// </summary>
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

        private void Compile(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
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

            if (_buildDMax.Equals(0D))
            {
                throw new Exception("Кривая должна иметь хотя-бы две уникальные точки");
            }

            _buildFBeg = _points[0];
            _buildFEnd = _points[_points.Length - 1];
            _buildDiff = _buildFEnd - _buildFBeg;
        }

        #region Constructors

        public CurveInterpolationCompiler(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            Compile(curve, mode);
        }

        public CurveInterpolationCompiler(IEnumerable<Point3D> curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            Compile(curve.ToArray(), mode);
        }

        #endregion

        private Point3D _mean;
        private readonly bool _meanCalculated = false;

        public Point3D GetMean()
        {
            if (!_meanCalculated)
            {
                _mean = Point3D.Empty;


                for (var i = 1; i < _points.Length; i++)
                {
                    var dist = _distance[i] - _distance[i - 1];
                    _mean += dist * (_points[i] + _points[i - 1]);
                }

                _mean /= 2 * _distance[_distance.Length - 1];


            }


            return _mean;
        }

        #region IInterpolation Items

        public Point3D GetValue3D(double d)
        {
            //Определяем функцию (непрерывно и гладко) на всей числовой прямой
            if (d < 0)
            {
                if (IsClosed)
                {
                    d = d - _buildDMax * Floor(d / _buildDMax);
                    return GetValue3D(d);
                }
                var div = (int)(d / _buildDMax);
                d = d - div * _buildDMax;

                if (div % 2 == 0)
                {
                    return 2 * _buildFBeg - GetValue3D(-d) + div * _buildDiff;
                }
                return GetValue3D(_buildDMax + d) + (div - 1) * _buildDiff;
            }
            if (d > _buildDMax)
            {
                if (IsClosed)
                {
                    d = d - _buildDMax * Floor(d / _buildDMax);
                    return GetValue3D(d);
                }
                var div = (int)(d / _buildDMax);
                d = d - div * _buildDMax;

                if (div % 2 == 0)
                {
                    return 2 * _buildFBeg - GetValue3D(-d) + div * _buildDiff;
                }
                return 2 * _buildFEnd - GetValue3D(_buildDMax - d) + (div - 1) * _buildDiff;
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
                return _points[beg];
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
            return (_points[beg] - _points[end]) * coef + _points[end];
        }

        public double DMax => _buildDMax;

        public double GetD(int index)
        {
            return _distance[index];
        }

        public int Length => _points.Length;

        public ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            return new CurveInterpolationCompiler(curve, mode);
        }

        public Point3D this[int index] => _points[index];

        public bool IsClosed => _mode == ExtrapolationMode.Closed;

        public ExtrapolationMode Mode => _mode;

        #endregion


    }
}