using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Leader.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class DashboardController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IGroupService _groupService;
        private readonly IElectionService _electionService;
        private readonly IEventService _eventService; // new — needed for events/chart data
        private readonly UserManager<User> _userManager;

        public DashboardController(
            IAnnouncementService announcementService,
            IGroupService groupService,
            IElectionService electionService,
            IEventService eventService,
            UserManager<User> userManager)
        {
            _announcementService = announcementService;
            _groupService = groupService;
            _electionService = electionService;
            _eventService = eventService;
            _userManager = userManager;
            // _dashboardService / ILeaderDashboardService removed — this controller
            // already resolves everything itself, no need for a second service.
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            // Fixed: null check now happens BEFORE .Value is ever called
            if (user.MemberId == null)
                return Forbid();

            int memberId = user.MemberId.Value;

            var electionId = await _electionService.GetOpenElectionMemberHasNotVotedAsync(user.Id);
            if (electionId.HasValue)
            {
                return RedirectToAction("TakeVoteElection", "Election",
                    new { area = "Leader", id = electionId.Value });
            }

            bool isLeader = await _groupService.IsMemberALeaderAsync(memberId);
            if (!isLeader) return Forbid();

            var leaderGroup = await _groupService.GetGroupLedByMemberAsync(memberId);
            if (leaderGroup == null) return Forbid();

            int groupId = leaderGroup.GroupId;

            // ---- Reused existing methods, no new service calls needed ----
            var pinnedAnnouncements = await _announcementService.GetPinnedAnnouncementsAsync(true, groupId);
            var announcements = await _announcementService.GetVisibleAnnouncementsAsync(memberId, true, groupId);
            var leaderOwnEvents = await _eventService.GetAllEventForLeaderAsync(memberId);
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync(isAdmin: false, currentUserId: memberId);

            // ---- New — small additions needed on IGroupService / IEventService (below) ----
            var totalMembers = await _groupService.GetMemberCountAsync(groupId);
            var participationPoints = await _groupService.GetTotalEarnedPointsAsync(groupId);
            var attendanceChart = await _eventService.GetGroupAttendanceChartAsync(groupId);

            var vm = new DashboardViewModel
            {
                PinnedAnnouncement = pinnedAnnouncements,
                Announcements = announcements,

                TotalMembers = totalMembers,
                ParticipationPoints = participationPoints,
                UpcomingActivitiesCount = upcomingEvents.Count,

                // Reused GetAllEventForLeaderAsync — no dedicated method needed
                PendingRequestCount = leaderOwnEvents.Count(e => e.ApprovalStatus == ApprovalStatus.Pending),

                UpcomingEvents = upcomingEvents.Take(3).ToList(),
                GroupAttendanceChart = attendanceChart,

                SubmissionStatuses = leaderOwnEvents
                    .OrderByDescending(e => e.DateCreated)
                    .Take(5)
                    .Select(e => new SubmissionStatusItemViewModel
                    {
                        Name = e.EventTitle,
                        Category = "Event",
                        Status = e.ApprovalStatus.ToString()
                    }).ToList()
                // TODO: append Media submissions once that model exists
            };

            return View(vm);
        }
    }
}