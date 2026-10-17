using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class SatisForm : Form
    {
        private readonly SqlConnection baglanti = Db.CreateConnection();

        // NOTE: Bu formun orijinal hali derleme hatası içeriyordu.
        // dtpTarih kontrolü Designer dosyasında var; kodda dtpTarih kullanılıyor.


        private sealed class SepetSatiri
        {
            public string UrunKodu { get; set; }
            public string UrunAdi { get; set; }
            public int Adet { get; set; }
            public decimal BirimFiyat { get; set; }
            public decimal SatirToplam => BirimFiyat * Adet;
        }

        private readonly BindingList<SepetSatiri> _sepet = new BindingList<SepetSatiri>();

        public SatisForm()
        {
            InitializeComponent();
        }

        private void SatisForm_Load(object sender, EventArgs e)
        {
            dgvSepet.AutoGenerateColumns = true;
            dgvSepet.DataSource = _sepet;
            Listele();
        }

        private void Listele()
        {
            using (var da = new SqlDataAdapter("SELECT * FROM Satislar ORDER BY ID DESC", baglanti))
            {
                var dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }

        private void txtUrunKodu_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUrunKodu.Text))
            {
                txtUrunAdi.Text = "";
                return;
            }

            baglanti.Open();
            try
            {
                using (var komut = new SqlCommand("SELECT UrunAdi FROM Urunler WHERE Barkod=@kodu", baglanti))
                {
                    komut.Parameters.AddWithValue("@kodu", txtUrunKodu.Text);
                    var sonuc = komut.ExecuteScalar();
                    txtUrunAdi.Text = sonuc == null ? "" : sonuc.ToString();
                }
            }
            finally
            {
                baglanti.Close();
            }
        }

        private void btnSepeteEkle_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtAdet.Text, out var adet) || adet <= 0)
            {
                MessageBox.Show("Satış adeti geçersiz.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUrunKodu.Text))
            {
                MessageBox.Show("Ürün kodu (barkod) boş olamaz.");
                return;
            }

            baglanti.Open();
            try
            {
                decimal fiyat;
                using (var fiyatKomut = new SqlCommand("SELECT SatisFiyat FROM Urunler WHERE Barkod=@kodu", baglanti))
                {
                    fiyatKomut.Parameters.AddWithValue("@kodu", txtUrunKodu.Text);
                    var fiyatObj = fiyatKomut.ExecuteScalar();
                    if (fiyatObj == null)
                    {
                        MessageBox.Show("Bu barkod ile ürün bulunamadı.");
                        return;
                    }
                    fiyat = Convert.ToDecimal(fiyatObj);
                }

                using (var stokOku = new SqlCommand("SELECT StokMiktari FROM Urunler WHERE Barkod=@kodu", baglanti))
                {
                    stokOku.Parameters.AddWithValue("@kodu", txtUrunKodu.Text);
                    var stokObj = stokOku.ExecuteScalar();
                    var mevcutStok = stokObj == null ? 0 : Convert.ToInt32(stokObj);
                    var sepetteki = _sepet.Where(s => s.UrunKodu == txtUrunKodu.Text).Sum(s => s.Adet);
                    if (mevcutStok < sepetteki + adet)
                    {
                        MessageBox.Show("Yetersiz stok (sepetteki adet dahil).");
                        return;
                    }
                }

                var satir = _sepet.FirstOrDefault(s => s.UrunKodu == txtUrunKodu.Text);
                if (satir != null)
                {
                    var yeniAdet = satir.Adet + adet;
                    _sepet.Remove(satir);
                    _sepet.Add(new SepetSatiri
                    {
                        UrunKodu = txtUrunKodu.Text,
                        UrunAdi = txtUrunAdi.Text,
                        Adet = yeniAdet,
                        BirimFiyat = fiyat
                    });
                }
                else
                {
                    _sepet.Add(new SepetSatiri
                    {
                        UrunKodu = txtUrunKodu.Text,
                        UrunAdi = txtUrunAdi.Text,
                        Adet = adet,
                        BirimFiyat = fiyat
                    });
                }

                txtToplam.Text = (fiyat * adet).ToString("0.##");
            }
            finally
            {
                baglanti.Close();
            }
        }

        private void btnSepetTemizle_Click(object sender, EventArgs e)
        {
            _sepet.Clear();
            txtToplam.Clear();
        }

        private void btnFisOnayla_Click(object sender, EventArgs e)
        {
            if (_sepet.Count == 0)
            {
                MessageBox.Show("Sepet boş. Önce ürün ekleyin.");
                return;
            }

            baglanti.Open();
            SqlTransaction tr = null;

            try
            {
                tr = baglanti.BeginTransaction();
                int fisNo;
                using (var fisCmd = new SqlCommand("SELECT ISNULL(MAX(FisNo), 0) + 1 FROM Satislar", baglanti, tr))
                {
                    fisNo = Convert.ToInt32(fisCmd.ExecuteScalar());
                }

                foreach (var satir in _sepet)
                {
                    using (var stokKomut = new SqlCommand(
                        "UPDATE Urunler SET StokMiktari = StokMiktari - @adet WHERE Barkod=@kodu AND StokMiktari >= @adet",
                        baglanti, tr))
                    {
                        stokKomut.Parameters.AddWithValue("@adet", satir.Adet);
                        stokKomut.Parameters.AddWithValue("@kodu", satir.UrunKodu);
                        if (stokKomut.ExecuteNonQuery() == 0)
                        {
                            tr.Rollback();
                            tr = null;
                            MessageBox.Show("Yetersiz stok: " + satir.UrunAdi);
                            return;
                        }
                    }

                    var toplam = satir.BirimFiyat * satir.Adet;
                    using (var komut = new SqlCommand(
                        "INSERT INTO Satislar (FisNo,UrunKodu,UrunAdi,SatisAdeti,SatisTarihi,ToplamFiyat) VALUES (@f,@a,@b,@c,@d,@e)",
                        baglanti, tr))
                    {
                        komut.Parameters.AddWithValue("@f", fisNo);
                        komut.Parameters.AddWithValue("@a", satir.UrunKodu);
                        komut.Parameters.AddWithValue("@b", satir.UrunAdi);
                        komut.Parameters.AddWithValue("@c", satir.Adet);
                        komut.Parameters.AddWithValue("@d", dtpTarih.Value);
                        komut.Parameters.AddWithValue("@e", toplam);
                        komut.ExecuteNonQuery();
                    }
                }

                tr.Commit();
                tr = null;
                _sepet.Clear();
                txtToplam.Clear();
                MessageBox.Show("Fiş kaydedildi. Fiş No: " + fisNo);
            }
            catch (Exception ex)
            {
                tr?.Rollback();
                MessageBox.Show("Fiş kaydedilirken hata:\n" + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                baglanti.Close();
            }

            Listele();
        }

        private void btnListele_Click(object sender, EventArgs e)
        {
            Listele();
        }
    }
}
