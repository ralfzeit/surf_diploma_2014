using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SURF.SURF
{
    /// <summary>
    /// Класс, осуществляющий работу с интегральным изображением
    /// </summary>
    public class IntegralImage
    {
        const float cR = .2989f;
        const float cG = .5870f;
        const float cB = .1140f;

        //Массив значений интегрального изображения
        internal float[,] Matrix;
        //Размеры изображений (ширина, высота)
        public int Width, Height;

        public float this[int y, int x]
        {
            get { return Matrix[y, x]; }
            set { Matrix[y, x] = value; }
        }

        private IntegralImage(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Matrix = new float[height, width];
        }

        /// <summary>
        /// Получение интегрального изображения из массива температур
        /// </summary>
        public static IntegralImage FromArray(float[,] tArray, float max, float min)
        {
            IntegralImage pic = new IntegralImage(tArray.GetLength(0), tArray.GetLength(1));
            float rowsum = 0;
            for (int x = 0; x < tArray.GetLength(0); x++)
            {
                float temp = tArray[x, 0];
                temp = (float)((temp - min));
                rowsum += temp;
                pic[0, x] = rowsum;
            }

            for (int y = 1; y < tArray.GetLength(1); y++)
            {
                rowsum = 0;
                for (int x = 0; x < tArray.GetLength(0); x++)
                {
                    float temp = tArray[x, y];
                    temp = (float)((temp - min));
                    rowsum += temp;
                    pic[y, x] = rowsum + pic[y - 1, x];
                }
            }

            return pic;
        }

        /// <summary>
        /// Получение интегрального изображения из bitmap
        /// </summary>
        public static IntegralImage FromImage(Bitmap image)
        {
            IntegralImage pic = new IntegralImage(image.Width, image.Height);

            float rowsum = 0;
            for (int x = 0; x < image.Width; x++)
            {
                Color c = image.GetPixel(x, 0);
                rowsum += (cR * c.R + cG * c.G + cB * c.B) / 255f;
                pic[0, x] = rowsum;
            }


            for (int y = 1; y < image.Height; y++)
            {
                rowsum = 0;
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    rowsum += (cR * c.R + cG * c.G + cB * c.B) / 255f;   
                    pic[y, x] = rowsum + pic[y - 1, x];
                }
            }

            return pic;
        }

        /// <summary>
        /// Вычисление суммы яркости пикселей внутри указанного прямоугольника
        /// </summary>
        public float BoxIntegral(int row, int col, int rows, int cols)
        {
            int r1 = Math.Min(row, Height) - 1;
            int c1 = Math.Min(col, Width) - 1;
            int r2 = Math.Min(row + rows, Height) - 1;
            int c2 = Math.Min(col + cols, Width) - 1;

            float A = 0, B = 0, C = 0, D = 0;
            if (r1 >= 0 && c1 >= 0) A = Matrix[r1, c1];
            if (r1 >= 0 && c2 >= 0) B = Matrix[r1, c2];
            if (r2 >= 0 && c1 >= 0) C = Matrix[r2, c1];
            if (r2 >= 0 && c2 >= 0) D = Matrix[r2, c2];

            //Формула для поиска интегральной площади
            return Math.Max(0, A - B - C + D);
        }

        /// <summary>
        /// Вычисление значения вейвлета Хаара (градиент по dX)
        /// </summary>
        public float HaarX(int row, int column, int size)
        {
            return BoxIntegral(row - size / 2, column, size, size / 2)
              - 1 * BoxIntegral(row - size / 2, column - size / 2, size, size / 2);
        }

        /// <summary>
        /// Вычисление значения вейвлета Хаара (градиент по dY)
        /// </summary>
        public float HaarY(int row, int column, int size)
        {
            return BoxIntegral(row, column - size / 2, size / 2, size)
              - 1 * BoxIntegral(row - size / 2, column - size / 2, size / 2, size);
        }
    }

}
