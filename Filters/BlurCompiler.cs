using System;
using System.Collections.Generic;
using Ruzil3D.Algebra;

namespace Ruzil3D.Filters
{
    /// <summary>
	/// - Компилированный объект сглаживающего фильтра.
	/// </summary>
	public class BlurCompiler
	{
		private class CompileItem
		{
			public readonly double[] ArrayRadius;
			public readonly double[] ArrayVolumes;
			public readonly BlurFunctionHandler Filter;

			public CompileItem(double[] arrayRadius, double[] arrayVolumes, BlurFunctionHandler filter)
			{
				ArrayRadius = arrayRadius;
				ArrayVolumes = arrayVolumes;
				Filter = filter;
			}
		}

		//private static readonly CompileItem[] CompileItemsCache = new CompileItem[9];

		//private static volatile CompileItem _defaultCompileItem = null;

		private readonly CompileItem _compileItem;

		#region Static

        private const double GausDivider = 1/Math.SqrtPi;

        private static double _GausFunction(double r)
		{
			const double sign = 2;
			return sign * GausDivider * Math.Exp(-sign * sign * r * r);
		}

		public static BlurFunctionHandler DefaultFilter => _GausFunction;

	    #endregion

		#region Compile

		/// <summary>
		/// Вычисляет площадь кривой вокруг точки center.
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="center"></param>
		/// <param name="dr"></param>
		/// <returns></returns>
		private static double GetArea(BlurFunctionHandler filter, double center, double dr)
		{
			//Степень многочлена которым будем аппроксимировать исходную функцию
			const int deg = 8;

			//Метод многочленов deg-степени
			var points = new PointD[deg + 1];

			//Левая граница
			var rm = center - dr;

			for (var i = 0; i <= deg; i++)
			{
				var x = i * 2D * dr / deg;
				points[i] = new PointD(i, filter(rm + x));
			}

			//Строим многочлен deg-степени проходящий через заданные точки
			var polynom = Polynomial.GetPolynomial(points);

			//Вычисляем площадь фигуры ограниченной полиномом
			return polynom.GetArea(0, deg) * 2D * dr / deg;




			/*
			const int deg = 8;

			//Метод многочленов deg-степени
			PointD[] points = new Point2D[deg + 1];

			double rm = center - dr / 2D;

			for (int i = 0; i <= deg; i++)
			{
			    double x = i * dr / deg;
			    points[i] = new PointD(x, filter(rm + x));
			}

			return Polynomial.Resolve(points).GetArea(0, dr);
			*/

		}

	    /// <summary>
	    /// Заполняет вспомогательные данные методом многочленов deg-степени.
	    /// </summary>
	    /// <param name="filter">Фильтр-функция</param>
	    /// <param name="accuracy"></param>
	    private static CompileItem CompileImplementation(BlurFunctionHandler filter, double accuracy = 1D)
		{
			//var tick = TickCounter.TickCount;

			//for (int m = 100000; m > 0.1; m--)
			//for (int s = 100; s >= 1; s--)
			//{

			#region Начальные значения
			
			var drStart = 0.18256D * accuracy;
			const double drMultiplier = 1.16D;

			var listRadius = new List<double>();
			var listVolumes = new List<double>();

			//Начальные значение радиуса
			double r = 0;

			//Начальное значение ширины сектора
			var dr = drStart / 2D;

			//Будем собирать сумму
			double vSum = 0;

			#endregion

			#region Вычисляем и заполняем площади

			var volume = GetArea(filter, r, dr);

			vSum += volume;

			//Сохраняем параметры текущей итерации
			listRadius.Add(0);
			listVolumes.Add(volume);

			var limit = 1E-50D * filter(0);

			do
			{
				//Предыдущее значение
				var saveDr = dr;

				//Текущее значение
				dr *= drMultiplier;

				r += saveDr + dr;

				//Метод многочленов
				volume = GetArea(filter, r, dr);

				vSum += 2 * volume;

				//Сохраняем параметры текущей итерации
				listRadius.Add(r);
				listVolumes.Add(volume);

				if (volume < limit)
				{
					break;
				}
			    if (listRadius.Count > 1000)
			    {
			        throw new Exception("Функция не сходится либо сходится очень медленно");
			    }
			} while (true);

			#endregion

			if (vSum.Equals(1D))
			{
				//MessageBox.Show(drStart + " " + drMultiplier);
			}


			#region Нормируем объемы

			if (!vSum.Equals(1D))
			{
				for (var i = 0; i < listVolumes.Count; i++)
				{
					listVolumes[i] /= vSum;
				}
			}

			#endregion
			//}
			//tick = TickCounter.TickCount - tick;
			//MessageBox.Show(tick.ToString());


			//return null;
			return new CompileItem(listRadius.ToArray(), listVolumes.ToArray(), filter);
		}

