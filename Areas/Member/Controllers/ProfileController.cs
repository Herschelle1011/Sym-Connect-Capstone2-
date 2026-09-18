using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Models;
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


        public ProfileController(IMemberService memberService, IProfileService profileService, UserManager<User> userManager)
        {
            _memberService = memberService;
            _profileService = profileService;
            _userManager = userManager;
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
    }
}
