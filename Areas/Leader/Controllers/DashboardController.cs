using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Leader.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IGroupService _groupService;

        public DashboardController(IAnnouncementService announcementService, IGroupService groupService)
        {
            _announcementService = announcementService;
            _groupService = groupService;
        }

        public async Task<IActionResult> Index()
        {
            int? memberId = HttpContext.Session.GetInt32("CurrentMemberId");

            if (memberId is null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            bool isLeader = await _groupService.IsMemberALeaderAsync(memberId);
            if (!isLeader)
                return Forbid();

            var leaderGroup = await _groupService.GetGroupLedByMemberAsync(memberId);
            if (leaderGroup == null)
                return Forbid();

            int groupId = leaderGroup.GroupId;

            var vm = new DashboardViewModel
            {
                PinnedAnnouncement = await _announcementService.GetPinnedAnnouncementsAsync(true, groupId),
                Announcements = await _announcementService.GetVisibleAnnouncementsAsync(memberId.Value, true, groupId)
            };

            return View(vm);
        }
    }
}