namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class RemoveMemberViewModel
    {
        public int groupId { get; set; }
        public int memberId { get; set; }

        public string MemberName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
    }
}
