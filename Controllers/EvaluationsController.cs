
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;
using System.Runtime.Intrinsics.X86;

[Authorize(Roles = "Admin")]
public class EvaluationsController : Controller
{
    private readonly IEvaluationService _evaluationservice;
    private readonly IEventService _eventService;
    private readonly UserManager<User> _userManager;


    public IEvaluationService Evaluationservice => _evaluationservice;

    public EvaluationsController(IEvaluationService service, IEventService eventService, UserManager<User> userManager)
    {
        _evaluationservice = service;
        _eventService = eventService;
        _userManager = userManager;
    }

    // GET: EVALUATIONS
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.MemberId == null)
            return Forbid();

        var isAdmin =
            await _userManager.IsInRoleAsync(user, "Admin");

        if (!isAdmin)
        {
            return Forbid();
        }

        var Evaluations = await _evaluationservice.GetAvailableEvaluationsAsync(user.MemberId.Value, isAdmin);
        var evaluation = Evaluations.Select(e => new ListEvaluationViewModel
        {
            EvaluationId = e.EvaluationId,
            EvaluationTitle = e.EvaluationTitle,
            Description = e.Description,
            OpenAt = e.OpenAt,
            ClosesAt = e.ClosedAt,
            DateCreated = e.DateCreated,
            EventTitle = e.EventTitle,
        }).ToList();

        return View(evaluation);
    }

    // GET: EVALUATIONS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.MemberId == null)
        {
            return Forbid();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        var evaluation = await _evaluationservice.GetEvaluationByIdAsync(
            id.Value,
            user.MemberId.Value,
            isAdmin
        );

        if (evaluation == null)
        {
            return NotFound();
        }

        evaluation.Submissions =
            await _evaluationservice.GetSubmissionsForEvaluationAsync(id.Value);

        return View(evaluation);
    }

    public async Task<IActionResult> SubmissionDetails(int id)
    {
        var details = await _evaluationservice.GetSubmissionDetailsAsync(id);
        if (details == null) return NotFound();

        return PartialView("_SubmissionDetails", details);
    }



    // GET: EVALUATIONS/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.MemberId == null)
        {
            return Forbid();
        }
        var isAdmin =
         await _userManager.IsInRoleAsync(user, "Admin");

        var getallquestions = await _evaluationservice.GetFrequentlyUsedQuestionsAsync();
        var CompletedEvents = await _eventService.GetAllEventForEvaluationAsync(user.MemberId.Value, isAdmin);

        var vm = new CreateEvaluationViewModel
        {
            OpenAt = DateTime.Today,
            ClosedAt = DateTime.Today.AddDays(1),

            EventOptions = CompletedEvents.Select(e => new EvaluationEventOptionViewModel
            {
                EventId = e.EventId,
                EventTitle = e.EventTitle,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                GroupNames = e.GroupNames,
                FacilityNames = e.FacilityNames
            }).ToList(),
            AvailableQuestions = getallquestions.Select(q => new AvailableQuestionViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                AnswerType = q.AnswerType,
            }).ToList()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> SelectEvent()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.MemberId == null)
        {
            return Forbid();
        }
        var isAdmin =
         await _userManager.IsInRoleAsync(user, "Admin");

        var completedEvents =
            await _eventService.GetAllEventForEvaluationAsync(user.MemberId.Value, isAdmin);

        var vm = completedEvents
            .Select(e => new EvaluationEventOptionViewModel
            {
                EventId = e.EventId,
                EventTitle = e.EventTitle,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                GroupNames = e.GroupNames,
                FacilityNames = e.FacilityNames
            })
            .ToList();

        return PartialView("_EvaluationEventPicker", vm);
    }

    // POST: EVALUATIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEvaluationViewModel vm)
    {
        DateValidation(vm);

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.MemberId == null)
        {
            return Forbid();
        }
        var isAdmin =
         await _userManager.IsInRoleAsync(user, "Admin");

        if (!ModelState.IsValid)
        {
            var frequentlyUsedQuestions =
                await _evaluationservice.GetFrequentlyUsedQuestionsAsync();

            vm.AvailableQuestions = frequentlyUsedQuestions
                .Select(q => new AvailableQuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    AnswerType = q.AnswerType
                })
                .ToList();

            return View(vm);
        }

        await _evaluationservice.CreateEvaluationAsync(vm, user.MemberId.Value, isAdmin);

        return RedirectToAction(nameof(Index));
    }
    // GET: EVALUATIONS/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var status = await _evaluationservice.GetCurrentStatusAsync(id);

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.MemberId == null)
        {
            return Forbid();
        }
        var isAdmin =
         await _userManager.IsInRoleAsync(user, "Admin");

        if (status == null)
        {
            return NotFound();
        }

        if (status != "Upcoming")
        {
            TempData["ErrorMessage"] =
                "This evaluation can only be edited while it's still Upcoming.";

            return RedirectToAction("Details", new { id });
        }

        var evaluation =
            await _evaluationservice.GetUpdateEvaluationAsync(id, user.MemberId.Value, isAdmin);

        if (evaluation == null)
        {
            return NotFound();
        }

        return View(evaluation);
    }

    // POST: EVALUATIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateEvaluationViewModel vm)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (user.MemberId == null)
        {
            return Forbid();
        }
        var isAdmin =
         await _userManager.IsInRoleAsync(user, "Admin");

        var status = await _evaluationservice.GetCurrentStatusAsync(vm.EvaluationId);
        if (status != "Upcoming")
        {
            TempData["ErrorMessage"] = "This evaluation can only be edited while it's still Upcoming.";
            return RedirectToAction("Details", new { vm.EvaluationId });
        }

        DateValidation(vm);

        if (vm.EvaluationId == 0)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            var allQuestions = await _evaluationservice.GetAllQuestionsAsync();
            vm.AvailableQuestions = allQuestions.Select(q => new AvailableQuestionViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                AnswerType = q.AnswerType
            }).ToList();
            return View(vm);
        }
        try
        {
            await _evaluationservice.UpdateEvaluationAsync(vm, user.MemberId.Value, isAdmin);
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {

            if (!await _evaluationservice.EvaluationExist(vm.EvaluationId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }

        }
    }

    //GET 
    public async Task<IActionResult> EditQuestionViewModel(int id)
    {
        if (id == 0) return NotFound();
        var question = await _evaluationservice.GetQuestionAsync(id);
        return View(question);
    }

    [HttpPost]
    public async Task<IActionResult> SaveQuestionViewModel(EditQuestionViewModel vm)
    {
        if (!ModelState.IsValid || vm.EvaluationDetailId == 0)
            return Json(new { success = false });

        await _evaluationservice.EditQuestionAsync(vm);
        return Json(new { success = true, questionText = vm.QuestionText });
    }




    // GET: EVALUATIONS/Delete/5
    public async Task<IActionResult> Archive(int? id)
    {
        if (id == null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Challenge();

        if (user.MemberId == null)
            return Forbid();

        var isAdmin =
            await _userManager.IsInRoleAsync(user, "Admin");

        var evaluation =
            await _evaluationservice
                .GetArchiveEvaluationConfirmation(
                    id.Value,
                    user.MemberId.Value,
                    isAdmin);

        if (evaluation == null)
            return NotFound();

        return PartialView(
            "_ArchiveEvaluationForm",
            evaluation);
    }

    // POST: EVALUATIONS/Archive/5
    [HttpPost, ActionName("Archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchivedConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Json(new
            {
                success = false,
                message = "User account was not found."
            });
        }

        if (user.MemberId == null)
        {
            return Json(new
            {
                success = false,
                message = "Member account was not found."
            });
        }

        var isAdmin =
            await _userManager.IsInRoleAsync(user, "Admin");

        var result =
            await _evaluationservice.ArchiveEvaluationAsync(
                id,
                user.MemberId.Value,
                isAdmin
            );

        return Json(new
        {
            success = result.Success,
            message = result.Message
        });
    }


    // GET: EVALUATIONS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var evaluation = await _evaluationservice.GetEvaluationByIdAsync(id.Value);
        if (evaluation == null)
        {
            return NotFound();
        }
        var eval = new DeleteEvaluationViewModel
        {
            EvaluationId = evaluation.EvaluationId,
            EvaluationTitle = evaluation.EvaluationTitle,
            Description = evaluation.Description,
            DateCreated = evaluation.DateCreated,
        };

        return View(eval);
    }

    // POST: EVALUATIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int evaluationid)
    {
        await _evaluationservice.DeleteEvaluationAsync(evaluationid);
        return RedirectToAction(nameof(Index));
    }

    //TO BE CHECKED, LACKING IS UI 
    public async Task<IActionResult> SummaryReports(int? id)
    {
        if (id == null) { return NotFound(); }
        var report = await _evaluationservice.summaryReport(id);
        if (report == null) { return NotFound(); }
        return PartialView("_SummaryReportsPartial", report);
    }


    private void DateValidation(EvaluationFormViewModel vm)
    {
        if (vm.OpenAt >= vm.ClosedAt)
        {
            ModelState.AddModelError(nameof(vm.ClosedAt), "Close date must be after the open date.");
        }

    }

}