using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Election;
using Sym_Connect__Capstone2_.ViewModels.PartyList;
using System.Threading.Tasks;


[Authorize(Roles = "Admin")]
public class PartyListsController : Controller
{
    private readonly IPartyListService _partyListService;
    private readonly IPositionService _positionService;

    public PartyListsController(IPartyListService partyListService, IPositionService positionService)
    {
        _partyListService = partyListService;
        _positionService = positionService;
    }

    // GET: PARTYLISTS
    public async Task<IActionResult> Index()
    {
        var partyLists = await _partyListService.GetAllPartyListsAsync();
        return View(partyLists);
    }

    public async Task<IActionResult> Position()
    {
        var positions = _positionService.GetPositionListViewModels();
        return View(positions);
    }


    // GET: PARTYLISTS/Details/5
    // FIX: Changed 'partylistid' to 'id' so it matches the view's routing output
    public async Task<IActionResult> Details(int id)
    {
        if (id == null) return NotFound();

        var partyList = await _partyListService.GetPositionsForPartyListAsync(id);
        if (partyList == null) return NotFound();

        return View(partyList);
    }

    // GET: PARTYLISTS/Create
    public IActionResult Create()
    {
        return PartialView("_CreatePartyList", new CreatePartyListViewModel());
    }

    // POST: PARTYLISTS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePartyListViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_CreatePartyList", model);
        }

        await _partyListService.CreatePartyListAsync(model);
        return RedirectToAction(nameof(Index));
    }

    // GET: PARTYLISTS/Edit/5
    // FIX: Changed 'partylistid' to 'id'
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var partyList = await _partyListService.GetPartyListByIdAsync(id.Value);
        if (partyList == null) return NotFound();

        var editModel = new EditPartyListViewModel
        {
            partyListId = partyList.partyListid,
            PartyListName = partyList.partyListName,
            PartyListDescription = partyList.partyDescription
        };

        return PartialView("_EditPartyListModal",editModel);
    }

    // POST: PARTYLISTS/Edit/5
    // FIX: Changed 'partylistid' to 'id'
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditPartyListViewModel model)
    {
        if (id != model.partyListId) return NotFound();
        if (!ModelState.IsValid) return PartialView("_EditPartyListModal", model);


        await _partyListService.EditPartyListAsync(model);
        return RedirectToAction(nameof(Index));
    }

    // GET: PARTYLISTS/Delete/5
    // FIX: Changed 'partylistid' to 'id'
    public async Task<IActionResult> Archive(int? id)
    {
        if (id == null) return NotFound();

        var modalVm = await _partyListService.GetArchiveModalAsync(id.Value);
        if (modalVm == null) return NotFound();

        return PartialView("_ArchiveFormModal", modalVm);
    }

    // POST: PARTYLISTS/Delete/5
    // FIX: Changed 'partylistid' to 'id'
    [HttpPost, ActionName("Archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveConfirmed(int id)
    {
        await _partyListService.ArchivePartyListAsync(id);
        return Json(new { success = true });
    }
    // GET: PARTYLISTS/ManageCandidates/5
    public async Task<IActionResult> ManageCandidates(int id)
    {
        var partyList = await _partyListService.GetPartyListByIdAsync(id);
        if (partyList == null) return NotFound();

        return View(partyList);
    }

    // POST: PARTYLISTS/AddCandidates
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCandidates([FromBody] BulkAddCandidatesViewModel model)
    {
        if (model == null || model.Assignments == null || model.Assignments.Count == 0)
        {
            return Json(new { success = false, message = "No assignments provided." });
        }

        var result = await _partyListService.AddOrUpdateCandidatesAsync(model);

        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid Submission." });
        }

        if (!result.Success)
        {
            return Json(new
            {
                success = false,
                message = result.Errors.Count > 0
                    ? string.Join(" ", result.Errors)
                    : (result.Message ?? "Failed to save positions.")
            });
        }

        return Json(new { success = true, message = result.Message ?? "Positions saved successfully." });
    }

    // GET: PARTYLISTS/SearchMembers?term=juan&partyListId=5

    [HttpGet]
    public async Task<IActionResult> SearchMembers([FromQuery] string term, [FromQuery] int partyListId)
    {
        if (string.IsNullOrWhiteSpace(term) || partyListId <= 0)
        {
            return Json(new object[] { });
        }

        try
        {
            var results = await _partyListService.SearchActiveMembersAsync(term, partyListId);
            return Json(results);
        }
        catch (Exception)
        {
            return Json(new object[] { });
        }
    }


}
