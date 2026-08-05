namespace MakerspaceFablabPlatform.Helpers;

public class SmtpOptions
{
    public string? Server { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    /// <summary>
    /// true = 465'teki gibi baglanti aninda SSL, false = 587'deki gibi STARTTLS.
    /// Bos birakilirsa porta bakilarak otomatik secilir.
    /// </summary>
    public bool? UseSsl { get; set; }

    /// <summary>Gonderen adresi. MimeMessage'in From basligi bundan kurulur.</summary>
    public string? From { get; set; }

    /// <summary>Gonderenin gorunen adi. Bos ise From adresi kullanilir.</summary>
    public string? DisplayName { get; set; }
}