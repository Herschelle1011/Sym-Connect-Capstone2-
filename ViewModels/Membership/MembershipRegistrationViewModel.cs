using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Membership
{
    public class MembershipRegistrationViewModel
    {
        // PERSONAL INFORMATION

        [Required]
        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = "Philippines";

        [Required]
        public string Province { get; set; } = "Cebu";

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Barangay { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please select a group.")]
        public int? GroupId { get; set; }


        // ACCOUNT INFORMATION


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public DateTime DateApplied { get; set; } 

        // AGREEMENTS

        [Range(
         typeof(bool),
         "true",
         "true",
         ErrorMessage = "You must accept the Membership Guidelines."
     )]
        public bool AcceptedGuidelines { get; set; }


        [Range(
            typeof(bool),
            "true",
            "true",
            ErrorMessage = "You must accept the Privacy Notice."
        )]
        public bool AcceptedPrivacy { get; set; }

        public string PhotoUrl { get; set; } = string.Empty;

    }
}
