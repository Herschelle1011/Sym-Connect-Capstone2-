using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.ViewModels.Profile;
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
            var user = await _userManager.GetUserAsync(User);


            var monthStart =
    new DateTime(
        today.Year,
        today.Month,
        1);

            var monthEnd =
                monthStart.AddMonths(1);

            var eventStatus =
                await _dashboardService
                    .GetEventStatusSummaryAsync(
                        monthStart,
                        monthEnd);

            var vm = new AdminDashboardViewModel
            {

                AdminName = user.DisplayName ?? "Admin",
                Today = today,

                TotalMembers = await _memberService.GetTotalCountAsync(), // adjust to your real method name
                TotalGroups = await _groupService.GetTotalCountAsync(),   // adjust to your real method name

                UpcomingEvents = await _dashboardService.GetUpcomingEventsAsync(),
                UpcomingActivitiesCount = await _dashboardService.GetActivitiesCountThisWeekAsync(startOfWeek, endOfWeek),
                EventsExpiredCount = await _dashboardService.GetExpiredEventCountAsync(),

                EventsApprovedCount = eventStatus.Approved,
                EventsPendingCount = eventStatus.Pending,  // TODO: wire to real count
                EventsRejectedCount = eventStatus.Rejected,  // TODO: wire to real count

                WeeklySummary =
            await _dashboardService
                .GetWeeklyActivityOverviewAsync()
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
                        Url = Url.Action("Index", "YouthMembers")
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
            vm.RecentEventRequests = await _dashboardService.GetRecentEventRequestsAsync();

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetEventStatusSummary(string period = "month")
        {
            var now = DateTime.Now;

            DateTime start;
            DateTime end;


            if (period == "today")
            {
                start = now.Date;
                end = start.AddDays(1);
            }
            else if (period == "week")
            {
                int daysSinceMonday =
                    ((int)now.DayOfWeek + 6) % 7;

                start =
                    now.Date.AddDays(-daysSinceMonday);

                end =
                    start.AddDays(7);
            }
            else
            {
                // Default = This Month
                start = new DateTime(
                    now.Year,
                    now.Month,
                    1);

                end = start.AddMonths(1);
            }


            var summary =
                await _dashboardService
                    .GetEventStatusSummaryAsync(
                        start,
                        end);


            return Json(new
            {
                approved = summary.Approved,
                pending = summary.Pending,
                rejected = summary.Rejected,
                expired = summary.Expired,
                total = summary.Total
            });
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


        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }


            var vm = new EditAccountProfileViewModel
            {
                UserId = user.Id,
                DisplayName = user.DisplayName ?? "",
                Email = user.Email ?? "",
                CurrentPhotoUrl = user.ProfilePictureUrl
            };


            return PartialView(
                "~/Views/Shared/_EditAccountProfileModal.cshtml",
                vm
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
     EditAccountProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unable to find your account."
                });
            }


            // ==========================================
            // RESTORE DISPLAY-ONLY DATA
            // ==========================================

            vm.CurrentPhotoUrl =
                user.ProfilePictureUrl;


            // ==========================================
            // VALIDATE FORM
            // ==========================================

            // Admin keeps DisplayName validation.
            if (!ModelState.IsValid)
            {
                ViewBag.FormArea = "";
                ViewBag.FormController = "Admin";

                return PartialView(
                    "~/Views/Shared/_EditAccountProfileModal.cshtml",
                    vm
                );
            }


            // ==========================================
            // CHECK EMAIL
            // ==========================================

            var emailOwner =
                await _userManager.FindByEmailAsync(
                    vm.Email.Trim()
                );

            if (emailOwner != null &&
                emailOwner.Id != user.Id)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email address is already being used."
                );

                ViewBag.FormArea = "";
                ViewBag.FormController = "Admin";

                return PartialView(
                    "~/Views/Shared/_EditAccountProfileModal.cshtml",
                    vm
                );
            }


            // ==========================================
            // UPDATE PHOTO
            // ==========================================

            if (vm.ProfilePicture != null &&
                vm.ProfilePicture.Length > 0)
            {
                if (vm.ProfilePicture.Length >
                    5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "ProfilePicture",
                        "Profile picture must be less than 5MB."
                    );

                    ViewBag.FormArea = "";
                    ViewBag.FormController = "Admin";

                    return PartialView(
                        "~/Views/Shared/_EditAccountProfileModal.cshtml",
                        vm
                    );
                }


                var imageUrl =
                    await _cloudinaryService
                        .UploadProfilePicture(
                            vm.ProfilePicture,
                            user.Id
                        );

                if (imageUrl == null)
                {
                    ModelState.AddModelError(
                        "ProfilePicture",
                        "Profile picture upload failed."
                    );

                    ViewBag.FormArea = "";
                    ViewBag.FormController = "Admin";

                    return PartialView(
                        "~/Views/Shared/_EditAccountProfileModal.cshtml",
                        vm
                    );
                }


                user.ProfilePictureUrl =
                    imageUrl;

                // Keep the ViewModel synchronized too.
                vm.CurrentPhotoUrl =
                    imageUrl;
            }


            // ==========================================
            // UPDATE NAME + EMAIL
            // ==========================================

            user.DisplayName =
                vm.DisplayName!.Trim();

            user.Email =
                vm.Email.Trim();

            user.UserName =
                vm.Email.Trim();


            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description
                    );
                }

                vm.CurrentPhotoUrl =
                    user.ProfilePictureUrl;

                ViewBag.FormArea = "";
                ViewBag.FormController = "Admin";

                return PartialView(
                    "~/Views/Shared/_EditAccountProfileModal.cshtml",
                    vm
                );
            }


            // ==========================================
            // CHANGE PASSWORD - ONLY WHEN ENTERED
            // ==========================================

            if (!string.IsNullOrWhiteSpace(
                vm.NewPassword))
            {
                var token =
                    await _userManager
                        .GeneratePasswordResetTokenAsync(user);

                var passwordResult =
                    await _userManager
                        .ResetPasswordAsync(
                            user,
                            token,
                            vm.NewPassword
                        );


                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(
                            "NewPassword",
                            error.Description
                        );
                    }

                    vm.CurrentPhotoUrl =
                        user.ProfilePictureUrl;

                    ViewBag.FormArea = "";
                    ViewBag.FormController = "Admin";

                    return PartialView(
                        "~/Views/Shared/_EditAccountProfileModal.cshtml",
                        vm
                    );
                }
            }


            return Json(new
            {
                success = true,
                message = "Profile updated successfully.",
                photoUrl = user.ProfilePictureUrl,
                displayName = user.DisplayName
            });
        }
    }
}
