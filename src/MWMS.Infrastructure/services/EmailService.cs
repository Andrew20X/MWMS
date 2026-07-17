using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MWMS.Application.Interfaces;
using MWMS.Application.Settings;
using MWMS.Application.Models;
using Microsoft.Extensions.Logging;

namespace MWMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null, List<EmailAttachment>? attachments = null)
    {
        await SendEmailAsync(new List<string> { to }, null, null, subject, htmlBody, plainTextBody, attachments);
    }

        public async Task SendEmailAsync(List<string> to, List<string>? cc, List<string>? bcc, string subject, string body, string? plainTextBody = null, List<EmailAttachment>? attachments = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                
                foreach (var email in to) message.To.Add(MailboxAddress.Parse(email));
                if (cc != null) foreach (var email in cc) message.Cc.Add(MailboxAddress.Parse(email));
                if (bcc != null) foreach (var email in bcc) message.Bcc.Add(MailboxAddress.Parse(email));

                message.Subject = subject;

                // Process body if it's not already full HTML
                var formattedBody = body;
                if (!formattedBody.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) && !formattedBody.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                {
                    // Auto-link processing for plain text-like inputs
                    if (!formattedBody.Contains("<br") && !formattedBody.Contains("<p>"))
                    {
                        formattedBody = formattedBody.Replace("\n", "<br/>");
                    }

                    var urlRegex = new System.Text.RegularExpressions.Regex(@"(http(s)?://[^\s<]+)");
                    formattedBody = urlRegex.Replace(formattedBody, "<div style='margin: 30px 0; text-align: center;'><a href='$1' style='background-color: #5b4fe8; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; font-size: 16px;'>Secure Action Link</a></div><div style='margin-top: 15px;'><span style='font-size: 13px; color: #a1a1aa; word-break: break-all;'>If the button doesn't work, copy this link: $1</span></div>");

                    string finalHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
</head>
<body style='background-color: #121212; margin: 0; padding: 40px 20px; font-family: ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #18181b; border-radius: 8px; overflow: hidden; border: 1px solid #3f3f46;'>
        
        <!-- Header -->
        <div style='text-align: center; background-color: #2c3e50; padding: 30px 20px; border-bottom: 2px solid #3f3f46;'>
            <img src='cid:profilePic' alt='Measuresoft' style='width: 70%; max-width: 300px; height: auto; display: inline-block;' />
        </div>
        
        <!-- Content -->
        <div style='padding: 40px 30px; font-size: 16px; color: #e4e4e7; line-height: 1.6;'>
            <h2 style='color: #f4f4f5; margin-top: 0; margin-bottom: 24px; font-size: 24px; font-weight: 700;'>{subject}</h2>
            {formattedBody}
        </div>
        
        <!-- Footer -->
        <div style='background-color: #2c3e50; padding: 24px 30px; text-align: center; font-size: 13px; color: #94a3b8; border-top: 2px solid #3f3f46;'>
            &copy; {DateTime.Now.Year} Measuresoft Oil Services. All rights reserved.<br/>
            <span style='color: #64748b; font-size: 12px; margin-top: 8px; display: block;'>This is an automated message, please do not reply.</span>
        </div>
        
    </div>
</body>
</html>";
                    formattedBody = finalHtml;
                }

                var builder = new BodyBuilder
                {
                    HtmlBody = formattedBody,
                    TextBody = plainTextBody ?? "This email requires an HTML capable email client."
                };

                // Add inline logo
                string imagePath = @"D:\MWMS\Email.png";
                if (System.IO.File.Exists(imagePath))
                {
                    var image = builder.LinkedResources.Add(imagePath);
                    image.ContentId = "profilePic";
                }

                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var file in attachments)
                    {
                        if (file.Data.Length > 0)
                        {
                            builder.Attachments.Add(file.FileName, file.Data, ContentType.Parse(file.ContentType));
                        }
                    }
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                var secureSocketOptions = _emailSettings.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto;
                
                await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, secureSocketOptions);
                await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", to));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", to));
                throw;
            }
        }
}
