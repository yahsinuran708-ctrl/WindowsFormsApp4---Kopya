using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class TahminMotoruForm : Form
    {
        private readonly Button btnCalistir = new Button();
        private readonly DataGridView dgv = new DataGridView();
        private readonly Label lbl = new Label();

        public TahminMotoruForm()
        {
            InitializeComponent();
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Tahmin Motoru";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1000;
            Height = 620;

            lbl.AutoSize = true;
            lbl.Location = new Point(14, 14);
            lbl.Text = "Stok tahmini çalıştırır (Son 30 gün satış ortalamasına göre).";

            btnCalistir.Text = "Tahmini Hesapla ve Kaydet";
            btnCalistir.Location = new Point(14, 40);
            btnCalistir.Width = 260;
            btnCalistir.Click += btnCalistir_Click;

            dgv.Location = new Point(14, 80);
            dgv.Size = new Size(960, 500);
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Controls.Add(lbl);
            Controls.Add(btnCalistir);
            Controls.Add(dgv);
        }

        private void btnCalistir_Click(object sender, EventArgs e)
        {
            try
            {
                using (var con = new SqlConnection(Db.ConnectionString))
                {
                    con.Open();

                    // Son 30 gün satış adedi
                    var sqlInsert = @"
;WITH S30 AS
(
    SELECT
        u.Barkod,
        u.UrunAdi,
        u.StokMiktari,
        SUM(s.SatisAdeti) AS Son30GunAdet
    FROM dbo.Urunler u
    LEFT JOIN dbo.Satislar s
        ON s.UrunAdi = u.UrunAdi
        AND s.SatisTarihi >= DATEADD(day,-30,GETDATE())
    GROUP BY u.Barkod, u.UrunAdi, u.StokMiktari
)
INSERT INTO dbo.StokTahminleri
(
    Barkod,
    UrunAdi,
    StokMiktari,
    OrtalamaGunlukSatis,
    TahminiBitisTarihi,
    KalanGun,
    UyariDurumu,
    SonGuncelleme
)
SELECT
    Barkod,
    UrunAdi,
    StokMiktari,
    CASE WHEN Son30GunAdet IS NULL THEN 0 ELSE CAST(Son30GunAdet / 30.0 AS DECIMAL(18,2)) END AS OrtalamaGunlukSatis,
    CASE
        WHEN StokMiktari = 0 THEN GETDATE()
        WHEN Son30GunAdet IS NULL OR Son30GunAdet = 0 THEN NULL
        ELSE DATEADD(day, CAST(ROUND(StokMiktari / NULLIF((Son30GunAdet / 30.0),0),0) AS INT), GETDATE())
    END AS TahminiBitisTarihi,
    CASE
        WHEN StokMiktari = 0 THEN 0
        WHEN Son30GunAdet IS NULL OR Son30GunAdet = 0 THEN NULL
        ELSE CAST(ROUND(StokMiktari / NULLIF((Son30GunAdet / 30.0),0),0) AS INT)
    END AS KalanGun,
    CASE
        WHEN StokMiktari = 0 THEN 'Kritik'
        WHEN Son30GunAdet IS NULL OR Son30GunAdet = 0 THEN 'Belirsiz'
        WHEN (StokMiktari / NULLIF((Son30GunAdet / 30.0),0)) <= 7 THEN 'Kritik'
        WHEN (StokMiktari / NULLIF((Son30GunAdet / 30.0),0)) <= 14 THEN 'Uyarı'
        ELSE 'Normal'
    END AS UyariDurumu,
    GETDATE()
FROM S30;";

                    // Benzersiz barkod constraint nedeniyle önce temizle
                    using (var del = new SqlCommand("DELETE FROM dbo.StokTahminleri", con))
                        del.ExecuteNonQuery();

                    using (var cmd = new SqlCommand(sqlInsert, con))
                        cmd.ExecuteNonQuery();

                    using (var da = new SqlDataAdapter("SELECT TOP 200 * FROM dbo.StokTahminleri ORDER BY SonGuncelleme DESC", con))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        dgv.DataSource = dt;
                    }

                    MessageBox.Show("Tahminler hesaplandı ve kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tahmin çalıştırılırken hata oluştu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

