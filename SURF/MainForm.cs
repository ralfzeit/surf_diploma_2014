using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using SURF.SURF;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Drawing.Drawing2D;

namespace SURF
{
    /// <summary>
    /// Главное окно приложения
    /// </summary>
    public partial class MainForm : Form
    {
        #region Описание используемых данных

        //Координаты вырезаемой области снимков
        Rectangle rect = new Rectangle(237, 105, 1920, 1615);

        //Комплект снимков левого глаза
        List<Bitmap> eyeImages_left = new List<Bitmap>();                           //Исходники
        List<List<InterestPoint>> iPoints_left = new List<List<InterestPoint>>();   //Ключевые точки
        List<Int32> avg_leftBrightness = new List<Int32>();                         //Средняя яркость снимков
        Int32 avg_Left;                                                             //Средняя яркость комплекта
        List<Int32> delta_leftBrightness = new List<Int32>();                       //Дельта яркости

        //Комплект снимков правого глаза
        List<Bitmap> eyeImages_right = new List<Bitmap>();                          //Исходники
        List<List<InterestPoint>> iPoints_right = new List<List<InterestPoint>>();  //Ключевые точки
        List<Int32> avg_rightBrightness = new List<Int32>();                        //Средняя яркость снимков
        Int32 avg_Right;                                                            //Средняя яркость комплекта
        List<Int32> delta_rightBrightness = new List<Int32>();                      //Дельта яркости

        //Пары ключевых точек 2х соседних изображений
        ConcurrentBag<InterestPointPair> iPointPairs = new ConcurrentBag<InterestPointPair>();

        #endregion

        #region SURF

        /// <summary>
        /// Отобразить результаты работы SURF
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="ipts">Ключевые точки</param>
        private void drawSURF(Bitmap image, List<InterestPoint> ipts)
        {
            //Установим перья
            Pen yellowP = new Pen(Color.Yellow);
            Pen blueP = new Pen(Color.Blue);
            Pen myPen;

            Graphics g = Graphics.FromImage(image);

            foreach (InterestPoint iPoint in ipts)
            {
                int S = 2 * Convert.ToInt32(2.5f * iPoint.scale);
                int R = Convert.ToInt32(S / 2f);

                Point pt = new Point(Convert.ToInt32(iPoint.x), Convert.ToInt32(iPoint.y));
                Point ptR = new Point(Convert.ToInt32(R * Math.Cos(iPoint.orientation)), Convert.ToInt32(R * Math.Sin(iPoint.orientation)));

                myPen = (iPoint.laplacian > 0 ? blueP : yellowP);

                g.DrawEllipse(myPen, pt.X - R, pt.Y - R, S, S);
                g.DrawLine(new Pen(Color.FromArgb(0, 255, 0)), new Point(pt.X, pt.Y), new Point(pt.X + ptR.X, pt.Y + ptR.Y));
            }
        }

        /// <summary>
        /// Поиск наиболее подходящей точки
        /// </summary>
        /// <returns>Индекс точки</returns>
        Int32 bestIPoint_idSearch()
        {
            Int32 bestIPoint = -1;
            for (Int32 ip = 0; ip < iPointPairs.Count; ip++)
            {
                if (((iPointPairs.ElementAt(ip).p1.x - iPointPairs.ElementAt(ip).p2.x) > 100) || ((iPointPairs.ElementAt(ip).p1.x - iPointPairs.ElementAt(ip).p2.x) < -100))
                {
                    bestIPoint = ip;
                    return bestIPoint;
                }
            }
            return bestIPoint;
        }

        /// <summary>
        /// Получает на вход два списка ключевых точек и составляет из них соответствующие пары путем сравнения дескрипторов
        /// </summary>
        /// <param name="CurIpts">Первый список</param>
        /// <param name="lastIpts">Второй список</param>
        /// <returns>Пары точек</returns>
        ConcurrentBag<InterestPointPair> CreatePairs(List<InterestPoint> CurIpts, List<InterestPoint> lastIpts)
        {
            ConcurrentBag<InterestPointPair> matched = new ConcurrentBag<InterestPointPair>();
            Parallel.ForEach(CurIpts, ip =>
            {
                InterestPoint neighbour = new InterestPoint();

                double dist1 = double.MaxValue;
                double dist2 = double.MaxValue;

                foreach (InterestPoint ip2 in lastIpts)
                {
                    if (ip.laplacian != ip2.laplacian) continue;
                    double d = InterestPoint.compareDescriptors(ip, ip2, dist2);
                    if (d < dist1)
                    {
                        dist2 = dist1;
                        dist1 = d;
                        neighbour = ip2;
                    }
                    else if (d < dist2)
                        dist2 = d;
                }

                if (dist1 < 0.2 * dist2)
                {
                    matched.Add(new InterestPointPair { p1 = ip, p2 = neighbour, dist = dist1 }); //Пары одинаковых ключевых точек из соседних кадров
                }
            });

            return matched;
        }

