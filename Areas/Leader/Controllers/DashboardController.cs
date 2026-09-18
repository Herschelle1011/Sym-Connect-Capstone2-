using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Leader.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Profile;

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
        private readonly CloudinaryService _cloudinaryService;


        public DashboardController(
            IAnnouncementService announcementService,
            IGroupService groupService,
            IElectionService electionService,
            IEventService eventService,
            CloudinaryService cloudinaryService,
            UserManager<User> userManager)
        {
            _announcementService = announcementService;
            _groupService = groupService;
            _electionService = electionService;
            _eventService = eventService;
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
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

            ViewBag.FormArea = "Member";
            ViewBag.FormController = "Profile";

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


            // Keep these so the shared form knows
            // where to submit again if validation fails.
            ViewBag.FormArea = "Member";
            ViewBag.FormController = "Profile";


            // ==========================================
            // VALIDATION
            // ==========================================

            if (!ModelState.IsValid)
            {
                vm.CurrentPhotoUrl = user.ProfilePictureUrl;

                return PartialView(
                    "~/Views/Shared/_EditAccountProfileModal.cshtml",
                    vm
                );
            }


            // ==========================================
            // CHECK EMAIL
            // ==========================================

            var existingUser =
                await _userManager.FindByEmailAsync(vm.Email);

            if (existingUser != null &&
                existingUser.Id != user.Id)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email is already being used."
                );

                vm.CurrentPhotoUrl = user.ProfilePictureUrl;

                return PartialView(
                    "~/Views/Shared/_EditAccountProfileModal.cshtml",
                    vm
                );
            }


            // ==========================================
            // PROFILE PHOTO
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

                    vm.CurrentPhotoUrl =
                        user.ProfilePictureUrl;

                    return PartialView(
                        "~/Views/Shared/_EditAccountProfileModal.cshtml",
                        vm
                    );
                }


                var imageUrl =
             await _cloudinaryService.UploadProfilePicture(
                 vm.ProfilePicture,
                 user.Id
             );

                if (imageUrl == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Profile picture upload failed."
                    });
                }

                user.ProfilePictureUrl = imageUrl;
            }


            // ==========================================
            // NAME + EMAIL
            // ==========================================

            user.DisplayName = vm.DisplayName.Trim();
            user.Email = vm.Email.Trim();
            user.UserName = vm.Email.Trim();


            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return Json(new
                {
                    success = false,
                    message = updateResult.Errors
                        .FirstOrDefault()?.Description
                        ?? "Unable to update profile."
                });
            }


            // ==========================================
            // PASSWORD
            // ==========================================

            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                var token =
                    await _userManager
                        .GeneratePasswordResetTokenAsync(user);

                var passwordResult =
                    await _userManager.ResetPasswordAsync(
                        user,
                        token,
                        vm.NewPassword
                    );

                if (!passwordResult.Succeeded)
                {
                    return Json(new
                    {
                        success = false,
                        message = passwordResult.Errors
                            .FirstOrDefault()?.Description
                            ?? "Unable to change password."
                    });
                }
            }


            return Json(new
            {
                success = true,
                message = "Profile updated successfully."
            });
        }
    }
}