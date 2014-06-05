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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.фАЙЛToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.выходToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.оТОБРАЖЕНИЕToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sourceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open_images = new System.Windows.Forms.OpenFileDialog();
            this.save_image = new System.Windows.Forms.SaveFileDialog();
            this.pictureImage = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.pixelInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.loadInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.leftTab = new System.Windows.Forms.TabPage();
            this.button_openLeft = new System.Windows.Forms.Button();
            this.button_saveLeft = new System.Windows.Forms.Button();
            this.leftListBox = new System.Windows.Forms.ListBox();
            this.rightTab = new System.Windows.Forms.TabPage();
            this.button_openRight = new System.Windows.Forms.Button();
            this.button_saveRight = new System.Windows.Forms.Button();
            this.rightListBox = new System.Windows.Forms.ListBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureImage)).BeginInit();
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.leftTab.SuspendLayout();
            this.rightTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.фАЙЛToolStripMenuItem,
            this.оТОБРАЖЕНИЕToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(884, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // фАЙЛToolStripMenuItem
            // 
            this.фАЙЛToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.выходToolStripMenuItem});
            this.фАЙЛToolStripMenuItem.Name = "фАЙЛToolStripMenuItem";
            this.фАЙЛToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.фАЙЛToolStripMenuItem.Text = "ФАЙЛ";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // выходToolStripMenuItem
            // 
            this.выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            this.выходToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.S)));
            this.выходToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.выходToolStripMenuItem.Text = "Выход";
            this.выходToolStripMenuItem.Click += new System.EventHandler(this.close_app);
            // 
            // оТОБРАЖЕНИЕToolStripMenuItem
            // 
            this.оТОБРАЖЕНИЕToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.оТОБРАЖЕНИЕToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fitMenuItem,
            this.sourceMenuItem});
            this.оТОБРАЖЕНИЕToolStripMenuItem.Name = "оТОБРАЖЕНИЕToolStripMenuItem";
            this.оТОБРАЖЕНИЕToolStripMenuItem.Size = new System.Drawing.Size(107, 20);
            this.оТОБРАЖЕНИЕToolStripMenuItem.Text = "ОТОБРАЖЕНИЕ";
            // 
            // fitMenuItem
            // 
            this.fitMenuItem.Checked = true;
            this.fitMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fitMenuItem.Name = "fitMenuItem";
            this.fitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D1)));
            this.fitMenuItem.Size = new System.Drawing.Size(276, 22);
            this.fitMenuItem.Text = "Подогнать под размеры окна";
            this.fitMenuItem.Click += new System.EventHandler(this.picture_fit);
            // 
            // sourceMenuItem
            // 
            this.sourceMenuItem.Name = "sourceMenuItem";
            this.sourceMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D2)));
            this.sourceMenuItem.Size = new System.Drawing.Size(276, 22);
            this.sourceMenuItem.Text = "Исходный размер";
            this.sourceMenuItem.Click += new System.EventHandler(this.picture_source);
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
            // pictureImage
            // 
            this.pictureImage.BackColor = System.Drawing.Color.Transparent;
            this.pictureImage.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pictureImage.Location = new System.Drawing.Point(0, 0);
            this.pictureImage.Name = "pictureImage";
            this.pictureImage.Size = new System.Drawing.Size(652, 512);
            this.pictureImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureImage.TabIndex = 0;
            this.pictureImage.TabStop = false;
            this.pictureImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.UpdateZoomedImage);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.pictureImage);
            this.panel1.Location = new System.Drawing.Point(231, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(652, 512);
            this.panel1.TabIndex = 12;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pixelInfo,
            this.loadInfo});
            this.statusStrip1.Location = new System.Drawing.Point(0, 539);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(884, 22);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // pixelInfo
            // 
            this.pixelInfo.AutoSize = false;
            this.pixelInfo.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.pixelInfo.Name = "pixelInfo";
            this.pixelInfo.Size = new System.Drawing.Size(230, 17);
            this.pixelInfo.Visible = false;
            // 
            // loadInfo
            // 
            this.loadInfo.Name = "loadInfo";
            this.loadInfo.Size = new System.Drawing.Size(0, 17);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tabControl1.Controls.Add(this.leftTab);
            this.tabControl1.Controls.Add(this.rightTab);
            this.tabControl1.Location = new System.Drawing.Point(0, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(225, 509);
            this.tabControl1.TabIndex = 14;
            // 
            // leftTab
            // 
            this.leftTab.Controls.Add(this.button_openLeft);
            this.leftTab.Controls.Add(this.button_saveLeft);
            this.leftTab.Controls.Add(this.leftListBox);
            this.leftTab.Location = new System.Drawing.Point(4, 22);
            this.leftTab.Name = "leftTab";
            this.leftTab.Padding = new System.Windows.Forms.Padding(3);
            this.leftTab.Size = new System.Drawing.Size(217, 483);
            this.leftTab.TabIndex = 0;
            this.leftTab.Text = "Левый глаз";
            this.leftTab.UseVisualStyleBackColor = true;
            // 
            // button_openLeft
            // 
            this.button_openLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_openLeft.Location = new System.Drawing.Point(6, 425);
            this.button_openLeft.Name = "button_openLeft";
            this.button_openLeft.Size = new System.Drawing.Size(205, 23);
            this.button_openLeft.TabIndex = 2;
            this.button_openLeft.Text = "Загрузить комплект";
            this.button_openLeft.UseVisualStyleBackColor = true;
            this.button_openLeft.Click += new System.EventHandler(this.loadLeftImages);
            // 
            // button_saveLeft
            // 
            this.button_saveLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_saveLeft.Location = new System.Drawing.Point(6, 454);
            this.button_saveLeft.Name = "button_saveLeft";
            this.button_saveLeft.Size = new System.Drawing.Size(205, 23);
            this.button_saveLeft.TabIndex = 1;
            this.button_saveLeft.Text = "Сохранить результат";
            this.button_saveLeft.UseVisualStyleBackColor = true;
            this.button_saveLeft.Click += new System.EventHandler(this.saveLeft);
            // 
            // leftListBox
            // 
            this.leftListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.leftListBox.FormattingEnabled = true;
            this.leftListBox.Location = new System.Drawing.Point(3, 3);
            this.leftListBox.Name = "leftListBox";
            this.leftListBox.Size = new System.Drawing.Size(211, 342);
            this.leftListBox.TabIndex = 0;
            this.leftListBox.SelectedIndexChanged += new System.EventHandler(this.viewLeftImage);
            // 
            // rightTab
            // 
            this.rightTab.Controls.Add(this.button_openRight);
            this.rightTab.Controls.Add(this.button_saveRight);
            this.rightTab.Controls.Add(this.rightListBox);
            this.rightTab.Location = new System.Drawing.Point(4, 22);
            this.rightTab.Name = "rightTab";
            this.rightTab.Padding = new System.Windows.Forms.Padding(3);
            this.rightTab.Size = new System.Drawing.Size(217, 483);
            this.rightTab.TabIndex = 1;
            this.rightTab.Text = "Правый глаз";
            this.rightTab.UseVisualStyleBackColor = true;
            // 
            // button_openRight
            // 
            this.button_openRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_openRight.Location = new System.Drawing.Point(6, 425);
            this.button_openRight.Name = "button_openRight";
            this.button_openRight.Size = new System.Drawing.Size(205, 23);
            this.button_openRight.TabIndex = 4;
            this.button_openRight.Text = "Загрузить комплект";
            this.button_openRight.UseVisualStyleBackColor = true;
            this.button_openRight.Click += new System.EventHandler(this.loadRightImages);
            // 
            // button_saveRight
            // 
            this.button_saveRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_saveRight.Location = new System.Drawing.Point(6, 454);
            this.button_saveRight.Name = "button_saveRight";
            this.button_saveRight.Size = new System.Drawing.Size(205, 23);
            this.button_saveRight.TabIndex = 3;
            this.button_saveRight.Text = "Сохранить результат";
            this.button_saveRight.UseVisualStyleBackColor = true;
            this.button_saveRight.Click += new System.EventHandler(this.saveRight);
            // 
            // rightListBox
            // 
            this.rightListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.rightListBox.FormattingEnabled = true;
            this.rightListBox.Location = new System.Drawing.Point(3, 3);
            this.rightListBox.Name = "rightListBox";
            this.rightListBox.Size = new System.Drawing.Size(211, 355);
            this.rightListBox.TabIndex = 1;
            this.rightListBox.SelectedIndexChanged += new System.EventHandler(this.viewRightImage);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Поиск ключевых точек методом SURF";
            this.SizeChanged += new System.EventHandler(this.windowsSizeChanged);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureImage)).EndInit();
            this.panel1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.leftTab.ResumeLayout(false);
            this.rightTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem фАЙЛToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem выходToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog open_images;
        private System.Windows.Forms.SaveFileDialog save_image;
        private System.Windows.Forms.PictureBox pictureImage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel pixelInfo;
        private System.Windows.Forms.ToolStripStatusLabel loadInfo;
        private System.Windows.Forms.ToolStripMenuItem оТОБРАЖЕНИЕToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sourceMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fitMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage leftTab;
        private System.Windows.Forms.TabPage rightTab;
        private System.Windows.Forms.ListBox leftListBox;
        private System.Windows.Forms.Button button_openLeft;
        private System.Windows.Forms.Button button_saveLeft;
        private System.Windows.Forms.ListBox rightListBox;
        private System.Windows.Forms.Button button_openRight;
        private System.Windows.Forms.Button button_saveRight;
    }
}

