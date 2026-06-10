using Sym_Connect__Capstone2_.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.ViewModels
{
    public class MemberViewModel
    {
       public class MemberListViewModel
        {
            public int MemberId { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string? MiddleName { get; set; } = string.Empty;
    
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;

            public string ContactNumber { get; set; } = string.Empty;
            public string? Address { get; set; } = string.Empty;
            public DateTime BirthDate { get; set; }

            public string Gender { get; set; } = string.Empty;
            public string Status { get; set; } = "Active";

            public DateTime DateRegistered { get; set; } = DateTime.Now;

            public ICollection<GroupDetails> GroupDetails { get; set; } = new List<GroupDetails>();
            public string FullName =>
          $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
        }

        public class MembersCreateViewModel
        {
            [Required(ErrorMessage = "Email is required")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Firstname is required.")]
            public string FirstName { get; set; } = string.Empty;

            public string? MiddleName { get; set; }
            [Required(ErrorMessage = "Lastname is required.")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Contactnumber is required.")]
            public string ContactNumber { get; set; } = string.Empty;
            public string? Address { get; set; } = string.Empty;
            [Required(ErrorMessage = "BirthDate is required.")]
            public DateTime BirthDate { get; set; }

            [Required(ErrorMessage = "Gender is required.")]
            public string Gender { get; set; } = string.Empty;

            [Required]
            public string Status { get; set; } = "Active";
        }

        public class MembersEditViewModel
        {
            public int memberId { get; set; }

            [Required(ErrorMessage = "Email is required")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Firstname is required.")]
            public string FirstName { get; set; } = string.Empty;

            public string? MiddleName { get; set; }
            [Required(ErrorMessage = "Lastname is required.")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Contactnumber is required.")]
            public string ContactNumber { get; set; } = string.Empty;
            public string? Address { get; set; } = string.Empty;
            [Required(ErrorMessage = "BirthDate is required.")]
            public DateTime BirthDate { get; set; }

            [Required]
            public string Gender { get; set; } = string.Empty;

            [Required]
            public string Status { get; set; } = "Active";
        }

        public class MemberDeleteViewModel
        {
            public int memberId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public int? groupId { get; set; }
            public string? groupName { get; set; }
        }
    }
}
