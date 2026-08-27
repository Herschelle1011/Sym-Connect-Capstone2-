
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

    public async Task<IActionResult> ScannerForTimeIn(int eventId)
    {
        var eventEntity = await _eventService.GetEventByIdAsync(eventId);
        if (eventEntity == null) return NotFound();

        var eventStart = eventEntity.StartDate.Date + (eventEntity.StartTime ?? TimeSpan.Zero);
        var eventEnd = eventEntity.EndDate.Date + (eventEntity.EndTime ?? TimeSpan.Zero);

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





    // --------GET: EVENTS--------------
    public async Task<IActionResult> Index(int? year, int? month)    
    {
        ViewBag.PendingRequestCount = await _eventService.GetPendingRequestCountAsync(); 
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

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_ViewDetailsEventRequests", eventDetails);

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
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join(" ", errors) });
        }

        int? memberId = HttpContext.Session.GetInt32("CurrentMemberId");
           if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return Json(new { success = false, message = "You must be logged in as Admin." });
        }


        var result = await _eventService.CreateEventAsync(vm, isAdminCreated: true, createByUserId: null);

        if (!result.Success)
        {
            return Json(new { success = false, message = result.ErrorMessage });
        }

        return Json(new { success = true, message = $"{vm.EventTitle} created successfully" });
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
        if (eventid != vm.EventId) return Json(new { success = false, message = "Event ID mismatch." });

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join(" ", errors) });
        }

        var result = await _eventService.UpdateEventAsync(eventid, vm);

        if (!result.Success)
        {
            return Json(new { success = false, message = result.ErrorMessage });
        }

        return Json(new { success = true, message = $"{vm.EventTitle} updated successfully" });
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
        var photoUrl = facility?.Photos?.FirstOrDefault()?.PhotoUrl;
        return Json(new { photoUrl });
    }

    [HttpGet]
    public async Task<IActionResult> ViewAllFacilitiesModal(int? id)
    {
        var facilities = await _eventService.GetFacilitiesForModalAsync(id);
        return PartialView("_ViewAllFacilitiesModal", facilities);
    }


    [HttpGet]
    public async Task<IActionResult> ViewAllGroupsModal(int? id)
    {
        var groups = await _eventService.GetGroupsForModalAsync(id);
        return PartialView("_ViewAllGroupsModal", groups);
    }


    public async Task<IActionResult> SearchPanel(string search)
    {
        var existingEvent = await _eventService.SearchPanelAsync(search);
        return Json(existingEvent);

    }

    //OPEN SCANNER TIME MODIFY MODAL
    [HttpGet]
    public IActionResult GetScannerTimeModal()
    {
        return PartialView("_ScannerTimeModal");
    }


    //OPEN SCANNER TIME MODIFY MODAL
    [HttpGet]
    public IActionResult GetScannerEditTimeModal()
    {
        return PartialView("_EditScannerTimeModal");
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


    //EVENT REQUEST CLICK CARD ==============
    public async Task<IActionResult> ViewEventsRequests()
    {
        var events = await _eventService.GetEventsSummaryForAdminAsync();
        return View(events);
    }







    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveEvent(int eventId)
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return Json(new { success = false, message = "You must be logged in as Admin." });
        }

        // TODO: [AUTH MIGRATION] approveByUserId is null — Admin has no Members
        // row to reference. See CreatedByUserId note in EventController.Create.
        var result = await _eventService.ApproveEventAsync(eventId, approveByUserId: null);


        if (!result.Success)
            return Json(new { success = false, message = result.ErrorMessage });

        return Json(new { success = true, message = "Event approved successfully." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectAnEvent(int eventId, string reason)
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return Json(new { success = false, message = "You must be logged in as Admin." });
        }

        // TODO: [AUTH MIGRATION] approveByUserId is null — Admin has no Members
        // row to reference. See CreatedByUserId note in EventController.Create.
        var result = await _eventService.RejectEventAsync(eventId, reason: reason);


        if (!result.Success)
            return Json(new { success = false, message = result.ErrorMessage });

        return Json(new { success = true, message = "Event rejected." });
    }



    //ATTENDANCE TAB VIEW DETAILS VIEW MEMBERS 
    public async Task<IActionResult> GroupAttendanceDetailsView(int? groupId, int? eventId)
    {
        if (groupId == null) return NotFound();
        if (eventId == null) return NotFound();


        var groupAttendanceDetails = await _eventService.GroupAttendanceDetailsView(groupId.Value, eventId.Value);

        if (groupAttendanceDetails == null) return NotFound();
        
        return PartialView("_GroupAttendanceModal", groupAttendanceDetails);
    }

}
