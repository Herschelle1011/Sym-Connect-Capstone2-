using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize]
    public class EvaluationController : Controller
    {
        private readonly IEvaluationService _evaluationService;
        private readonly UserManager<User> _userManager;

        public EvaluationController(
            IEvaluationService evaluationService,
            UserManager<User> userManager)
        {
            _evaluationService = evaluationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }

            if (user.MemberId == null)
            {
                return Forbid();
            }

            var evaluations =
                await _evaluationService
                    .GetAvailableEvaluationsAsync(
                        user.MemberId.Value,
                        user.Id);

            return View(evaluations);
        }


        [HttpGet]
        public async Task<IActionResult> TakeEvaluation(int evaluationId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" });
            }

            if (user.MemberId == null)
            {
                return Forbid();
            }

            var canTake =
                await _evaluationService.CanTakeEvaluationAsync(
                    evaluationId,
                    user.MemberId.Value,
                    user.Id);

            if (!canTake)
            {
                TempData["Error"] =
                    "This evaluation is not available to you.";

                return RedirectToAction("Index");
            }

            var evaluation =
                await _evaluationService
                    .GetEvaluationByIdAsync(evaluationId);

            if (evaluation == null)
            {
                return NotFound();
            }

            var viewModel = new TakeEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,

                MemberFullName = user.DisplayName ?? "",
                MemberEmail = user.Email ?? "",

                Questions = evaluation.EvaluationDetails
                    .Select(ed => new AnswerInputViewModel
                    {
                        EvaluationDetailId =
                            ed.EvaluationDetailId,

                        QuestionText =
                            ed.EvaluationQuestion!.QuestionText,

                        AnswerType =
                            ed.EvaluationQuestion.AnswerType
                    })
                    .ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeEvaluation(
            SubmitEvaluationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (user.MemberId == null)
                return Forbid();

            var success = await _evaluationService.SaveEvaluationAsync(
                user.Id,
                model
            );

            if (!success)
            {
                TempData["Error"] =
                    "This evaluation has already been submitted.";

                return RedirectToAction(
                    "TakeEvaluation",
                    new { evaluationId = model.EvaluationId }
                );
            }

            if (model.IsFinalSubmit)
                return RedirectToAction("ThankYou");

            TempData["Message"] =
                "Your progress has been saved.";

            return RedirectToAction(
                "TakeEvaluation",
                new { evaluationId = model.EvaluationId }
            );
        }


        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}