using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailServiceMailKit : IEmailServiceMailKit
{
    private readonly IConfiguration _configuration;

    public EmailServiceMailKit(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var port = int.Parse(_configuration["EmailSettings:Port"]);
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];
        var from = _configuration["EmailSettings:From"];

        using (var client = new SmtpClient(smtpServer, port))
        {
            client.Credentials = new NetworkCredential(username, password);
            client.EnableSsl = true;

            var mailMessage = new MailMessage(from, email, subject, message);
            await client.SendMailAsync(mailMessage);
        }
    }
}