using System;
using System.Collections.Generic;
using static Ruzil3D.Math;

namespace Ruzil3D.Filters
{
    /// <summary>
    /// - Сглаживатель поверхностей.
    /// </summary>
    public class SurfaceBlurFilter
    {
        #region Fields

        //Функция к которой нужно применить фильтр
        private readonly Func<double, double, double> _function;

        //Массивы с параметрами фильтра
        private readonly double[] _arrayRadius;
        private readonly double[] _arrayVolumes;
        private readonly int[] _arraySectors;

        #endregion

        #region Methods

        /// <summary>
        /// Заполняет параметрами фильтра.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="arrayRadius"></param>
        /// <param name="arrayVolumes"></param>
        /// <param name="arraySectors"></param>
        private static void PrepareBlur(BlurFunctionHandler filter, out double[] arrayRadius, out double[] arrayVolumes, out int[] arraySectors)
        {
            //Начальное значение ширины сектора
            const double drStart = 0.01D;

            //Мультипликатор для dr
            const double drMultiplier = 1.5D;

            //Предельный радиус
            const double rMax = 3;

            double vSum = 0;

            var listRadius = new List<double>();
            var listVolumes = new List<double>();
            var listSectors = new List<int>();

            //Начальные значение радиуса
            double r = 0;

            //Начальное значение ширины сектора
            var dr = drStart;

            var volume = filter(0) * Pi * dr * dr / 4D;
            vSum += volume;

            //Сохраняем параметры текущей итерации
            listRadius.Add(0);
            listVolumes.Add(volume);
            listSectors.Add(1);

            //double tick = TickCounter.TickCount;

            do
            {
                //Предыдущее значение
                var saveDr = dr;

                //Текущее значение
                dr *= drMultiplier;

                r += (saveDr + dr) / 2D;

                //Количество делений угла в зависимости от радиуса и от шага радиуса
                var aCount = (int)(Tau * r / dr + 1);

                //Гауссовский объем

                //Первый способ (метод прямоугольников)
                volume = filter(r) * Tau * r * dr;
                vSum += volume;

                //Второй способ (метод трапеций)
                //volume = (filter(r - dr / 2) + filter(r + dr / 2)) * Math.Pi * r * dr;
                //vSum += volume;

                //Сохраняем параметры текущей итерации
                listRadius.Add(r);
                listVolumes.Add(volume / aCount);
                listSectors.Add(aCount);
            }
            while (r < rMax);

            //MessageBox.Show((TickCounter.TickCount - tick).ToString() + "!!!");

            //Нормируем объемы
            for (var i = 0; i < listVolumes.Count; i++)
            {
                listVolumes[i] /= vSum;
            }

            arrayRadius = listRadius.ToArray();
            arraySectors = listSectors.ToArray();
            arrayVolumes = listVolumes.ToArray();

        }
        #endregion

        #region Constructors

        public SurfaceBlurFilter(BlurFunctionHandler filter)
        {
            PrepareBlur(filter, out _arrayRadius, out _arrayVolumes, out _arraySectors);
        }

        public SurfaceBlurFilter(BlurFunctionHandler filter, Func<double, double, double> function)
        {
            _function = function;
            PrepareBlur(filter, out _arrayRadius, out _arrayVolumes, out _arraySectors);
        }

        #endregion

        #region Public Methods

        public double GetValue(double x, double y)
        {
            return GetValue(x, y, _function);
        }

        public double GetValue(double x, double y, Func<double, double, double> function)
        {
            var result = function(x, y) * _arrayVolumes[0];

            for (var i = 1; i < _arrayRadius.Length; i++)
            {
                //Текущее расстояние до средней линии бублика
                var radius = _arrayRadius[i];

                //Количество делений угла
                var aCount = _arraySectors[i];

                //Шаг деления угла
                var angleStep = Tau / aCount;

                //Суммируем значения функции по текущему бублику
                double preResult = 0;

                for (var j = 0; j < aCount; j++)
                {
                    var angle = j * angleStep;
                    preResult += function(x + radius * Cos(angle), y + radius * Sin(angle));
                }

                result += preResult * _arrayVolumes[i];
            }

            return result;
        }

        #endregion
    }
}