using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class StokForm : Form
    {
        private readonly SqlConnection baglanti = Db.CreateConnection();

        public StokForm()
        {
            InitializeComponent();
        }

        private void Listele()
        {
            try
            {
                using (var da = new SqlDataAdapter("SELECT * FROM Urunler ORDER BY ID DESC", baglanti))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Liste yüklenemedi:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StokForm_Load(object sender, EventArgs e)
        {
            Listele();
        }

        private void btnListele_Click(object sender, EventArgs e)
        {
            Listele();
        }

        private void btnStokEkle_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Lütfen listeden bir ürün seçin.");
                return;
            }

            if (!int.TryParse(txtStok.Text, out var yeniStok) || yeniStok < 0)
            {
                MessageBox.Show("Stok miktarı geçersiz.");
                return;
            }

            baglanti.Open();
            try
            {
                var id = StokSeciliUrunId();
                if (id == null)
                {
                    MessageBox.Show("Kayıt seçilemedi.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var komut = new SqlCommand(
                    "UPDATE Urunler SET StokMiktari=@stok WHERE ID=@id",
                    baglanti))
                {
                    komut.Parameters.AddWithValue("@stok", yeniStok);
                    komut.Parameters.AddWithValue("@id", id.Value);
                    komut.ExecuteNonQuery();
                }

                MessageBox.Show("Stok güncellendi");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Stok güncellenemedi:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (baglanti.State == System.Data.ConnectionState.Open)
                    baglanti.Close();
            }

            Listele();
        }

        private int? StokSeciliUrunId()
        {
            var row = dataGridView1.CurrentRow;
            if (row == null || row.IsNewRow)
                return null;
            if (dataGridView1.Columns.Contains("ID"))
                return Convert.ToInt32(row.Cells["ID"].Value);
            return Convert.ToInt32(row.Cells[0].Value);
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Lütfen listeden bir ürün seçin.");
                return;
            }

            if (MessageBox.Show("Seçili ürünü silmek istediğinize emin misiniz?", "Onay",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var id = StokSeciliUrunId();
            if (id == null)
                return;

            try
            {
                baglanti.Open();
                using (var komut = new SqlCommand("DELETE FROM Urunler WHERE ID=@id", baglanti))
                {
                    komut.Parameters.AddWithValue("@id", id.Value);
                    komut.ExecuteNonQuery();
                }
                MessageBox.Show("Ürün silindi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silinemedi:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (baglanti.State == System.Data.ConnectionState.Open)
                    baglanti.Close();
            }

            Listele();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            var row = dataGridView1.Rows[e.RowIndex];
            if (row.IsNewRow)
                return;

            txtUrunKodu.Text = Hucre(row, "Barkod", 2);
            txtUrunAdi.Text = Hucre(row, "UrunAdi", 1);
            txtReyon.Text = Hucre(row, "Reyon", 4);
            txtStok.Text = Hucre(row, "StokMiktari", 7);
            txtAlis.Text = Hucre(row, "AlisFiyat", 5);
            txtSatis.Text = Hucre(row, "SatisFiyat", 6);
            dtpSonGuncelleme.Value = DateTime.Now;
        }

        private static string Hucre(DataGridViewRow row, string kolonAdi, int yedekIndeks)
        {
            object v = null;
            if (row.DataGridView.Columns.Contains(kolonAdi))
                v = row.Cells[kolonAdi].Value;
            else if (row.Cells.Count > yedekIndeks)
                v = row.Cells[yedekIndeks].Value;
            return v == null || v == DBNull.Value ? "" : v.ToString();
        }
    }
}