using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Dashboard;
using System.Text.RegularExpressions;

namespace Sym_Connect__Capstone2_.Controllers
{
    [AllowAnonymous] //TEMPORARY - REMOVE THIS LATER
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IAnnouncementService _announcementService;
        private readonly IMemberService _memberService;
        private readonly IGroupService _groupService;
        private readonly IEventService _eventService;
        private readonly IMediaService _mediaService;
        private readonly IDashboardService _dashboardService;


        public AdminController(UserManager<User> userManager,
        CloudinaryService cloudinaryService, IAnnouncementService announcementService, IMemberService memberService, IGroupService groupService, IEventService eventService, IMediaService mediaService, IDashboardService dashboardService)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _announcementService = announcementService;
            _memberService = memberService;
            _groupService = groupService;
            _eventService = eventService;
            _mediaService = mediaService;
            _dashboardService = dashboardService;
        }

        // ── GET: /Admin/Profile ─────────────────────────────────────────────
        public IActionResult Profile()
        {
            return View();
        }
        //--------------UPLOAD PICTURE THROUGH CLOUD GET POST METHOD //
        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0) 
            {
                TempData["Error"] = "Please select a valid profile picture.";
                return RedirectToAction("Profile");
            }

            if(profilePicture.Length > 5 * 1024 * 1024) // 5MB limit
            {
                TempData["Error"] = "Profile picture must be less than 5MB.";
                return RedirectToAction("Profile");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Unauthorized();

            //Send to Cloundinary and get the URL
            var imageUrl = await _cloudinaryService.UploadProfilePicture(profilePicture, user.Id);

            if(imageUrl == null)
            {
                TempData["Error"] = "Upload failed. Please Try Again"; 
                return RedirectToAction("Profile");
            }

            user.ProfilePictureUrl = imageUrl;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile picture updated!";
            return RedirectToAction("Dashboard");

        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var vm = new AdminDashboardViewModel
            {
                AdminName = currentUser?.Member?.FirstName ?? "Admin",
                Today = DateTime.Today,

                TotalMembers = await _memberService.GetTotalCountAsync(),
                TotalGroups = await _groupService.GetTotalCountAsync(),
                UpcomingActivitiesCount = await _eventService.GetUpcomingCountAsync(),
            };
            var eventSummary = await _eventService.GetEventsSummaryForAdminAsync();
            vm.TotalEventsThisPeriod = eventSummary.TotalRequests;

            vm.EventsApprovedCount = eventSummary.ApprovedCount;

            vm.EventsPendingCount = eventSummary.PendingCount;

            vm.EventsRejectedCount = eventSummary.RejectedCount;

            vm.RecentEventRequests = eventSummary.Events
            .OrderByDescending(e => e.DateCreated)
            .Take(5)
            .ToList();


            vm.PendingActions = new List<PendingActionViewModel>
            {
                new PendingActionViewModel
                {
                    Label = "Event Requests",
                    Count = await _eventService.GetPendingRequestCountAsync()
                },
                new PendingActionViewModel
                {
                    Label = "Documents & Media",
                    Count = await _mediaService.GetPendingFilesCountAsync()
                }
            };


            vm.PendingApprovalsCount = vm.PendingActions.Sum(p => p.Count);
            vm.UpcomingEvents = await _dashboardService.GetUpcomingEventsAsync();

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            vm.WeeklySummary = new List<ChartPointViewModel>
            {
                new ChartPointViewModel
                {
                    Label = "Activities",
                    Value = await _dashboardService.GetActivitiesCountThisWeekAsync(startOfWeek, endOfWeek)
                },
                new ChartPointViewModel
                {
                    Label = "Attendance",
                    Value = await _dashboardService.GetAttendanceCountThisWeekAsync(startOfWeek, endOfWeek)
                },
                new ChartPointViewModel
                {
                    Label = "Participation",
                    Value = await _dashboardService.GetParticipationPointsThisWeekAsync(startOfWeek, endOfWeek)
                },
                new ChartPointViewModel
                {
                    Label = "Evaluation",
                    Value = await _dashboardService.GetEvaluationCountThisWeekAsync(startOfWeek, endOfWeek)
                },
                new ChartPointViewModel
                {
                    Label = "Announcement",
                    Value = await _dashboardService.GetAnnouncementCountThisWeekAsync(startOfWeek, endOfWeek)
                }
            };


            return View(vm);
        }
    }
}
