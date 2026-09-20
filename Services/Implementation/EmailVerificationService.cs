using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;

        public EmailVerificationService(
            AppDbContext context,
            IEmailService emailService,
            UserManager<User> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }


        public async Task<(bool Success, string Message)> SendCodeAsync(
            string email,
            string? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Please enter your email address.");
            }

            email = email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null &&
                existingUser.Id != excludeUserId)
            {
                return (false, "This email address is already registered.");
            }

            var code = Random.Shared.Next(100000, 999999).ToString();

            var existingVerification = await _context.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email == email);

            if (existingVerification == null)
            {
                existingVerification = new EmailVerification
                {
                    Email = email,
                    VerificationCode = code,
                    ExpiresAt = DateTime.Now.AddMinutes(10),
                    IsVerified = false
                };

                _context.EmailVerifications.Add(existingVerification);
            }
            else
            {
                existingVerification.VerificationCode = code;
                existingVerification.ExpiresAt = DateTime.Now.AddMinutes(10);
                existingVerification.IsVerified = false;
            }

            await _context.SaveChangesAsync();

            await _emailService.SendVerificationCodeAsync(email, code);

            return (true, $"A verification code was sent to {email}.");
        }

        public async Task<(bool Success, string Message)> VerifyCodeAsync(
            string email,
            string code)
        {
            var verification = await _context.EmailVerifications
                .FirstOrDefaultAsync(v =>
                    v.Email == email &&
                    v.VerificationCode == code);

            if (verification == null)
            {
                return (false, "Invalid verification code.");
            }

            if (verification.ExpiresAt < DateTime.Now)
            {
                return (false, "Verification code has expired.");
            }

            verification.IsVerified = true;
            await _context.SaveChangesAsync();

            return (true, "Email verified successfully.");
        }
    }
}

