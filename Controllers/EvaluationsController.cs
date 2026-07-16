
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;
using System.Runtime.Intrinsics.X86;
using Sym_Connect__Capstone2_.Services.Interfaces;

public class EvaluationsController : Controller
{
    private readonly IEvaluationService _evaluationservice;

    public IEvaluationService Evaluationservice => _evaluationservice;

    public EvaluationsController(IEvaluationService service)
    {
        _evaluationservice = service;
    }

    // GET: EVALUATIONS
    public async Task<IActionResult> Index()
    {
        var Evaluations = await _evaluationservice.GetAllEvaluationsAsync();
        var evaluation = Evaluations.Select(e => new ListEvaluationViewModel
        {
            EvaluationId = e.EvaluationId,
            EvaluationTitle = e.EvaluationTitle,
            Description = e.Description,  
            OpenAt = e.OpenAt,
            ClosesAt = e.ClosedAt,
            DateCreated = e.DateCreated
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

        var evaluation = await _evaluationservice.GetEvaluationByIdAsync(id.Value);
        if (evaluation == null)
        {
            return NotFound();
        }

        var Evaluations = new DetailsEvaluationViewModel
        {
            EvaluationId = evaluation.EvaluationId,
            EvaluationTitle = evaluation.EvaluationTitle,
            Description = evaluation.Description,
            OpenAt = evaluation.OpenAt,
            ClosesAt = evaluation.ClosedAt,
            DateCreated = evaluation.DateCreated,

            Questions = evaluation.EvaluationDetails.Select(q => new EvaluationQuestionViewModel
            {
                EvaluationDetailId = q.EvaluationDetailId,
                QuestionId = q.QuestionId,
                QuestionText = q.EvaluationQuestion!.QuestionText,
                AnswerType = q.EvaluationQuestion.AnswerType,
                
            }).ToList(),
                    Submissions = await _evaluationservice.GetSubmissionsForEvaluationAsync(id.Value) // ✅ new

        };

        return View(Evaluations);
    }

    public async Task<IActionResult> SubmissionDetails(int id)
    {
        var details = await _evaluationservice.GetSubmissionDetailsAsync(id);
        if (details == null) return NotFound();

        return PartialView("_SubmissionDetails", details);
    }

    // GET: EVALUATIONS/Create
    public async Task <IActionResult> Create()
    {
        var getallquestions = await _evaluationservice.GetAllQuestionsAsync();

        var vm = new CreateEvaluationViewModel
        {
            OpenAt = DateTime.Today,
            ClosedAt = DateTime.Today.AddDays(1),

            AvailableQuestions = getallquestions.Select(q => new AvailableQuestionViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
            }).ToList()
        };
               
        return View(vm);
    }

    // POST: EVALUATIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEvaluationViewModel vm)
    {
        DateValidation(vm);

        var questions = await _evaluationservice.GetAllQuestionsAsync();

        if (!ModelState.IsValid)
        {
            vm.AvailableQuestions = questions.Select(q => new AvailableQuestionViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
            }).ToList();

            return View(vm);
        }
           await _evaluationservice.CreateEvaluationAsync(vm);
           return RedirectToAction(nameof(Index));
    }

    // GET: EVALUATIONS/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var evaluation = await _evaluationservice.GetUpdateEvaluationAsync(id);
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
        DateValidation(vm);

        if (vm.EvaluationId == 0)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return PartialView("_EditEvaluationForm",vm);
        }
        try
        {
            await _evaluationservice.UpdateEvaluationAsync(vm);
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
        {
            return NotFound();
        }

        var evaluation = await _evaluationservice.GetArchiveEvaluationConfirmation(id.Value);
        if (evaluation == null)
        {
            return NotFound();
        }
        return PartialView("_ArchiveEvaluationForm", evaluation);
    }

    // POST: EVALUATIONS/Archive/5
    [HttpPost, ActionName("Archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchivedConfirmed(int id)
    {
        await _evaluationservice.ArchiveEvaluation(id);

        return RedirectToAction(nameof(Index));
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


    private void DateValidation(EvaluationFormViewModel vm)
    {
        if (vm.OpenAt >= vm.ClosedAt)
        {
            ModelState.AddModelError(nameof(vm.ClosedAt), "Close date must be after the open date.");
        }
        
    }

}
