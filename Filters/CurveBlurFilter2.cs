using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
	/// <summary>
	/// -
	/// </summary>
    public class CurveBlurFilter2 : ICurveApproximation
    {
        private readonly BlurCompiler _blurCompiler;
        private readonly ICurveInterpolation _linearCompiler;

        public CurveBlurFilter2(Point3D[] curve, BlurFunctionHandler filter, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            _blurCompiler = new BlurCompiler(filter);
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
            BuildCorners();
        }

        public CurveBlurFilter2(Point3D[] curve, BlurCompiler filter, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            _blurCompiler = filter;
            _linearCompiler = new CurveInterpolationCompiler(curve, mode);
            BuildCorners();
        }

        #region Build Corners

        private InterpolationCompiler _cornerInterpolation;

        private double _cornerBreakPoint;
        private Point3D _cornerDoubleBreakValue;

        private Point3D corner_GetValue1(double d)
        {
            if (d < _cornerBreakPoint)
            {
                return _linearCompiler.GetValue3D(d);
            }
            return _cornerDoubleBreakValue - _linearCompiler.GetValue3D(2*_cornerBreakPoint - d);
        }

        private Point3D corner_GetValue2(double d)
        {
            if (d > _cornerBreakPoint)
            {
                return _linearCompiler.GetValue3D(d);
            }
            return _cornerDoubleBreakValue - _linearCompiler.GetValue3D(2*_cornerBreakPoint - d);
        }

        private void BuildCorners()
        {
            //Окрестность для вычисления приращения
            const double delta = 0.000000001D;

            //var closed = _linearCompiler.IsClosed;

            //var maxDistance = _linearCompiler.DMax;

            var distance = new double[_linearCompiler.Length];

            var derPonts = new List<PointD>();

            _cornerBreakPoint = 0;
            var lastPoint = _linearCompiler[0];
            for (var i = 0; i < _linearCompiler.Length; i++)
            {
                var curPoint = _linearCompiler[i];

                //Заполняем вспомогательные значения
                _cornerBreakPoint += lastPoint.Distance(curPoint);
                lastPoint = curPoint;
                distance[i] = _cornerBreakPoint;

                _cornerDoubleBreakValue = 2*_linearCompiler.GetValue3D(_cornerBreakPoint);

                //Вычисляем приращения по разную сторону точки
                var diff1 = -_blurCompiler.GetDifferential(corner_GetValue1, _cornerBreakPoint, delta);
                var diff2 = _blurCompiler.GetDifferential(corner_GetValue2, _cornerBreakPoint, delta);


                //Угол функции в данной точке
                var value = diff1.Cos(diff2);


                derPonts.Add(new PointD(_cornerBreakPoint, Math.Acos(value)/ Math.Pi));
            }

            _cornerInterpolation = new InterpolationCompiler(derPonts.ToArray(), _linearCompiler.Mode);
        }

        #endregion

        #region Temporary Items

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

        public ICurveApproximation LoadFrom(Point3D[] curve, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            throw new NotImplementedException();
        }

        public Point3D GetValue3D(double d)
        {
            //Для вычисления значения в точке нужно знать угол излома в данной точке
            //Зная угол излома, будем знать какой фильтр применять

            //Вычисляем сглаженное значение угла в точке d (Значение 1 - соответствует углу 2*PI)
            var corner = _blurCompiler.GetValue(_cornerInterpolation.GetValue, d);
            var scale = corner;

            //corner = GetCornerDerValue(d);
            //scale = Math.Max(0, 1 - corner);

            return _blurCompiler.GetValue3D(_linearCompiler.GetValue3D, d, scale);
        }


        public double DMax => _linearCompiler.DMax;

        public double GetD(int index)
        {
            return _linearCompiler.GetD(index);
        }

        public double GetCornerValue(double d, double scale = 1)
        {
            //Вычисляем сглаженное значение угла в точке d (Значение 1 - соответствует углу 2*PI)
            return _blurCompiler.GetValue(_cornerInterpolation.GetValue, d, scale);
        }

        public double GetCornerDerValue(double d)
        {
            //Вычисляем скорость изменения угла (производная)

            const double eps = 0.01;
            const double eps2 = 2*eps;

            return Math.Abs(GetCornerValue(d + eps) - GetCornerValue(d - eps))/eps2;
        }
    }
}