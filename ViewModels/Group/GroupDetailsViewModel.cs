using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class GroupDetailsViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LeaderName { get; set; } = string.Empty;
        public string LeaderContact { get; set; } = string.Empty;
        public int? LeaderId { get; set; }
        public string? PhotoUrl { get; set; }

        public DateTime? DateJoined { get; set; }


        [EmailAddress]
        public string LeaderEmail { get; set; } = string.Empty;

        public List<GroupMemberViewModel> Members { get; set; } = new List<GroupMemberViewModel>();

        public int TotalMembers => Members.Count;
        public int ActiveMembers => Members.Count(m => m.Status == "Active");
        public int InactiveMembers => Members.Count(m => m.Status == "Inactive");

        public int TotalPoints { get; set; }

        public int? CurrentMemberId { get; set; }
    }
}
