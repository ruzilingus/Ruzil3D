using System;
using Ruzil3D.Algebra;
using Ruzil3D.Geometry;
using static System.Math;

namespace Ruzil3D.Curves
{
    /// <summary>
    /// Эллиптическая дуга в трехмерном евклидовом пространстве.
    /// </summary>
    public class EllipticArcCurve : PlanarCurve
	{
		#region Fields

		/// <summary>
		/// Полуось параллельная оси X.
		/// </summary>
		/// <remarks>Задается в конструкторе.</remarks>
		public readonly double A;

		/// <summary>
		/// Полуось параллельная оси Y.
		/// </summary>
		/// <remarks>Задается в конструкторе.</remarks>
		public readonly double B;

		/// <summary>
		/// Угол (в радианах), который измеряется против часовой стрелки, начиная от оси X и заканчивая начальной точкой дуги.
		/// </summary>
		/// <remarks>Задается в конструкторе.</remarks>
		public readonly double AngleStart;

		/// <summary>
		/// Угол (в радианах), который измеряется против часовой стрелки, начиная от значения параметра <see cref="AngleStart"/> и заканчивая конечной точкой дуги.
		/// </summary>
		/// <remarks>Задается в конструкторе.</remarks>
		public readonly double AngleSweep;

		private readonly double _paramStart;
		private readonly double _paramSweep;

		#endregion

