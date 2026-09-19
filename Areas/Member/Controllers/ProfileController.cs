using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Profile;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class ProfileController : Controller
    {

        private readonly IMemberService _memberService;
        private readonly IProfileService _profileService;
        private readonly UserManager<User> _userManager;
        private readonly CloudinaryService _cloudinaryService;


        public ProfileController(IMemberService memberService, IProfileService profileService, UserManager<User> userManager, CloudinaryService cloudinaryService)
        {
            _memberService = memberService;
            _profileService = profileService;
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
        }
        public async Task<IActionResult> Index()
        {
            var memberId = HttpContext.Session.GetInt32("CurrentMemberId");
            if (memberId == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var profile = await _profileService.GetProfileAsync(memberId);
            if (profile == null)
                return NotFound(); // or a friendly "profile not found" view

            return View(profile);
        }

        //GET PROFILE



        public async Task<IActionResult> MyQrCode()
        {
            var memberId = HttpContext.Session.GetInt32("CurrentMemberId");
            if (memberId == null) return Unauthorized();
            var token = await _memberService.GetOrCreateQrTokenAsync(memberId.Value);
            var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(token, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.PngByteQRCode(qrData);
            byte[] qrImageBytes = qrCode.GetGraphic(20);
            return File(qrImageBytes, "image/png");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }


            var vm = new EditAccountProfileViewModel
            {
                UserId = user.Id,
                DisplayName = user.DisplayName ?? "",
                Email = user.Email ?? "",
                CurrentPhotoUrl = user.ProfilePictureUrl
            };


            ViewBag.FormArea = "Member";
            ViewBag.FormController = "Profile";


            return PartialView(
                "~/Views/Shared/_EditAccountProfileModal.cshtml",
                vm
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
    EditAccountProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Leader does not edit DisplayName.
            ModelState.Remove(nameof(vm.DisplayName));

            // Restore display-only data.
            // This is needed if we return the modal because of validation errors.
            vm.CurrentPhotoUrl = user.ProfilePictureUrl;

            if (!ModelState.IsValid)
            {
                ViewBag.FormArea = "Member";
                ViewBag.FormController = "Profile";

                return PartialView(
                    "~/Views/Shared/_EditAccountProfileModal.cshtml",
                    vm
                );
            }

            // Leader can update email.
            user.Email = vm.Email;

            // Upload a new photo only when one was selected.
            if (vm.ProfilePicture != null &&
                vm.ProfilePicture.Length > 0)
            {
                var photoUrl = await _cloudinaryService
                    .UploadProfilePicture(
                        vm.ProfilePicture,
                        user.Id
                    );

                if (photoUrl == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Photo upload failed. Please try again."
                    );

                    // Keep showing the existing photo.
                    vm.CurrentPhotoUrl =
                        user.ProfilePictureUrl;

                    ViewBag.FormArea = "Member";
                    ViewBag.FormController = "Profile";

                    return PartialView(
                        "~/Views/Shared/_EditAccountProfileModal.cshtml",
                        vm
                    );
                }

                user.ProfilePictureUrl = photoUrl;
            }

            var result =
                await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description
                    );
                }

                // Restore current photo before returning modal.
                vm.CurrentPhotoUrl =
                    user.ProfilePictureUrl;

                ViewBag.FormArea = "Member";
                ViewBag.FormController = "Profile";

                return PartialView(
                    "~/Views/Shared/_EditAccountProfileModal.cshtml",
                    vm
                );
            }

            return Json(new
            {
                success = true,
                photoUrl = user.ProfilePictureUrl,
                displayName = user.DisplayName
            });
        }


        // =========================================
        // GET EDIT PERSONAL INFO
        // =========================================

        [HttpGet]
        public async Task<IActionResult> EditPersonalInfo()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }


            if (user.MemberId == null)
            {
                return Forbid();
            }


            ViewBag.FormArea = "Member";
            ViewBag.FormController = "Profile";

            var model = await _profileService
                .GetEditPersonalInfoAsync(
                    user.MemberId.Value);


            if (model == null)
            {
                return NotFound();
            }


            return PartialView(
               "~/Views/Shared/_EditPersonalInfoModal.cshtml",
       model
   );
        }


        // =========================================
        // POST EDIT PERSONAL INFO
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPersonalInfo(
            EditPersonalInfoViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }


            if (user.MemberId == null)
            {
                return Forbid();
            }


            ViewBag.FormArea = "Member";
            ViewBag.FormController = "Profile";

            if (!ModelState.IsValid)
            {
                return PartialView(
              "~/Views/Shared/_EditPersonalInfoModal.cshtml",
                model
     );
            }


            var success = await _profileService
                .UpdatePersonalInfoAsync(
                    user.MemberId.Value,
                    model);


            if (!success)
            {
                return NotFound();
            }


            return Json(new
            {
                success = true,
                message = "Personal information updated successfully."
            });
        }
    }
}
