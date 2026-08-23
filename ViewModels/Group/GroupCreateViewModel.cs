using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class GroupCreateViewModel
    {

        [Required(ErrorMessage = "Group Name is Required")]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is Required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is Required")]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Leader")]
        public int? LeaderId { get; set; }

        public string? SelectedLeaderName { get; set; }
        public string? SelectedLeaderEmail { get; set; }
        public int? SelectedLeaderPoints { get; set; }

    }
}
