namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard
{
    public class MembershipSummaryViewModel
    {
        public int? GroupId { get; set; }

        public string GroupName { get; set; } = "No Group";

        public string GroupDescription { get; set; } = string.Empty;

        public int MemberCount { get; set; }
    }
}
