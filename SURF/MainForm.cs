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
        List<Bitmap> eyeImages_left_surfed = new List<Bitmap>();                    //После обработки SURF
        List<List<InterestPoint>> iPoints_left = new List<List<InterestPoint>>();   //Ключевые точки

        //Комплект снимков правого глаза
        List<Bitmap> eyeImages_right = new List<Bitmap>();                          //Исходники
        List<Bitmap> eyeImages_right_surfed = new List<Bitmap>();                   //После обработки SURF
        List<List<InterestPoint>> iPoints_right = new List<List<InterestPoint>>();  //Ключевые точки

        //Ключевые точки
        List<InterestPoint> iPoints = new List<InterestPoint>();

        //Несколько изображений и соответствующие им ключевые точки
        List<Bitmap> imageN = new List<Bitmap>();
        List<List<InterestPoint>> iPointsN = new List<List<InterestPoint>>();

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

            foreach (InterestPoint iPoint in iPoints)
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
            Int32 bestIPoint = 0;
            for (Int32 ip = 0; ip < iPointPairs.Count; ip++)
            {
                if (((iPointPairs.ElementAt(ip).p1.x - iPointPairs.ElementAt(ip).p2.x) > 10) || ((iPointPairs.ElementAt(ip).p1.x - iPointPairs.ElementAt(ip).p2.x) < -10))
                {
                    bestIPoint = ip;
                    break;
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
            // foreach(var ip in CurIpts)
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
            eyeImages_right_surfed = eyeImages_right;

            //Интегральное изображение
            IntegralImage intImage; 

            for (Int32 i = 0; i < 7; i++)
            {
                try
                {
                    //Загружаем изображение
                    loadInfo.Text = "Обработка изображения " + (i + 1).ToString();

                    if (bkgrFilter(eyeImages_right[i], Color.FromArgb(0, 0, 0), 30))
                    {
                        //Получение интегрального изображения
                        loadInfo.Text = "Получение интегрального изображения снимка " + (i + 1).ToString();
                        intImage = IntegralImage.FromImage(eyeImages_right[i]);

                        //Поиск ключевых точек
                        loadInfo.Text = "Поиск ключевых точек снимка " + (i + 1).ToString(); ;
                        iPoints_right.Add(FastHessian.getIpoints(0.0002f, 5, 2, intImage));
                        loadInfo.Text = "Поиск дескрипторов ключевых точек снимка " + (i + 1).ToString(); ;
                        SURFDescriptor.DecribeInterestPoints(iPoints_right.Last(), intImage);

                        //Отобразим результаты работы SURF
                        loadInfo.Text = "Применение результатов для " +(i + 1).ToString();
                        drawSURF(eyeImages_right_surfed[i], iPoints_right.Last());

                        loadInfo.Text = "";
                    }
                    else MessageBox.Show("Ошибка при обработке изображения " + (i + 1).ToString(), "Ошибка SURF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch
                {
                    MessageBox.Show("Ошибка при обработке изображения " + (i+1).ToString(), "Ошибка SURF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            this.Cursor = Cursors.Arrow;

            MessageBox.Show(CreatePairs(iPoints_right[0], iPoints_right[1]).Count.ToString());
        }

        #endregion

        #region Загрузка данных

        /// <summary>
        /// Загрузка одного изображения
        /// </summary>
        private void open_image()
        {
            //Диалог открытия файла
            OpenFileDialog openFD = new OpenFileDialog();
            if (openFD.ShowDialog() == DialogResult.OK)
            {
                string imgPath = openFD.FileName;
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    loadInfo.Text = "Загрузка изображения " + imgPath;
                    Bitmap image = new Bitmap(imgPath);

                    pictureImage.Image = image;
                    pictureImage.BackColor = Color.White;

                    loadInfo.Text = "Исходное изображение получено";

                    bkgrFilter(image, Color.FromArgb(1, 1, 1), 30);

                    loadInfo.Text = "Удален фон";

                    loadInfo.Text = "Получение интегрального изображения";

                    IntegralImage iImage = IntegralImage.FromImage(image);

                    loadInfo.Text = "Получение ключевых точек";
                    iPoints = FastHessian.getIpoints(0.0002f, 5, 2, iImage);
                    SURFDescriptor.DecribeInterestPoints(iPoints, iImage);

                    loadInfo.Text = "Отображение результатов работы SURF";
                    drawSURF(image, iPoints);

                    loadInfo.Text = "";

                }
                catch
                {
                    MessageBox.Show("Ошибка при обработке изображения", imgPath.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                this.Cursor = Cursors.Arrow;

                pictureImage.Refresh();
            }
        }

        /// <summary>
        /// Загрузка нескольких изображений и поиск ключевых точек для каждого изображения
        /// </summary>
        private void images_n_points()
        {
            //Диалог открытия файлов
            if (open_images.ShowDialog() == DialogResult.OK && open_images.FileNames.Count() > 0)
            {
                //Очистка списков изображений и соответствующих ключевых точек
                imageN.Clear();
                iPointsN.Clear();

                this.Cursor = Cursors.WaitCursor;

                Int32 imgCount = open_images.FileNames.Count(); //Количество изображений
                IntegralImage intImage; //Интегральное изображение

                for (Int32 i = 0; i < imgCount; i++)
                {
                    string imgPath = open_images.FileNames[i];
                    try
                    {
                        //Загружаем изображение
                        loadInfo.Text = "Загрузка изображения " + imgPath;

                        //Обрезка и добавление изображения
                        imageN.Add(CropImg(new Bitmap(imgPath), rect));

                        if (bkgrFilter(imageN.Last(), Color.FromArgb(0, 0, 0), 30))
                        {

                            //pictureBox.Image = imageN.Last();

                            //Получим интегральное изображение
                            loadInfo.Text = "Получение интегрального изображения";
                            intImage = IntegralImage.FromImage(imageN.Last());

                            //Найдем ключевые точки
                            loadInfo.Text = "Поиск ключевых точек";
                            iPointsN.Add(FastHessian.getIpoints(0.0002f, 5, 2, intImage));
                            loadInfo.Text = "Поиск дескрипторов ключевых точек";
                            SURFDescriptor.DecribeInterestPoints(iPointsN.Last(), intImage);

                            //Отобразим результаты работы SURF
                            loadInfo.Text = "Применение результатов для " + imgPath;
                            drawSURF(imageN.Last(), iPointsN.Last());

                            loadInfo.Text = "";
                        }
                        else MessageBox.Show("Ошибка при обработке изображения", imgPath.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при обработке изображения", imgPath.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                MessageBox.Show("Загрузка изображений завершена", "Загрузка изображений", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (imageN.Count > 1)
                {
                    iPointPairs = CreatePairs(iPointsN[0], iPointsN[1]);
                    if (iPointPairs.Count == 0)
                        MessageBox.Show("Нет общих ключевых точек!");
                    else
                    {
                        String pairs1 = "";
                        for (Int32 pi1 = 0; pi1 < iPointPairs.Count; pi1++)
                            pairs1 += "(" + iPointPairs.ElementAt(pi1).p1.x.ToString() + ";" + iPointPairs.ElementAt(pi1).p1.y.ToString() + ")  " +
                                            "(" + iPointPairs.ElementAt(pi1).p2.x.ToString() + ";" + iPointPairs.ElementAt(pi1).p2.y.ToString() + ")" + Environment.NewLine;


                        MessageBox.Show(pairs1, "Пары найденных ключевых точек"); //DEBUG

                        drawFull(imageN[0], imageN[1], bestIPoint_idSearch());
                    }
                }

                //  MessageBox.Show(InterestPoint.compareDescriptors(iPointsN[0][1], iPointsN[2][1], 1).ToString());

                this.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Загрузка комплекта снимков левого глаза
        /// </summary>
        private void loadLeftImages(object sender, EventArgs e)
        {
            //Диалог открытия файлов
            if (open_images.ShowDialog() == DialogResult.OK && open_images.FileNames.Count() == 7)
            {
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
            }
            else MessageBox.Show("Должны быть выбраны 7 снимков", "Ошибка загрузки изображений", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Загрузка комплекта снимков правого глаза
        /// </summary>
        private void loadRightImages(object sender, EventArgs e)
        {
            //Диалог открытия файлов
            if (open_images.ShowDialog() == DialogResult.OK && open_images.FileNames.Count() == 7)
            {
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

                //Метод SURF
                rightSURF();
            }
            else MessageBox.Show("Должны быть выбраны 7 снимков", "Ошибка загрузки изображений", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        

        /// <summary>
        /// Открыть (Ctrl+O)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void open_img_click(object sender, EventArgs e)
        {
            //open_image();
            images_n_points();
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
        /// Сшивка изображения
        /// </summary>
        private void drawFull(Bitmap image1, Bitmap image2, Int32 i)
        {
            Double x1 = iPointPairs.ElementAt(i).p1.x;
            Double y1 = iPointPairs.ElementAt(i).p1.y;
            Double x2 = iPointPairs.ElementAt(i).p2.x;
            Double y2 = iPointPairs.ElementAt(i).p2.y;
            Double xx = x1 - x2;
            Double yy = y1 - y2;

            MessageBox.Show(image1.Width.ToString() + "   " + image1.Height.ToString());

            Bitmap img = new Bitmap(image1.Width + (image2.Width - Convert.ToInt32(xx)), image1.Height + (image2.Height - Convert.ToInt32(yy)));

            Graphics g = Graphics.FromImage(img);
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            SolidBrush blck = new SolidBrush(Color.Black);

            Region fill = new Region(rect);
            g.FillRegion(blck, fill);

            g.DrawImage(image1, new Point(0, 0));
            g.DrawImage(image2, new Point(Convert.ToInt32(xx), Convert.ToInt32(yy)));


            //Brush tBrush = new TextureBrush(img, new Rectangle(0, 0, img.Width, img.Height));
            //Bitmap image = new Bitmap(img.Width, img.Height);

            // img.MakeTransparent(Color.FromArgb(1,1,1));


            pictureImage.Image = img;

            MessageBox.Show(xx.ToString() + "  " + yy.ToString());
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
        /// Отображение изображений в listBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void leftListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            int pos = e.Index;
            e.DrawBackground();
            e.Graphics.DrawImage((Image)leftListBox.Items[pos], new Point(5, e.Bounds.Top + 5));
            e.Graphics.DrawString(string.Format("Item #{0}", pos), new Font("Arial", 32.0f,
                FontStyle.Bold | FontStyle.Italic), Brushes.DarkOliveGreen, new Point(125, e.Bounds.Top + 5));
            e.DrawFocusRectangle();
        }


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
            pictureImage.Image = eyeImages_right_surfed[rightListBox.SelectedIndex];
        }

        /// <summary>
        /// Лупа
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
                    ((tempBitmap.GetPixel(halfWidth, halfHeight).R + tempBitmap.GetPixel(halfWidth, halfHeight).G + tempBitmap.GetPixel(halfWidth, halfHeight).B) / 3).ToString();

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

        /// <summary>
        /// Сохранить как (Ctrl+S)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_image_as(object sender, EventArgs e)
        {
            sia();
        }
       
        /// <summary>
        /// Сохранить как
        /// </summary>
        private void sia()
        {
            if (pictureImage.Image != null)
            {
                if (save_image.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        pictureImage.Image.Save(save_image.FileName, ImageFormat.Jpeg);
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при сохранении изображения", save_image.FileName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    this.Cursor = Cursors.Arrow;
                }
            }
            else MessageBox.Show("Нет изображения", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

    }
}
