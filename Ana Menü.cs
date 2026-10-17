using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var f = new UrunForm();
            f.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var f = new ReyonForm();
            f.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var f = new StokForm();
            f.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var f = new SatisForm();
            f.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var f = new AkilliKategoriForm();
            f.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Yeni: Akıllı Sohbet formu
            var f = new AkilliSohbetForm();
            f.Show();
        }


        private void button8_Click(object sender, EventArgs e)
        {
            var f = new AnalizForm();
            f.Show();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var f = new FiyatOneriForm();
            f.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var cevap = MessageBox.Show("Programdan çıkmak istiyor musunuz?", "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (cevap == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