		#region Constructors

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="EllipticArcCurve"/> указанными параметрами.
		/// </summary>
		/// <param name="a">Полуось параллельная оси X.</param>
		/// <param name="b">Полуось параллельная оси Y.</param>
		/// <param name="angleStart">Угол (в радианах), который измеряется против часовой стрелки, начиная от оси X и заканчивая начальной точкой дуги.</param>
		/// <param name="angleSweep">Угол (в радианах), который измеряется против часовой стрелки, начиная от значения параметра <see cref="AngleStart"/> и заканчивая конечной точкой дуги.</param>
		/// <param name="isometry">Конгруэнтное преобразование трехмерного евклидова пространства.</param>
		public EllipticArcCurve(double a, double b, double angleStart, double angleSweep, Isometry isometry)
			: base(isometry)
		{
			if (a <= 0 || b <= 0)
			{
				throw new ArgumentException("Аргументы a и b, задающие полуоси эллипса должны быть больше нуля.");
			}


			A = a;
			B = b;
			AngleStart = angleStart;
			AngleSweep = angleSweep;

			if (a.Equals(b))
			{
				_paramStart = angleStart;
				_paramSweep = angleSweep;
			}
			else
			{
				var sinStart = Sin(AngleStart);
				var cosStart = Cos(AngleStart);

				var angleStop = angleStart + angleSweep;

				var sinStop = Sin(angleStop);
				var cosStop = Cos(angleStop);

				_paramStart = Atan2(A*sinStart, B*cosStart);

				var paramStop = Atan2(A*sinStop, B*cosStop);
				if (AngleSweep*paramStop < 0)
				{
					paramStop -= Sign(paramStop)*Math.Tau;
				}
				var turns = Truncate(AngleSweep/ Math.Tau);
				paramStop += turns* Math.Tau;

				_paramSweep = paramStop - _paramStart;
			}
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="EllipticArcCurve"/> указанными параметрами.
		/// </summary>
		/// <param name="a">Полуось параллельная оси X.</param>
		/// <param name="b">Полуось параллельная оси Y.</param>
		/// <param name="angleStart">Угол (в радианах), который измеряется против часовой стрелки, начиная от оси X и заканчивая начальной точкой дуги.</param>
		/// <param name="angleSweep">Угол (в радианах), который измеряется против часовой стрелки, начиная от значения параметра <see cref="AngleStart"/> и заканчивая конечной точкой дуги.</param>
		/// <param name="offset">Перемещение центра эллипса.</param>
		public EllipticArcCurve(double a, double b, double angleStart, double angleSweep, Point3D offset) : this(a, b, angleStart, angleSweep, new Isometry(offset))
		{
			
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="EllipticArcCurve"/> указанными параметрами.
		/// </summary>
		/// <param name="a">Полуось параллельная оси X.</param>
		/// <param name="b">Полуось параллельная оси Y.</param>
		/// <param name="angleStart">Угол (в радианах), который измеряется против часовой стрелки, начиная от оси X и заканчивая начальной точкой дуги.</param>
		/// <param name="angleSweep">Угол (в радианах), который измеряется против часовой стрелки, начиная от значения параметра <see cref="AngleStart"/> и заканчивая конечной точкой дуги.</param>
		public EllipticArcCurve(double a, double b, double angleStart, double angleSweep)
			: this(a, b, angleStart, angleSweep, Isometry.Identity)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="EllipticArcCurve"/> указанными параметрами.
		/// </summary>
		/// <param name="radius">Радиус дуги.</param>
		/// <param name="angleStart">Угол (в радианах), который измеряется против часовой стрелки, начиная от оси X и заканчивая начальной точкой дуги.</param>
		/// <param name="angleSweep">Угол (в радианах), который измеряется против часовой стрелки, начиная от значения параметра <see cref="AngleStart"/> и заканчивая конечной точкой дуги.</param>
		/// <param name="isometry">Конгруэнтное преобразование трехмерного евклидова пространства.</param>
		public EllipticArcCurve(double radius, double angleStart, double angleSweep, Isometry isometry)
			: this(radius, radius, angleStart, angleSweep, isometry)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="EllipticArcCurve"/> указанными параметрами.
		/// </summary>
		/// <param name="radius">Радиус дуги.</param>
		/// <param name="angleSweep">Угол (в радианах), который измеряется против часовой стрелки, начиная от оси X и заканчивая конечной точкой дуги.</param>
		/// <param name="isometry">Конгруэнтное преобразование трехмерного евклидова пространства.</param>
		public EllipticArcCurve(double radius, double angleSweep, Isometry isometry)
			: this(radius, radius, 0, angleSweep, isometry)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="EllipticArcCurve"/> указанными параметрами.
		/// </summary>
		/// <param name="radius">Радиус дуги.</param>
		/// <param name="angleStart">Угол (в радианах), который измеряется против часовой стрелки, начиная от оси X и заканчивая начальной точкой дуги.</param>
		/// <param name="angleSweep">Угол (в радианах), который измеряется против часовой стрелки, начиная от значения параметра <see cref="AngleStart"/> и заканчивая конечной точкой дуги.</param>
		public EllipticArcCurve(double radius, double angleStart, double angleSweep)
			: this(radius, radius, angleStart, angleSweep, Isometry.Identity)
		{
		}

		#endregion

		/// <summary>
		/// Получает значение указывающее, что эквивалентная репараметризация <i>p = <see cref="ParametricCurve.Length"/>*t</i> данной кривой является натуральной.
		/// </summary>
		/// <remarks>Натуральность означает, что длина любого участка от <i>t₀</i> до <i>t₁</i> (0 ≤ <i>t₀</i> ≤ <i>t₁</i> ≤ 1) кривой равняется <i><see cref="ParametricCurve.Length"/>*(t₁ - t₀)</i>.</remarks>
		public override bool IsNatural => A.Equals(B);

		/// <summary>
		/// Возвращает производную от функции выпрямления эллиптической дуги.
		/// </summary>
		/// <returns>Производная от функции выпрямления эллиптической дуги.</returns>
		protected override Func<double, double> GetRectificationDerivative()
		{
			var a = A*A*_paramSweep*_paramSweep;
			var c = B*B*_paramSweep*_paramSweep - a;

			return (x) =>
			{
				var cos = Cos(x*_paramSweep + _paramStart);
				return Sqrt(a + c*cos*cos);
			};
		}

		/// <summary>
		/// Вычисляет и возвращают длину участка эллиптической дуги между параметрами t0 до t1.
		/// </summary>
		/// <param name="t0">Параметр начала участка.</param>
		/// <param name="t1">Параметр конца участка.</param>
		/// <returns>Длина участка эллиптической дуги.</returns>
		public override double GetDistance(double t0, double t1)
		{
			if (A.Equals(B))
			{
				return Abs(AngleSweep*A*(t1 - t0));
			}
			else
			{
				return base.GetDistance(t0, t1);
			}
		}

		/// <summary>
		/// Координаты точки эллиптической дуги на плоскости соответствующую параметру.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Координаты точки эллиптической дуги на плоскости представленной структурой <see cref="PointD"/>.</returns>
		public override PointD GetPlanarValue(double parameter)
		{
			var t = parameter*_paramSweep + _paramStart;
			return new PointD(A*Cos(t), B*Sin(t));
		}

		/// <summary>
		/// Возвращает вектор касательной к эллиптической дуги на плоскости в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение вектора касательной на плоскости представленный структурой <see cref="PointD"/>.</returns>
		public override PointD GetPlanarTangent(double parameter)
		{
			var t = parameter*_paramSweep + _paramStart;

			if (A.Equals(B))
			{
				return new PointD(-Sin(t), Cos(t));
			}

			var x1 = -A*Sin(t);
			var y1 = B*Cos(t);
			var len = Sqrt(x1*x1 + y1*y1);

			return new PointD(x1/len, y1/len);
		}

		/// <summary>
		/// Возвращает значение кривизны эллиптической дуги на плоскости в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение кривизны на плоскости.</returns>
		/// <remarks>Значение равное Z-состовляющей псевдовектора кривизны направленого перпендикулярно к плоскости.</remarks>
		public override double GetPlanarCurvature(double parameter)
		{
			if (A.Equals(B))
			{
				return 1/A;
			}

			var t = parameter*_paramSweep + _paramStart;

			var x1 = -A*Sin(t);
			var y1 = B*Cos(t);
			var len = Sqrt(x1*x1 + y1*y1);

			return A*B/(len*len*len);
		}

		/// <summary>
		/// Приводит эллиптическую дугу к кривой Безье.
		/// </summary>
		/// <returns>Возвращают кривую Безье близкую к заданной эллиптической дуги.</returns>
		public override BezierCurve RecastToBezierCurve()
		{
			//Норма тангента
			var l = Tan(_paramSweep/4)*4/3;
			var ab = A/B;

			var point0 = GetPlanarValue(0);
			var point1 = point0 + new PointD(-point0.Y*ab, point0.X/ab)*l;

			var point3 = GetPlanarValue(1);
			var point2 = point3 - new PointD(-point3.Y*ab, point3.X/ab)*l;

			return new BezierCurve(
				Isometry*point0,
				Isometry*point1,
				Isometry*point2,
				Isometry*point3
				);
		}
	}
}