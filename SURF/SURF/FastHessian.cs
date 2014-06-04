using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace SURF.SURF
{
    public class FastHessian
    {
        /// <summary>
        /// Слой значений
        /// </summary>
        private class ResponseLayer
        {
            public int width, height, step, filter;
            public float[] responses;
            public byte[] laplacian;

            public ResponseLayer(int width, int height, int step, int filter)
            {
                this.width = width;     //ширина слоя
                this.height = height;   //высота слоя
                this.step = step;       //шаг
                this.filter = filter;

                responses = new float[width * height];
                laplacian = new byte[width * height];
            }

            /// <summary>
            /// получить значение лапласиана в указанной точке
            /// </summary>
            public byte getLaplacian(int row, int column)
            {
                return laplacian[row * width + column];
            }

            /// <summary>
            /// Получить значение Лапласиана в указанной точке в указанном слое
            /// </summary>
            public byte getLaplacian(int row, int column, ResponseLayer src)
            {
                int scale = this.width / src.width;
                return laplacian[(scale * row) * width + (scale * column)];
            }

            /// <summary>
            /// Получить значение Гессиана 
            /// </summary>
            public float getResponse(int row, int column)
            {
                return responses[row * width + column];
            }

            /// <summary>
            /// Получить значение Гессиана в указанной точке в указанном слое
            /// </summary>
            public float getResponse(int row, int column, ResponseLayer src)
            {
                int scale = this.width / src.width;
                return responses[(scale * row) * width + (scale * column)];
            }

        }

        //Получить ключевые точки
        public static List<InterestPoint> getIpoints(float thresh, int octaves, int init_sample, IntegralImage img)
        {
            FastHessian fh = new FastHessian(thresh, octaves, init_sample, img);
            return fh.getIpoints();
        }


        public FastHessian(float thresh, int octaves, int init_sample, IntegralImage img)
        {
            this.thresh = thresh;
            this.octaves = octaves;
            this.init_sample = init_sample;
            this.img = img;
        }
        private int init_sample;   //расстояние между пропускаемыми точками при переходе между масштабами октавы
        private float thresh;      //значение порога отсечения 
        private int octaves;       //количество октав масштаба
        private IntegralImage img; //интегральное представление изображения

        private List<InterestPoint> iPoints; //ключевые точки
        private List<ResponseLayer> responseMap; //карта значений


        /// <summary>
        /// Найти ключевые точки изображения и записать их в вектор значений
        /// </summary>
        public List<InterestPoint> getIpoints()
        {
            //Карта индексов фильтров октав
            int[,] filter_map = { { 0, 1, 2, 3 }, { 1, 3, 4, 5 }, { 3, 5, 6, 7 }, { 5, 7, 8, 9 }, { 7, 9, 10, 11 } };

            // Очистим вектор от старых значений
            if (iPoints == null) iPoints = new List<InterestPoint>();
            else iPoints.Clear();

            // Создать карту значений
            buildResponseMap();

            // Получить значений для слоев октав
            ResponseLayer b, m, t;
            for (int o = 0; o < octaves; ++o)
                for (int i = 0; i <= 1; ++i)
                {
                    b = responseMap[filter_map[o, i]];
                    m = responseMap[filter_map[o, i + 1]];
                    t = responseMap[filter_map[o, i + 2]];

                    //Поиск максимума по масштабам и по плоскостям изображений
                    for (int r = 0; r < t.height; ++r)
                    {
                        for (int c = 0; c < t.width; ++c)
                        {
                            if (isExtremum(r, c, t, m, b))
                            {
                                interpolateExtremum(r, c, t, m, b);
                            }
                        }
                    }
                }
            return iPoints;
        }


        /// <summary>
        /// Создать карту (набор слоев) масштабов по установленному количеству октав
        /// </summary>
        void buildResponseMap()
        {
            // Oкт1: 9,  15, 21, 27
            // Oкт2: 15, 27, 39, 51
            // Oкт3: 27, 51, 75, 99
            // Oкт4: 51, 99, 147,195
            // Oкт5: 99, 195,291,387

            if (responseMap == null) responseMap = new List<ResponseLayer>();
            else responseMap.Clear();

            // Получить атрибуты картинки
            int w = (img.Width / init_sample);
            int h = (img.Height / init_sample);
            int s = (init_sample);

            if (octaves >= 1)
            {
                responseMap.Add(new ResponseLayer(w, h, s, 9));
                responseMap.Add(new ResponseLayer(w, h, s, 15));
                responseMap.Add(new ResponseLayer(w, h, s, 21));
                responseMap.Add(new ResponseLayer(w, h, s, 27));
            }

            if (octaves >= 2)
            {
                responseMap.Add(new ResponseLayer(w / 2, h / 2, s * 2, 39));
                responseMap.Add(new ResponseLayer(w / 2, h / 2, s * 2, 51));
            }

            if (octaves >= 3)
            {
                responseMap.Add(new ResponseLayer(w / 4, h / 4, s * 4, 75));
                responseMap.Add(new ResponseLayer(w / 4, h / 4, s * 4, 99));
            }

            if (octaves >= 4)
            {
                responseMap.Add(new ResponseLayer(w / 8, h / 8, s * 8, 147));
                responseMap.Add(new ResponseLayer(w / 8, h / 8, s * 8, 195));
            }

            if (octaves >= 5)
            {
                responseMap.Add(new ResponseLayer(w / 16, h / 16, s * 16, 291));
                responseMap.Add(new ResponseLayer(w / 16, h / 16, s * 16, 387));
            }

            // Извлечь значения из изображения
            for (int i = 0; i < responseMap.Count; ++i)
            {
                buildResponseLayer(responseMap[i]);
            }
        }


        /// <summary>
        /// Вычислить значения гессианов для указанного слоя масштаба
        /// </summary>
        private void buildResponseLayer(ResponseLayer rl)
        {
            int step = rl.step;                      // размер шага для фильтра
            int b = (rl.filter - 1) / 2;             // граница фильтра
            int l = rl.filter / 3;                   // доля для фильтра (размер фильтра / 3)
            int w = rl.filter;                       // размер фильтра
            float inverse_area = 1f / (w * w);       // нормализация
            float Dxx, Dyy, Dxy;

            for (int r, c, ar = 0, index = 0; ar < rl.height; ++ar)
            {
                for (int ac = 0; ac < rl.width; ++ac, index++)
                {
                    // получить координаты изображения
                    r = ar * step;
                    c = ac * step;

                    // Вычислить компоненты
                    Dxx = img.BoxIntegral(r - l + 1, c - b, 2 * l - 1, w)
                        - img.BoxIntegral(r - l + 1, c - l / 2, 2 * l - 1, l) * 3;
                    Dyy = img.BoxIntegral(r - b, c - l + 1, w, 2 * l - 1)
                        - img.BoxIntegral(r - l / 2, c - l + 1, l, 2 * l - 1) * 3;
                    Dxy = +img.BoxIntegral(r - l, c + 1, l, l)
                          + img.BoxIntegral(r + 1, c - l, l, l)
                          - img.BoxIntegral(r - l, c - l, l, l)
                          - img.BoxIntegral(r + 1, c + 1, l, l);

                    // Нормализовать фильтры с учетом их размеров
                    Dxx *= inverse_area;
                    Dyy *= inverse_area;
                    Dxy *= inverse_area;

                    // Получить значение гессиана и лапласиана
                    rl.responses[index] = (Dxx * Dyy - 0.81f * Dxy * Dxy);
                    rl.laplacian[index] = (byte)(Dxx + Dyy >= 0 ? 1 : 0);
                }
            }
        }



        /// <summary>
        /// Проверка для указанной точки, является ли она локальным максимумом 
        /// Гессиана по сравнению с окружающими точками на одном с ней слое, 
        /// с точками на слое масштаба больше и на слое масштаба меньше для октавы. 
        /// (Метод соседних точек 3x3x3)
        /// </summary>
        /// <param name="r">Строка</param>
        /// <param name="c">Колонка</param>
        /// <param name="t">Верхний слой </param>
        /// <param name="m">Средний слой</param>
        /// <param name="b">Нижний слой</param>
        /// <returns></returns>
        bool isExtremum(int r, int c, ResponseLayer t, ResponseLayer m, ResponseLayer b)
        {
            // проверка границ
            int layerBorder = (t.filter + 1) / (2 * t.step);
            if (r <= layerBorder || r >= t.height - layerBorder || c <= layerBorder || c >= t.width - layerBorder)
                return false;

            // проверить, что значение в точки среднего слоя больше порогового значения
            float candidate = m.getResponse(r, c, t);
            if (candidate < thresh)
                return false;

            for (int rr = -1; rr <= 1; ++rr)
            {
                for (int cc = -1; cc <= 1; ++cc)
                {
                    // Если любой из соседей 3x3x3 больше, то это не экстремум
                    if (t.getResponse(r + rr, c + cc) >= candidate ||
                      ((rr != 0 || cc != 0) && m.getResponse(r + rr, c + cc, t) >= candidate) ||
                      b.getResponse(r + rr, c + cc, t) >= candidate)
                    {
                        return false;
                    }
                }
            }

            return true;
        }



        /// <summary>
        /// Интерполирование найденных Гессианов соседей 3x3x3
        /// </summary>
        void interpolateExtremum(int r, int c, ResponseLayer t, ResponseLayer m, ResponseLayer b)
        {
            Matrix D = Matrix.Create(BuildDerivative(r, c, t, m, b));
            Matrix H = Matrix.Create(BuildHessian(r, c, t, m, b));
            Matrix Hi = H.Inverse();
            Matrix Of = -1 * Hi * D;

            double[] O = { Of[0, 0], Of[1, 0], Of[2, 0] };

            // шаг между фильтрами
            int filterStep = (m.filter - b.filter);

            // если точка достаточно близка к фактическому экстремуму
            if (Math.Abs(O[0]) < 0.5f && Math.Abs(O[1]) < 0.5f && Math.Abs(O[2]) < 0.5f)
            {
                InterestPoint iPoint = new InterestPoint();
                iPoint.x = (float)((c + O[0]) * t.step);
                iPoint.y = (float)((r + O[1]) * t.step);
                iPoint.scale = (float)((0.1333f) * (m.filter + O[2] * filterStep));
                iPoint.laplacian = (int)(m.getLaplacian(r, c, t));
                iPoints.Add(iPoint);
            }
        }



        /// <summary>
        /// Вычисление производной (методом конечных разностей соседних точек)
        /// </summary>
        /// <param name="octave"></param>
        /// <param name="interval"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns>Матрица 3x1</returns>
        private double[,] BuildDerivative(int r, int c, ResponseLayer t, ResponseLayer m, ResponseLayer b)
        {
            double dx, dy, ds;

            dx = (m.getResponse(r, c + 1, t) - m.getResponse(r, c - 1, t)) / 2f;
            dy = (m.getResponse(r + 1, c, t) - m.getResponse(r - 1, c, t)) / 2f;
            ds = (t.getResponse(r, c) - b.getResponse(r, c, t)) / 2f;

            double[,] D = { { dx }, { dy }, { ds } };
            return D;
        }



        /// <summary>
        /// Построить матрицу Гессе 
        /// </summary>
        /// <param name="octave"></param>
        /// <param name="interval"></param>r
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns>Матрица 3x3</returns>
        private double[,] BuildHessian(int r, int c, ResponseLayer t, ResponseLayer m, ResponseLayer b)
        {
            double v, dxx, dyy, dss, dxy, dxs, dys;

            v = m.getResponse(r, c, t);
            dxx = m.getResponse(r, c + 1, t) + m.getResponse(r, c - 1, t) - 2 * v;
            dyy = m.getResponse(r + 1, c, t) + m.getResponse(r - 1, c, t) - 2 * v;
            dss = t.getResponse(r, c) + b.getResponse(r, c, t) - 2 * v;
            dxy = (m.getResponse(r + 1, c + 1, t) - m.getResponse(r + 1, c - 1, t) -
                    m.getResponse(r - 1, c + 1, t) + m.getResponse(r - 1, c - 1, t)) / 4f;
            dxs = (t.getResponse(r, c + 1) - t.getResponse(r, c - 1) -
                    b.getResponse(r, c + 1, t) + b.getResponse(r, c - 1, t)) / 4f;
            dys = (t.getResponse(r + 1, c) - t.getResponse(r - 1, c) -
                    b.getResponse(r + 1, c, t) + b.getResponse(r - 1, c, t)) / 4f;

            double[,] H = new double[3, 3];
            H[0, 0] = dxx;
            H[0, 1] = dxy;
            H[0, 2] = dxs;
            H[1, 0] = dxy;
            H[1, 1] = dyy;
            H[1, 2] = dys;
            H[2, 0] = dxs;
            H[2, 1] = dys;
            H[2, 2] = dss;
            return H;
        }
    } 
}
