namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IEmailVerificationService
    {
        Task<(bool Success, string Message)> SendCodeAsync(
          string email,
          string? excludeUserId = null);

        Task<(bool Success, string Message)> VerifyCodeAsync(
            string email,
            string code);
    }
}
