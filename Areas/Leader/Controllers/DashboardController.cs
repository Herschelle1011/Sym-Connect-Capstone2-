using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Leader.ViewModels.Dashboard;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    public class DashboardController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IMemberService _memberService;

        public DashboardController(IAnnouncementService announcementService, IMemberService memberService)
        {
            _announcementService = announcementService;
            _memberService = memberService;
        }

        public async Task<IActionResult> Dashboard()
        {

            return View();
        }

        public async Task<IActionResult> Index()
        {

            int? memberId = HttpContext.Session.GetInt32("CurrentMemberId");
            if (memberId is null)
                return RedirectToAction("Login", "Account");

            var (groupId, isLeader) = await _memberService.GetAnnouncementContextAsync(memberId.Value);

            var pinned = await _announcementService.GetPinnedAnnouncementAsync(isLeader, groupId);
            var announcements = await _announcementService.GetVisibleAnnouncementsAsync(memberId.Value, isLeader, groupId);

            var vm = new DashboardViewModel
            {
                PinnedAnnouncement = pinned,
                Announcements = announcements
            };
            return View(vm);
        }


    }
}
