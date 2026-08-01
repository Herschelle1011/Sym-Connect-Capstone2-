using System.ComponentModel.DataAnnotations;
namespace Sym_Connect__Capstone2_.ViewModels.Member
{
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

        public string? PhotoUrl { get; set; }

    }
}
