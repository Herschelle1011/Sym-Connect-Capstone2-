using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class GroupListViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LeaderName { get; set; } = string.Empty;

        [EmailAddress]
        public string LeaderEmail { get; set; } = string.Empty;

        public List<GroupMemberViewModel> Members { get; set; } = new();
        public int MemberCount { get; set; }
    }
}
