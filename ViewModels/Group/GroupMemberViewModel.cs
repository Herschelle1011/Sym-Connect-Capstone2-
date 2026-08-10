namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class GroupMemberViewModel
    {
        public int? MemberId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime DateRegistered { get; set; }
    }
}
