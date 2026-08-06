namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Profile
{
    public class ProfileViewModel
    {
        public int memberId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string MyQrToken { get; set; } = string.Empty;

        public string PhotoUrl { get; set; } = string.Empty;
    }
}
