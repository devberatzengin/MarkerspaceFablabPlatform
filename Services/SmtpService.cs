using MailKit.Net.Smtp;
using MailKit.Security;
using MakerspaceFablabPlatform.Helpers;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MakerspaceFablabPlatform.Services;

public class SmtpService
{
    private readonly SmtpOptions _options;

    public SmtpService(IOptionsMonitor<SmtpOptions> options)
    {
        _options = options.CurrentValue;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Server))
            throw new InvalidOperationException("Smtp:Server yapılandırılmamış.");

        if (string.IsNullOrWhiteSpace(_options.From))
            throw new InvalidOperationException("Smtp:From yapılandırılmamış.");

        var mail = new MimeMessage
        {
            Subject = subject,
            Date = DateTime.UtcNow
        };

        mail.From.Add(new MailboxAddress(_options.DisplayName ?? _options.From, _options.From));
        mail.To.Add(MailboxAddress.Parse(to));
        mail.Body = new TextPart("plain") { Text = body };

        // Timeout olmazsa cevap vermeyen bir sunucu istegi suresiz bloklar
        using var client = new SmtpClient { Timeout = 15_000 };

        var secureOptions = _options.UseSsl switch
        {
            true => SecureSocketOptions.SslOnConnect,
            false => SecureSocketOptions.StartTlsWhenAvailable,
            null => SecureSocketOptions.Auto
        };

        await client.ConnectAsync(_options.Server, _options.Port ?? 587, secureOptions, cancellationToken);

        if (!string.IsNullOrEmpty(_options.Username))
            await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty, cancellationToken);

        await client.SendAsync(mail, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}