using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    public class ProfileController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly IProfileService _profileService;


        public ProfileController(IMemberService memberService, IProfileService profileService)
        {
            _memberService = memberService;
            _profileService = profileService;
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
    }
}
