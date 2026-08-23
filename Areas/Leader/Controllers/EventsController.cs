using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
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
    }


}

