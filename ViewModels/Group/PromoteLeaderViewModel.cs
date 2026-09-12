namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class PromoteLeaderViewModel
    {
        public int GroupId { get; set; }
        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;

        public string CurrentLeaderName { get; set; } = string.Empty;


    }
}
