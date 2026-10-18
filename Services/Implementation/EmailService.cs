using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Configuration;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Emails;
using System.Net;
using System.Net.Mail;
namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _configuration;
        private readonly IRazorViewToStringRenderer _viewRenderer;

        public EmailService(IOptions<EmailSettings> emailSettings, IConfiguration configuration, IRazorViewToStringRenderer viewRenderer)
        {
            _emailSettings = emailSettings.Value;
            _configuration = configuration;
            _viewRenderer = viewRenderer;
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
            string loginUrl = "https://excuse-fastball-crunchy.ngrok-free.dev/Identity/Account/Login?returnUrl=%2FLogin%2FAccount";

            string body = status switch
            {
                MembershipApplicationStatus.Approved => $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2e7d32;'>Congratulations, {fullName}!</h2>
                    <p>We're excited to let you know that your <strong>SYM-Connect membership application has been approved</strong>.</p>
                    <p>You can now log in and start exploring your membership benefits.</p>
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{loginUrl}' style='background-color: #2e7d32; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Log In to Your Account</a>
                    </p>
                    <p>If the button above doesn't work, copy and paste this link into your browser:</p>
                    <p><a href='{loginUrl}'>{loginUrl}</a></p>
                    <p>Welcome aboard!</p>
                    <p style='color: #777; font-size: 12px; margin-top: 40px;'>— The SYM-Connect Team</p>
                </div>
            </body>
            </html>",

                MembershipApplicationStatus.Rejected => $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #c62828;'>Membership Application Update</h2>
                    <p>Hi {fullName},</p>
                    <p>Thank you for applying for SYM-Connect membership. Unfortunately, we're unable to approve your application at this time.</p>
                    {(string.IsNullOrWhiteSpace(reason) ? "" : $"<p><strong>Reason:</strong> {reason}</p>")}
                    <p>If you believe this was a mistake or would like more information, feel free to reach out to our support team.</p>
                    <p style='color: #777; font-size: 12px; margin-top: 40px;'>— The SYM-Connect Team</p>
                </div>
            </body>
            </html>",

                MembershipApplicationStatus.Pending => $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #f9a825;'>Application Under Review</h2>
                    <p>Hi {fullName},</p>
                    <p>Your SYM-Connect membership application is currently <strong>under review</strong>. We'll notify you as soon as a decision has been made.</p>
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{loginUrl}' style='background-color: #f9a825; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Check Your Status</a>
                    </p>
                    <p style='color: #777; font-size: 12px; margin-top: 40px;'>— The SYM-Connect Team</p>
                </div>
            </body>
            </html>",

                _ => $@"
            <html>
            <body style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2>Membership Application Update</h2>
                    <p>Hi {fullName},</p>
                    <p>There has been an update to your SYM-Connect membership application.</p>
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='{loginUrl}' style='background-color: #1976d2; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Log In to Check Details</a>
                    </p>
                    <p style='color: #777; font-size: 12px; margin-top: 40px;'>— The SYM-Connect Team</p>
                </div>
            </body>
            </html>"
            };

            await SendEmailAsync(email, subject, body); // reuses your working helper
        }


        public async Task SendPasswordResetEmailAsync(string email, string fullName, string resetLink)
        {
            string subject = "Reset your SYM-Connect password";

            var model = new PasswordResetEmailViewModel
            {
                FirstName = fullName,
                ResetLink = resetLink
            };

            string body = await _viewRenderer.RenderViewToStringAsync("/Views/EmailTemplates/PasswordResetEmail.cshtml", model);

            await SendEmailAsync(email, subject, body);
        }
    }

}

