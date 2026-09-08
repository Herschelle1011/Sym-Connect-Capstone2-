using Sym_Connect__Capstone2_.ViewModels.Group;

namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class MemberDetailsViewModel
    {
        public int memberId { get; set; }
        //personal info
        public string PhotoUrl { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }


        //contactInfo
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;

        public string? Barangay { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }

        public string Address { get; set; } = string.Empty;
        //memberShip
        public string GroupName { get; set; } = string.Empty;

        public List<GroupDetailsViewModel> groupDetails { get; set; } = new();

        public int EventAttendedCount { get; set; }

        public DateTime? DateJoinedGroup { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; } = false;     
        public DateTime? DateApplied { get; set; }

    }

}
