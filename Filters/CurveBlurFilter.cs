using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;
using Ruzil3D.Curves;
using static Ruzil3D.Math;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// - Сглаживатель кривых в трехмерном пространстве.
    /// </summary>
    public class CurveBlurFilter : ICurveApproximation
    {
        private readonly BlurCompiler _blurCompiler;
        private readonly ICurveInterpolation _linearCompiler;

        #region Search Corners
        private double _cornerBreakPoint;
        private double _cornerDoubleBreakPoint;
        private Point3D _cornerDoubleBreakValue;

        private struct CIndexValue : IComparable
        {
            public readonly int Index;
            // ReSharper disable once MemberCanBePrivate.Local
            public readonly double Value;

            public CIndexValue(int index, double value)
            {
                Index = index;
                Value = value;
            }

            public int CompareTo(object obj)
            {
                return Value.CompareTo(((CIndexValue)obj).Value);
            }

            public override string ToString()
            {
                return nameof(Index) + ": " + Index + ", "+ nameof(Value) + ": " + Value;
            }
        }

        private Point3D corner_GetValue1(double d)
        {
            if (d <= _cornerBreakPoint)
            {
                return _linearCompiler.GetValue3D(d);
            }
            return _cornerDoubleBreakValue - _linearCompiler.GetValue3D(_cornerDoubleBreakPoint - d);
        }

        private Point3D corner_GetValue2(double d)
        {
            if (d >= _cornerBreakPoint)
            {
                return _linearCompiler.GetValue3D(d);
            }
            return _cornerDoubleBreakValue - _linearCompiler.GetValue3D(_cornerDoubleBreakPoint - d);
        }

        //Окрестность для вычисления приращения
        private const double Delta = 0.000000001D;

        public Point3D GetCorrection(double value)
        {
            _cornerBreakPoint = value;
            _cornerDoubleBreakPoint = 2 * _cornerBreakPoint;
            _cornerDoubleBreakValue = 2 * _linearCompiler.GetValue3D(value);

            //Вычисляем приращения по разную сторону точки
            var diff1 = -_blurCompiler.GetDifferential(corner_GetValue1, _cornerBreakPoint, Delta);
            var diff2 = _blurCompiler.GetDifferential(corner_GetValue2, _cornerBreakPoint, Delta);

            //Угол функции в данной точке
            var cos = diff1.Cos(diff2);
            var angle = Acos(cos);

            var sum = diff1 / diff1.Length + diff2 / diff2.Length;
            sum = -sum / sum.Length;
            sum *= (Pi - angle) / angle;

            return sum;
        }

        /// <summary>
        /// Находит углы излома кривой.
        /// </summary>
        /// <param name="derivative"></param>
        /// <param name="smoothness"></param>
        /// <param name="maxAngle"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public int[] SearchCorners(ref double[] derivative, double smoothness = 0.5D, double maxAngle = 120, double scale = 1)
        {
            maxAngle = Max(0, Min(Pi, maxAngle * Pi / 180));

            //Предельное значение угла меньше которого не считается изломом
            var maxCos = Cos(Pi - maxAngle);



            var closed = _linearCompiler.IsClosed;


            var maxDistance = _linearCompiler.DMax;

            var bestPoints = new List<CIndexValue>();
            var distanceArray = new double[_linearCompiler.Length];

            _cornerBreakPoint = 0;
            var lastPoint = _linearCompiler[0];

            var len = closed ? _linearCompiler.Length - 1 : _linearCompiler.Length;

            for (var i = 0; i < len; i++)
            {
                var curPoint = _linearCompiler[i];

                //Заполняем вспомогательные значения
                _cornerBreakPoint += lastPoint.Distance(curPoint);
                _cornerDoubleBreakPoint = 2 * _cornerBreakPoint;
                lastPoint = curPoint;
                distanceArray[i] = _cornerBreakPoint;

                if (!closed)
                {
                    if (_cornerBreakPoint < smoothness)
                    {
                        //Игнорируем начальные точки
                        continue;
                    }
                    if (maxDistance - _cornerBreakPoint < smoothness)
                    {
                        //Игнорируем конечные точки
                        break;
                    }
                }

                _cornerDoubleBreakValue = 2 * _linearCompiler.GetValue3D(_cornerBreakPoint);

                //Вычисляем приращения по разную сторону точки
                var diff1 = _blurCompiler.GetDifferential(corner_GetValue1, _cornerBreakPoint, Delta, scale);
                var diff2 = _blurCompiler.GetDifferential(corner_GetValue2, _cornerBreakPoint, Delta, scale);

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

        public int[] SearchCorners(double smoothness = 0.5D, double maxAngle = 120, double scale = 1)
        {
            double[] derivative = null;
            return SearchCorners(ref derivative, smoothness, maxAngle, scale);
        }

        #endregion

        #region Temporary Items

        public Point3D[] FilterPoints()
        {
            //int count = _linearCompiler.Length;
            var count = (int)Round(_linearCompiler.DMax * 25) + 2;

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

        public CurveBlurFilter(IEnumerable<Point3D> curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            _blurCompiler = new BlurCompiler();
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
        }

        public CurveBlurFilter(BlurCompiler filter, ICurveInterpolation interpolator)
        {
            _blurCompiler = filter;
            _linearCompiler = interpolator;
        }

        public CurveBlurFilter(IEnumerable<Point3D> curve, BlurCompiler filter, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            _blurCompiler = filter;
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
        }

        public CurveBlurFilter(IEnumerable<Point3D> curve, BlurFunctionHandler filter, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            _blurCompiler = new BlurCompiler(filter);
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
        }

        #endregion

        public BezierCurve Resolve(double d0, double d1)
        {
            var p0 = _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, d0);
            var p3 = _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, d1);

            var delta = 0.000001D;// *p0.Distance(p3);

            var s0 = _blurCompiler.GetDerivative(_linearCompiler.GetValue3D, d0, delta);
            var s1 = _blurCompiler.GetDerivative(_linearCompiler.GetValue3D, d1, delta);

            var step = (d1 - d0) / 3D;
            var m0 = _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, d0 + step);
            var m1 = _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, d0 + step * 2);

            return BezierCurve.ResolveXY(p0.ToPointD(), m0.ToPointD(), m1.ToPointD(), p3.ToPointD(), new PointD(s0.X, s0.Y), new PointD(s1.X, s1.Y));
            
        }

        #region IInterpolation Items

        public ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            return new CurveBlurFilter(curve, mode);
        }

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
                        throw new ArgumentOutOfRangeException();
                }
            }

            return _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, d);
        }

        public double DMax => _linearCompiler.DMax;

        public double GetD(int index)
        {
            return _linearCompiler.GetD(index);
        }

        #endregion
    }
}