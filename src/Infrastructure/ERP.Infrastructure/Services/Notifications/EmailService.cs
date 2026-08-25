using System.Net;
using System.Net.Mail;
using ERP.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services.Notifications;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var smtpPort = int.TryParse(_configuration["Smtp:Port"], out var port) ? port : 587;
        var smtpUser = _configuration["Smtp:UserName"] ?? "noreply@kirtasiye-erp.com";
        var smtpPass = _configuration["Smtp:Password"] ?? "";
        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@kirtasiye-erp.com";
        var fromName = _configuration["Smtp:FromName"] ?? "Kırtasiye ERP Sistemi";

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                Timeout = 5000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            // Geliştirme ortamında şifre tanımlı değilse simüle et ve logla
            if (string.IsNullOrWhiteSpace(smtpPass) || smtpUser.Contains("noreply@kirtasiye-erp.com"))
            {
                _logger.LogInformation("[EMAIL SIMULATION] To: {ToEmail} | Subject: {Subject} | Body length: {Length} chars",
                    toEmail, subject, htmlBody.Length);
                return;
            }

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("E-posta başarıyla gönderildi: {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "E-posta gönderimi başarısız oldu (Simülasyona devam ediliyor): {ToEmail} - {Message}", toEmail, ex.Message);
        }
    }
}
