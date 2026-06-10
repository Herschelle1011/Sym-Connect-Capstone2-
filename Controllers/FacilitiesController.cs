
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services;
using static Sym_Connect__Capstone2_.ViewModels.FacilityViewModel;
using Sym_Connect__Capstone2_.Data;

public class FacilitiesController : Controller
{
    private readonly IFacilityService _facilityService;
    private readonly CloudinaryService _cloudinaryService;


    public FacilitiesController(IFacilityService facilityService, CloudinaryService cloudinaryService)
    {
        _facilityService = facilityService;
        _cloudinaryService = cloudinaryService;
    }

    // GET: FACILITYS
    public async Task<IActionResult> Index()    
    {
        var facilities = await _facilityService.GetAllFacilitiesAsync();

        var vm = facilities.Select(f => new FacilityListViewModel
        {
            FacilityId = f.FacilityId,
            FacilityName = f.FacilityName,
            FacilityDescription = f.FacilityDescription,
            Capacity = f.Capacity,
            Status = f.Status,

            FacilityTypes = (f.FacilityTypes ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList(),

            Photos = f.FacilityPhotos?
                .Select(p => p.PhotoUrl)
                .ToList()
                ?? new List<string>()
        }).ToList();

        return View(vm);
    }

    // GET: FACILITYS/Details/5
    public async Task<IActionResult> Details(int? facilityid)
    {
        if (facilityid == null)
        {
            return NotFound();
        }
        var facility = await _facilityService.GetFacilityAsyncById(facilityid.Value);
        
        if (facility == null)
        {
            return NotFound();
        }

        var FACILITY = new FacilityDetailsViewModel
        {
            FacilityId = facility.FacilityId,
            FacilityName = facility.FacilityName,
            FacilityDescription = facility.FacilityDescription,
            FacilityType = facility.FacilityTypes,
            Capacity = facility.Capacity,
            Status = facility.Status,
            Photos = facility.FacilityPhotos.Select(e => e.PhotoUrl).ToList() ?? new List<string>()
        };

 
        return View(FACILITY);
    }

    // GET: FACILITYS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: FACILITYS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFacilityViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if(await _facilityService.FacilityNameExist(vm.FacilityName))
        {
            ModelState.AddModelError("FacilityName", "Facility Name already exist");
            return View(vm);
        }

        var facility = new Facility
        {
            FacilityName = vm.FacilityName,
            FacilityDescription = vm.FacilityDescription,
            FacilityTypes = vm.FacilityTypes,
            Capacity = vm.Capacity,
            Status = vm.Status,
            IsArchived = false
        };


        //for photos
        if(vm.Photos != null && vm.Photos.Count > 0)
        {
            foreach(var Photo in vm.Photos)
            {
                var photoUrl = await _cloudinaryService.UploadFacilityPhotoAsync(Photo, vm.FacilityName);

                if(photoUrl != null)
                {
                    facility.FacilityPhotos.Add(new FacilityPhoto
                    {
                        PhotoUrl = photoUrl
                    });
                }
            }
        }

        await _facilityService.CreateFacilityAsync(facility);
        return RedirectToAction(nameof(Index));

    }

    // GET: FACILITYS/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var facilityid = await _facilityService.GetFacilityAsyncById(id);

        if (facilityid == null) return NotFound();
        var facility = new EditFacilityViewModel
        {
            FacilityId = facilityid.FacilityId,
            FacilityName = facilityid.FacilityName,
            FacilityDescription = facilityid.FacilityDescription,
            FacilityTypes = facilityid.FacilityTypes,
            Capacity = facilityid.Capacity,
            Status = facilityid.Status,
            ExistingPhotos = facilityid.FacilityPhotos?.Select(p => new ExistingPhotoViewModel
            {
                PhotoId = p.FacilityPhotoId,
                PhotoUrl = p.PhotoUrl
            }).ToList() ?? new List<ExistingPhotoViewModel>()

        };

        return PartialView("_EditFacilityForm", facility);

    }

    // POST: FACILITYS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditFacilityViewModel vm)
    {
        if (vm.FacilityId == 0)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return PartialView("_EditFacilityForm", vm);
        }
        if (await _facilityService.FacilityNameExist(vm.FacilityName, vm.FacilityId))
        {
            ModelState.AddModelError("FacilityName", "Facility name already exists.");
            return PartialView("_EditFacilityForm", vm);
        }

        var facility = await _facilityService.GetFacilityAsyncById(vm.FacilityId);
        if (facility == null) return NotFound();

        facility.FacilityName = vm.FacilityName;
        facility.FacilityDescription = vm.FacilityDescription;
        facility.FacilityTypes = vm.FacilityTypes;
        facility.Capacity = vm.Capacity;
        facility.Status = vm.Status;

        //for photos
        if (vm.RemovedPhotoIds != null && vm.RemovedPhotoIds.Count > 0)
        {
            await _facilityService.DeletePhotosAsync(vm.RemovedPhotoIds);
        }

        // ✅ Step 4: Handle new photo uploads
        if (vm.Photos != null && vm.Photos.Count > 0)
        {
            // Initialize the list if null (safety check)
            facility.FacilityPhotos ??= new List<FacilityPhoto>();

            foreach (var photo in vm.Photos)
            {
                var photoUrl = await _cloudinaryService.UploadFacilityPhotoAsync(photo, vm.FacilityName);

                if (photoUrl != null)
                {
                    facility.FacilityPhotos.Add(new FacilityPhoto
                    {
                        PhotoUrl = photoUrl,
                        FacilityId = facility.FacilityId  // link back to parent
                    });
                }
            }
        }

        //  Save
        await _facilityService.UpdateFacilityAsync(facility);

        return RedirectToAction(nameof(Index));
    }

    //GET ARCHIVED
    public async Task<IActionResult> Archived(int? id)
    {
        if (id == null) return NotFound();
        var facilityId = await _facilityService.GetFacilityAsyncById(id.Value);
        if (facilityId == null) return NotFound();

        var facility = new ArchiveFacilityViewModel
        {
            facilityId = facilityId.FacilityId,
            FacilityName = facilityId.FacilityName,
            Description = facilityId.FacilityDescription,
            Capacity = facilityId.Capacity
        };
        return PartialView("_ArchiveFacilityForm",facility);

    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archived(int id)
    {
        await _facilityService.ArchiveFacilityAsync(id);
        return RedirectToAction(nameof(Index));
    }



    // GET: FACILITYS/Delete/5
    public async Task<IActionResult> UnderMaintenance(int? facilityid)
    {
        if (facilityid == null)
        {
            return NotFound();
        }

        var facility = await _facilityService.GetFacilityAsyncById(facilityid.Value);
        if (facility == null)
        {
            return NotFound();
        }
        var Facility = new MaintenanceFacilityViewModel
        {
            facilityId = facility.FacilityId,
            FacilityName = facility.FacilityName,
            Description = facility.FacilityDescription,
            Capacity = facility.Capacity
        };


        return View(Facility);
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
