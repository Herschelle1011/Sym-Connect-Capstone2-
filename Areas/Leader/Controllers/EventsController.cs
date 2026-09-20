using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.EventModels;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class EventsController : Controller
    {

        private readonly IGroupService _groupService;
        private readonly IFacilityService _facilityService;
        private readonly IEventService _eventService;
        private readonly UserManager<User> _userManager;

        public EventsController(IGroupService groupService, IEventService eventService, IFacilityService facilityService, UserManager<User> userManager)
        {
            _groupService = groupService;
            _eventService = eventService;
            _facilityService = facilityService;
            _userManager = userManager;
        }

        private async Task<IActionResult?> AuthorizeEvent(int eventId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

      
            if (user.MemberId == null)
            {
                return Forbid();
            }

            bool isLeader =
                await _groupService.IsMemberALeaderAsync(
                    user.MemberId.Value
                );

            if (!isLeader)
            {
                return Forbid();
            }

            bool canAccess =
                await _eventService.LeaderOwnsEventsAsync(
                    eventId,
                    user.MemberId.Value
                );

            if (!canAccess)
            {
                return Forbid();
            }

            return null;
        }


        public async Task<IActionResult> Index(int? openEventId)
        {

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" }
                );
            }

            int? memberId = user.MemberId;


            bool isLeader = await _groupService.IsMemberALeaderAsync(memberId);
            if (!isLeader) return Forbid();

            var events = await _eventService.GetAllEventForLeaderAsync(memberId.Value);

            ViewBag.Facilities = await _facilityService.GetAllFacilitiesAsync();
            ViewBag.Groups = await _groupService.GetAllGroupsAsync();


            var LeaderGroup = await _groupService.GetGroupLedByMemberAsync(memberId);
            ViewBag.LeaderGroupId = LeaderGroup?.GroupId;
            ViewBag.LeaderGroupName = LeaderGroup?.GroupName;

            ViewBag.FormController = "Events";
            ViewBag.FormArea = "Leader";
            ViewBag.OpenEventId = openEventId;

            return View(events);
        }

        //========== LEADERS VIEW SCANNER =================
        private async Task<int?> GetCurrentLeaderIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.MemberId == null)
            {
                return null;
            }

            return user.MemberId.Value;
        }



        //============= SCANNER CODES BELOW > ======

        [HttpGet]
        public async Task<IActionResult> GetScannerState(int eventId)
        {
            var guard = await AuthorizeEvent(eventId);
            if (guard != null) return NotFound(); 

            var state = await _eventService.GetScannerStateAsync(eventId); // REUSED
            if (state == null) return NotFound();

            return Json(state);
        }

        //============== TIME IN CODE SCANNER ===========

        //GET
        [HttpGet]
        public async Task<IActionResult> ScannerForTimeIn(int eventId)
        {
            var guard = await AuthorizeEvent(eventId);
            if (guard != null) return guard;

            var eventEntity = await _eventService.GetEventByIdAsync(eventId); // REUSED
            if (eventEntity == null) return NotFound();

            var eventStart = eventEntity.StartDate.Date + (eventEntity.StartTime ?? TimeSpan.Zero);
            var eventEnd = eventEntity.EndDate.Date + (eventEntity.EndTime ?? TimeSpan.Zero);

            bool isLive = DateTime.Now >= eventStart.AddMinutes(-30) && DateTime.Now <= eventEnd;
            if (!isLive)
                return RedirectToAction("Details", new { id = eventId });

            var state = await _eventService.GetScannerStateAsync(eventId); // REUSED
            if (state == null) return NotFound();

            return View(state); // new view: copy Scanner.cshtml, swap layout + fetch URLs
        }


        [HttpPost]
        public async Task<IActionResult> ScanTimeIn([FromBody] ScanAttendanceRequest req)
        {
            var guard = await AuthorizeEvent(req.EventId);
            if (guard != null) return guard;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Session expired. Please verify your identity again." });

            var result = await _eventService.RecordTimeInAsync(
                req.QrToken,
                req.EventId,
                user.Id,
                user.MemberId,
                isAdmin: false);

            return Json(new { success = result.Success, message = result.Message });
        }


        //============== TIME OUT CODE SCANNER ===========

        [HttpGet]
        public async Task<IActionResult> ScannerForTimeOut(int eventId)
        {
            var guard = await AuthorizeEvent(eventId);
            if (guard != null) return guard;

            var state = await _eventService.GetScannerStateAsync(eventId); // REUSED
            if (state == null) return NotFound();

            return View(state); // new view: copy ScannerForTimeOut.cshtml, swap layout + fetch URLs
        }


        [HttpPost]
        public async Task<IActionResult> ScanTimeOut([FromBody] ScanAttendanceRequest req)
        {
            // Keep your existing event authorization check.
            var guard = await AuthorizeEvent(req.EventId);

            if (guard != null)
            {
                return guard;
            }


            // Get the currently logged-in user.
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Unable to identify the logged-in user."
                });
            }


            // Check whether the scanner is an Admin.
            bool isAdmin =
                await _userManager.IsInRoleAsync(
                    user,
                    "Admin"
                );


            // Pass the scanner information to the service.
            var result =
                await _eventService.RecordTimeOutAsync(
                    req.QrToken,
                    req.EventId,
                    user.Id,
                    user.MemberId,
                    isAdmin
                );


            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }


        // GET: /Leader/LeaderEvent/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.MemberId == null)
            {
                return Forbid();
            }

            // Get the group this leader manages
            var leaderGroup =
                await _groupService.GetGroupLedByMemberAsync(
                    user.MemberId.Value);

            if (leaderGroup == null)
            {
                return Forbid();
            }

            // Data needed by the shared Create Event partial
            ViewBag.Facilities =
                await _facilityService.GetAllFacilitiesAsync();

            ViewBag.Groups =
                await _groupService.GetAllGroupsAsync();

            // Tells the shared partial:
            // "This form belongs to the Leader Area"
            ViewBag.FormArea = "Leader";

            ViewBag.FormController = "Events";

            // Leader can only create an event for their own group
            ViewBag.LeaderGroupId =
                leaderGroup.GroupId;

            ViewBag.LeaderGroupName =
                leaderGroup.GroupName;

            var vm = new CreateEventViewModel();

            return PartialView(
                "~/Views/Events/_CreateEventPanel.cshtml",
                vm);
        }

        [HttpGet]
        public IActionResult GetScannerTimeModal()
        {
            return PartialView(
                "~/Views/Shared/_ScannerTimeModal.cshtml");
        }


        [HttpGet]
        public IActionResult GetScannerEditTimeModal()
        {
            return PartialView(
                "~/Views/Events/_EditScannerTimeModal.cshtml"
            );
        }




        [HttpGet]
        public async Task<IActionResult> GetEditForm(int eventid)
        {
            // Make sure the leader owns this event
            var guard = await AuthorizeEvent(eventid);

            if (guard != null)
            {
                return guard;
            }


            ViewBag.Facilities =
                await _facilityService.GetAllFacilitiesAsync();

            ViewBag.Groups =
                await _groupService.GetAllGroupsAsync();


            // IMPORTANT:
            // The shared edit form needs to know
            // where it should POST.
            ViewBag.FormController = "Events";
            ViewBag.FormArea = "Leader";


            var existingEvent =
                await _eventService.GetEventForEditAsync(eventid);

            if (existingEvent == null)
            {
                return NotFound();
            }


            return PartialView(
                "~/Views/Events/_EditEventPanel.cshtml",
                existingEvent
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            int? memberId = HttpContext.Session.GetInt32("CurrentMemberId");
            if (memberId == null)
                return Json(new { success = false, message = "Session expired. Please verify your identity again." });

            var result = await _eventService.CreateEventAsync(vm, isAdminCreated: false, memberId.Value);

            if (!result.Success)
                return Json(new { success = false, message = result.ErrorMessage });

            return Json(new { success = true, message = $"{vm.EventTitle} request submitted for approval." });
        }

        // GET: /Leader/LeaderEvent/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.MemberId == null)
                return Unauthorized();

            var eventDetails =
                await _eventService.GetDetailsByAsync(id);

            if (eventDetails == null)
                return NotFound();

            return PartialView("_ViewDetailsModal",eventDetails);
        }


        public async Task<IActionResult> EventViewScannerDetails(int? id)
        {
            if (id == null) return NotFound();

            var guard = await AuthorizeEvent(id.Value);
            if (guard != null) return guard;

            var eventDetails = await _eventService.GetDetailsByAsync(id.Value); // REUSED
            if (eventDetails == null) return NotFound();

            var leaderId = await GetCurrentLeaderIdAsync();
            var leaderGroup = await _groupService.GetGroupLedByMemberAsync(leaderId);
            if (leaderGroup != null)
            {
                ViewBag.GroupAttendanceDetails =
                    await _eventService.GroupAttendanceDetailsView(leaderGroup.GroupId, id.Value);
            }
            return View(eventDetails); 
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int? eventid,
    EditEventViewModel vm)
        {
            // ==============================
            // 1. CHECK EVENT ID
            // ==============================

            if (eventid == null ||
                eventid != vm.EventId)
            {
                return Json(new
                {
                    success = false,
                    message = "Event ID mismatch."
                });
            }


            // ==============================
            // 2. MAKE SURE LEADER OWNS EVENT
            // ==============================

            var guard =
                await AuthorizeEvent(eventid.Value);

            if (guard != null)
            {
                return Json(new
                {
                    success = false,
                    message = "You are not allowed to edit this event."
                });
            }


            // ==============================
            // 3. VALIDATE FORM
            // ==============================

            if (!ModelState.IsValid)
            {
                var errors =
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                return Json(new
                {
                    success = false,
                    message = string.Join(" ", errors)
                });
            }


            // ==============================
            // 4. UPDATE EVENT
            // ==============================

            var result =
                await _eventService.UpdateEventAsync(
                    eventid,
                    vm
                );


            if (!result.Success)
            {
                return Json(new
                {
                    success = false,
                    message = result.ErrorMessage
                });
            }


            // ==============================
            // 5. SUCCESS
            // ==============================

            return Json(new
            {
                success = true,
                message = $"{vm.EventTitle} updated successfully"
            });
        }


        // ====== CHECK ATTENDANCE for 
        public async Task<IActionResult> GroupAttendanceDetailsView(int? groupId, int? eventId)
        {
            if (groupId == null || eventId == null) return NotFound();

            var guard = await AuthorizeEvent(eventId.Value);
            if (guard != null) return guard;

            var groupAttendanceDetails = await _eventService.GroupAttendanceDetailsView(groupId.Value, eventId.Value); // REUSED
            if (groupAttendanceDetails == null) return NotFound();

            return PartialView("_GroupAttendanceModal", groupAttendanceDetails); // reuse the partial too
        }


        // GET: /Leader/Events/AllEvents
        [HttpGet]
        public async Task<IActionResult> AllEvents()
        {
            var events = await _eventService.GetAllApprovedEventsForOrgAsync();

            ViewBag.Groups = await _groupService.GetAllGroupsAsync();
            ViewBag.Facilities = await _facilityService.GetAllFacilitiesAsync();

            return View(events);
        }

        // GET: /Leader/Events/AllEventsDetails/5
        // Separate from the existing Details action so the modal can hide
        // Edit/Approve/Reject controls that only make sense for the owning leader.
        [HttpGet]
        public async Task<IActionResult> AllEventsDetails(int id)
        {
            var eventDetails = await _eventService.GetEventDetailsReadOnlyAsync(id);
            if (eventDetails == null) return NotFound();

            return PartialView("_AllEventsDetailsModal", eventDetails);
        }

        [HttpGet]
        public async Task<IActionResult> AssignedEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.MemberId == null) return Unauthorized();

            bool isLeader = await _groupService.IsMemberALeaderAsync(user.MemberId);
            if (!isLeader) return Forbid();

            var events = await _eventService.GetAssignedEventsForLeaderAsync(user.MemberId.Value);

            ViewBag.FormController = "Events";
            ViewBag.FormArea = "Leader";

            return View(events);
        }


        [HttpGet]
        public async Task<IActionResult> ViewAllFacilitiesModal(int id)
        {
            var guard = await AuthorizeEvent(id);
            if (guard != null) return guard;

            var facilities = await _eventService.GetFacilitiesForModalAsync(id);
            return PartialView("~/Views/Events/_AllFacilitiesModal.cshtml", facilities);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllGroupsModal(int id)
        {
            var guard = await AuthorizeEvent(id);
            if (guard != null) return guard;

            var groups = await _eventService.GetGroupsForModalAsync(id);
            return PartialView("~/Views/Events/_ViewAllGroupsModal.cshtml", groups);
        }

        public async Task<IActionResult> CancelledDetails(int id)
        {
            var vm = await _eventService.GetCancelledEventDetailsAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

    }


}

