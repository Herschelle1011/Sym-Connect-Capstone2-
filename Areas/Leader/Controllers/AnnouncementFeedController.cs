using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Announcement;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
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

            if (leaderGroup == null)
                return Forbid();

            int groupId = leaderGroup.GroupId;
            var user = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUserId = user?.Id;

            var announcements = await _announcementService.GetVisibleAnnouncementsAsync(memberId.Value, true, groupId);

            return View(announcements);
        }

        //==== HTTP GET ==== CREATE
        // Leader form has no audience/group picker — it's always the leader's own group,
        // enforced server-side in AnnouncementService.CreateAsync.
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var leaderGroup = await GetCurrentLeaderGroupAsync();
            if (leaderGroup == null) return Forbid();

            var vm = new AnnouncementViewModel();
            return PartialView("_AnnouncementFormCreate", vm);
        }

        //==== HTTP POST ==== CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementViewModel vm)
        {
            var leaderGroup = await GetCurrentLeaderGroupAsync();
            if (leaderGroup == null) return Forbid();

            if (!ModelState.IsValid)
            {
                return PartialView("_AnnouncementFormCreate", vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _announcementService.CreateAsync(vm, user.Id);

            return Json(new { success = result.Success, message = result.Message });
        }

        //==== HTTP GET ==== EDIT
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var leaderGroup = await GetCurrentLeaderGroupAsync();
            if (leaderGroup == null) return Forbid();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();




            var vm = await _announcementService.GetEditFormForLeaderAsync(id, user.Id, leaderGroup.GroupId);
            if (vm == null) return NotFound();

            return PartialView("_AnnouncementFormEdit", vm);
        }

        //==== HTTP POST ==== EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AnnouncementViewModel vm)
        {
            var leaderGroup = await GetCurrentLeaderGroupAsync();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();
            if (leaderGroup == null) return Forbid();

            if (!ModelState.IsValid)
            {
                return PartialView("_AnnouncementFormEdit", vm);
            }

            var result = await _announcementService.UpdateLeaderAnnouncementAsync(vm, user.Id,leaderGroup.GroupId);

            return Json(new { success = result.Success, message = result.Message });
        }


        public async Task<IActionResult> GetArchive(int? announcementId)
        {

            var leaderGroup = await GetCurrentLeaderGroupAsync();
            if (leaderGroup == null) return Forbid();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            if (announcementId == null) return NotFound();

            var vm = await _announcementService.GetArchivePreviewAsync(announcementId.Value);
            if (vm == null) return NotFound();

            return PartialView("_ArchiveAnnouncementModal", vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int AnnouncementId)
        {
            var leaderGroup = await GetCurrentLeaderGroupAsync();
            if (leaderGroup == null) return Forbid();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _announcementService.ArchiveLeaderAnnouncementAsync(AnnouncementId, user.Id, leaderGroup.GroupId);
            return Json(new { success = result.Success, message = result.Message });
        }


    }
}