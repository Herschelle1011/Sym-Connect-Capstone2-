using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordModel(UserManager<User> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);

            // Don't reveal whether the user exists or is confirmed — always redirect the same way.
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPasswordModel",
                    pageHandler: null,
                    values: new { area = "Identity", code, email = user.Email },
                    protocol: Request.Scheme);



                await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    user.DisplayName ?? user.Email, // adjust to whatever your User model exposes
                    callbackUrl);
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}