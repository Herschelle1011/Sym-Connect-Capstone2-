using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.PartyList.Positions;

public class PositionController : Controller
{
    private readonly IPositionService _positionService;

    public PositionController(IPositionService positionService)
    {
        _positionService = positionService;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var positions = await _positionService.GetPositionListViewModels();
        return View(positions);
    }

    // GET
    public async Task<IActionResult> GetEditModal(int id)
    {
        var vm = await _positionService.GetEditedPositionAsync(id);
        if (vm == null) return NotFound();
        return PartialView("_EditPositionModal", vm);
    }

    public IActionResult GetCreateModal()
    {
        return PartialView("_CreatePositionModal", new CreatePositionViewModel());
    }

    // POST
    [HttpPost]
    public async Task<IActionResult> Create(CreatePositionViewModel vm)
    {
        if (!ModelState.IsValid) return Json(new { success = false });
        await _positionService.CreateAsync(vm);
        return Json(new { success = true });
    }

    // POST: update
    [HttpPost]
    public async Task<IActionResult> Edit(EditPositionViewModel vm)
    {
        if (!ModelState.IsValid) return Json(new { success = false });
        var result = await _positionService.UpdatePositionAsync(vm);
        return Json(new { success = result });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id, bool isActive)
    {
        var result = await _positionService.SetActiveStatusAsync(id, isActive);
        return Json(new { success = result });
    }
}