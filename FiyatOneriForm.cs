using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class FiyatOneriForm : Form
    {
        private readonly Button btnHesapla = new Button();
        private readonly DataGridView dgv = new DataGridView();
        private readonly Label lbl = new Label();

        public FiyatOneriForm()
        {
            InitializeComponent();
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Fiyat Öneri";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1000;
            Height = 620;

            lbl.AutoSize = true;
            lbl.Location = new System.Drawing.Point(14, 14);
            lbl.Text = "Son 30 gün satış adedi vs önceki 30 gün değişimine göre öneri.";

            btnHesapla.Text = "Fiyat Önerilerini Hesapla ve Kaydet";
            btnHesapla.Location = new System.Drawing.Point(14, 40);
            btnHesapla.Width = 320;
            btnHesapla.Click += btnHesapla_Click;

            dgv.Location = new System.Drawing.Point(14, 80);
            dgv.Size = new System.Drawing.Size(960, 500);
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Controls.Add(lbl);
            Controls.Add(btnHesapla);
            Controls.Add(dgv);
        }

        private void btnHesapla_Click(object sender, EventArgs e)
        {
            try
            {
                using (var con = new SqlConnection(Db.ConnectionString))
                {
                    con.Open();

                    using (var del = new SqlCommand("DELETE FROM dbo.FiyatOnerileri", con))
                        del.ExecuteNonQuery();

                    // Son 30 gün ve önceki 30 gün satış adedi
                    // Not: Satislar.UrunKodu yerine urunAdi ile eşleştirme yoksa, mevcut tabloda UrunAdi ile join ediyoruz.
                    var sql = @"
;WITH P AS (
    SELECT
        u.Barkod,
        u.UrunAdi,
        u.SatisFiyat AS MevcutFiyat,
        ISNULL(SUM(CASE WHEN s.SatisTarihi >= DATEADD(day,-30,GETDATE()) THEN s.SatisAdeti ELSE 0 END),0) AS SonDonemAdet,
        ISNULL(SUM(CASE WHEN s.SatisTarihi < DATEADD(day,-30,GETDATE()) AND s.SatisTarihi >= DATEADD(day,-60,GETDATE()) THEN s.SatisAdeti ELSE 0 END),0) AS OncekiDonemAdet
    FROM dbo.Urunler u
    LEFT JOIN dbo.Satislar s
        ON s.UrunAdi = u.UrunAdi
    GROUP BY u.Barkod, u.UrunAdi, u.SatisFiyat
)
INSERT INTO dbo.FiyatOnerileri
(
    AnalizZamani,
    Barkod,
    UrunAdi,
    MevcutFiyat,
    SonDonemAdet,
    OncekiDonemAdet,
    SatisDegisimOrani,
    OneriTipi,
    OnerilenFiyat,
    Aciklama
)
SELECT
    GETDATE() AS AnalizZamani,
    Barkod,
    UrunAdi,
    MevcutFiyat,
    SonDonemAdet,
    OncekiDonemAdet,
    CASE
        WHEN OncekiDonemAdet = 0 THEN NULL
        ELSE CAST((SonDonemAdet - OncekiDonemAdet) * 1.0 / NULLIF(OncekiDonemAdet,0) AS DECIMAL(18,4))
    END AS SatisDegisimOrani,
    CASE
        WHEN OncekiDonemAdet = 0 AND SonDonemAdet > 0 THEN 'Artış'
        WHEN SonDonemAdet > OncekiDonemAdet THEN 'Artış'
        WHEN SonDonemAdet < OncekiDonemAdet THEN 'İndirim'
        ELSE 'Durağan'
    END AS OneriTipi,
    CASE
        WHEN SonDonemAdet > OncekiDonemAdet AND OncekiDonemAdet > 0 THEN CAST(MevcutFiyat * 1.05 AS DECIMAL(18,2))
        WHEN SonDonemAdet < OncekiDonemAdet AND SonDonemAdet > 0 THEN CAST(MevcutFiyat * 0.95 AS DECIMAL(18,2))
        WHEN OncekiDonemAdet = 0 AND SonDonemAdet > 0 THEN CAST(MevcutFiyat * 1.03 AS DECIMAL(18,2))
        ELSE CAST(MevcutFiyat AS DECIMAL(18,2))
    END AS OnerilenFiyat,
    CASE
        WHEN OncekiDonemAdet = 0 AND SonDonemAdet > 0 THEN N'Önceki dönem satış yok, güncel talebe göre küçük artış.'
        WHEN SonDonemAdet > OncekiDonemAdet THEN N'Talep artışı gözlendi, küçük artış önerilir.'
        WHEN SonDonemAdet < OncekiDonemAdet THEN N'Talep düşüşü gözlendi, küçük indirim önerilir.'
        ELSE N'Durağan talep.'
    END AS Aciklama
FROM P;";

                    using (var cmd = new SqlCommand(sql, con))
                        cmd.ExecuteNonQuery();

                    using (var da = new SqlDataAdapter(
                               "SELECT TOP 200 * FROM dbo.FiyatOnerileri ORDER BY AnalizZamani DESC", con))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgv.DataSource = dt;
                    }

                    MessageBox.Show("Fiyat önerileri hesaplandı ve kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fiyat önerileri hesaplanırken hata oluştu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

