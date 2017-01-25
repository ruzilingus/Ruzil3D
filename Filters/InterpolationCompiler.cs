using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
	/// <summary>
	/// -
	/// </summary>
    public class InterpolationCompiler
    {
        private PointD[] _points;
        private ExtrapolationMode _mode;

        //Вспомогательные поля
        private double _buildDMax;
        private double _buildFBeg;
        private double _buildFEnd;
        private double _buildDiff;

        public bool IsClosed => _mode == ExtrapolationMode.Closed;

        public double GetValue(double d)
        {
            //Определяем функцию (непрерывно и гладко) на всей числовой прямой
            if (d < 0)
            {
                if (IsClosed)
                {
                    d = d - _buildDMax * Math.Floor(d / _buildDMax);
                    return GetValue(d);
                }
                var div = (int)(d / _buildDMax);
                d = d - div * _buildDMax;

                if (div % 2 == 0)
                {
                    return 2 * _buildFBeg - GetValue(-d) + div * _buildDiff;
                }
                return GetValue(_buildDMax + d) + (div - 1) * _buildDiff;
            }
            if (d > _buildDMax)
            {
                if (IsClosed)
                {
                    d = d - _buildDMax * Math.Floor(d / _buildDMax);
                    return GetValue(d);
                }
                var div = (int)(d / _buildDMax);
                d = d - div * _buildDMax;

                if (div % 2 == 0)
                {
                    return 2 * _buildFBeg - GetValue(-d) + div * _buildDiff;
                }
                return 2 * _buildFEnd - GetValue(_buildDMax - d) + (div - 1) * _buildDiff;
            }


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
            _points = points;
            _mode = mode;

            _buildDMax = points[points.Length - 1].X;
            _buildFBeg = points[0].Y;
            _buildFEnd = points[points.Length - 1].Y;
            _buildDiff = _buildFEnd - _buildFBeg;
        }

        public InterpolationCompiler(PointD[] points, ExtrapolationMode mode = ExtrapolationMode.Mirror)
        {
            Compile(points, mode);
        }
    }
}