using System;
using Ruzil3D.Approximation;
using Ruzil3D.Algebra;

namespace Ruzil3D.Curves
{
	/// <summary>
	/// Универсальный класс компилированной кривой для обращения к точкам через дистанцию.
	/// </summary>
	public class ParametricCurveDistanceCompiler<T> where T : ParametricCurve
	{
		#region Types

		private interface IMonotonicGridPolynomialInterpolation : IMonotonicFunctionApproximation
		{
			Polynomial GetPolynom(int index);
		}

		private class CubicInterpolator : CubicInterpolation, IMonotonicGridPolynomialInterpolation
		{
			public CubicInterpolator(PointD[] points, bool check = false) : base(points, check)
			{
				LValue = Points[0].Y;
				RValue = Points[Points.Length - 1].Y;
				MinValue = LValue < RValue ? LValue : RValue; // Min(LValue, RValue);
				MaxValue = LValue > RValue ? LValue : RValue; // Max(LValue, RValue);
			}

			public double LValue { get; }

			public double MaxValue { get; }

			public double MinValue { get; }

			public double RValue { get; }

			public double GetArgument(double y)
			{
				throw new NotImplementedException();
			}

			public override double GetValue(double x)
			{
				var result = base.GetValue(x);

				if (result < MinValue)
				{
					return MinValue;
				}

				if (result > MaxValue)
				{
					return MaxValue;
				}

				return result;

				//return Max(MinValue, Min(MaxValue, base.GetValue(x)));
			}
		}

		private class LinearInterpolator : LinearInterpolation, IMonotonicGridPolynomialInterpolation
		{
			public LinearInterpolator(PointD[] points, bool check = false) : base(points, check)
			{
				LValue = Points[0].Y;
				RValue = Points[Points.Length - 1].Y;
				MinValue = LValue < RValue ? LValue : RValue; // Min(LValue, RValue);
				MaxValue = LValue > RValue ? LValue : RValue; // Max(LValue, RValue);
			}

			public double LValue { get; }

			public double MaxValue { get; }

			public double MinValue { get; }

			public double RValue { get; }

			public double GetArgument(double y)
			{
				throw new NotImplementedException();
			}

			public override double GetValue(double x)
			{
				var result = base.GetValue(x);

				if (result < MinValue)
				{
					return MinValue;
				}

				if (result > MaxValue)
				{
					return MaxValue;
				}

				return result;

				//return Max(MinValue, Min(MaxValue, base.GetValue(x)));
			}
		}

		private class CustomInterpolator : IMonotonicFunctionApproximation
		{
			private readonly Func<double, double> _invertRectification;

			public CustomInterpolator(Func<double, double> invertRectification, double length)
			{
				_invertRectification = invertRectification;
				LArgument = 0;
				RArgument = length;

				MinValue = LValue = 0;
				MaxValue = RValue = 1;
			}

			public double LArgument { get; }
			public double RArgument { get; }

			public double GetValue(double x)
			{
				return _invertRectification(x);
			}

			public double LValue { get; }
			public double RValue { get; }
			public double MinValue { get; }
			public double MaxValue { get; }

			public double GetArgument(double y)
			{
				throw new NotImplementedException();
			}

			public Polynomial GetPolynom(int index)
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		private T _curve;

		/// <summary>
		/// Получает и задает исходную кривую.
		/// </summary>
		/// <remarks>Задается конструктором.</remarks>
		/// <remarks>Изменение этого параметра сбрасывает результаты компиляции и как следствие приводит к повторной компиляции при следующем обращении к функциям <see cref="GetParameter"/> и <see cref="GetValue"/>.</remarks>
		public T Curve
		{
			get { return _curve; }
			set
			{
				if (_curve != value)
				{
					_curve = value;
					Reset();
				}
			}
		}

		private int _accuracy;

		/// <summary>
		/// Получает и задает точность аппроксимации аппроксимации.
		/// </summary>
		/// <remarks>Изменение этого параметра сбрасывает результаты компиляции и как следствие приводит к повторной компиляции при следующем обращении к функциям <see cref="GetParameter"/> и <see cref="GetValue"/>.</remarks>
		public int Accuracy
		{
			get { return _accuracy; }
			set
			{
				if (_accuracy != value)
				{
					if (value < 1)
					{
						throw new ArgumentException("Точность аппроксимации должна быть больше 0.", nameof(Accuracy));
					}

					_accuracy = value;
					Reset();
				}
			}
		}

