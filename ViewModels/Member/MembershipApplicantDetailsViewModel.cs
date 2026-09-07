using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class MembershipApplicantDetailsViewModel
    {
        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = string.Empty;

        public string FullName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(MiddleName))
                {
                    return $"{FirstName} {MiddleName[0]}. {LastName}";
                }

                return $"{FirstName} {LastName}";
            }
        }


        public DateTime? DateOfBirth { get; set; }

  
        public string Gender { get; set; } = string.Empty;


        public string Email { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Country { get; set; } = "Philippines";

        public string Province { get; set; } = "Cebu";

        public string City { get; set; } = string.Empty;


        public string Barangay { get; set; } = string.Empty;

        public int? GroupId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

 
        public string ConfirmPassword { get; set; } = string.Empty;


 
        public bool AcceptedGuidelines { get; set; }

        public bool AcceptedPrivacy { get; set; }


        public int MemberId { get; set; }

        public string Address { get; set; } = string.Empty;

        public string ProfilePictureUrl { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;

        public DateTime? DateApplied { get; set; }

        public MembershipApplicationStatus Status { get; set; }

    }



}

