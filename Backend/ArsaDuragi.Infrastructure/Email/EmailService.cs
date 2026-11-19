using System.Net;
using System.Net.Mail;
using ArsaDuragi.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArsaDuragi.Infrastructure.Email;

/// <summary>
/// E-mail abstraction implementation. Sends through SMTP only when Email:Enabled=true, otherwise it just logs.
/// It never throws: a failing mail server must not break approving a listing.
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogInformation("E-mail not sent (SMTP disabled). To: {To}, Subject: {Subject}", to, subject);
            return;
        }

        try
        {
            using var message = new MailMessage(new MailAddress(_options.FromAddress, _options.FromName), new MailAddress(to))
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = string.IsNullOrWhiteSpace(_options.Username)
                    ? null
                    : new NetworkCredential(_options.Username, _options.Password)
            };

            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send e-mail to {To}", to);
        }
    }
}