		private EApproximationType _type;

		/// <summary>
		/// Получает и задает способ аппроксимации.
		/// </summary>
		/// <remarks>Изменение этого параметра сбрасывает результаты компиляции и как следствие приводит к повторной компиляции при следующем обращении к функциям <see cref="GetParameter"/> и <see cref="GetValue"/>.</remarks>
		public EApproximationType Type
		{
			get { return _type; }
			set
			{
				if (_type != value)
				{
					_type = value;
					Reset();
				}
			}
		}

		#region Approx

		private void Reset()
		{
			_approx = null;
		}

		private IMonotonicFunctionApproximation _approx;

		private IMonotonicFunctionApproximation Approx
		{
			get
			{
				if (_approx == null)
				{
					Compile();
				}

				return _approx;
			}
		}

		#endregion

		/// <summary>
		/// Инициализирует новый экземпляр класса <see cref="ParametricCurveDistanceCompiler{T}"/> для обращения к точкам на кривой через дистанцию.
		/// </summary>
		/// <param name="curve">Исходная кривая.</param>
		/// <param name="acc">Показатель разбиения. Кривая делится на 2ᵃᶜᶜ участков.</param>
		/// <param name="type">Способ аппроксимации.</param>
		/// <exception cref="ArgumentException">Передано значение меньше 1</exception>
		public ParametricCurveDistanceCompiler(T curve, int acc = 6, EApproximationType type = EApproximationType.Default)
		{
			Curve = curve;
			Accuracy = acc;
			Type = type;
		}

		private void Compile()
		{
			var rectification = Curve.GetInvertRectificationFunction();
			if (rectification != null)
			{
				_approx = new CustomInterpolator(rectification, Curve.Length);
				return;
			}

			var count = Curve.IsNatural ? 1 : 1 << Accuracy;
			//var count = 20;


			var points = new PointD[count + 1];
			points[count] = new PointD(Curve.Length, 1);


			//var a = 1D/(1 << (Accuracy+15));
			//var st = (1D - a)/(count - 1D);


			var step = 1D/count;

			for (var i = 1; i < count; i++)
			{
				//var t = a + (i - 1) * (1 - 2 * a) / (count - 2);
				//var t = a + (i - 1)*st;
				//Sign(Cos(x))*Sqrt(Abs(Cos(x)))
				//var t = (1 - Sign(Cos(i*PI/count))*Sqrt(Abs(Cos(i*PI/count))))/2D;

				var t = i * step;
				//var t = (1 - Cos(i*PI/count))/2D;
				

				var l = Curve.GetDistance(0, t);
				points[i] = new PointD(l, t);
			}

			//Аппроксимация (интерполяция) t от расстояния
			switch (Type)
			{

				case EApproximationType.HighSpeed:
					_approx = new LinearInterpolator(points);
					break;

				case EApproximationType.Linear:
					_approx = new LinearInterpolator(points);
					break;

				case EApproximationType.Default:
					_approx = new CubicInterpolator(points);
					break;

				case EApproximationType.HighQuality:
					_approx = new CubicInterpolator(points);
					break;

				case EApproximationType.Cubic:
					_approx = new CubicInterpolator(points);
					break;

				default:
					_approx = new CubicInterpolator(points);
					break;
			}
		}

		/// <summary>
		/// Получает параметр кривой соответствующий указанной дистанции.
		/// </summary>
		/// <param name="distance">Исходная дистанция.</param>
		/// <returns>Параметр кривой соответствующий указанной дистанции.</returns>
		public double GetParameter(double distance)
		{
			return Approx.GetValue(distance);
		}

		/// <summary>
		/// Получает точку на кривой соответствующую указанной дистанции.
		/// </summary>
		/// <param name="distance"></param>
		/// <returns>Точка на кривой соответствующую указанной дистанции.</returns>
		public Point3D GetValue(double distance)
		{
			return Curve.GetValue(GetParameter(distance));
		}

	}
}