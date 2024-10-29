using ArtChatean.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ArtChatean
{
    public interface IEmailService
    {
        Task SendEmailWithAttachmentsAsync(string toEmail, string subject, string body, List<byte[]> attachments);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailWithAttachmentsAsync(string toEmail, string subject, string body, List<byte[]> attachments)
        {
            var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.Username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            // Додаємо PDF як вкладення
            for (int i = 0; i < attachments.Count; i++)
            {
                var attachment = new Attachment(new MemoryStream(attachments[i]), $"Ticket-{i + 1}.pdf", "application/pdf");
                mailMessage.Attachments.Add(attachment);
            }

            await client.SendMailAsync(mailMessage);
        }
    }
}
