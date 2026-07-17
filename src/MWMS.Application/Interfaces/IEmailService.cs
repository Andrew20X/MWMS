using MWMS.Application.Models;

namespace MWMS.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null, List<EmailAttachment>? attachments = null);
    
    Task SendEmailAsync(List<string> to, List<string>? cc, List<string>? bcc, string subject, string htmlBody, string? plainTextBody = null, List<EmailAttachment>? attachments = null);
}
