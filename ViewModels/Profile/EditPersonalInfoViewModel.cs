using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Profile
{
    public class EditPersonalInfoViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100)]
        public string FirstName { get; set; } = "";

        [StringLength(100)]
        public string? MiddleName { get; set; }   // optional — no [Required]

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100)]
        public string LastName { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime? Birthdate { get; set; }


        [Required]
        public string Gender { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;


        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        public string? Province { get; set; } = string.Empty;

        [Required]
        public string? City { get; set; } = string.Empty;

        [Required]
        public string? Barangay { get; set; } = string.Empty;
    }
}
