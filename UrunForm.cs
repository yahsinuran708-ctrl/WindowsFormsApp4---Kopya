using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class UrunForm : Form
    {
        private readonly SqlConnection baglanti = Db.CreateConnection();

        public UrunForm(SqlConnection conn)
        {
            InitializeComponent();
        }

        public UrunForm()
        {
            InitializeComponent();
            this.Load += UrunForm_Load;
        }

        private void UrunForm_Load(object sender, EventArgs e)
        {
            Listele();
        }

        void Listele()
        {
            try
            {
                using (SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Urunler", baglanti))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürünler listelenirken hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Temizle()
        {
            txtUrunAdi.Clear();
            txtBarkod.Clear();
            txtKategori.Clear();
            txtReyon.Clear();
            txtAlis.Clear();
            txtSatis.Clear();
            txtStok.Clear();
        }
        void UrunGetir(string barkod)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barkod))
                {
                    Listele();
                    return;
                }

                using (var cmd = new SqlCommand("SELECT * FROM Urunler WHERE Barkod = @barkod", baglanti))
                {
                    cmd.Parameters.AddWithValue("@barkod", barkod.Trim());
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            dataGridView1.DataSource = dt;
                        }
                        else
                        {
                            MessageBox.Show("Ürün bulunamadı!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün aranırken hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnListele_Click(object sender, EventArgs e)
        {
            Listele();
        }

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            Temizle();
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUrunAdi.Text) ||
                    string.IsNullOrWhiteSpace(txtBarkod.Text))
                {
                    MessageBox.Show("Ürün adı ve barkod boş olamaz.", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtAlis.Text, out var alis))
                {
                    MessageBox.Show("Alış fiyatı sayısal olmalıdır.", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtSatis.Text, out var satis))
                {
                    MessageBox.Show("Satış fiyatı sayısal olmalıdır.", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtStok.Text, out var stok))
                {
                    MessageBox.Show("Stok miktarı tam sayı olmalıdır.", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                baglanti.Open();

                using (var komut = new SqlCommand(
                    "INSERT INTO Urunler (UrunAdi,Barkod,Kategori,Reyon,AlisFiyat,SatisFiyat,StokMiktari) VALUES (@a,@b,@c,@d,@e,@f,@g)",
                    baglanti))
                {
                    komut.Parameters.AddWithValue("@a", txtUrunAdi.Text);
                    komut.Parameters.AddWithValue("@b", txtBarkod.Text);
                    komut.Parameters.AddWithValue("@c", txtKategori.Text);
                    komut.Parameters.AddWithValue("@d", txtReyon.Text);
                    komut.Parameters.AddWithValue("@e", alis);
                    komut.Parameters.AddWithValue("@f", satis);
                    komut.Parameters.AddWithValue("@g", stok);

                    komut.ExecuteNonQuery();
                }

                MessageBox.Show("Ürün eklendi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün eklenirken hata oluştu:\n" + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (baglanti.State == ConnectionState.Open)
                    baglanti.Close();
            }

            Listele();
            Temizle();
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Lütfen listeden bir ürün seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                baglanti.Open();

                var komut = new SqlCommand("DELETE FROM Urunler WHERE ID=@id", baglanti);
                // ID'nin her zaman ilk sütun olduğunu varsayıyoruz
                komut.Parameters.AddWithValue("@id", dataGridView1.CurrentRow.Cells[0].Value);
                komut.ExecuteNonQuery();

                MessageBox.Show("Ürün silindi");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün silinirken hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (baglanti.State == ConnectionState.Open)
                    baglanti.Close();
            }

            Listele();
            Temizle();
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Lütfen listeden bir ürün seçin.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtAlis.Text, out var alis) ||
                !decimal.TryParse(txtSatis.Text, out var satis) ||
                !int.TryParse(txtStok.Text, out var stok))
            {
                MessageBox.Show("Alış/Satış fiyatı ve stok sayısal olmalıdır.", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                baglanti.Open();

                var komut = new SqlCommand(
                    "UPDATE Urunler SET UrunAdi=@a,Barkod=@b,Kategori=@c,Reyon=@d,AlisFiyat=@e,SatisFiyat=@f,StokMiktari=@g WHERE ID=@id",
                    baglanti);

                komut.Parameters.AddWithValue("@a", txtUrunAdi.Text);
                komut.Parameters.AddWithValue("@b", txtBarkod.Text);
                komut.Parameters.AddWithValue("@c", txtKategori.Text);
                komut.Parameters.AddWithValue("@d", txtReyon.Text);
                komut.Parameters.AddWithValue("@e", alis);
                komut.Parameters.AddWithValue("@f", satis);
                komut.Parameters.AddWithValue("@g", stok);
                // ID'nin her zaman ilk sütun olduğunu varsayıyoruz
                komut.Parameters.AddWithValue("@id", dataGridView1.CurrentRow.Cells[0].Value);

                komut.ExecuteNonQuery();

                MessageBox.Show("Ürün güncellendi");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün güncellenirken hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (baglanti.State == ConnectionState.Open)
                    baglanti.Close();
            }

            Listele();
            Temizle();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            // Sütun sırasını: 0=ID, 1=UrunAdi, 2=Barkod, 3=Kategori, 4=Reyon, 5=AlisFiyat, 6=SatisFiyat, 7=StokMiktari varsayıyoruz
            if (dataGridView1.CurrentRow.Cells.Count >= 8)
            {
                txtUrunAdi.Text = dataGridView1.CurrentRow.Cells[1].Value?.ToString();
                txtBarkod.Text = dataGridView1.CurrentRow.Cells[2].Value?.ToString();
                txtKategori.Text = dataGridView1.CurrentRow.Cells[3].Value?.ToString();
                txtReyon.Text = dataGridView1.CurrentRow.Cells[4].Value?.ToString();
                txtAlis.Text = dataGridView1.CurrentRow.Cells[5].Value?.ToString();
                txtSatis.Text = dataGridView1.CurrentRow.Cells[6].Value?.ToString();
                txtStok.Text = dataGridView1.CurrentRow.Cells[7].Value?.ToString();
            }
        }

        private void btnAra_Click_1(object sender, EventArgs e)
        {
            UrunGetir(txtBarkod.Text);
        }



        private void txtBarkod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UrunGetir(txtBarkod.Text);
                e.SuppressKeyPress = true;
            }
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            UrunGetir(textBox1.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}