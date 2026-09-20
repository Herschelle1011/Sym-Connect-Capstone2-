using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public ResetPasswordModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Code { get; set; }
        }

        public IActionResult OnGet(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                return BadRequest("A code and email must be supplied for password reset.");
            }

            Input = new InputModel
            {
                Code = code, // keep it base64url-encoded; decode on POST
                Email = email
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
            var result = await _userManager.ResetPasswordAsync(user, decodedCode, Input.Password);

            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}