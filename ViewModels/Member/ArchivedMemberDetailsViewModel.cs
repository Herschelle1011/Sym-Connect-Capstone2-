namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class ArchivedMemberDetailsViewModel
    {
        public int MemberId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public string ContactNumber { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public string Address { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string GroupName { get; set; } 

        public DateTime? MemberSince { get; set; }

        public DateTime? ArchiveDate { get; set; }
    }
    
}
