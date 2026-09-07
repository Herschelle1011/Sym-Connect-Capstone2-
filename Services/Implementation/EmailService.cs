using Microsoft.Extensions.Options;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Configuration;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using System.Net;
using System.Net.Mail;
namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _configuration;

        public EmailService(IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
        }


        public async Task SendVerificationCodeAsync(string email, string code)
        {
            string subject = "SYM-Connect Email Verification";
            string body =
                $"Your SYM-Connect verification code is: {code}<br><br>" +
                "This code will expire in 10 minutes.";

            await SendEmailAsync(email, subject, body); 
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

        public async Task SendMembershipStatusEmailAsync(string email, string fullName, MembershipApplicationStatus status, string? reason = null)
        {
            string subject = "SYM-Connect Membership Update";
            string body = status switch
            {
                MembershipApplicationStatus.Approved => "Congrats, your membership has been approved!",
                MembershipApplicationStatus.Rejected => string.IsNullOrWhiteSpace(reason)
                    ? $"Hi {fullName}, unfortunately your application was not approved this time."
                    : $"Hi {fullName}, unfortunately your application was not approved this time. Reason: {reason}",
                MembershipApplicationStatus.Pending => "Your application is still under review.",
                _ => "There is an update to your membership application."
            };

            await SendEmailAsync(email, subject, body); // reuses your working helper
        }
    }
}
