using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Controllers
{
    [Authorize]
    public class EmailVerificationController : Controller
    {
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly UserManager<User> _userService;

        public EmailVerificationController(IEmailVerificationService emailVerificationService, UserManager<User> userService)
        {
            _emailVerificationService = emailVerificationService;
            _userService = userService;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode([FromForm] string email)
        {
            var currentUser = await _userService.GetUserAsync(User);

            // Exclude the current user's own ID so changing email
            // back to what it already was isn't wrongly rejected.
            var (success, message) = await _emailVerificationService.SendCodeAsync(
                email,
                excludeUserId: currentUser?.Id);

            return Json(new { success, message });
        }
    }
}
