using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Election;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

public class ElectionsController : Controller
{
    private readonly IElectionService _electionService;
    private readonly IPartyListService _partyListService;

    public ElectionsController(IElectionService electionService, IPartyListService partyListService)
    {
        _electionService = electionService;
        _partyListService = partyListService;
    }

    // GET: ELECTIONS
    public async Task<IActionResult> Index()
    {
        var elections = await _electionService.GetAllElectionListAsync();
        return View(elections);
    }
    public async Task<IActionResult> Result(int? electionId)
    {
        if (electionId == null) return NotFound();
        var Result = await _electionService.GetElectionResultAsync(electionId.Value);
        if (Result == null) return NotFound();
        return View(Result);
    }

    // GET: ELECTIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var electionDetails = await _electionService.GetElectionByIdAsync(id);
        if (electionDetails == null) return NotFound();

        return View(electionDetails);
    }

    // GET: ELECTIONS/Create
    // Loads Step 1 fields + the unassigned party lists needed for Step 2 — everything the
    // single-page wizard needs, so no further page navigation is required.
    public async Task<IActionResult> Create()
    {
        var vm = new CreateElectionWizardViewModel
        {
            AvailablePartyLists = await _partyListService.GetUnassignedPartyListsAsync()
        };

        return View(vm);
    }

   

    // POST: ELECTIONS/Create
    // Single submit for the entire wizard: creates the Election, assigns the 2 chosen
    // party lists to it, and creates every Candidate row — all in one transaction.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateElectionWizardViewModel model) // 1. Change parameter type here
    {
        if (!ModelState.IsValid)
        {
            model.AvailablePartyLists = await _partyListService.GetUnassignedPartyListsAsync();
            return View(model);
        }
        try
        {
            // 2. Call the new method here instead of CreateElectionAsync
            await _electionService.CreateElectionWithPartyListsAsync(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.AvailablePartyLists = await _partyListService.GetUnassignedPartyListsAsync();
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }
    // GET: ELECTIONS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var editModel = await _electionService.GetEditElectionByIdAsync(id.Value);
        if (editModel == null) return NotFound();

        return View(editModel);
    }

    // POST: ELECTIONS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditElectionViewModel model)
    {
        if (id != model.ElectionId) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _electionService.EditElectionAsync(model);
        return RedirectToAction(nameof(Index));
    }

    // ---------------- Archive ----------------\





    public async Task<IActionResult> GetArchive(int? id)
    {
        if (id == null) return NotFound();

        var election = await _electionService.GetArchiveElectionAsync(id);
        if (election == null) return NotFound();

        return PartialView("_ArchiveElectionModal",election);
    }

    // POST: ELECTIONS/Delete/5
    [HttpPost]
    [ActionName("Archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveConfirmed(int electionId)
    {
        await _electionService.ArchiveElectionAsync(electionId);
        return Json(new { success = true });
    }

    // GET: ELECTIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var election = await _electionService.GetElectionByIdAsync(id);
        if (election == null) return NotFound();

        return View(election);
    }

    // POST: ELECTIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null) return NotFound();

        await _electionService.ArchiveElectionAsync(id);
        return RedirectToAction(nameof(Index));
    }
}