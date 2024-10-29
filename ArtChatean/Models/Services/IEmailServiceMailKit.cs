using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;

public interface IEmailServiceMailKit
{
    Task SendEmailAsync(string email, string subject, string message);
}
