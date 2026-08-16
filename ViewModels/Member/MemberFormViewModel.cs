using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class MemberFormViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Firstname is required.")]
        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Lastname is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^(09\d{9}|\+639\d{9})$",
            ErrorMessage = "Enter a valid PH mobile number (e.g. 09123456789)")]
        public string ContactNumber { get; set; } = string.Empty;

        // Structured address instead of one free-text field
        public string Country { get; set; } = "Philippines";

        [Required(ErrorMessage = "Province is required.")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "City/Municipality is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Barangay is required.")]
        public string Barangay { get; set; } = string.Empty;



        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Active";

        [System.Text.Json.Serialization.JsonPropertyName("photoUrl")]
        public string? PhotoUrl { get; set; }
    }
}
