namespace Sym_Connect__Capstone2_.Models
{
    public class EmailVerification
    {
        public int EmailVerificationId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string VerificationCode { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsVerified { get; set; }
    }
}
