using System.Diagnostics.Contracts;

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
        public string Country { get; set; } = "Philippines";
        public string Province { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Barangay { get; set; } = string.Empty;

        public string Address =>
            string.Join(", ", new[] { Barangay, City, Province, Country }
                .Where(p => !string.IsNullOrWhiteSpace(p) && p != "N/A"));
        // QR
        public string MyQrToken { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;

        // Recent activities
        public List<ActivityLogViewModel> RecentActivities { get; set; } = new();
    }
}
