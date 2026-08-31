
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize]
    public class AnnouncementFeedController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IGroupService _groupService;
        private readonly UserManager<User> _userManager;




        public AnnouncementFeedController(IAnnouncementService announcementService, 
                             IGroupService groupService, UserManager<User> userManager)
        {
            _announcementService = announcementService;
            _groupService = groupService;
            _userManager = userManager;
        }

        private async Task<Group?> GetCurrentLeaderGroupAsync()
        {
            int? memberId =
                HttpContext.Session.GetInt32("CurrentMemberId");

            if (memberId == null)
                return null;

            return await _groupService
                .GetGroupByLeaderIdAsync(memberId.Value);
        }




        public async Task<IActionResult> Index()
        {
            int? memberId = HttpContext.Session.GetInt32("CurrentMemberId");

            if (memberId is null) return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "Identity" }
                ); 

            bool isLeader =
                await _groupService.IsMemberALeaderAsync(memberId);

            var leaderGroup =
                 await _groupService.GetGroupLedByMemberAsync(memberId);

            if (!isLeader)
                return Forbid();

            int groupId = leaderGroup.GroupId;

                     
            var announcements = await _announcementService.GetVisibleAnnouncementsAsync(memberId.Value, true, groupId);

            return View(announcements);
        }
    }
}
