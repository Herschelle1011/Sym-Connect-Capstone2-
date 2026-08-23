namespace Sym_Connect__Capstone2_.ViewModels
{
    public class OperationalResult
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }

        public string? UserId { get; set; }

        public string PasswordToken { get; set; } = string.Empty;

        public string? EmailConfirmationToken { get; set; }
    }
}
