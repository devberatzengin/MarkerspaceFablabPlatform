namespace MakerspaceFablabPlatform.Helpers;

public static class DateTimeExtensions
{
    /// <summary>
    /// Gelen tarihi UTC'ye çevirir. API sözleşmesi: zaman dilimi belirtilmemiş
    /// (Kind = Unspecified) bir tarih zaten UTC kabul edilir; sunucunun yerel
    /// saatine göre kaydırılmaz. Doğrulama ve servis katmanı aynı kuralı
    /// kullanmalı, aksi halde ikisi arasında saat farkı oluşur.
    /// </summary>
    public static DateTime ToUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
