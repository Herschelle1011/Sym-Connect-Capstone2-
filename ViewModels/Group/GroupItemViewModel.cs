namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class GroupItemViewModel
    {
        public int groupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int? LeaderId { get; set; }
        public string FullName { get; set; } = string.Empty;

        // This closes your open definition using a clean, dedicated child view model
        public ICollection<GroupMemberItemViewModel> Members { get; set; } = new List<GroupMemberItemViewModel>();
    }

    public class GroupMemberItemViewModel
    {
        public int MemberId { get; set; }
        public string MemberFullName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }
}