	    /// <summary>
	    /// Заполняет вспомогательные данные методом многочленов deg-степени.
	    /// </summary>
	    /// <param name="filter">Фильтр-функция</param>
	    /// <param name="accuracy"></param>
	    private static CompileItem Compile(BlurFunctionHandler filter, double accuracy = 1D)
		{
			return CompileImplementation(filter, accuracy);



			/*
			CompileItem result = CompileItemsCache[deg];

			if (result == null)
			{
			    if (deg >= 0 && deg <= 8)
			    {
				CompileItemsCache[deg] = result = CompileImplementation(filter, deg);
			    }
			    else
			    {
				throw new ArgumentException("Значение deg должно быть от 0 до 8");
			    }

			}

			return result;
			 * */
		}

		#endregion

		#region Public Items

		public BlurFunctionHandler Filter => _compileItem.Filter;

	    /// <summary>
	    /// Возвращает сглаженное значение функции function.
	    /// </summary>
	    /// <param name="function"></param>
	    /// <param name="x"></param>
	    /// <returns></returns>
	    public Point3D GetValue3D(Func<double, Point3D> function, double x)
		{
			var result = function(x) * _compileItem.ArrayVolumes[0];
			for (var i = 1; i < _compileItem.ArrayRadius.Length; i++)
			{
				var r = _compileItem.ArrayRadius[i];
				result.Add((function(x - r) + function(x + r)) * _compileItem.ArrayVolumes[i]);
			}

			return result;
		}




		public Point3D GetValue3D(Func<double, Point3D> function, double x, double scale = 1)
		{
			//При scale = Infinity, получается средняя значение кривой не зависящее от x
			//При scale = 0, получается сама кривая
			//При scale = 1, значение по умолчанию

			if (scale < 0)
			{
				throw new ArgumentException("Значение scale не должно быть меньше нуля");
			}
		    if (double.IsPositiveInfinity(scale))
		    {
		        throw new NotImplementedException("Пока не реализовано");
		    }
		    if (scale.Equals(0D))
		    {
		        return function(x);
		    }

		    var result = function(x) * _compileItem.ArrayVolumes[0];
			for (var i = 1; i < _compileItem.ArrayRadius.Length; i++)
			{
				var r = _compileItem.ArrayRadius[i];
				result.Add((function(x - scale * r) + function(x + scale * r)) * _compileItem.ArrayVolumes[i]);
			}

			return result;
		}

	    /// <summary>
	    /// Возвращает приращение функции на delta-окрестности точки d.
	    /// </summary>
	    /// <param name="function"></param>
	    /// <param name="x"></param>
	    /// <param name="delta"></param>
	    /// <param name="scale"></param>
	    /// <returns></returns>
	    public Point3D GetDifferential(Func<double, Point3D> function, double x, double delta, double scale = 1)
		{
			return GetValue3D(function, x + delta, scale) - GetValue3D(function, x - delta, scale);
		}

		/// <summary>
		/// Возвращает производную в точке.
		/// </summary>
		/// <param name="function"></param>
		/// <param name="x"></param>
		/// <param name="delta"></param>
		/// <returns></returns>
		public Point3D GetDerivative(Func<double, Point3D> function, double x, double delta)
		{

			return GetDifferential(function, x, delta) / (2 * delta);
		}

		public double GetValue(Func<double, double> function, double x)
		{
			var result = function(x) * _compileItem.ArrayVolumes[0];
			for (var i = 1; i < _compileItem.ArrayRadius.Length; i++)
			{
				var r = _compileItem.ArrayRadius[i];
				result += (function(x - r) + function(x + r)) * _compileItem.ArrayVolumes[i];
			}

			return result;
		}

		public double GetValue(Func<double, double> function, double x, double scale = 1)
		{
			//При scale = Infinity, получается средняя значение кривой не зависящее от x
			//При scale = 0, получается сама кривая
			//При scale = 1, значение по умолчанию

			if (scale < 0)
			{
				throw new ArgumentException("Значение scale не должно быть меньше нуля");
			}
		    if (double.IsPositiveInfinity(scale))
		    {
		        throw new NotImplementedException("Пока не реализовано");
		    }
		    if (scale.Equals(0))
		    {
		        return function(x);
		    }

		    var result = function(x) * _compileItem.ArrayVolumes[0];
			for (var i = 1; i < _compileItem.ArrayRadius.Length; i++)
			{
				var r = _compileItem.ArrayRadius[i];
				result += (function(x - scale * r) + function(x + scale * r)) * _compileItem.ArrayVolumes[i];
			}

			return result;
		}

		#endregion

		#region Constructors

		public BlurCompiler(double accuracy = 1D)
		{
			_compileItem = Compile(DefaultFilter, accuracy);
		}

		public BlurCompiler(BlurFunctionHandler filter, double accuracy = 1D)
		{
			_compileItem = Compile(filter, accuracy);
		}

		#endregion
	}
}
