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
    [Authorize(Roles = "Admin")]
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
        private readonly SignInManager<User> _signInManager;



        public AdminController(UserManager<User> userManager,
        CloudinaryService cloudinaryService, IAnnouncementService announcementService, IMemberService memberService, IGroupService groupService, IEventService eventService, IMediaService mediaService, IDashboardService dashboardService, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
            _announcementService = announcementService;
            _memberService = memberService;
            _groupService = groupService;
            _eventService = eventService;
            _mediaService = mediaService;
            _dashboardService = dashboardService;
            _signInManager = signInManager;
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
            var today = DateTime.Now;
            var startOfWeek = today.Date.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            int count = await _eventService.GetPendingRequestCountAsync();

            var vm = new AdminDashboardViewModel
            {
                AdminName = User.Identity?.Name ?? "Admin",
                Today = today,

                TotalMembers = await _memberService.GetTotalCountAsync(), // adjust to your real method name
                TotalGroups = await _groupService.GetTotalCountAsync(),   // adjust to your real method name

                UpcomingEvents = await _dashboardService.GetUpcomingEventsAsync(),
                UpcomingActivitiesCount = await _dashboardService.GetActivitiesCountThisWeekAsync(startOfWeek, endOfWeek),

                EventsApprovedCount = await _eventService.GetApprovedCountAsync(),  // TODO: wire to real count
                EventsPendingCount = await _eventService.GetPendingRequestCountAsync(),  // TODO: wire to real count
                EventsRejectedCount = await _eventService.GetRejectedCountAsync(),  // TODO: wire to real count
            };

            var (membershipCount, membershipLatest) = await _dashboardService.GetPendingMembershipApplicationsAsync();
            var (eventCount, eventLatest) = await _dashboardService.GetPendingEventRequestsAsync();
            var (mediaCount, mediaLatest) = await _dashboardService.GetPendingMediaFilesAsync();

            vm.PendingApprovals = new List<PendingApprovalItemViewModel>
                {
                    new() {
                        Type = "Membership", Label = "Membership Applications",
                        Description = "New member registrations",
                        Count = membershipCount, LatestDate = membershipLatest,
                        IconClass = "bi-person",
                        Url = Url.Action("Index", "Members")
                    },
                    new() {
                        Type = "EventRequest", Label = "Event Requests",
                        Description = "Pending event approvals",
                        Count = eventCount, LatestDate = eventLatest,
                        IconClass = "bi-calendar-event",
                        Url = Url.Action("ViewEventsRequests", "Events")
                    },
                    new() {
                        Type = "Media", Label = "Media Files",
                        Description = "Files waiting for approval",
                        Count = mediaCount, LatestDate = mediaLatest,
                        IconClass = "bi-file-earmark",
                        Url = Url.Action("PendingFiles", "MediaAndDocuments")
                    }
                };

            vm.PendingApprovalsCount = membershipCount + eventCount + mediaCount;
            vm.RecentRegistrations = await _dashboardService.GetRecentApprovedMembersAsync();

            return View(vm);
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Remove Identity login
            await _signInManager.SignOutAsync();

            // Remove all Session values
            HttpContext.Session.Clear();

            // Send user to Identity Login page
            return Redirect("/Identity/Account/Login");

        }
    }
}
