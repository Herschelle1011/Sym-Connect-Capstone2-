using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
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
        private readonly IElectionService _electionService;
        private readonly UserManager<User> _userManager;
        private readonly IMemberDashboardService _memberDashboardService;

        public MemberController(IMemberService memberService, IAnnouncementService announcementService,
             IElectionService electionService, UserManager<User> userManager, IMemberDashboardService memberDashboardService)
        {
            _memberService = memberService;
            _announcementService = announcementService;
            _userManager = userManager;
            _electionService = electionService;
            _memberDashboardService = memberDashboardService;
        }

        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in Identity user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "Identity" }
                );
            }

            // Every Member account should be connected
            // to a Member record.
            if (user.MemberId == null)
            {
                return Forbid();
            }

            int memberId = user.MemberId.Value;

            // -------------------------------------------------
            // CHECK FOR OPEN ELECTION
            // -------------------------------------------------
            var electionId =
                await _electionService
                    .GetOpenElectionMemberHasNotVotedAsync(user.Id);

            if (electionId.HasValue)
            {
                return RedirectToAction(
                    "TakeVoteElection",
                    "Elections",
                    new
                    {
                        area = "Member",
                        id = electionId.Value
                    }
                );
            }

            // -------------------------------------------------
            // LOAD MEMBER DASHBOARD
            // -------------------------------------------------
            var model =
                await _memberDashboardService
                    .GetMemberDashboardAsync(memberId);

            // -------------------------------------------------
            // GET ANNOUNCEMENT CONTEXT
            // -------------------------------------------------
            var (groupId, isLeader) =
                await _memberService
                    .GetAnnouncementContextAsync(memberId);

            // -------------------------------------------------
            // LOAD PINNED ANNOUNCEMENTS
            // -------------------------------------------------
            if (groupId.HasValue)
            {
                model.PinnedAnnouncement =
                    await _announcementService
                        .GetPinnedAnnouncementsAsync(
                            isLeader,
                            groupId.Value
                        );
            }

            return View(model);
        }

        public async Task<IActionResult> Dashboard()
        {
            return View();
        }
        public IActionResult SimulateLogin(int memberId)
        {
            HttpContext.Session.SetInt32("CurrentMemberId", memberId);
            return RedirectToAction("Index", "Profile");
        }

    }
}
