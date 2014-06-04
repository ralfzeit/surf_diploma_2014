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
    /// Окно приложения
    /// </summary>
    public partial class MainForm : Form
    {
        //Координаты необходимой области
        Rectangle rec = new Rectangle(238, 105, 1675, 1320);

        //Ключевые точки
        List<InterestPoint> iPoints = new List<InterestPoint>();

        //Несколько изображений и соответствующие им ключевые точки
        List<Bitmap> imageN = new List<Bitmap>();
        List<List<InterestPoint>> iPointsN = new List<List<InterestPoint>>();

        //Пары ключевых точек 2х соседних изображений
        ConcurrentBag<InterestPointPair> iPointPairs = new ConcurrentBag<InterestPointPair>();

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Отобразить результаты работы SURF
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="ipts">Ключевые точки</param>
        private void drawSURF(Bitmap image, List<InterestPoint> ipts)
        {
            //Установим перья
            Pen yellowP = new Pen(Color.Yellow);
            Pen blueP   = new Pen(Color.Blue);
            Pen myPen;

            Graphics g = Graphics.FromImage(image);

            foreach (InterestPoint iPoint in iPoints)
            {
                int S = 2 * Convert.ToInt32(2.5f * iPoint.scale);
                int R = Convert.ToInt32(S / 2f);

                Point pt  = new Point(Convert.ToInt32(iPoint.x), Convert.ToInt32(iPoint.y));
                Point ptR = new Point(Convert.ToInt32(R * Math.Cos(iPoint.orientation)), Convert.ToInt32(R * Math.Sin(iPoint.orientation)));

                myPen = (iPoint.laplacian > 0 ? blueP : yellowP);

                g.DrawEllipse(myPen, pt.X - R, pt.Y - R, S, S);
                g.DrawLine(new Pen(Color.FromArgb(0, 255, 0)), new Point(pt.X, pt.Y), new Point(pt.X + ptR.X, pt.Y + ptR.Y));
            }
        }

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
                filenameLabel.Text = imgPath.ToString();
                

                this.Cursor = Cursors.WaitCursor;

                try
                {
                    //Загружаем изображение
                    Bitmap image = new Bitmap(imgPath);

                    pictureImage.Image = image;
                    pictureImage.BackColor = Color.White;

                    MessageBox.Show("Исходник");

                    bkgrFilter(image, Color.FromArgb(1, 1, 1), 30);
                    MessageBox.Show("Прозрачный фон");
                    
                    //Получим интегральное изображение
                    IntegralImage iImage = IntegralImage.FromImage(image);
                    
                    //Получим ключевые точки
                    iPoints = FastHessian.getIpoints(0.0002f, 5, 2, iImage);
                    SURFDescriptor.DecribeInterestPoints(iPoints, iImage);

                    //Отобразим результаты работы SURF
                    drawSURF(image, iPoints);
  
                }
                catch
                {
                    MessageBox.Show("Ошибка при обработке изображения", imgPath.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                this.Cursor = Cursors.Arrow;
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
                        filenameLabel.Text = "Загрузка изображения " + imgPath;

                        //Обрезка и добавление изображения
                        Rectangle rect1 = new Rectangle(237, 105, 1920, 1615);
                        imageN.Add(CropImg(new Bitmap(imgPath), rect1));
                        
                        if (bkgrFilter(imageN.Last(), Color.FromArgb(0, 0, 0), 30))
                        {

                            //pictureBox.Image = imageN.Last();

                            //Получим интегральное изображение
                            filenameLabel.Text = "Получение интегрального изображения";
                            intImage = IntegralImage.FromImage(imageN.Last());

                            //Найдем ключевые точки
                            filenameLabel.Text = "Поиск ключевых точек";
                            iPointsN.Add(FastHessian.getIpoints(0.0002f, 5, 2, intImage));
                            filenameLabel.Text = "Поиск дескрипторов ключевых точек";
                            SURFDescriptor.DecribeInterestPoints(iPointsN.Last(), intImage);

                            //Отобразим результаты работы SURF
                            filenameLabel.Text = "Применение результатов для " + imgPath;
                            drawSURF(imageN.Last(), iPointsN.Last());

                            filenameLabel.Text = "";
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
                        
                        
                        MessageBox.Show(pairs1,"Пары найденных ключевых точек"); //DEBUG
                        
                        drawFull(imageN[0], imageN[1], bestIPoint_idSearch());
                    }
                }

                //  MessageBox.Show(InterestPoint.compareDescriptors(iPointsN[0][1], iPointsN[2][1], 1).ToString());

                this.Cursor = Cursors.Arrow;
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
                catch {}
                return false;
            }
            return true;
        }

        /// <summary>
        /// Склейка изображения
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
            
            MessageBox.Show(xx.ToString()+"  "+yy.ToString());
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

        /// <summary>
        /// Обрезка изображения
        /// </summary>
        /// <param name="srcBitmap">Исхожное изображение</param>
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
        /// Открытие изображения в редакторе по умолчанию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openDirectory(object sender, EventArgs e)
        {
            if(filenameLabel.Text!="") openDir(filenameLabel.Text);
        }
        
        /// <summary>
        /// Открытие изображения в редакторе по умолчанию
        /// </summary>
        /// <param name="dir"></param>
        private void openDir(String dir)
        {
            Process.Start(dir);
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

                if (dist1 < 0.3 * dist2)
                {
                    matched.Add(new InterestPointPair { p1 = ip, p2 = neighbour, dist = dist1 }); //Пары одинаковых ключевых точек из соседних кадров
                }
            });

            return matched;
        }

        /*
        private void ResizeAndDisplayImage(Image _OriginalImage)
        {
            // Set the backcolor of the pictureboxes

            pictureImage.BackColor = Color.Black;
            pictureZoom.BackColor = Color.Black;

            // If _OriginalImage is null, then return. This situation can occur

            // when a new backcolor is selected without an image loaded.

            if (_OriginalImage == null)
                return;

            // sourceWidth and sourceHeight store
            // the original image's width and height

            // targetWidth and targetHeight are calculated
            // to fit into the picImage picturebox.

            int sourceWidth = _OriginalImage.Width;
            int sourceHeight = _OriginalImage.Height;
            int targetWidth;
            int targetHeight;
            double ratio;

            // Calculate targetWidth and targetHeight, so that the image will fit into

            // the picImage picturebox without changing the proportions of the image.

            if (sourceWidth > sourceHeight)
            {
                // Set the new width

                targetWidth = pictureImage.Width;
                // Calculate the ratio of the new width against the original width

                ratio = (double)targetWidth / sourceWidth;
                // Calculate a new height that is in proportion with the original image

                targetHeight = (int)(ratio * sourceHeight);
            }
            else if (sourceWidth < sourceHeight)
            {
                // Set the new height

                targetHeight = pictureImage.Height;
                // Calculate the ratio of the new height against the original height

                ratio = (double)targetHeight / sourceHeight;
                // Calculate a new width that is in proportion with the original image

                targetWidth = (int)(ratio * sourceWidth);
            }
            else
            {
                // In this case, the image is square and resizing is easy

                targetHeight = pictureImage.Height;
                targetWidth = pictureImage.Width;
            }

            // Calculate the targetTop and targetLeft values, to center the image

            // horizontally or vertically if needed

            int targetTop = (pictureImage.Height - targetHeight) / 2;
            int targetLeft = (pictureImage.Width - targetWidth) / 2;

            // Create a new temporary bitmap to resize the original image

            // The size of this bitmap is the size of the picImage picturebox.

            Bitmap tempBitmap = new Bitmap(pictureImage.Width, pictureImage.Height,
                                           PixelFormat.Format24bppRgb);

            // Set the resolution of the bitmap to match the original resolution.

            tempBitmap.SetResolution(_OriginalImage.HorizontalResolution,
                                     _OriginalImage.VerticalResolution);

            // Create a Graphics object to further edit the temporary bitmap

            Graphics bmGraphics = Graphics.FromImage(tempBitmap);

            // First clear the image with the current backcolor

            bmGraphics.Clear(Color.Black);

            // Set the interpolationmode since we are resizing an image here

            bmGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw the original image on the temporary bitmap, resizing it using

            // the calculated values of targetWidth and targetHeight.

            bmGraphics.DrawImage(_OriginalImage,
                                 new Rectangle(targetLeft, targetTop, targetWidth, targetHeight),
                                 new Rectangle(0, 0, sourceWidth, sourceHeight),
                                 GraphicsUnit.Pixel);

            // Dispose of the bmGraphics object

            bmGraphics.Dispose();

            // Set the image of the picImage picturebox to the temporary bitmap

            pictureImage.Image = tempBitmap;
        }
        */
        
        /// <summary>
        /// Лупа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateZoomedImage(object sender, MouseEventArgs e)
        {
            if (pictureImage.Image != null)
            {

                int _ZoomFactor = 1;

                int zoomWidth = pictureZoom.Width / _ZoomFactor;
                int zoomHeight = pictureZoom.Height / _ZoomFactor;

                // Calculate the horizontal and vertical midpoints for the crosshair

                // cursor and correct centering of the new image

                int halfWidth = zoomWidth / 2;
                int halfHeight = zoomHeight / 2;

                // Create a new temporary bitmap to fit inside the picZoom picturebox

                Bitmap tempBitmap = new Bitmap(zoomWidth, zoomHeight,
                                               PixelFormat.Format24bppRgb);

                // Create a temporary Graphics object to work on the bitmap

                Graphics bmGraphics = Graphics.FromImage(tempBitmap);

                // Clear the bitmap with the selected backcolor

                bmGraphics.Clear(Color.Black);

                // Set the interpolation mode

                bmGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Draw the portion of the main image onto the bitmap

                // The target rectangle is already known now.

                // Here the mouse position of the cursor on the main image is used to

                // cut out a portion of the main image.

                bmGraphics.DrawImage(pictureImage.Image,
                                     new Rectangle(0, 0, zoomWidth, zoomHeight),
                                     new Rectangle(e.X - halfWidth, e.Y - halfHeight,
                                     zoomWidth, zoomHeight), GraphicsUnit.Pixel);

                // Draw the bitmap on the picZoom picturebox

                pictureZoom.Image = tempBitmap;

                // Draw a crosshair on the bitmap to simulate the cursor position

                bmGraphics.DrawLine(Pens.Black, halfWidth + 1,
                                    halfHeight - 4, halfWidth + 1, halfHeight - 1);
                bmGraphics.DrawLine(Pens.Black, halfWidth + 1, halfHeight + 6,
                                    halfWidth + 1, halfHeight + 3);
                bmGraphics.DrawLine(Pens.Black, halfWidth - 4, halfHeight + 1,
                                    halfWidth - 1, halfHeight + 1);
                bmGraphics.DrawLine(Pens.Black, halfWidth + 6, halfHeight + 1,
                                    halfWidth + 3, halfHeight + 1);

                // Dispose of the Graphics object

                bmGraphics.Dispose();

                // Refresh the picZoom picturebox to reflect the changes

                pictureZoom.Refresh();
            }
        }

    }
}
