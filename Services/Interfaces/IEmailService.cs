using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.ViewModels.Emails;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);

        Task SendMembershipStatusEmailAsync(string email, MembershipApplicationStatus status);
        Task SendVerificationCodeAsync(string email, string code);
    }
}
