using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    public class EvaluationController : Controller
    {
        private readonly IEvaluationService _evaluationService;
        public EvaluationController(IEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var memberId = HttpContext.Session.GetString("CurrentMemberId");

            if (string.IsNullOrEmpty(memberId))
            {
                TempData["Error"] = "Please verify your identity first.";
                return RedirectToAction("VerifyMember");
            }

            var evaluations = await _evaluationService.GetAvailableEvaluationsAsync();
            return View(evaluations); 
        }

        // GET: shows the "enter your name + email" form
        [HttpGet]
        public IActionResult VerifyMember()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyMember(string firstName, string email)
        {
            var member = await _evaluationService.FindMemberForEvaluationAsync(firstName, email);

            if (member == null)
            {
                ModelState.AddModelError("", "No matching member found.");
                return View();
            }

            HttpContext.Session.SetInt32("CurrentMemberId", member.MemberId);
            HttpContext.Session.SetString("CurrentMemberName", $"{member.FirstName} {member.LastName}");
            HttpContext.Session.SetString("CurrentMemberEmail", member.User.Email ?? "");

            return RedirectToAction("Index"); // 
        }
        // GET: 
        [HttpGet]
        public async Task<IActionResult> TakeEvaluation(int evaluationId)
        {
            var memberId = HttpContext.Session.GetString("CurrentMemberId");

            if (string.IsNullOrEmpty(memberId))
            {
                TempData["Error"] = "Please verify your identity first.";
                return RedirectToAction("VerifyMember");
            }

            var evaluation = await _evaluationService.GetEvaluationByIdAsync(evaluationId);
            if (evaluation == null)
                return NotFound();

            var viewModel = new TakeEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,
                MemberFullName = HttpContext.Session.GetString("CurrentMemberName") ?? "",
                MemberEmail = HttpContext.Session.GetString("CurrentMemberEmail") ?? "",
                Questions = evaluation.EvaluationDetails.Select(ed => new AnswerInputViewModel
                {
                    EvaluationDetailId = ed.EvaluationDetailId,
                    QuestionText = ed.EvaluationQuestion!.QuestionText,
                    AnswerType = ed.EvaluationQuestion!.AnswerType
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> TakeEvaluation(SubmitEvaluationViewModel model)
        {
            var memberId = HttpContext.Session.GetString("CurrentMemberId");

            if (string.IsNullOrEmpty(memberId))
            {
                TempData["Error"] = "Please verify your identity first.";
                return RedirectToAction("VerifyMember");
            }

            var success = await _evaluationService.SaveEvaluationAsync(memberId, model);

            if (!success)
            {
                TempData["Error"] = "This evaluation has already been submitted.";
                return RedirectToAction("TakeEvaluation");
            }

            if (model.IsFinalSubmit)
                return RedirectToAction("ThankYou");

            TempData["Message"] = "Your progress has been saved.";
            return RedirectToAction("TakeEvaluation");
        }

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }
    }


}

