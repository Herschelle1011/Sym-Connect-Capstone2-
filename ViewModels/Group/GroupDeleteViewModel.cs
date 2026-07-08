namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class GroupDeleteViewModel
    {

        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string LeaderName { get; set; } = string.Empty;

        public int TotalMembers { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
