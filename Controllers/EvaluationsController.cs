
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Services;
using static Sym_Connect__Capstone2_.ViewModels.EvaluationViewModel;

public class EvaluationsController : Controller
{
    private readonly IEvaluationService _evaluationservice;

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
            Status = e.Status,
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
            Status = evaluation.Status,
            DateCreated = evaluation.DateCreated,
        };

        return View(Evaluations);
    }

    // GET: EVALUATIONS/Create
    public IActionResult Create()
    {
        return PartialView("_CreateEvaluationForm");
    }

    // POST: EVALUATIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEvaluationViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_CreateEvaluationForm", vm);

        }
        var evaluation = new Evaluation
        {
            EvaluationTitle = vm.EvaluationTitle,
            Description = vm.Description,
            DateCreated = DateTime.Now,
            Status = vm.Status,
        };
       
            await _evaluationservice.CreateEvaluationAsync(evaluation);
            return RedirectToAction(nameof(Index));
            
    }

    // GET: EVALUATIONS/Edit/5

    public async Task<IActionResult> Edit(int id)
    {

        var evaluation = await _evaluationservice.GetEvaluationByIdAsync(id);
        if (evaluation == null)
        {
            return NotFound();
        }
        var eval = new UpdateEvaluationViewModel
        {
            EvaluationId = evaluation.EvaluationId,
            EvaluationTitle = evaluation.EvaluationTitle,
            Description = evaluation.Description,
            Status = evaluation.Status,
        };
        return PartialView("_EditEvaluationForm", eval);
    }

    // POST: EVALUATIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateEvaluationViewModel vm)
    {
        if (vm.EvaluationId == 0)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return PartialView("_EditEvaluationForm",vm);
        }
        var update = new Evaluation
        {
            EvaluationId = vm.EvaluationId,
            EvaluationTitle = vm.EvaluationTitle,
            Description = vm.Description,
            Status = vm.Status,
            UpdatedAt = DateTime.Now,
        };
        try
        {

            await _evaluationservice.UpdateEvaluationAsync(update);
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
    // GET: EVALUATIONS/Delete/5
    public async Task<IActionResult> Archive(int? id)
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
        var eval = new ArchiveEvaluationViewModel
        {
            EvaluationId = evaluation.EvaluationId,
            EvaluationTitle = evaluation.EvaluationTitle,
            Description = evaluation.Description,
           
        };

        return PartialView("_ArchiveEvaluationForm",eval);
    }

    // POST: EVALUATIONS/Archive/5
    [HttpPost, ActionName("Archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchivedConfirmed(int id)
    {
        await _evaluationservice.ArchiveEvaluationAsync(id);

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

}
