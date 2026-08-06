
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Event;


public class EventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly IFacilityService _facilityService;
    private readonly IGroupService _groupService;

    public EventsController(IEventService eventService, IFacilityService facilityService, IGroupService groupService)
    {
        _eventService = eventService;
        _facilityService = facilityService;
        _groupService = groupService;
    }

    // GET: EVENTS
    public async Task<IActionResult> Index(int? year, int? month)    
    {
        ViewBag.Facilities = await _facilityService.GetAllFacilitiesAsync();
        ViewBag.Groups = await _groupService.GetAllGroupsAsync();
        //set default values 
        var targetYear = year ?? DateTime.Now.Year;
        var targetMonth = month ?? DateTime.Now.Month;

        var vm = new EventListViewModel
        {
            Year = targetYear,
            Month = targetMonth,
            Events = await _eventService.GetAllEventAsync(targetYear, targetMonth)
        };

        return View(vm);
    }

    // GET: EVENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var eventDetails = await _eventService.GetDetailsByAsync(id.Value);
        if (eventDetails == null) return NotFound();

        return View(eventDetails);
    }

    // GET: EVENTS/Create
    // GET: EVENTS/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Facilities = await _facilityService.GetAllFacilitiesAsync();
        ViewBag.Groups = await _groupService.GetAllGroupsAsync();
        return PartialView("_CreateEventPanel");
    }

    // POST: EVENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnToCalendarWithFailedForm(vm);
        }

        var success = await _eventService.CreateEventAsync(vm);
        if (!success)
        {
            ModelState.AddModelError("", "This facility is already booked during the selected date/time.");
            return await ReturnToCalendarWithFailedForm(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    // Small helper so both failure paths build the SAME correct view model
    private async Task<IActionResult> ReturnToCalendarWithFailedForm(CreateEventViewModel vm)
    {
        ViewBag.Facilities = await _facilityService.GetAllFacilitiesAsync();
        ViewBag.Groups = await _groupService.GetAllGroupsAsync();

        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;

        var indexVm = new EventListViewModel
        {
            Year = year,
            Month = month,
            Events = await _eventService.GetAllEventAsync(year, month)
        };

        ViewBag.FailedCreateVm = vm;
        ViewBag.ShowCreatePanel = true;

        return View("Index", indexVm);
    }

    [HttpGet]
    public async Task<IActionResult> GetEditForm(int eventid)
    {
        ViewBag.Facilities = await _facilityService.GetAllFacilitiesAsync();
        ViewBag.Groups = await _groupService.GetAllGroupsAsync();

        var existingEvent = await _eventService.GetEventForEditAsync(eventid);
        if (existingEvent == null) return NotFound();

        return PartialView("_EditEventPanel", existingEvent);
    }

    // POST: EVENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? eventid, EditEventViewModel vm)
    {
        if (eventid != vm.EventId) return NotFound();     
        if (!ModelState.IsValid) return View(vm);
  
                var success = await _eventService.UpdateEventAsync(eventid, vm);
                    if (!success)
                    {
                        ModelState.AddModelError("", "This facility is already booked during the selected date/time.");
                        return View(vm);
                    }
                      return RedirectToAction(nameof(Index));
 
    }

    // GET: EVENTS/Archive/5
    public async Task<IActionResult> ConfirmationArchive(int? eventid)
    {
        if (eventid == null)
        {
            return NotFound();
        }
        var confirmationVm = await _eventService.ConfirmationArchiveAsync(eventid);
        if (confirmationVm == null) return NotFound();
       
        return PartialView("_ArchiveModal",confirmationVm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmedArchived(int eventid)
    {
        await _eventService.ArchiveEventAsync(eventid);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetFacilityPhotoUrl(int id)
    {
        var facility = await _facilityService.GetFacilityAsyncById(id);
        var photoUrl = facility?.FacilityPhotos?.FirstOrDefault()?.PhotoUrl;
        return Json(new { photoUrl });
    }



}
