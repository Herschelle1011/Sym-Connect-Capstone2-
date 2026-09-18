using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Profile
{
    public class EditAccountProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string? CurrentPhotoUrl { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;


        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Display(Name = "Profile Photo")]
        public IFormFile? ProfilePicture { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }


        [DataType(DataType.Password)]
        [Compare(
            "NewPassword",
            ErrorMessage = "Passwords do not match."
        )]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }
}
