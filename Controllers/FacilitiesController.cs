using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.ViewModels.Facility;
using Sym_Connect__Capstone2_.Services.Interfaces;

public class FacilitiesController : Controller
{
    private readonly IFacilityService _facilityService;

    public FacilitiesController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    // GET: FACILITYS
    public async Task<IActionResult> Index()
    {
        // GetAllFacilitiesAsync already returns fully-mapped FacilityListViewModel — no remapping needed
        var facilities = await _facilityService.GetAllFacilitiesAsync();
        return View(facilities);
    }

    // GET: FACILITYS/Details/5
    public async Task<IActionResult> Details(int? facilityid)
    {
        if (facilityid == null) return NotFound();

        var facility = await _facilityService.GetFacilityAsyncById(facilityid.Value);
        if (facility == null) return NotFound();

        return View(facility);
    }

    // GET: FACILITYS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: FACILITYS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFacilityViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _facilityService.CreateFacilityAsync(vm);

        if (!result.Success)
        {
            ModelState.AddModelError("FacilityName", result.Message);
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: FACILITYS/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var existingFacility = await _facilityService.GetFacilityAsyncById(id);
        if (existingFacility == null) return NotFound();

        var vm = new EditFacilityViewModel
        {
            FacilityId = existingFacility.FacilityId,
            FacilityName = existingFacility.FacilityName,
            FacilityDescription = existingFacility.FacilityDescription,
            FacilityTypes = existingFacility.FacilityType,
            Capacity = existingFacility.Capacity,
            Status = existingFacility.Status,
            ExistingPhotos = existingFacility.Photos ?? new List<ExistingPhotoViewModel>()
        };

        return PartialView("_EditFacilityForm", vm);
    }

    // POST: FACILITYS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditFacilityViewModel vm)
    {
        if (vm.FacilityId == 0) return NotFound();

        if (!ModelState.IsValid)
        {
            return PartialView("_EditFacilityForm", vm);
        }

        var result = await _facilityService.UpdateFacilityAsync(vm);

        if (!result.Success)
        {
            ModelState.AddModelError("FacilityName", result.Message);
            return PartialView("_EditFacilityForm", vm);
        }

        return RedirectToAction(nameof(Index));
    }

    //GET ARCHIVED
    public async Task<IActionResult> Archived(int? id)
    {
        if (id == null) return NotFound();

        var existingFacility = await _facilityService.GetFacilityAsyncById(id.Value);
        if (existingFacility == null) return NotFound();

        var vm = new ArchiveFacilityViewModel
        {
            facilityId = existingFacility.FacilityId,
            FacilityName = existingFacility.FacilityName,
            Description = existingFacility.FacilityDescription,
            Capacity = existingFacility.Capacity
        };

        return PartialView("_ArchiveFacilityForm", vm);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archived(int id)
    {
        var result = await _facilityService.ArchiveFacilityAsync(id);
        if (!result.Success) return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // GET: FACILITYS/UnderMaintenance/5
    public async Task<IActionResult> UnderMaintenance(int? facilityid)
    {
        if (facilityid == null) return NotFound();

        var existingFacility = await _facilityService.GetFacilityAsyncById(facilityid.Value);
        if (existingFacility == null) return NotFound();

        var vm = new MaintenanceFacilityViewModel
        {
            facilityId = existingFacility.FacilityId,
            FacilityName = existingFacility.FacilityName,
            Description = existingFacility.FacilityDescription,
            Capacity = existingFacility.Capacity
        };

        return View(vm);
    }

    // POST:
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnderMaintenance(int facilityid)
    {
        await _facilityService.MaintenanceFacilityAsync(facilityid);
        return RedirectToAction(nameof(Index));
    }
}