using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Election;
using Sym_Connect__Capstone2_.ViewModels.Election.PartyList;
using System.Threading.Tasks;

public class PartyListsController : Controller
{
    private readonly IPartyListService _partyListService;

    public PartyListsController(IPartyListService partyListService)
    {
        _partyListService = partyListService;
    }

    // GET: PARTYLISTS
    public async Task<IActionResult> Index()
    {
        var partyLists = await _partyListService.GetAllPartyListsAsync();
        return View(partyLists);
    }

    // GET: PARTYLISTS/Details/5
    // FIX: Changed 'partylistid' to 'id' so it matches the view's routing output
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var partyList = await _partyListService.GetPartyListByIdAsync(id.Value);
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

        return View(editModel);
    }

    // POST: PARTYLISTS/Edit/5
    // FIX: Changed 'partylistid' to 'id'
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditPartyListViewModel model)
    {
        if (id != model.partyListId) return NotFound();
        if (!ModelState.IsValid) return View(model);

        await _partyListService.EditPartyListAsync(model);
        return RedirectToAction(nameof(Index));
    }

    // GET: PARTYLISTS/Delete/5
    // FIX: Changed 'partylistid' to 'id'
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var partyList = await _partyListService.GetPartyListByIdAsync(id.Value);
        if (partyList == null) return NotFound();

        return View(partyList);
    }

    // POST: PARTYLISTS/Delete/5
    // FIX: Changed 'partylistid' to 'id'
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _partyListService.ArchivePartyListAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // GET: PARTYLISTS/ManageCandidates/5
    // FIX: Changed 'partylistid' to 'id'
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

        try
        {
            await _partyListService.AddOrUpdateCandidatesAsync(model);
            return Json(new { success = true });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "Failed to save assignments." });
        }
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
