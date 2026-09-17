namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Events
{
    public class MemberEventsViewModel
    {
        public int? GroupId { get; set; }

        public string GroupName { get; set; } = "No Group";

        public int GroupMemberCount { get; set; }


        // Events assigned to the member's group
        public List<MemberEventItemViewModel> Events { get; set; }
            = new List<MemberEventItemViewModel>();
    }
}
