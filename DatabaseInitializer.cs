using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    internal static class DatabaseInitializer
    {
        // Sunucu adı: .  (Varsayılan SQL Server instance)
        private const string ServerConnectionString = "Server=.;Database=master;Trusted_Connection=True;";
        private const string MarketDbName = "MarketDB";

        // Uygulama açılırken tahmin sorgularının sağlıklı çalışması için (stok/öneri vb.)
        // tüm tabloların doğru kolonlarla oluşması kritik.


        internal static void EnsureDatabaseAndTables()
        {
            try
            {
                EnsureDatabase();
                EnsureTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Veritabanı başlatılırken hata oluştu:\n" + ex.Message,
                    "Veritabanı Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void EnsureDatabase()
        {
            using (var connection = new SqlConnection(ServerConnectionString))
            {
                connection.Open();
                var cmd = new SqlCommand(
                    @"IF DB_ID(N'" + MarketDbName + @"') IS NULL
                        BEGIN
                            CREATE DATABASE " + MarketDbName + @";
                        END",
                    connection);


                cmd.ExecuteNonQuery();
            }
        }

        private static void EnsureTables()
        {
            var dbConnectionString = "Server=.;Database=" + MarketDbName + ";Trusted_Connection=True;";

            using (var connection = new SqlConnection(dbConnectionString))
            {
                connection.Open();

                // Not: Bazı tablolar uygulamadaki sorgularda farklı kolon adlarıyla eşleşiyor.
                // “tahmin butonu ürünleri görmüyor” şikayeti en sık kolon adı / ilişki uyumsuzluğundan oluyor.



                // Urunler tablosu
                var createUrunler = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Urunler', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Urunler
    (
        ID           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Urunler PRIMARY KEY,
        UrunAdi      NVARCHAR(100)      NOT NULL,
        Barkod       NVARCHAR(50)       NOT NULL,
        Kategori     NVARCHAR(100)      NULL,
        Reyon        NVARCHAR(100)      NULL,
        AlisFiyat    DECIMAL(18,2)      NOT NULL CONSTRAINT DF_Urunler_Alis DEFAULT(0),
        SatisFiyat   DECIMAL(18,2)      NOT NULL CONSTRAINT DF_Urunler_Satis DEFAULT(0),
        StokMiktari  INT                NOT NULL CONSTRAINT DF_Urunler_Stok DEFAULT(0)
    );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Urunler_Barkod' AND object_id = OBJECT_ID('dbo.Urunler'))
        CREATE UNIQUE INDEX UX_Urunler_Barkod ON dbo.Urunler(Barkod);
END
", connection);
                createUrunler.ExecuteNonQuery();

                // Satislar tablosu (FisNo: aynı fişte birden fazla kalem = birlikte alınan ürünler)
                var createSatislar = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Satislar', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Satislar
    (
        ID           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Satislar PRIMARY KEY,
        FisNo        INT                NOT NULL CONSTRAINT DF_Satislar_FisNo DEFAULT(0),
        UrunKodu     NVARCHAR(50)       NOT NULL,
        UrunAdi      NVARCHAR(100)      NOT NULL,
        SatisAdeti   INT                NOT NULL,
        SatisTarihi  DATETIME           NOT NULL,
        ToplamFiyat  DECIMAL(18,2)      NOT NULL
    );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Satislar_Tarih' AND object_id = OBJECT_ID('dbo.Satislar'))
        CREATE INDEX IX_Satislar_Tarih ON dbo.Satislar(SatisTarihi DESC);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Satislar_FisNo' AND object_id = OBJECT_ID('dbo.Satislar'))
        CREATE INDEX IX_Satislar_FisNo ON dbo.Satislar(FisNo);
END
", connection);
                createSatislar.ExecuteNonQuery();


                var migrateSatislarFisNo = new SqlCommand(@"
IF COL_LENGTH(N'dbo.Satislar', N'FisNo') IS NULL
BEGIN
    ALTER TABLE dbo.Satislar ADD FisNo INT NOT NULL CONSTRAINT DF_Satislar_FisNo_Mig DEFAULT(0);
END

-- Mevcut kayıtlarda FisNo boş/0 kalmışsa ID ile doldur
UPDATE dbo.Satislar SET FisNo = ID WHERE FisNo = 0;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Satislar_FisNo' AND object_id = OBJECT_ID('dbo.Satislar'))
    CREATE INDEX IX_Satislar_FisNo ON dbo.Satislar(FisNo);
", connection);

                migrateSatislarFisNo.ExecuteNonQuery();

                // Stok tahmin sonuç tablosu
                var createTahminler = new SqlCommand(@"
IF OBJECT_ID(N'dbo.StokTahminleri', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StokTahminleri
    (
        ID                   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StokTahminleri PRIMARY KEY,
        Barkod               NVARCHAR(50)      NOT NULL,
        UrunAdi              NVARCHAR(100)     NOT NULL,
        StokMiktari          INT               NOT NULL,
        OrtalamaGunlukSatis  DECIMAL(18,2)     NOT NULL,
        TahminiBitisTarihi   DATETIME          NULL,
        KalanGun             INT               NULL,
        UyariDurumu          NVARCHAR(30)      NOT NULL,
        SonGuncelleme        DATETIME          NOT NULL CONSTRAINT DF_StokTahminleri_SonGuncelleme DEFAULT(GETDATE())
    );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_StokTahminleri_Barkod' AND object_id = OBJECT_ID('dbo.StokTahminleri'))
        CREATE UNIQUE INDEX UX_StokTahminleri_Barkod ON dbo.StokTahminleri(Barkod);
END
", connection);
                createTahminler.ExecuteNonQuery();

                // Fiyat önerisi (satış trendine göre indirim / artış) sonuç tablosu
                var createFiyatOneri = new SqlCommand(@"
IF OBJECT_ID(N'dbo.FiyatOnerileri', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FiyatOnerileri
    (
        ID                   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FiyatOnerileri PRIMARY KEY,
        AnalizZamani         DATETIME          NOT NULL CONSTRAINT DF_FiyatOnerileri_Zaman DEFAULT(GETDATE()),
        Barkod               NVARCHAR(50)      NOT NULL,
        UrunAdi              NVARCHAR(100)     NOT NULL,
        MevcutFiyat          DECIMAL(18,2)     NOT NULL,
        SonDonemAdet         INT               NOT NULL,
        OncekiDonemAdet      INT               NOT NULL,
        SatisDegisimOrani    DECIMAL(18,4)     NULL,
        OneriTipi            NVARCHAR(20)      NOT NULL,
        OnerilenFiyat        DECIMAL(18,2)     NOT NULL,
        Aciklama             NVARCHAR(500)     NULL
    );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FiyatOnerileri_Barkod' AND object_id = OBJECT_ID('dbo.FiyatOnerileri'))
        CREATE INDEX IX_FiyatOnerileri_Barkod ON dbo.FiyatOnerileri(Barkod);
END
", connection);
                createFiyatOneri.ExecuteNonQuery();




            }
        }
    }
}



