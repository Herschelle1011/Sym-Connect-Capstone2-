using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services;

namespace Sym_Connect__Capstone2_.Controllers
{
    [AllowAnonymous] //TEMPORARY - REMOVE THIS LATER
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly CloudinaryService _cloudinaryService;

        public AdminController(UserManager<User> userManager,
        CloudinaryService cloudinaryService)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
        }

        // ── GET: /Admin/Profile ─────────────────────────────────────────────
        public IActionResult Profile()
        {
            return View();
        }
        //--------------UPLOAD PICTURE THROUGH CLOUD GET POST METHOD //
        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0) 
            {
                TempData["Error"] = "Please select a valid profile picture.";
                return RedirectToAction("Profile");
            }

            if(profilePicture.Length > 5 * 1024 * 1024) // 5MB limit
            {
                TempData["Error"] = "Profile picture must be less than 5MB.";
                return RedirectToAction("Profile");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Unauthorized();

            //Send to Cloundinary and get the URL
            var imageUrl = await _cloudinaryService.UploadProfilePicture(profilePicture, user.Id);

            if(imageUrl == null)
            {
                TempData["Error"] = "Upload failed. Please Try Again"; 
                return RedirectToAction("Profile");
            }

            user.ProfilePictureUrl = imageUrl;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile picture updated!";
            return RedirectToAction("Dashboard");

        }

        //[Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
