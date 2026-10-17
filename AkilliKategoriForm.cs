using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public class AkilliKategoriForm : Form
    {
        private readonly SqlConnection baglanti = Db.CreateConnection();
        private readonly TextBox txtUrunAdi = new TextBox();
        private readonly Button btnKategoriyeGoreListele = new Button();
        private readonly Button btnTumunuKategorilendir = new Button();
        private readonly Label lblKategoriSonuc = new Label();
        private readonly DataGridView dataGridViewSonuc = new DataGridView();

        private readonly Dictionary<string, string[]> kategoriKurallari = new Dictionary<string, string[]>
        {
            { "Atıştırmalık", new[] { "cips", "kraker", "gofret", "bisküvi", "çikolata", "cikolata", "kuruyemiş", "kuruyemis", "kek" } },
            { "Temizlik", new[] { "deterjan", "çamaşır suyu", "camasir suyu", "sabun", "şampuan", "sampuan", "temizlik", "yumuşatıcı", "yumusatici" } },
            { "İçecek", new[] { "su", "soda", "meyve suyu", "kola", "gazoz", "ayran", "ice tea", "kahve", "çay", "cay" } },
            { "Süt Ürünleri", new[] { "süt", "sut", "yoğurt", "yogurt", "peynir", "tereyağı", "tereyagi", "labne" } },
            { "Bakliyat", new[] { "pirinç", "pirinc", "bulgur", "mercimek", "fasulye", "nohut", "makarna", "un" } },
            { "Kişisel Bakım", new[] { "diş macunu", "dis macunu", "diş fırçası", "dis fircasi", "deodorant", "jilet", "kolonya" } }
        };

        public AkilliKategoriForm()
        {
            Text = "Akıllı Kategori Sistemi";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 980;
            Height = 550;

            var lblBaslik = new Label
            {
                Text = "Ürün adını yazın, yapay zeka mantığıyla kategori bulup aynı kategorideki ürünleri listeleyelim:",
                AutoSize = true,
                Location = new Point(16, 16)
            };

            txtUrunAdi.Location = new Point(16, 44);
            txtUrunAdi.Width = 320;

            btnKategoriyeGoreListele.Text = "Kategoriyi Bul ve Listele";
            btnKategoriyeGoreListele.Location = new Point(350, 42);
            btnKategoriyeGoreListele.Width = 170;
            btnKategoriyeGoreListele.Click += btnKategoriyeGoreListele_Click;

            btnTumunuKategorilendir.Text = "Tüm Ürünleri Otomatik Kategorilendir";
            btnTumunuKategorilendir.Location = new Point(535, 42);
            btnTumunuKategorilendir.Width = 250;
            btnTumunuKategorilendir.Click += btnTumunuKategorilendir_Click;

            lblKategoriSonuc.AutoSize = true;
            lblKategoriSonuc.Location = new Point(16, 78);
            lblKategoriSonuc.Text = "Tahmini kategori: -";

            dataGridViewSonuc.Location = new Point(16, 108);
            dataGridViewSonuc.Size = new Size(930, 380);
            dataGridViewSonuc.ReadOnly = true;
            dataGridViewSonuc.AllowUserToAddRows = false;
            dataGridViewSonuc.AllowUserToDeleteRows = false;
            dataGridViewSonuc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewSonuc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            Controls.Add(lblBaslik);
            Controls.Add(txtUrunAdi);
            Controls.Add(btnKategoriyeGoreListele);
            Controls.Add(btnTumunuKategorilendir);
            Controls.Add(lblKategoriSonuc);
            Controls.Add(dataGridViewSonuc);
        }

        private void btnTumunuKategorilendir_Click(object sender, EventArgs e)
        {
            try
            {
                baglanti.Open();
                var urunler = new List<(int id, string urunAdi)>();

                using (var komut = new SqlCommand("SELECT ID, UrunAdi FROM Urunler", baglanti))
                using (var reader = komut.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        urunler.Add((Convert.ToInt32(reader["ID"]), reader["UrunAdi"].ToString()));
                    }
                }

                foreach (var urun in urunler)
                {
                    var kategori = KategoriTahminEt(urun.urunAdi);
                    using (var guncelle = new SqlCommand("UPDATE Urunler SET Kategori=@kategori WHERE ID=@id", baglanti))
                    {
                        guncelle.Parameters.AddWithValue("@kategori", kategori);
                        guncelle.Parameters.AddWithValue("@id", urun.id);
                        guncelle.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Tüm ürünler otomatik kategorilendirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Otomatik kategorizasyon sırasında hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (baglanti.State == ConnectionState.Open)
                {
                    baglanti.Close();
                }
            }
        }

        private void btnKategoriyeGoreListele_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUrunAdi.Text))
            {
                MessageBox.Show("Lütfen ürün adı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kategori = KategoriTahminEt(txtUrunAdi.Text);
            lblKategoriSonuc.Text = "Tahmini kategori: " + kategori;

            try
            {
                baglanti.Open();

                // Kullanıcının yazdığı ürüne de kategori atayarak sistemde güncel tut.
                using (var urunGuncelle = new SqlCommand(
                    "UPDATE Urunler SET Kategori=@kategori WHERE UrunAdi LIKE @urunAdi",
                    baglanti))
                {
                    urunGuncelle.Parameters.AddWithValue("@kategori", kategori);
                    urunGuncelle.Parameters.AddWithValue("@urunAdi", "%" + txtUrunAdi.Text.Trim() + "%");
                    urunGuncelle.ExecuteNonQuery();
                }

                using (var da = new SqlDataAdapter(
                    @"SELECT ID, UrunAdi, Barkod, Kategori, Reyon, StokMiktari, SatisFiyat
                      FROM Urunler
                      WHERE Kategori = @kategori
                      ORDER BY UrunAdi", baglanti))
                {
                    da.SelectCommand.Parameters.AddWithValue("@kategori", kategori);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dataGridViewSonuc.DataSource = dt;
                }

                if (dataGridViewSonuc.Rows.Count == 0)
                {
                    MessageBox.Show("Bu kategoride ürün bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategoriye göre listeleme sırasında hata oluştu:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (baglanti.State == ConnectionState.Open)
                {
                    baglanti.Close();
                }
            }
        }

        private string KategoriTahminEt(string urunAdi)
        {
            var metin = (urunAdi ?? string.Empty).ToLowerInvariant();

            foreach (var kural in kategoriKurallari)
            {
                if (kural.Value.Any(anahtar => metin.Contains(anahtar)))
                {
                    return kural.Key;
                }
            }

            return "Diğer";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AkilliKategoriForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "AkilliKategoriForm";
            this.Load += new System.EventHandler(this.AkilliKategoriForm_Load);
            this.ResumeLayout(false);

        }

        private void AkilliKategoriForm_Load(object sender, EventArgs e)
        {

        }
    }
}
