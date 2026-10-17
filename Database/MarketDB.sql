/*
  MarketDB kurulum scripti

  Çalıştırma:
  - SSMS'te bu dosyayı aç -> Execute
  - Ya da sqlcmd (kuruluysa):
    sqlcmd -S . -E -i "MarketDB.sql"
*/

IF DB_ID(N'MarketDB') IS NULL
BEGIN
    CREATE DATABASE MarketDB;
END
GO

USE MarketDB;
GO

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
        StokMiktari  INT               NOT NULL CONSTRAINT DF_Urunler_Stok DEFAULT(0)
    );

    CREATE UNIQUE INDEX UX_Urunler_Barkod ON dbo.Urunler(Barkod);
END
GO

IF OBJECT_ID(N'dbo.Satislar', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Satislar
    (
        ID          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Satislar PRIMARY KEY,
        FisNo       INT                NOT NULL CONSTRAINT DF_Satislar_FisNo DEFAULT(0),
        UrunKodu    NVARCHAR(50)       NOT NULL,
        UrunAdi     NVARCHAR(100)      NOT NULL,
        SatisAdeti  INT                NOT NULL,
        SatisTarihi DATETIME           NOT NULL,
        ToplamFiyat DECIMAL(18,2)      NOT NULL
    );

    CREATE INDEX IX_Satislar_Tarih ON dbo.Satislar(SatisTarihi DESC);
    CREATE INDEX IX_Satislar_FisNo ON dbo.Satislar(FisNo);
END
GO

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

    CREATE INDEX IX_FiyatOnerileri_Barkod ON dbo.FiyatOnerileri(Barkod);
END
GO

