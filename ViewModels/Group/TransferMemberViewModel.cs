using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class TransferMemberViewModel
    {
        [Required]
        public int memberId { get; set; }
        [Required(ErrorMessage = "Select a new group")]
        public int groupId { get; set; }

        public int currentGroupId { get; set; }
        public int newGroupId { get; set; }

        public List<GroupOptionViewModel>? availableGroups { get; set; } = new();
       
    }
    public class GroupOptionViewModel
    {
        public int groupId { get; set; }
        public string groupName { get; set; } = string.Empty;
        public string memberName { get; set; }

    }
}
