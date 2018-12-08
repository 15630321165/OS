using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace operating_system
{
    public partial class Form3 : Form
    {
        private int time;
        public Form3()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            time = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 10;
            progressBar1.Value = 4;
            timer1.Start();
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time++;
            progressBar1.Value += 2;
            if (progressBar1.Value == 10)
            {
                progressBar1.Value = 0;
                this.Close();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }
}
