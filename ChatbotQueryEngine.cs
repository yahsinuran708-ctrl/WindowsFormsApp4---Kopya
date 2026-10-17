using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace WindowsFormsApp4
{
    internal sealed class ChatbotQueryEngine
    {
        private readonly string _connectionString;

        public ChatbotQueryEngine(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public string Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "Soruyu boş bırakamazsınız.";

            var q = question.Trim();
            var qLower = q.ToLowerInvariant();

            try
            {
                // Basit ama işe yarar örnekler.
                if (qLower.Contains("stok") || qLower.Contains("stokta") || qLower.Contains("kaç adet"))
                {
                    // Kullanıcı örn: "Barkod 1234 stok nedir" veya "stok elma" gibi yazabilir.
                    var barkod = ExtractBarkod(q);
                    if (!string.IsNullOrWhiteSpace(barkod))
                        return GetStokByBarkod(barkod);

                    var urunAdi = ExtractProductNameHeuristic(q);
                    if (!string.IsNullOrWhiteSpace(urunAdi))
                        return GetStokByUrunAdiLike(urunAdi);

                    return "Stok sorgusu için barkod veya ürün adı içeren bir ifade girin.";
                }

                if (qLower.Contains("fiyat") || qLower.Contains("satış"))
                {
                    var barkod = ExtractBarkod(q);
                    if (!string.IsNullOrWhiteSpace(barkod))
                        return GetFiyatByBarkod(barkod);

                    var urunAdi = ExtractProductNameHeuristic(q);
                    if (!string.IsNullOrWhiteSpace(urunAdi))
                        return GetFiyatByUrunAdiLike(urunAdi);

                    return "Fiyat sorgusu için barkod veya ürün adı içeren bir ifade girin.";
                }

                if (qLower.Contains("kategori"))
                {
                    var urunAdi = ExtractProductNameHeuristic(q);
                    if (!string.IsNullOrWhiteSpace(urunAdi))
                        return GetKategoriByUrunAdiLike(urunAdi);

                    return "Kategori sorgusu için ürün adı yazın.";
                }

                // Yeni kurallar: en az kalan / en çok satan / bugün en çok satan
                if (qLower.Contains("en az") && (qLower.Contains("stok") || qLower.Contains("ürün") || qLower.Contains("urun")))
                {
                    return GetEnAzStokluUrun(1);
                }

                if ((qLower.Contains("en çok") || qLower.Contains("en cok")) && (qLower.Contains("sat") || qLower.Contains("satis")))
                {
                    // Genel: tüm zaman
                    return GetEnCokSatanUrun(null);
                }

                if ((qLower.Contains("bugün") || qLower.Contains("bugun") || qLower.Contains("today")) && (qLower.Contains("en çok") || qLower.Contains("en cok")) && (qLower.Contains("sat") || qLower.Contains("satis")))
                {
                    // Bugün: gün bazlı satış
                    return GetEnCokSatanUrun(DateTime.Today);
                }

                return "Anladığım kadarıyla bu soru için hazır kuralım yok. Örn: 'stok nedir', 'barkod 123 stok', 'fiyat nedir' gibi yazabilirsiniz.";
            }
            catch (Exception ex)
            {
                return "Sorgu sırasında hata oluştu: " + ex.Message;
            }
        }

        private static string ExtractBarkod(string question)
        {
            // Barkod genelde sayısal/harf karışık 6-20 arası olabilir.
            // Kaba yaklaşım: "barkod" kelimesinden sonra gelen ilk token.
            var lower = question.ToLowerInvariant();
            var idx = lower.IndexOf("barkod", StringComparison.Ordinal);
            if (idx < 0) return null;

            var after = question.Substring(idx + "barkod".Length).Trim();
            if (after.Length == 0) return null;

            var parts = after.Split(new[] { ' ', '\t', '\r', '\n', ':', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;
            return parts[0];
        }

        private static string ExtractProductNameHeuristic(string question)
        {
            // Çok basit yaklaşım:
            // "urun elma" / "elma stok" gibi durumlarda ilk uzun kelimeyi dene.
            var cleaned = question.Trim();
            cleaned = cleaned.Replace("stok", " ").Replace("fiyat", " ").Replace("satış", " ");
            cleaned = cleaned.Replace("barkod", " ");
            cleaned = cleaned.Replace("nedir", " ").Replace("kaç", " ").Replace("adet", " ");
            cleaned = cleaned.Replace("\t", " ");

            var tokens = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                if (t.Length >= 3 && t.Length <= 60)
                    return t;
            }
            return null;
        }

        private string GetStokByBarkod(string barkod)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 UrunAdi, StokMiktari
FROM dbo.Urunler
WHERE Barkod = @barkod", con))
            {
                cmd.Parameters.AddWithValue("@barkod", barkod);
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                        return "Barkod bulunamadı: " + barkod;

                    var adi = r["UrunAdi"].ToString();
                    var stok = Convert.ToInt32(r["StokMiktari"]);
                    return string.Format("{0} stok: {1}", adi, stok);

                }
            }
        }

        private string GetStokByUrunAdiLike(string urunAdi)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 5 UrunAdi, Barkod, StokMiktari
FROM dbo.Urunler
WHERE UrunAdi LIKE @u
ORDER BY UrunAdi", con))
            {
                cmd.Parameters.AddWithValue("@u", "%" + urunAdi + "%");
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Eşleşen ürünler (stok):");
                    var any = false;
                    while (r.Read())
                    {
                        any = true;
                        sb.AppendLine(string.Format("- {0} ({1}): {2}", r["UrunAdi"], r["Barkod"], r["StokMiktari"]));

                    }
                    if (!any) return "Ürün bulunamadı: " + urunAdi;
                    return sb.ToString().TrimEnd();
                }
            }
        }

        private string GetFiyatByBarkod(string barkod)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 1 UrunAdi, SatisFiyat
