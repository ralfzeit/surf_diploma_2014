using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SURF
{
    /// <summary>
    /// Класс формы настроек
    /// </summary>
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            try
            {
                StreamReader sr = new StreamReader("settings.dat");

                trackBar1.Value = Convert.ToInt32(sr.ReadLine());

                if (sr.ReadLine() == "1")
                    checkBox1.Checked = true;
                else checkBox1.Checked = false;

                sr.Close();
            }
            catch { }
        }

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter("settings.dat");

            sw.WriteLine(trackBar1.Value);

            if (checkBox1.Checked == true)
                sw.WriteLine(1);
            else sw.WriteLine(0);

            sw.Close();

            this.Close();
        }
    }
}
