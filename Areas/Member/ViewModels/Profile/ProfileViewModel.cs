namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Profile
{
    public class ProfileViewModel
    {
        public int memberId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Header info
        public string GroupName { get; set; } = string.Empty;     // e.g. "Rondalla Group"
        public DateTime MemberSince { get; set; }

        // Contact / details
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public DateTime? Birthdate { get; set; }
        public string Address { get; set; } = string.Empty;

        // QR
        public string MyQrToken { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;

        // Recent activities
        public List<ActivityLogViewModel> RecentActivities { get; set; } = new();
    }
}
