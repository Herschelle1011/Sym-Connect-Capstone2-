using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class FacilityController : Controller
    {
        private readonly IFacilityService _facilityService;

        public FacilityController(IFacilityService facilityService)
        {
            _facilityService = facilityService;
        }

        public async Task<IActionResult> Index()
        {
            // GetAllFacilitiesAsync already returns fully-mapped FacilityListViewModel — no remapping needed
            var facilities = await _facilityService.GetAllFacilitiesAsync();
            return View(facilities);
        }

        public async Task<IActionResult> Details(int? facilityid)
        {
            if (facilityid == null) return NotFound();

            var facility = await _facilityService.GetFacilityAsyncById(facilityid.Value);
            if (facility == null) return NotFound();

            return View(facility);
        }


    }
}
