using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    public class AnnouncementFeedController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IMemberService _memberService;


        public AnnouncementFeedController(IAnnouncementService announcementService, IMemberService memberService)
        {
            _announcementService = announcementService;
            _memberService = memberService;
        }

        public async Task<IActionResult> Index()
        {
            int? memberId = HttpContext.Session.GetInt32("CurrentMemberId");
            if (memberId is null) return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "Identity" }
                );

            var (groupId, isLeader) = await _memberService.GetAnnouncementContextAsync(memberId.Value);

            var announcements = await _announcementService.GetVisibleAnnouncementsAsync(memberId.Value, isLeader, groupId);

            return View(announcements);
        }
    }
}
