using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
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


        public MemberController(IMemberService memberService, IAnnouncementService announcementService,
             IElectionService electionService, UserManager<User> userManager)
        {
            _memberService = memberService;
            _announcementService = announcementService;
            _userManager = userManager;
            _electionService = electionService;
        }

        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new { area = "Identity" }
                );
            }

            // The Identity user should already be connected
            // to its Member record.
            if (user.MemberId == null)
            {
                return Forbid();
            }

            int memberId = user.MemberId.Value;


            // Check whether there is an open election
            // that this member has not voted in yet.
            var electionId =
                await _electionService
                    .GetOpenElectionMemberHasNotVotedAsync(
                        user.Id
                    );

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
            var (groupId, isLeader) = await _memberService.GetAnnouncementContextAsync(memberId);

            var model = new DashboardViewModel();
            if (groupId.HasValue) 
            {
                model.PinnedAnnouncement = await _announcementService.GetPinnedAnnouncementsAsync(isLeader, groupId.Value);
            }
         
            return View(model);
        }
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

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
                    });
            }

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
