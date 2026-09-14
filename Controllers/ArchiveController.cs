using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Archive;

[Authorize(Roles = "Admin")]
public class ArchiveController : Controller
{
    private readonly IArchiveService _archiveService;
    private readonly IFacilityService _facilityService;
    private readonly IGroupService _groupService;
    private readonly IMemberService _memberService;
    private readonly IPartyListService _partyListService;
    private readonly IElectionService _electionService;
    private readonly IMediaService _mediaService;



    public ArchiveController(IArchiveService archiveService, IFacilityService facilityService, 
                                        IGroupService groupService, IMemberService memberService, IPartyListService partyListService, IElectionService electionService, IMediaService mediaService)
    {
        _archiveService = archiveService;
        _facilityService = facilityService;
        _groupService = groupService;
        _memberService = memberService;
        _partyListService = partyListService;
        _electionService = electionService;
        _mediaService = mediaService;

    }

    // GET: Archive
    public async Task<IActionResult> Index()
    {
        var items = await _archiveService.GetAllArchivedAsync();
        return View(items);
    }




    public async Task<IActionResult> ViewDetails(
    int id,
    ArchiveCategory category)
    {
        switch (category)
        {
            case ArchiveCategory.Member:

                var member =
                    await _memberService
                        .GetArchivedDetailsAsync(id);

                if (member == null)
                    return NotFound();

                return PartialView(
                    "_ArchivedMemberDetails",
                    member);


            case ArchiveCategory.Group:

                var group =
                    await _groupService
                        .GetArchivedDetailsAsync(id);

                if (group == null)
                    return NotFound();

                return PartialView(
                    "_ArchivedGroupDetails",
                    group);


            case ArchiveCategory.Facility:

                var facility =
                    await _facilityService
                        .GetArchivedDetailsAsync(id);

                if (facility == null)
                    return NotFound();

                return PartialView(
                    "_ArchivedFacilityDetails",
                    facility);


            case ArchiveCategory.Election:

                var election =
                    await _electionService
                        .GetArchivedDetailsAsync(id);

                if (election == null)
                    return NotFound();

                return PartialView(
                    "_ArchivedElectionDetails",
                    election);


            case ArchiveCategory.PartyList:

                var partyList =
                    await _partyListService
                        .GetArchivedDetailsAsync(id);

                if (partyList == null)
                    return NotFound();

                return PartialView(
                    "_ArchivedPartyListDetails",
                    partyList);

            case ArchiveCategory.Media:
                {
                    var media =
                        await _mediaService
                            .GetArchivedDetailsAsync(id);

                    if (media == null)
                        return NotFound();

                    return PartialView(
                        "_ArchivedMediaDetails",
                        media);
                }


            default:
                return NotFound();
        }
    }



    // GET: Archive/ConfirmationRestore?id=5&category=Group&itemName=Spartan League
    [HttpGet]
    public IActionResult ConfirmationRestore(int id, ArchiveCategory category, string itemName)
    {
        var vm = new ArchiveConfirmationViewModel { Id = id, Category = category, ItemName = itemName };
        return PartialView("_RestoreArchiveModal", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, ArchiveCategory category)
    {
        var result = await _archiveService.RestoreAsync(id, category);
        return Json(new { success = result.Success, message = result.Message });
    }

    // GET: Archive/ConfirmationHardDelete?id=5&category=Member&itemName=Juan Cruz
    [HttpGet]
    public IActionResult ConfirmationHardDelete(int id, ArchiveCategory category, string itemName)
    {
        var vm = new ArchiveConfirmationViewModel { Id = id, Category = category, ItemName = itemName };
        return PartialView("_HardDeleteArchiveModal", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDelete(int id, ArchiveCategory category)
    {
        var result = await _archiveService.HardDeleteAsync(id, category);
        return Json(new { success = result.Success, message = result.Message });
    }

    public IActionResult ViewItem(int id, ArchiveCategory category)
    {
        return category switch
        {
            ArchiveCategory.Facility => RedirectToAction("Details", "Facilities", new { facilityid = id }), // confirmed

            ArchiveCategory.Group => RedirectToAction("Details", "Groups", new { id }),
            ArchiveCategory.Member => RedirectToAction("Details", "Profile", new { id }),
            ArchiveCategory.Evaluation => RedirectToAction("Details", "Evaluations", new { id }),
            ArchiveCategory.PartyList => RedirectToAction("Details", "PartyList", new { id }),
            ArchiveCategory.Election => RedirectToAction("Details", "Election", new { id }),
            _ => NotFound()
        };
    }
}