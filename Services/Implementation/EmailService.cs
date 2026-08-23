using Microsoft.Extensions.Options;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Configuration;
using Sym_Connect__Capstone2_.Services.Interfaces;
using System.Net;
using System.Net.Mail;
namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mailMessage = new MailMessage();

            mailMessage.From = new MailAddress(
                _emailSettings.SenderEmail,
                _emailSettings.SenderName
                );

            mailMessage.To.Add(toEmail);
            mailMessage.Subject = subject;
            mailMessage.Body = body;

            mailMessage.IsBodyHtml = true;

            using var smtpClient = new SmtpClient(
                _emailSettings.SmtpServer,
                _emailSettings.Port
            );

            smtpClient.Credentials = new NetworkCredential(
                _emailSettings.Username,
                _emailSettings.Password
            );

            smtpClient.EnableSsl = true;

            await smtpClient.SendMailAsync(mailMessage);

        }
    }
}
