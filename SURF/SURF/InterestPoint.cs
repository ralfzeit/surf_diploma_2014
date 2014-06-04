using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SURF.SURF
{
    /// <summary>
    /// Пара ключевых точек
    /// </summary>
    public class InterestPointPair
    {
        public InterestPoint p1;//первая ключевая точка
        public InterestPoint p2;//вторая ключевая точка	
        public double dist;//расстояние между точками
    }

    /// <summary>
    /// Ключевая точка
    /// </summary>
    public class InterestPoint
    {
        public InterestPoint()
        {
            orientation = 0;
        }

        //Координаты найденной ключевой точки
        public float x, y;

        //Масштаб
        public float scale;
        public float response;

        //Ориентация ключевой точки
        public float orientation;

        //Знак Лапласиана
        public int laplacian;

        //Вектор дескриптора
        public int descriptorLength;
        public float[] descriptor = null;
        public void SetDescriptorLength(int Size)
        {
            descriptorLength = Size;
            descriptor = new float[Size];
        }

        /// <summary>
        /// Сравнение двух дескрипторов
        /// </summary>
        /// <param name="pt1">Ключевая точка 1</param>
        /// <param name="pt2">Ключевая точка 2</param>
        /// <param name="best">Максимально возможное расстояние</param>
        /// <returns></returns>
        public static double compareDescriptors(InterestPoint pt1, InterestPoint pt2, double best)
        {
            double total_cost = 0;

            for (int i = 0; i < pt1.descriptorLength; i += 4)
            {
                double t0 = pt1.descriptor[i] - pt2.descriptor[i];
                double t1 = pt1.descriptor[i + 1] - pt2.descriptor[i + 1];
                double t2 = pt1.descriptor[i + 2] - pt2.descriptor[i + 2];
                double t3 = pt1.descriptor[i + 3] - pt2.descriptor[i + 3];
                total_cost += t0 * t0 + t1 * t1 + t2 * t2 + t3 * t3;
                if (total_cost > best)
                    break;
            }
            return total_cost;
        }
    }
}
