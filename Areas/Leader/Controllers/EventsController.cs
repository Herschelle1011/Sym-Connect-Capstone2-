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
            var leaderId = CurrentLeaderId;
            if (leaderId == null) return Unauthorized();

            if (!await _eventService.LeaderOwnsEventsAsync(eventId, leaderId.Value))
                return Forbid();

            return null; // null = authorized, keep going
        }


        public async Task<IActionResult> Index()
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
            return View(events);
        }

        //========== LEADERS VIEW SCANNER =================
        private int? CurrentLeaderId => HttpContext.Session.GetInt32("CurrentMemberId");




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

            var result = await _eventService.RecordTimeInAsync(req.QrToken, req.EventId);
            return Json(new { sucess = result.Success, message = result.Message });
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
            var guard = await AuthorizeEvent(req.EventId);
            if (guard != null) return guard;

            var result = await _eventService.RecordTimeOutAsync(req.QrToken, req.EventId); 
            return Json(new { success = result.Success, message = result.Message });
        }

        
            // GET: /Leader/LeaderEvent/Create
            [HttpGet]
        public async Task<IActionResult> Create()
        {           
            return PartialView("~/Views/Events/_CreateEventPanel.cshtml", new CreateEventViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> GetEditForm(int eventid)
        {
            ViewBag.Facilities = await _facilityService.GetAllFacilitiesAsync();
            ViewBag.Groups = await _groupService.GetAllGroupsAsync();

            var existingEvent = await _eventService.GetEventForEditAsync(eventid);
            if (existingEvent == null) return NotFound();

            return PartialView("~/Views/Events/_EditEventPanel.cshtml", existingEvent);
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

            var leaderId = CurrentLeaderId;
            var leaderGroup = await _groupService.GetGroupLedByMemberAsync(leaderId);
            if (leaderGroup != null)
            {
                ViewBag.GroupAttendanceDetails =
                    await _eventService.GroupAttendanceDetailsView(leaderGroup.GroupId, id.Value);
            }
            return View(eventDetails); 
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
    }


}

