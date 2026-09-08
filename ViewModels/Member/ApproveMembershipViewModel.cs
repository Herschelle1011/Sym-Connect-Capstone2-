namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class ApproveMembershipViewModel
    {
        public int MemberId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string GroupName { get; set; } = string.Empty;

        public string? Reason { get; set; }
    }


    public class RejectMembershipViewModel
    {
        public int MemberId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }
}
