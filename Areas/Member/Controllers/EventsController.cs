using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class EventsController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly IFacilityService _facilityService;
        private readonly IEventService _eventService;
        private readonly UserManager<User> _userManager;

        public EventsController(IEventService eventService, UserManager<User> userManager)
        {
            _eventService = eventService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
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


            var model = await _eventService
                .GetEventsForMemberAsync(user.MemberId.Value);


            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CancelledDetails(int id)
        {
            var vm = await _eventService.GetCancelledEventDetailsAsync(id);

            // Still cancelled: show the full cancelled page
            if (vm != null)
                return View(vm);

            var existingEvent = await _eventService.GetEventByIdAsync(id);

            if (existingEvent == null || existingEvent.IsArchived)
                return NotFound();

            // Restored: send them to the Events page and open its modal
            TempData["EventRestoredNotice"] = $"\"{existingEvent.EventTitle}\" was cancelled earlier but has been restored.";

            return RedirectToAction("Index", "Events", new
            {
                area = "Member",
                openEventId = existingEvent.EventId
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
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

            var model = await _eventService
                .GetMemberEventDetailsAsync(
                    id,
                    user.MemberId.Value);

            if (model == null)
            {
                return NotFound();
            }

            return PartialView("_EventDetailsModal", model);
        }


        [HttpGet]
        public async Task<IActionResult> EventDetails(int id)
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


            var model = await _eventService.GetMemberEventDetailsAsync(id, user.MemberId.Value);

            if (model == null)
                return NotFound();

            return PartialView("_EventDetailsModal", model);
        }
    }
}
