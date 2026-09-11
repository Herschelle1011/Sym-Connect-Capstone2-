using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.AnnouncementModels;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Announcement;
namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly IAnnouncementService _announcementService;


        public MemberController(IMemberService memberService, IAnnouncementService announcementService)
        {
            _memberService = memberService;
            _announcementService = announcementService;
        }

        public async Task<IActionResult> Index()
        {
            int? memberId = HttpContext.Session.GetInt32("CurrentMemberId");
            if (memberId is null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var (groupId, isLeader) = await _memberService.GetAnnouncementContextAsync(memberId.Value);

            var model = new DashboardViewModel();
            if (groupId.HasValue) 
            {
                model.PinnedAnnouncement = await _announcementService.GetPinnedAnnouncementsAsync(isLeader, groupId.Value);
            }
         
            return View(model);
        }
        public async Task<IActionResult> Dashboard()
        {
            int? memberId = HttpContext.Session.GetInt32("CurrentMemberId");
            if (memberId is null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var (groupId, isLeader) = await _memberService.GetAnnouncementContextAsync(memberId.Value);
            var model = new DashboardViewModel();

            if (groupId.HasValue)
            {
                model.PinnedAnnouncement = await _announcementService.GetPinnedAnnouncementsAsync(isLeader, groupId.Value);
            }

            return View(model);
        }
        public IActionResult SimulateLogin(int memberId)
        {
            HttpContext.Session.SetInt32("CurrentMemberId", memberId);
            return RedirectToAction("Index", "Profile");
        }

    }
}
