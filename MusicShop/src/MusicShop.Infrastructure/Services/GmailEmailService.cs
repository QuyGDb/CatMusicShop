using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MusicShop.Application.Common.Interfaces.Repositories;
using MusicShop.Application.Common.Interfaces.Services;
using MusicShop.Infrastructure.Settings;

namespace MusicShop.Infrastructure.Services;

public sealed class GmailEmailService(IOptions<EmailSettings> emailSettings) : IEmailService
{
    private readonly EmailSettings _settings = emailSettings.Value;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        MimeMessage message = new();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.Email));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        BodyBuilder bodyBuilder = new()
        {
            HtmlBody = body
        };
        message.Body = bodyBuilder.ToMessageBody();

        using SmtpClient client = new();
        
        // Gmail SMTP usually uses StartTLS on port 587
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        
        await client.AuthenticateAsync(_settings.Email, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

