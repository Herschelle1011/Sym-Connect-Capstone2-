
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.EventModels;
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


    //------------ SCAN TIME IN ------
    [HttpPost]
    public async Task<IActionResult> ScanTimeIn([FromBody] ScanAttendanceRequest request)
    {
        var result = await _eventService.RecordTimeInAsync(request.QrToken, request.EventId);
        return Json(new { success = result.Success, message = result.Message });
    }

    public async Task<IActionResult> Scanner(int eventId)
    {
        var eventEntity = await _eventService.GetEventByIdAsync(eventId);
        if (eventEntity == null) return NotFound();

        var eventStart = eventEntity.StartDate.Date + (eventEntity.StartTime ?? TimeSpan.Zero);
        var eventEnd = eventEntity.EndDate.Value.Date + (eventEntity.EndTime ?? TimeSpan.Zero);

        bool isLive = DateTime.Now >= eventStart.AddMinutes(-30) && DateTime.Now <= eventEnd;
        if (!isLive)
            return RedirectToAction("Details", new { id = eventId });  

        var state = await _eventService.GetScannerStateAsync(eventId);
        if (state == null) return NotFound();

        return View(state);
    }

    [HttpGet]
    public async Task<IActionResult> GetScannerState(int eventId)
    {
        var state = await _eventService.GetScannerStateAsync(eventId);
        if (state == null) return NotFound();

        return Json(state);
    }


    //------------ SCAN TIME OUT ------

    public async Task<IActionResult> ScannerForTimeOut(int eventId)
    {
        var state = await _eventService.GetScannerStateAsync(eventId);
        if (state == null) return NotFound();
        return View(state);
    }

    [HttpPost]
    public async Task<IActionResult> ScanTimeOut([FromBody] ScanAttendanceRequest request)
    {
        var result = await _eventService.RecordTimeOutAsync(request.QrToken, request.EventId);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> NoTimeOut(string qrToken, int eventId)
    {
        bool success = await _eventService.NoTimeOut(qrToken, eventId);

        if (!success)
        {
            return Json(new { success = false, message = "Member or event not found." });
        }

        return Json(new { success = true, message = "Attendance reconciled." });
    }




    // --------GET: EVENTS--------------
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
        if(!ModelState.IsValid) return await ReturnToCalendarWithFailedForm(vm);

        var result = await _eventService.CreateEventAsync(vm);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage!);
            return await ReturnToCalendarWithFailedForm(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    //if fail or error reload facilities and groups
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
  
                var result = await _eventService.UpdateEventAsync(eventid, vm);
                    if (!result.Success)
                    {
                        ModelState.AddModelError("", result.ErrorMessage!);
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


    public async Task<IActionResult> SearchPanel(string search)
    {
        var existingEvent = await _eventService.SearchPanelAsync(search);
        return Json(existingEvent);

    }

    public async Task<IActionResult> AvailableFacilities()
    {
        var facilities = await _eventService.GetAvailableFacilitiesAsync();
        return View(facilities);
    }


    [HttpGet]
    public async Task<IActionResult> Schedule(int id, int? year, int? month)
    {
        var today = DateTime.Today;
        var vm = await _eventService.GetFacilityScheduleAsync(id, year ?? today.Year, month ?? today.Month);

        if (vm == null) return NotFound();

        return PartialView("_FacilityScheduleModal", vm);
    }
}
