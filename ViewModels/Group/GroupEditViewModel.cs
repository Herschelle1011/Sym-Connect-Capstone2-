using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class GroupEditViewModel
    {
        public int GroupId { get; set; }

        [Required(ErrorMessage = "GroupName is required.")]
        [Display(Name = "Group Name")]
        public string GroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Leader")]
        public int? LeaderId { get; set; }

        // e dropdown list of members
        public SelectList? LeaderOptions { get; set; }
    }
}
