using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels
{
    public class GroupViewModel
    {

        //LIST OF GROUPS
        public class GroupListViewModel
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string LeaderName { get; set; } = string.Empty;

            [EmailAddress]
            public string LeaderEmail { get; set; } = string.Empty;

            public List<GroupMemberViewModel> Members { get; set; } = new();
        }

        public class GroupMemberViewModel
        {
            public int MemberId { get; set; }

            public string FullName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string ContactNumber { get; set; } = string.Empty;

            public string Status { get; set; } = string.Empty;

            public DateTime DateRegistered { get; set; }
        }
        public class GroupDetailsViewModel
        {
            public int GroupId { get; set; }
            public string GroupName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string LeaderName { get; set; } = string.Empty;
            public string LeaderContact { get; set; } = string.Empty;

            [EmailAddress]
            public string LeaderEmail { get; set; } = string.Empty;

            public List<GroupMemberViewModel> Members { get; set; } = new List<GroupMemberViewModel>();

            public int TotalMembers => Members.Count;
            public int ActiveMembers => Members.Count(m => m.Status == "Active");
            public int InactiveMembers => Members.Count(m => m.Status == "Inactive");

        }

        //CREATE VIEW MODEL
        public class GroupCreateViewModel
        {
            public int GroupId { get; set; }

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

            // the dropdown list of members
            public SelectList? LeaderOptions { get; set; }
        }

        //EDITVIEW MODEL
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

        //DELETE MODAL = INFO FOR CONFIRM DELETE
        public class GroupDeleteViewModel {
            
            public int GroupId { get; set; }
            public string GroupName { get; set; } = string.Empty;
            public string LeaderName { get; set; } = string.Empty;

            public int TotalMembers { get; set; }

            public string Status { get; set; } = string.Empty;
        }



  

    }
}
