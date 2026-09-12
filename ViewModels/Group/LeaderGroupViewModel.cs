namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class LeaderGroupViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int TotalMembers { get; set; }
        public List<LeaderGroupMemberViewModel> Members { get; set; } = new();
    }
}
