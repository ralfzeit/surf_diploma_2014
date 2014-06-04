namespace SURF
{
    partial class MainForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pictureImage = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.фАЙЛToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.открытьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filenameLabel = new System.Windows.Forms.ToolStripMenuItem();
            this.open_images = new System.Windows.Forms.OpenFileDialog();
            this.save_image = new System.Windows.Forms.SaveFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.pictureZoom = new System.Windows.Forms.PictureBox();
            this.labelR = new System.Windows.Forms.Label();
            this.labelG = new System.Windows.Forms.Label();
            this.labelB = new System.Windows.Forms.Label();
            this.labelBrightness = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureImage)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureImage
            // 
            this.pictureImage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureImage.BackColor = System.Drawing.Color.Black;
            this.pictureImage.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureImage.Location = new System.Drawing.Point(99, 24);
            this.pictureImage.Name = "pictureImage";
            this.pictureImage.Size = new System.Drawing.Size(724, 536);
            this.pictureImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureImage.TabIndex = 0;
            this.pictureImage.TabStop = false;
            this.pictureImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateZoomedImage);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.фАЙЛToolStripMenuItem,
            this.filenameLabel});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1008, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // фАЙЛToolStripMenuItem
            // 
            this.фАЙЛToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.открытьToolStripMenuItem,
            this.сToolStripMenuItem,
            this.toolStripSeparator1,
            this.выходToolStripMenuItem});
            this.фАЙЛToolStripMenuItem.Name = "фАЙЛToolStripMenuItem";
            this.фАЙЛToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.фАЙЛToolStripMenuItem.Text = "ФАЙЛ";
            // 
            // открытьToolStripMenuItem
            // 
            this.открытьToolStripMenuItem.Name = "открытьToolStripMenuItem";
            this.открытьToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.открытьToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.открытьToolStripMenuItem.Text = "Открыть...";
            this.открытьToolStripMenuItem.Click += new System.EventHandler(this.open_img_click);
            // 
            // сToolStripMenuItem
            // 
            this.сToolStripMenuItem.Name = "сToolStripMenuItem";
            this.сToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.сToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.сToolStripMenuItem.Text = "Сохранить как...";
            this.сToolStripMenuItem.Click += new System.EventHandler(this.save_image_as);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(199, 6);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S)));
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.close_app);
            // 
            // filenameLabel
            // 
            this.filenameLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.filenameLabel.Enabled = false;
            this.filenameLabel.Name = "filenameLabel";
            this.filenameLabel.Size = new System.Drawing.Size(12, 20);
            this.filenameLabel.Click += new System.EventHandler(this.openDirectory);
            // 
            // open_images
            // 
            this.open_images.Filter = "JPG|*.jpg|BMP|*.bmp|GIF|*.gif";
            this.open_images.Multiselect = true;
            // 
            // save_image
            // 
            this.save_image.DefaultExt = "jpg";
            this.save_image.Filter = "JPG|*.jpg";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Левый глаз";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 57);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(86, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Правый глаз";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // pictureZoom
            // 
            this.pictureZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureZoom.Location = new System.Drawing.Point(832, 24);
            this.pictureZoom.Name = "pictureZoom";
            this.pictureZoom.Size = new System.Drawing.Size(167, 163);
            this.pictureZoom.TabIndex = 4;
            this.pictureZoom.TabStop = false;
            // 
            // labelR
            // 
            this.labelR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelR.AutoSize = true;
            this.labelR.Location = new System.Drawing.Point(842, 193);
            this.labelR.Name = "labelR";
            this.labelR.Size = new System.Drawing.Size(21, 13);
            this.labelR.TabIndex = 5;
            this.labelR.Text = "R: ";
            // 
            // labelG
            // 
            this.labelG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelG.AutoSize = true;
            this.labelG.Location = new System.Drawing.Point(842, 209);
            this.labelG.Name = "labelG";
            this.labelG.Size = new System.Drawing.Size(21, 13);
            this.labelG.TabIndex = 6;
            this.labelG.Text = "G: ";
            // 
            // labelB
            // 
            this.labelB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelB.AutoSize = true;
            this.labelB.Location = new System.Drawing.Point(842, 225);
            this.labelB.Name = "labelB";
            this.labelB.Size = new System.Drawing.Size(20, 13);
            this.labelB.TabIndex = 7;
            this.labelB.Text = "B: ";
            // 
            // labelBrightness
            // 
            this.labelBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBrightness.AutoSize = true;
            this.labelBrightness.Location = new System.Drawing.Point(842, 241);
            this.labelBrightness.Name = "labelBrightness";
            this.labelBrightness.Size = new System.Drawing.Size(62, 13);
            this.labelBrightness.TabIndex = 8;
            this.labelBrightness.Text = "Brightness: ";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(1008, 561);
            this.Controls.Add(this.labelBrightness);
            this.Controls.Add(this.labelB);
            this.Controls.Add(this.labelG);
            this.Controls.Add(this.labelR);
            this.Controls.Add(this.pictureZoom);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureImage);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Поиск ключевых точек методом SURF";
            ((System.ComponentModel.ISupportInitialize)(this.pictureImage)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureZoom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureImage;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem фАЙЛToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem открытьToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filenameLabel;
        private System.Windows.Forms.OpenFileDialog open_images;
        private System.Windows.Forms.ToolStripMenuItem сToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog save_image;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureZoom;
        private System.Windows.Forms.Label labelR;
        private System.Windows.Forms.Label labelG;
        private System.Windows.Forms.Label labelB;
        private System.Windows.Forms.Label labelBrightness;
    }
}

