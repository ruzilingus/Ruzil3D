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
				_paramStart = Atan2(A*Sin(angleStart), B*Cos(angleStart));

				//Параметр эллипса лежит в той же четверти, что и полярный угол точки, поэтому их разность по модулю меньше π/2,
				//а развёртка параметра имеет тот же знак и то же число полных оборотов, что и развёртка угла. Прежде развёртка
				//поправлялась только по знаку конечного параметра, без сравнения с начальным: дуга могла идти в обратную
				//сторону или охватывать лишний оборот (например, эллипс с развёрткой 2π обходился дважды).
				_paramSweep = angleSweep + GetParameterOffset(angleStart + angleSweep) - GetParameterOffset(angleStart);
			}
		}

		/// <summary>
		/// Возвращает разность между параметром эллипса и полярным углом его точки.
		/// </summary>
		/// <param name="angle">Полярный угол точки эллипса в радианах.</param>
		/// <returns>Разность, по модулю меньшая π/2.</returns>
		private double GetParameterOffset(double angle)
		{
			var offset = Atan2(A*Sin(angle), B*Cos(angle)) - angle;

			//Atan2 возвращает значение от −π до π, поэтому разность отличается от искомой на целое число оборотов.
			return offset - Math.Tau*Round(offset/Math.Tau);
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
		/// <returns>Длина участка эллиптической дуги со знаком: отрицательная, если <paramref name="t1"/> меньше <paramref name="t0"/>.</returns>
		/// <remarks>Как и для остальных кривых, <i>GetDistance(t₁, t₀) = −GetDistance(t₀, t₁)</i>.</remarks>
		public override double GetDistance(double t0, double t1)
		{
			if (A.Equals(B))
			{
				//Прежде для дуги окружности возвращался модуль, а для эллиптической дуги — значение со знаком.
				return Abs(AngleSweep*A)*(t1 - t0);
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
		/// Получает направление обхода дуги: 1 — против часовой стрелки, −1 — по часовой стрелке.
		/// </summary>
		private double Orientation => _paramSweep < 0 ? -1D : 1D;

		/// <summary>
		/// Возвращает вектор касательной к эллиптической дуги на плоскости в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Единичный вектор касательной на плоскости, направленный в сторону возрастания параметра, представленный структурой <see cref="PointD"/>.</returns>
		public override PointD GetPlanarTangent(double parameter)
		{
			var t = parameter*_paramSweep + _paramStart;

			//Касательная направлена по ходу дуги. Прежде знак развёртки не учитывался, и у дуги, идущей по часовой
			//стрелке, касательная была направлена против движения.
			var orientation = Orientation;

			if (A.Equals(B))
			{
				return new PointD(-Sin(t)*orientation, Cos(t)*orientation);
			}

			var x1 = -A*Sin(t);
			var y1 = B*Cos(t);
			var len = Sqrt(x1*x1 + y1*y1)*orientation;

			return new PointD(x1/len, y1/len);
		}

		/// <summary>
		/// Возвращает значение кривизны эллиптической дуги на плоскости в заданной параметром точке.
		/// </summary>
		/// <param name="parameter">Параметр кривой.</param>
		/// <returns>Значение кривизны на плоскости: положительное для дуги, идущей против часовой стрелки, и отрицательное для дуги, идущей по часовой стрелке.</returns>
		/// <remarks>Значение равное Z-состовляющей псевдовектора кривизны направленого перпендикулярно к плоскости.</remarks>
		public override double GetPlanarCurvature(double parameter)
		{
			//Прежде знак развёртки не учитывался, и кривизна дуги, идущей по часовой стрелке, получалась положительной.
			var orientation = Orientation;

			if (A.Equals(B))
			{
				return orientation/A;
			}

			var t = parameter*_paramSweep + _paramStart;

			var x1 = -A*Sin(t);
			var y1 = B*Cos(t);
			var len = Sqrt(x1*x1 + y1*y1);

			return orientation*A*B/(len*len*len);
		}

		/// <summary>
		/// Приводит эллиптическую дугу к кривой Безье.
		/// </summary>
		/// <returns>Возвращают кривую Безье близкую к заданной эллиптической дуги.</returns>
		/// <remarks>Одна кубическая кривая хорошо приближает только небольшую дугу: для четверти окружности отклонение от неё
		/// не превышает 2.8·10⁻⁴ радиуса, для половины окружности достигает 1.8·10⁻² радиуса, а при развёртке, близкой к
		/// полному обороту, результат непригоден. Для больших дуг используйте <see cref="RecastToBezierCurves"/>.</remarks>
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

		/// <summary>
		/// Приводит эллиптическую дугу к последовательности кубических кривых Безье.
		/// </summary>
		/// <returns>Массив кривых Безье, последовательно приближающих дугу: первая кривая начинается в начальной точке дуги,
		/// каждая следующая — в конечной точке предыдущей, последняя заканчивается в конечной точке дуги.</returns>
		/// <remarks>Дуга делится на наименьшее число равных по параметру эллипса участков, не превышающих четверти оборота;
		/// кривая с индексом <i>k</i> приближает участок дуги с параметрами от <i>k</i>/<i>n</i> до (<i>k</i> + 1)/<i>n</i>,
		/// где <i>n</i> — длина массива. Отклонение от дуги окружности не превышает 2.8·10⁻⁴ радиуса, от эллиптической
		/// дуги — 2.8·10⁻⁴ большей полуоси, при любой развёртке, в том числе для полного эллипса.</remarks>
		public BezierCurve[] RecastToBezierCurves()
		{
			//Одна кубическая кривая (RecastToBezierCurve) приближает дугу тем хуже, чем больше развёртка, а при развёртке 2π
			//её промежуточные точки уходят в бесконечность, поэтому дуга делится на участки не больше четверти оборота.
			//Небольшой допуск не даёт погрешности округления добавить лишний участок (например, для развёртки ровно 2π).
			var quarters = Abs(_paramSweep)/(PI/2);
			var count = quarters > 1 ? (int)Ceiling(quarters - 1e-9) : 1;

			var result = new BezierCurve[count];

			//Норма тангента для участка
			var l = Tan(_paramSweep/count/4)*4/3;
			var ab = A/B;

			var point0 = GetPlanarValue(0);
			for (var k = 0; k < count; k++)
			{
				var point3 = GetPlanarValue((k + 1D)/count);

				var point1 = point0 + new PointD(-point0.Y*ab, point0.X/ab)*l;
				var point2 = point3 - new PointD(-point3.Y*ab, point3.X/ab)*l;

				result[k] = new BezierCurve(
					Isometry*point0,
					Isometry*point1,
					Isometry*point2,
					Isometry*point3
					);

				point0 = point3;
			}

			return result;
		}
	}
}