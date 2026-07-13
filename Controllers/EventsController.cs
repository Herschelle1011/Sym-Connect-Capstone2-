
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

public class EventsController : Controller
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    // GET: EVENTS
    public async Task<IActionResult> Index(int? year, int? month)    
    {
        //set default values 
        var targetYear = year ?? DateTime.Now.Year;
        var targetMonth = year ?? DateTime.Now.Month;

        return View(await _eventService.GetAllEventAsync(targetYear, targetMonth));
    }

    // GET: EVENTS/Details/5
    public async Task<IActionResult> Details(int? eventid)
    {
        if (eventid == null)
        {
            return NotFound();
        }

        var event = await _context.Events
            .FirstOrDefaultAsync(m => m.EventId == eventid);
        if (event == null)
        {
            return NotFound();
        }

        return View(event);
    }

    // GET: EVENTS/Create
    public IActionResult Create()
    {
        return View();
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
            return View(vm);
        }
        await _eventService.CreateEventAsync(vm);
        return RedirectToAction(nameof(Index));
    }

    // GET: EVENTS/Edit/5
    public async Task<IActionResult> Edit(int? eventid)
    {
        if (eventid == null)
        {
            return NotFound();
        }

        var existingEvent = _eventService.GetEventForEditAsync(eventid);
        if (existingEvent == null) return NotFound();

        return View(existingEvent);
    }

    // POST: EVENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? eventid, EditEventViewModel vm)
    {
        if (eventid != vm.EventId)
        {
            return NotFound();
        }
        if (!ModelState.IsValid) return View(vm);

            try
            {
                await _eventService.UpdateEventAsync(eventid, vm);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(vm.EventId))
                {
                    return NotFound();
                }
            throw;        
            }
            return RedirectToAction(nameof(Index));
 
    }

    // GET: EVENTS/Delete/5
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

    // POST: EVENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmedArchived(int eventid)
    {
        await _eventService.ArchiveEventAsync(eventid); 
        return RedirectToAction(nameof(Index));
    }

    private bool EventExists(int? eventid)
    {
        return _context.Events.Any(e => e.EventId == eventid);
    }

}
