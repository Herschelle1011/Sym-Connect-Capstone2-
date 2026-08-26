using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class MemberListViewModel
    {
        public int MemberId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }

        public string Country { get; set; } = "Philippines";
        public string Province { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Barangay { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public int Points { get; set; }

        public string PhotoUrl { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        public DateTime DateRegistered { get; set; } = DateTime.Now;

        public ICollection<MemberGroupDetailsViewModel> GroupDetailsVM { get; set; } = new List<MemberGroupDetailsViewModel>();
        public string FullName =>
                $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();

        public bool isEmailConfirmed { get; set; }
        
    }
}