FROM dbo.Urunler
WHERE Barkod = @barkod", con))
            {
                cmd.Parameters.AddWithValue("@barkod", barkod);
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                        return "Barkod bulunamadı: " + barkod;

                    var adi = r["UrunAdi"].ToString();
                    var fiyat = Convert.ToDecimal(r["SatisFiyat"]);
                    return string.Format("{0} satış fiyatı: {1:0.00}", adi, fiyat);

                }
            }
        }

        private string GetFiyatByUrunAdiLike(string urunAdi)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 5 UrunAdi, Barkod, SatisFiyat
FROM dbo.Urunler
WHERE UrunAdi LIKE @u
ORDER BY UrunAdi", con))
            {
                cmd.Parameters.AddWithValue("@u", "%" + urunAdi + "%");
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Eşleşen ürünler (fiyat):");
                    var any = false;
                    while (r.Read())
                    {
                        any = true;
                        sb.AppendLine(string.Format("- {0} ({1}): {2:0.00}", r["UrunAdi"], r["Barkod"], Convert.ToDecimal(r["SatisFiyat"])));


                    }

                    if (!any) return "Ürün bulunamadı: " + urunAdi;
                    return sb.ToString().TrimEnd();
                }
            }
        }

        private string GetKategoriByUrunAdiLike(string urunAdi)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 5 UrunAdi, Barkod, Kategori
FROM dbo.Urunler
WHERE UrunAdi LIKE @u
ORDER BY UrunAdi", con))
            {
                cmd.Parameters.AddWithValue("@u", "%" + urunAdi + "%");
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Eşleşen ürünler (kategori):");
                    var any = false;
                    while (r.Read())
                    {
                        any = true;
                        var kategori = r["Kategori"] == DBNull.Value ? "(boş)" : r["Kategori"].ToString();
                        sb.AppendLine(string.Format("- {0} ({1}): {2}", r["UrunAdi"], r["Barkod"], kategori));
                    }

                    if (!any) return "Ürün bulunamadı: " + urunAdi;
                    return sb.ToString().TrimEnd();
                }
            }
        }

        private string GetEnAzStokluUrun(int take)
        {
            if (take <= 0) take = 1;

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP (@take) UrunAdi, Barkod, StokMiktari
FROM dbo.Urunler
WHERE StokMiktari IS NOT NULL
ORDER BY StokMiktari ASC, UrunAdi ASC", con))
            {
                cmd.Parameters.AddWithValue("@take", take);
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                        return "Kayıt bulunamadı (stoklar boş olabilir).";

                    var sb = new StringBuilder();
                    sb.AppendLine("En az kalan ürün:");
                    var any = false;

                    do
                    {
                        any = true;
                        var adi = r["UrunAdi"].ToString();
                        var barkod = r["Barkod"].ToString();
                        var stok = Convert.ToInt32(r["StokMiktari"]);
                        sb.AppendLine($"- {adi} ({barkod}): {stok} adet");
                    } while (r.Read());

                    if (!any) return "Kayıt bulunamadı.";
                    return sb.ToString().TrimEnd();
                }
            }
        }

        private string GetEnCokSatanUrun(DateTime? date)
        {
            // date null => tüm zaman
            // date => sadece o gün
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 5
    u.UrunAdi,
    u.Barkod,
    SUM(s.SatisAdeti) AS ToplamAdet
FROM dbo.Satislar s
INNER JOIN dbo.Urunler u ON u.Barkod = s.UrunKodu
WHERE (@date IS NULL)
   OR (CONVERT(date, s.SatisTarihi) = @date)
GROUP BY u.UrunAdi, u.Barkod
ORDER BY ToplamAdet DESC, u.UrunAdi ASC", con))
            {
                var dParam = date.HasValue ? (object)date.Value.Date : DBNull.Value;
                cmd.Parameters.AddWithValue("@date", dParam);
                con.Open();

                using (var r = cmd.ExecuteReader())
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(date.HasValue ? "Bugün en çok satılan ürün:" : "Tüm zamanlar en çok satılan ürün:" );

                    var any = false;
                    while (r.Read())
                    {
                        any = true;
                        var adi = r["UrunAdi"].ToString();
                        var barkod = r["Barkod"].ToString();
                        var adet = Convert.ToInt32(r["ToplamAdet"]);
                        sb.AppendLine($"- {adi} ({barkod}): {adet} adet");
                    }

                    if (!any)
                        return date.HasValue ? "Bugün için satış kaydı bulunamadı." : "Satış kaydı bulunamadı.";

                    return sb.ToString().TrimEnd();
                }
            }
        }
    }
}