        /// <summary>
        /// Метод SURF для снимков левого глаза
        /// </summary>
        private void leftSURF()
        {
            //Курсор ожидания
            this.Cursor = Cursors.WaitCursor;

            //Очистка списка ключевых точек
            iPoints_left.Clear();
            for (Int32 i = 0; i < 7; i++)
                iPoints_left.Add(null);


            Parallel.For(0, 7, i =>
            {
                try
                {
                    //Загружаем изображение
                    loadInfo.Text = "Обработка изображения " + (i + 1).ToString();

                    if (bkgrFilter(eyeImages_left[i], Color.FromArgb(0, 0, 0), 30))
                    {
                        //Получение интегрального изображения
                        loadInfo.Text = "Получение интегрального изображения снимка " + (i + 1).ToString();
                        IntegralImage intImage = IntegralImage.FromImage(eyeImages_left[i]);

                        //Поиск ключевых точек
                        loadInfo.Text = "Поиск ключевых точек снимка " + (i + 1).ToString(); ;
                        iPoints_left[i] = FastHessian.getIpoints(0.0002f, 5, 2, intImage);
                        loadInfo.Text = "Поиск дескрипторов ключевых точек снимка " + (i + 1).ToString(); ;
                        SURFDescriptor.DecribeInterestPoints(iPoints_left[i], intImage);

                        //Отобразим результаты работы SURF
                        loadInfo.Text = "Применение результатов для " + (i + 1).ToString();
                        //drawSURF(eyeImages_left[i], iPoints_left[i]);

                        loadInfo.Text = "";
                    }
                    else MessageBox.Show("Ошибка при обработке изображения " + (i + 1).ToString(), "Ошибка SURF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Ошибка при обработке изображения " + (i + 1).ToString(), "Ошибка SURF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Метод SURF для снимков правого глаза
        /// </summary>
        private void rightSURF()
        {
            //Курсор ожидания
            this.Cursor = Cursors.WaitCursor;

            //Очистка списка ключевых точек
            iPoints_right.Clear();
            for (Int32 i = 0; i < 7; i++)
                iPoints_right.Add(null);

            Parallel.For(0, 7, i =>
            {
                try
                {
                    //Загружаем изображение
                    loadInfo.Text = "Обработка изображения " + (i + 1).ToString();

                    if (bkgrFilter(eyeImages_right[i], Color.FromArgb(0, 0, 0), 30))
                    {
                        //Получение интегрального изображения
                        loadInfo.Text = "Получение интегрального изображения снимка " + (i + 1).ToString();
                        //Интегральное изображение
                        IntegralImage intImage = IntegralImage.FromImage(eyeImages_right[i]);

                        //Поиск ключевых точек
                        loadInfo.Text = "Поиск ключевых точек снимка " + (i + 1).ToString(); ;
                        iPoints_right[i] = FastHessian.getIpoints(0.0002f, 5, 2, intImage);
                        loadInfo.Text = "Поиск дескрипторов ключевых точек снимка " + (i + 1).ToString(); ;
                        SURFDescriptor.DecribeInterestPoints(iPoints_right[i], intImage);

                        //Отобразим результаты работы SURF
                        loadInfo.Text = "Применение результатов для " + (i + 1).ToString();
                        //drawSURF(eyeImages_right[i], iPoints_right[i]);

                        loadInfo.Text = "";
                    }
                    else MessageBox.Show("Ошибка при обработке изображения " + (i + 1).ToString(), "Ошибка SURF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Ошибка при обработке изображения " + (i + 1).ToString(), "Ошибка SURF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });

            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Сшивка снимков левого глаза
        /// </summary>
        private void leftDRAWING()
        {
            List<Int32> x = new List<Int32>(7);
            List<Int32> y = new List<Int32>(7);
            Int32 iPoint_number;

            //Double x1 = 850, y1 = 750;

            //Расставляем снимки по схеме
            x.Add(1050); y.Add(1150);
            x.Add(2000); y.Add(1150);
            x.Add(2850); y.Add(1150);
            x.Add(2000); y.Add(400);
            x.Add(2000); y.Add(1900);
            x.Add(250);  y.Add(400);
            x.Add(200);  y.Add(1900);

            Int32 check = 0;

            //Сомещаем, если есть общие ключевые точки
            //-Для первых трех снимков
            for (Int32 i = 0; i < 3; i++)
            {
                iPointPairs = CreatePairs(iPoints_left[i], iPoints_left[i+1]);
                iPoint_number = bestIPoint_idSearch();

                if (iPoint_number != -1)
                {
                    Double x1 = iPointPairs.ElementAt(iPoint_number).p1.x - iPointPairs.ElementAt(iPoint_number).p2.x;
                    Double y1 = iPointPairs.ElementAt(iPoint_number).p1.y - iPointPairs.ElementAt(iPoint_number).p2.y;

                    x[i+1] = Convert.ToInt32(x[i] + x1);
                    y[i+1] = Convert.ToInt32(y[i] + y1);
                    check++;
                }
            }
            //-Для 4го и 5го снимков
            for (Int32 i = 3; i < 5; i++)
            {
                iPointPairs = CreatePairs(iPoints_left[1], iPoints_left[i]);
                iPoint_number = bestIPoint_idSearch();

                if (iPoint_number != -1)
                {
                    Double x1 = iPointPairs.ElementAt(iPoint_number).p1.x - iPointPairs.ElementAt(iPoint_number).p2.x;
                    Double y1 = iPointPairs.ElementAt(iPoint_number).p1.y - iPointPairs.ElementAt(iPoint_number).p2.y;

                    x[i] = Convert.ToInt32(x[1] + x1);
                    y[i] = Convert.ToInt32(y[1] + y1);
                    check++;
                }
            }
            //-Для 6го и 7го снимков
            for (Int32 i = 5; i < 7; i++)
            {
                iPointPairs = CreatePairs(iPoints_left[0], iPoints_left[i]);
                iPoint_number = bestIPoint_idSearch();

                if (iPoint_number != -1)
                {
                    Double x1 = iPointPairs.ElementAt(iPoint_number).p1.x - iPointPairs.ElementAt(iPoint_number).p2.x;
                    Double y1 = iPointPairs.ElementAt(iPoint_number).p1.y - iPointPairs.ElementAt(iPoint_number).p2.y;

                    x[i] = Convert.ToInt32(x[0] + x1);
                    y[i] = Convert.ToInt32(y[0] + y1);
                    check++;
                }
            }

            MessageBox.Show((7 - check).ToString() + " снимков из 7 будут расположены по предустановленным координатам", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            Bitmap img = new Bitmap(5000, 4000);

            Graphics g = Graphics.FromImage(img);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            SolidBrush blck = new SolidBrush(Color.Black);

            Region fill = new Region(rect);
            g.FillRegion(blck, fill);

            
            for(Int32 i = x.Count-1; i >= 0; i--)
                g.DrawImage(eyeImages_left[i], new Point(x[i], y[i]));
            /*
            for (Int32 i = 0; i < 2; i++)
                g.DrawImage(eyeImages_left[i], new Point(x[i], y[i]));
            */
            pictureImage.Image = img;

            eyeImages_left.Add(img);
            leftListBox.Items.Add("Результат сшивки");
        }

        /// <summary>
        /// Сшивка снимков правого глаза
        /// </summary>
        private void rightDRAWING()
        {
            List<Int32> x = new List<Int32>(7);
            List<Int32> y = new List<Int32>(7);
            Int32 iPoint_number;

            //Double x1 = 850, y1 = 750;

            //Расставляем снимки по схеме
            x.Add(1900); y.Add(1500);
            x.Add(1500); y.Add(1500);
            x.Add(500); y.Add(1350);
            x.Add(400); y.Add(450);
            x.Add(900); y.Add(2350);
            x.Add(2250); y.Add(150);
            x.Add(2850); y.Add(2650);

            Int32 check = 0;

            //Сомещаем, если есть общие ключевые точки
            //-Для первых трех снимков
            for (Int32 i = 0; i < 3; i++)
            {
                iPointPairs = CreatePairs(iPoints_right[i], iPoints_right[i + 1]);
                iPoint_number = bestIPoint_idSearch();

                if (iPoint_number != -1)
                {
                    Double x1 = iPointPairs.ElementAt(iPoint_number).p1.x - iPointPairs.ElementAt(iPoint_number).p2.x;
                    Double y1 = iPointPairs.ElementAt(iPoint_number).p1.y - iPointPairs.ElementAt(iPoint_number).p2.y;

                    x[i + 1] = Convert.ToInt32(x[i] + x1);
                    y[i + 1] = Convert.ToInt32(y[i] + y1);
                    check++;
                }
            }
            //-Для 4го и 5го снимков
            for (Int32 i = 3; i < 5; i++)
            {
                iPointPairs = CreatePairs(iPoints_right[1], iPoints_right[i]);
                iPoint_number = bestIPoint_idSearch();

                if (iPoint_number != -1)
                {
                    Double x1 = iPointPairs.ElementAt(iPoint_number).p1.x - iPointPairs.ElementAt(iPoint_number).p2.x;
                    Double y1 = iPointPairs.ElementAt(iPoint_number).p1.y - iPointPairs.ElementAt(iPoint_number).p2.y;

                    x[i] = Convert.ToInt32(x[1] + x1);
                    y[i] = Convert.ToInt32(y[1] + y1);
                    check++;
                }
            }
            //-Для 6го и 7го снимков
            for (Int32 i = 5; i < 7; i++)
            {
                iPointPairs = CreatePairs(iPoints_right[0], iPoints_right[i]);
                iPoint_number = bestIPoint_idSearch();

                if (iPoint_number != -1)
                {
                    Double x1 = iPointPairs.ElementAt(iPoint_number).p1.x - iPointPairs.ElementAt(iPoint_number).p2.x;
                    Double y1 = iPointPairs.ElementAt(iPoint_number).p1.y - iPointPairs.ElementAt(iPoint_number).p2.y;

                    x[i] = Convert.ToInt32(x[0] + x1);
                    y[i] = Convert.ToInt32(y[0] + y1);
                    check++;
                }
            }

            MessageBox.Show((7-check).ToString() + " снимков из 7 будут расположены по предустановленным координатам", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);


            Bitmap img = new Bitmap(5000, 4500);

            Graphics g = Graphics.FromImage(img);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            SolidBrush blck = new SolidBrush(Color.Black);

            Region fill = new Region(rect);
            g.FillRegion(blck, fill);

            /*
            for (Int32 i = x.Count - 1; i >= 0; i--)
                g.DrawImage(eyeImages_right[i], new Point(x[i], y[i]));
            */
            for (Int32 i = 0; i < x.Count; i++)
                g.DrawImage(eyeImages_right[i], new Point(x[i], y[i]));
            
            pictureImage.Image = img;

            eyeImages_right.Add(img);
            rightListBox.Items.Add("Результат сшивки");
        }

        /// <summary>
        /// Возможна ли сшивка снимков левого глаза
        /// </summary>
        /// <returns></returns>
        private bool check_leftDRAW()
        {
            if (CreatePairs(iPoints_left[0], iPoints_left[6]).Count == 0) return false;
            if (CreatePairs(iPoints_left[0], iPoints_left[5]).Count == 0) return false;
            if (CreatePairs(iPoints_left[0], iPoints_left[1]).Count == 0 && CreatePairs(iPoints_left[1], iPoints_left[2]).Count == 0) return false;

            if (CreatePairs(iPoints_left[3], iPoints_left[0]).Count == 0 && CreatePairs(iPoints_left[3], iPoints_left[1]).Count == 0 && CreatePairs(iPoints_left[3], iPoints_left[2]).Count == 0) return false;

            if (CreatePairs(iPoints_left[4], iPoints_left[0]).Count == 0 && CreatePairs(iPoints_left[4], iPoints_left[1]).Count == 0 && CreatePairs(iPoints_left[4], iPoints_left[2]).Count == 0) return false;
            
            return true;
        }

        /// <summary>
        /// Возможна ли сшивка снимков правого глаза
        /// </summary>
        /// <returns></returns>
        private bool check_rightDRAW()
        {
            if (CreatePairs(iPoints_right[0], iPoints_right[6]).Count == 0) return false;
            if (CreatePairs(iPoints_right[0], iPoints_right[5]).Count == 0) return false;
            if (CreatePairs(iPoints_right[0], iPoints_right[1]).Count == 0 && CreatePairs(iPoints_right[1], iPoints_right[2]).Count == 0) return false;
            if (CreatePairs(iPoints_right[3], iPoints_right[0]).Count == 0 && CreatePairs(iPoints_right[3], iPoints_right[1]).Count == 0 && CreatePairs(iPoints_right[3], iPoints_right[2]).Count == 0) return false;
            if (CreatePairs(iPoints_right[4], iPoints_right[0]).Count == 0 && CreatePairs(iPoints_right[4], iPoints_right[1]).Count == 0 && CreatePairs(iPoints_right[4], iPoints_right[2]).Count == 0) return false;

            return true;
        }

        #endregion

        #region Загрузка данных


        /// <summary>
        /// Загрузка комплекта снимков левого глаза
        /// </summary>
        private void loadLeftImages(object sender, EventArgs e)
        {
            //Загрузка настроек
            bool check_normalize = false;
            Double contrastK = 1.0;

            try
            {
                StreamReader sr = new StreamReader("settings.dat");

                contrastK = Convert.ToDouble(sr.ReadLine()) / 10;

                if (sr.ReadLine() == "1")
                    check_normalize = true;

                sr.Close();
            }
            catch { }


            //Диалог открытия файлов
            if (open_images.ShowDialog() == DialogResult.OK)
            {
                if (open_images.FileNames.Count() != 7)
                {
                    MessageBox.Show("Должны быть выбраны 7 снимков", "Ошибка загрузки изображений", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Очистка списка для хранения снимков
                eyeImages_left.Clear();
                leftListBox.Items.Clear();

                //Курсор ожидания
                this.Cursor = Cursors.WaitCursor;

                for (Int32 i = 0; i < 7; i++)
                {
                    string imgPath = open_images.FileNames[i];
                    try
                    {
                        //Загружаем изображение
                        loadInfo.Text = "Загрузка изображения " + imgPath;

                        //Обрезка и добавление изображения в список
                        eyeImages_left.Add(CropImg(new Bitmap(imgPath), rect));

                        if(imgPath.Length > 20) leftListBox.Items.Add("Снимок " + (i + 1).ToString() + "  (..." + imgPath.Substring(imgPath.Length - 20) + ")");
                        else leftListBox.Items.Add("Снимок " + (i + 1).ToString() + "  (" + imgPath + ")");
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при загрузке изображения " + imgPath.ToString(), "Ошибка загрузки изображений", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                //Восстанавливаем курсор
                this.Cursor = Cursors.Arrow;
                loadInfo.Text = "";

                //Вычисление средних яркостей снимков
                do_avg_leftBrightness();

                //Нормализация яркости
                if(check_normalize == true)
                    normalize_leftBrightness();

                //Установка контраста
                contrast_left(contrastK);

                //Метод SURF
                leftSURF();

                leftDRAWING();

                MessageBox.Show("Сшивка завершена", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                button_saveLeft.Enabled = true;
            }
        }

        /// <summary>
        /// Загрузка комплекта снимков правого глаза
        /// </summary>
        private void loadRightImages(object sender, EventArgs e)
        {
            //Загрузка настроек
            bool check_normalize = false;
            Double contrastK = 1.0;

            try
            {
                StreamReader sr = new StreamReader("settings.dat");

                contrastK = Convert.ToDouble(sr.ReadLine()) / 10;

                if (sr.ReadLine() == "1")
                    check_normalize = true;

                sr.Close();
            }
            catch { }


            //Диалог открытия файлов
            if (open_images.ShowDialog() == DialogResult.OK)
            {
                if (open_images.FileNames.Count() != 7)
                {
                    MessageBox.Show("Должны быть выбраны 7 снимков", "Ошибка загрузки изображений", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Очистка списка для хранения снимков
                eyeImages_right.Clear();
                rightListBox.Items.Clear();

                //Курсор ожидания
                this.Cursor = Cursors.WaitCursor;

                for (Int32 i = 0; i < 7; i++)
                {
                    string imgPath = open_images.FileNames[i];
                    try
                    {
                        //Загружаем изображение
                        loadInfo.Text = "Загрузка изображения " + imgPath;

                        //Обрезка и добавление изображения в список
                        eyeImages_right.Add(CropImg(new Bitmap(imgPath), rect));

                        if (imgPath.Length > 20) rightListBox.Items.Add("Снимок " + (i + 1).ToString() + "  (..." + imgPath.Substring(imgPath.Length - 20) + ")");
                        else rightListBox.Items.Add("Снимок " + (i + 1).ToString() + "  (" + imgPath + ")");
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при загрузке изображения " + imgPath.ToString(), "Ошибка загрузки изображений", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                //Восстанавливаем курсор
                this.Cursor = Cursors.Arrow;
                loadInfo.Text = "";

                //Вычисление средних яркостей снимков
                do_avg_rightBrightness();

                //Нормализация яркости
                if (check_normalize == true)
                    normalize_rightBrightness();

                //Установка контраста
                contrast_right(contrastK);

                //Метод SURF
                rightSURF();

                //Сшивка снимков
                rightDRAWING();

                MessageBox.Show("Сшивка завершена", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                button_saveRight.Enabled = true;
            }
        }
        

        #endregion

        #region Обработка изображений

        /// <summary>
        /// Удаление фона и артефактов вокруг снимка
        /// </summary>
        /// <param name="bmpRadarImage">Исходное изображение</param>
        /// <param name="clr">Удаляемый цвет</param>
        /// <param name="tolerance">Интервал цвета</param>
        /// <returns>Успешность выполнения функции</returns>
        private unsafe bool bkgrFilter(Bitmap bmpRadarImage, Color clr, int tolerance)
        {
            BitmapData bmData = null;
            try
            {
                bmData = bmpRadarImage.LockBits(new Rectangle(0, 0, bmpRadarImage.Width, bmpRadarImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                int w = bmData.Width;
                int h = bmData.Height;

                Parallel.For(0, h, y =>
                {
                    byte* p = (byte*)bmData.Scan0.ToPointer();
                    p += (y * bmData.Stride);

                    for (int x = 0; x < w; x++)
                    {
                        if (p[0] <= (byte)clr.B + tolerance &&
                            p[1] <= (byte)clr.G + tolerance &&
                            p[2] <= (byte)clr.R + tolerance &&
                            p[0] >= (byte)clr.B - tolerance &&
                            p[1] >= (byte)clr.G - tolerance &&
                            p[2] >= (byte)clr.R - tolerance)

                            p[0] = p[1] = p[2] = p[3] = (byte)0;
                        p += 4;
                    }
                });

                bmpRadarImage.UnlockBits(bmData);
            }
            catch
            {
                try
                {
                    bmpRadarImage.UnlockBits(bmData);
                }
                catch { }
                return false;
            }
            return true;
        }


        /// <summary>
        /// Нормализация яркости снимков левого глаза
        /// </summary>
        private unsafe void normalize_leftBrightness()
        {
            for (Int32 img = 0; img < 7; img++)
            {
                BitmapData bmData = null;
                try
                {
                    bmData = eyeImages_left[img].LockBits(new Rectangle(0, 0, eyeImages_left[img].Width, eyeImages_left[img].Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    int w = bmData.Width;
                    int h = bmData.Height;

                    for (int y = 0; y < h; y++)
                    {
                        byte* p = (byte*)bmData.Scan0.ToPointer();
                        p += (y * bmData.Stride);

                        for (int x = 0; x < w; x++)
                        {
                            int r = p[0], g = p[1], b = p[2];

                            if (delta_leftBrightness[img] >= 0)
                            {
                                r -= (byte)delta_leftBrightness[img];
                                g -= (byte)delta_leftBrightness[img];
                                b -= (byte)delta_leftBrightness[img];
                            }
                            else
                            {
                                r += (byte)(delta_leftBrightness[img] * (-1));
                                g += (byte)(delta_leftBrightness[img] * (-1));
                                b += (byte)(delta_leftBrightness[img] * (-1));
                            }

                            if (r < 0)   r = 0;
                            if (r > 255) r = 255;
                            if (g < 0)   g = 0;
                            if (g > 255) g = 255;
                            if (b < 0)   b = 0;
                            if (b > 255) b = 255;

                            p[0] = (byte)r;
                            p[1] = (byte)g;
                            p[2] = (byte)b;

                            p += 4;
                        }
                    };

                    eyeImages_left[img].UnlockBits(bmData);
                }
                catch
                {
                    try
                    {
                        eyeImages_left[img].UnlockBits(bmData);
                    }
                    catch { }
                }
            }
        }


        /// <summary>
        /// Изменение контраста снимков левого глаза
        /// </summary>
        /// <param name="k">Коэффициент контраста</param>
        private unsafe void contrast_left(Double k)
        {
            for (Int32 img = 0; img < 7; img++)
            {
                BitmapData bmData = null;
                try
                {
                    bmData = eyeImages_left[img].LockBits(new Rectangle(0, 0, eyeImages_left[img].Width, eyeImages_left[img].Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    int w = bmData.Width;
                    int h = bmData.Height;

                    for (int y = 0; y < h; y++)
                    {
                        byte* p = (byte*)bmData.Scan0.ToPointer();
                        p += (y * bmData.Stride);

                        for (int x = 0; x < w; x++)
                        {
                            double r = p[0], g = p[1], b = p[2];

                            r = (((r - avg_leftBrightness[img]) * k) + avg_leftBrightness[img]);
                            g = (((g - avg_leftBrightness[img]) * k) + avg_leftBrightness[img]);
                            b = (((b - avg_leftBrightness[img]) * k) + avg_leftBrightness[img]);


                            int iR = (int)r;
                            iR = iR > 255 ? 255 : iR;
                            iR = iR < 0 ? 0 : iR;
                            int iG = (int)g;
                            iG = iG > 255 ? 255 : iG;
                            iG = iG < 0 ? 0 : iG;
                            int iB = (int)b;
                            iB = iB > 255 ? 255 : iB;
                            iB = iB < 0 ? 0 : iB;

                            p[0] = (byte)iR;
                            p[1] = (byte)iG;
                            p[2] = (byte)iB;

                            p += 4;
                        }
                    };

                    eyeImages_left[img].UnlockBits(bmData);
                }
                catch
                {
                    try
                    {
                        eyeImages_left[img].UnlockBits(bmData);
                    }
                    catch { }
                }
            }
        }


        /// <summary>
        /// Изменение контраста снимков правого глаза
        /// </summary>
        /// <param name="k">Коэффициент контраста</param>
        private unsafe void contrast_right(Double k)
        {
            for (Int32 img = 0; img < 7; img++)
            {
                BitmapData bmData = null;
                try
                {
                    bmData = eyeImages_right[img].LockBits(new Rectangle(0, 0, eyeImages_right[img].Width, eyeImages_right[img].Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    int w = bmData.Width;
                    int h = bmData.Height;

                    for (int y = 0; y < h; y++)
                    {
                        byte* p = (byte*)bmData.Scan0.ToPointer();
                        p += (y * bmData.Stride);

                        for (int x = 0; x < w; x++)
                        {
                            double r = p[0], g = p[1], b = p[2];

                            r = (((r - avg_rightBrightness[img]) * k) + avg_rightBrightness[img]);
                            g = (((g - avg_rightBrightness[img]) * k) + avg_rightBrightness[img]);
                            b = (((b - avg_rightBrightness[img]) * k) + avg_rightBrightness[img]);


                            int iR = (int)r;
                            iR = iR > 255 ? 255 : iR;
                            iR = iR < 0 ? 0 : iR;
                            int iG = (int)g;
                            iG = iG > 255 ? 255 : iG;
                            iG = iG < 0 ? 0 : iG;
                            int iB = (int)b;
                            iB = iB > 255 ? 255 : iB;
                            iB = iB < 0 ? 0 : iB;

                            p[0] = (byte)iR;
                            p[1] = (byte)iG;
                            p[2] = (byte)iB;

                            p += 4;
                        }
                    };

                    eyeImages_right[img].UnlockBits(bmData);
                }
                catch
                {
                    try
                    {
                        eyeImages_right[img].UnlockBits(bmData);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Нормализация яркости снимков правого глаза
        /// </summary>
        private unsafe void normalize_rightBrightness()
        {
            for (Int32 img = 0; img < 7; img++)
            {
                BitmapData bmData = null;
                try
                {
                    bmData = eyeImages_right[img].LockBits(new Rectangle(0, 0, eyeImages_right[img].Width, eyeImages_right[img].Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    int w = bmData.Width;
                    int h = bmData.Height;

                    for (int y = 0; y < h; y++)
                    {
                        byte* p = (byte*)bmData.Scan0.ToPointer();
                        p += (y * bmData.Stride);

                        for (int x = 0; x < w; x++)
                        {
                            int r = p[0], g = p[1], b = p[2];

                            if (delta_leftBrightness[img] >= 0)
                            {
                                r -= (byte)delta_leftBrightness[img];
                                g -= (byte)delta_leftBrightness[img];
                                b -= (byte)delta_leftBrightness[img];
                            }
                            else
                            {
                                r += (byte)(delta_leftBrightness[img] * (-1));
                                g += (byte)(delta_leftBrightness[img] * (-1));
                                b += (byte)(delta_leftBrightness[img] * (-1));
                            }

                            if (r < 0) r = 0;
                            if (r > 255) r = 255;
                            if (g < 0) g = 0;
                            if (g > 255) g = 255;
                            if (b < 0) b = 0;
                            if (b > 255) b = 255;

                            p[0] = (byte)r;
                            p[1] = (byte)g;
                            p[2] = (byte)b;

                            p += 4;
                        }
                    };

                    eyeImages_right[img].UnlockBits(bmData);
                }
                catch
                {
                    try
                    {
                        eyeImages_right[img].UnlockBits(bmData);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Вычисление средней яркости каждого снимка левого глаза
        /// </summary>
        private unsafe void do_avg_leftBrightness()
        {
            for(Int32 img = 0; img < 7; img++)
            {
                Int32 brig = 0;
                BitmapData bmData = null;
                try
                {
                    bmData = eyeImages_left[img].LockBits(new Rectangle(0, 0, eyeImages_left[img].Width, eyeImages_left[img].Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    int w = bmData.Width;
                    int h = bmData.Height;

                    for(int y = 0; y < h; y++)
                    {
                        byte* p = (byte*)bmData.Scan0.ToPointer();
                        p += (y * bmData.Stride);

                        for (int x = 0; x < w; x++)
                        {
                            brig += Convert.ToInt32(0.3 * p[0] + 0.59 * p[1] + 0.11 * p[2]);
                            p += 4;
                        }
                    };

                    eyeImages_left[img].UnlockBits(bmData);
                }
                catch
                {
                    try
                    {
                        eyeImages_left[img].UnlockBits(bmData);
                    }
                    catch {}
                }
                avg_leftBrightness.Add(brig / (eyeImages_left[img].Height * eyeImages_left[img].Width));
            }

            //Вычисление средней яркости левого комплекта
            avg_Left = avg_leftBrightness.Sum() / 7;

            //Вычисление дельт яркости
            for (Int32 i = 0; i < 7; i++)
                delta_leftBrightness.Add(avg_leftBrightness[i] - avg_Left);

        }


        /// <summary>
        /// Вычисление средней яркости каждого снимка правого глаза
        /// </summary>
        private unsafe void do_avg_rightBrightness()
        {
            for (Int32 img = 0; img < 7; img++)
            {
                Int32 brig = 0;
                BitmapData bmData = null;
                try
                {
                    bmData = eyeImages_right[img].LockBits(new Rectangle(0, 0, eyeImages_right[img].Width, eyeImages_right[img].Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    int w = bmData.Width;
                    int h = bmData.Height;

                    for (int y = 0; y < h; y++)
                    {
                        byte* p = (byte*)bmData.Scan0.ToPointer();
                        p += (y * bmData.Stride);

                        for (int x = 0; x < w; x++)
                        {
                            brig += Convert.ToInt32(0.3 * p[0] + 0.59 * p[1] + 0.11 * p[2]);
                            p += 4;
                        }
                    };

                    eyeImages_right[img].UnlockBits(bmData);
                }
                catch
                {
                    try
                    {
                        eyeImages_right[img].UnlockBits(bmData);
                    }
                    catch { }
                }
                avg_rightBrightness.Add(brig / (eyeImages_right[img].Height * eyeImages_right[img].Width));
            }

            //Вычисление средней яркости правого комплекта
            avg_Right = avg_rightBrightness.Sum() / 7;

            //Вычисление дельт яркости
            for (Int32 i = 0; i < 7; i++)
                delta_rightBrightness.Add(avg_rightBrightness[i] - avg_Right);

        }

        /// <summary>
        /// Обрезка изображения
        /// </summary>
        /// <param name="srcBitmap">Исходное изображение</param>
        /// <param name="r">Область обрезки</param>
        /// <returns>Обрезанное изображение</returns>
        private Bitmap CropImg(Bitmap srcBitmap, Rectangle r)
        {
            // Вырезаем выбранный кусок картинки
            Bitmap bmp11 = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(bmp11))
            {
                g.DrawImage(srcBitmap, 0, 0, r, GraphicsUnit.Pixel);
            }
            //Возвращаем кусок картинки.
            return bmp11;
        }

        #endregion

        #region Методы для управления отображением изображения

        /// <summary>
        /// Отобразить выбранное изображение левого глаза
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewLeftImage(object sender, EventArgs e)
        {
            pictureImage.Image = eyeImages_left[leftListBox.SelectedIndex];
        }

        /// <summary>
        /// Отобразить выбранное изображение правого глаза
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewRightImage(object sender, EventArgs e)
        {
            pictureImage.Image = eyeImages_right[rightListBox.SelectedIndex];
        }

        /// <summary>
        /// Подогнать изображение под размеры окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picture_fit(object sender, EventArgs e)
        {
            fitMenuItem.Checked = true;
            sourceMenuItem.Checked = false;
            panel1.VerticalScroll.Value = 0;
            panel1.HorizontalScroll.Value = 0;
            this.pictureImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureImage.Width = this.panel1.Width;
            this.pictureImage.Height = this.panel1.Height;
            pixelInfo.Visible = false;
        }

        /// <summary>
        /// Исходный размер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picture_source(object sender, EventArgs e)
        {
            fitMenuItem.Checked = false;
            sourceMenuItem.Checked = true;
            this.pictureImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            pixelInfo.Visible = true;
        }

        /// <summary>
        /// Исправить отображение при изменении размеров окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void windowsSizeChanged(object sender, EventArgs e)
        {
            this.pictureImage.Width = this.panel1.Width;
            this.pictureImage.Height = this.panel1.Height;
        }


        /// <summary>
        /// Отображение информации о пикселе изображения под курсором мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateZoomedImage(object sender, MouseEventArgs e)
        {
            if (pictureImage.Image != null && pixelInfo.Visible == true)
            {

                int zoomWidth = 10;
                int zoomHeight = 10;


                //Вычисляем центр

                int halfWidth = 5;
                int halfHeight = 5;


                Bitmap tempBitmap = new Bitmap(zoomWidth, zoomHeight,
                                               PixelFormat.Format24bppRgb);
                Graphics bmGraphics = Graphics.FromImage(tempBitmap);


                bmGraphics.Clear(Color.Black);

                bmGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                bmGraphics.DrawImage(pictureImage.Image,
                                     new Rectangle(0, 0, zoomWidth, zoomHeight),
                                     new Rectangle(e.X - halfWidth, e.Y - halfHeight,
                                     zoomWidth, zoomHeight), GraphicsUnit.Pixel);

                //Отобразим информацию о пикселе под курсором

                pixelInfo.Text = "R: " + tempBitmap.GetPixel(halfWidth, halfHeight).R.ToString() +
                            "  G: " + tempBitmap.GetPixel(halfWidth, halfHeight).G.ToString() +
                            "  B: " + tempBitmap.GetPixel(halfWidth, halfHeight).B.ToString() + "  Brightness: " +
                            (Convert.ToInt32(tempBitmap.GetPixel(halfWidth, halfHeight).R * 0.3 + 
                            tempBitmap.GetPixel(halfWidth, halfHeight).G * 0.59 + 
                            tempBitmap.GetPixel(halfWidth, halfHeight).B * 0.11)).ToString();

                //Рисуем крест на увеличенном изображении

                bmGraphics.DrawLine(Pens.Black, halfWidth + 1,
                                    halfHeight - 4, halfWidth + 1, halfHeight - 1);
                bmGraphics.DrawLine(Pens.Black, halfWidth + 1, halfHeight + 6,
                                    halfWidth + 1, halfHeight + 3);
                bmGraphics.DrawLine(Pens.Black, halfWidth - 4, halfHeight + 1,
                                    halfWidth - 1, halfHeight + 1);
                bmGraphics.DrawLine(Pens.Black, halfWidth + 6, halfHeight + 1,
                                    halfWidth + 3, halfHeight + 1);


                bmGraphics.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Конструктор главной формы
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Закрыть приложение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void close_app(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Сохранение результатов

        /// <summary>
        /// Сохранить результаты обработки правого глаза
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveRight(object sender, EventArgs e)
        {
            /*
            Parallel.For(0, 7, img =>
            {
                eyeImages_right[img].Save("left_" + (img + 1).ToString() + ".png", ImageFormat.Png);
            });
             */

            if(save_image.ShowDialog() == DialogResult.OK)
                eyeImages_right.Last().Save(save_image.FileName, ImageFormat.Jpeg);
        }

        /// <summary>
        /// Сохранить результаты обработки левого глаза
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveLeft(object sender, EventArgs e)
        {
            /*
            Parallel.For(0, 7, img =>
            {
                eyeImages_left[img].Save("left_" + (img + 1).ToString() + ".png", ImageFormat.Png);
            });
            */

            if (save_image.ShowDialog() == DialogResult.OK)
                eyeImages_left.Last().Save(save_image.FileName, ImageFormat.Jpeg);
        }

        #endregion

        /// <summary>
        /// Настройки препарирования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void open_settings(object sender, EventArgs e)
        {
            SettingsForm sf = new SettingsForm();
            sf.ShowDialog();
        }
    


    }
}
