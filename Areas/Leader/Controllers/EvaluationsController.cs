using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Areas.Leader.Services.Interfaces;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class EvaluationsController : Controller
    {
        private readonly IEvaluationService _evaluationService;
        private readonly ILeaderEvaluationService _leaderEvaluationService;
        private readonly IEventService _eventService;
        private readonly UserManager<User> _userManager;
        private readonly IGroupService _groupService;

        public EvaluationsController(IEvaluationService evaluationService, IEventService eventService, UserManager<User> userManager, IGroupService groupService, ILeaderEvaluationService leaderEvaluationService)
        {
            _evaluationService = evaluationService;
            _eventService = eventService;
            _userManager = userManager;
            _groupService = groupService;
            _leaderEvaluationService = leaderEvaluationService;
        }

        //GET
        public async Task<IActionResult> Index()
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

            bool isLeader =
                await _groupService.IsMemberALeaderAsync(
                    user.MemberId.Value);

            if (!isLeader)
            {
                return Forbid();
            }


            if (user.MemberId == null)
            {
                return Forbid();
            }


            if (user == null)
            {
                return Challenge();
            }


            if (user.MemberId == null)
            {
                return Forbid();
            }

            // NOT admin 
            var evaluations =
                await _evaluationService.GetAvailableEvaluationsAsync(
                    user.MemberId.Value,
                    user.Id,
                    false);

            var vm = evaluations
                .Select(e => new ListEvaluationViewModel
                {
                    EvaluationId = e.EvaluationId,
                    EvaluationTitle = e.EvaluationTitle,
                    Description = e.Description,
                    OpenAt = e.OpenAt,
                    ClosesAt = e.ClosedAt,
                    DateCreated = e.DateCreated,
                    EventTitle = e.EventTitle,
                    Points = e.Points
                })
                .ToList();

            return View(vm);
        }

        public async Task<IActionResult> SummaryReports(int? id)
        {
            if (id == null) { return NotFound(); }
            var report = await _evaluationService.summaryReport(id);
            if (report == null) { return NotFound(); }
            return PartialView("_SummaryReportsPartial", report);
        }



        [HttpGet]
        public async Task<IActionResult> AssignedToMe()
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

            // Make sure this member is actually a leader
            bool isLeader = await _groupService
                .IsMemberALeaderAsync(user.MemberId.Value);

            if (!isLeader)
            {
                return Forbid();
            }

            var evaluations =
          await _leaderEvaluationService
         .GetAssignedEvaluationsAsync(
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
                return Challenge();
            }

            if (user.MemberId == null)
            {
                return Forbid();
            }

            // Make sure the current member is really a leader
            bool isLeader = await _groupService
                .IsMemberALeaderAsync(user.MemberId.Value);

            if (!isLeader)
            {
                return Forbid();
            }

            // Check if this leader is actually allowed
            // to answer this evaluation
            var canTake =
                await _leaderEvaluationService.CanTakeAssignedEvaluationAsync(
                    evaluationId,
                    user.MemberId.Value,
                    user.Id);

            if (!canTake)
            {
               

                return RedirectToAction(nameof(AssignedToMe));
            }

            // Get evaluation + questions
            var evaluation =
                await _leaderEvaluationService
                    .GetEvaluationByIdAsync(evaluationId);

            if (evaluation == null)
            {
                return NotFound();
            }

            var vm = new TakeEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,

                EvaluationTitle =
                    evaluation.EvaluationTitle,

                MemberFullName =
                    user.DisplayName ?? "",

                MemberEmail =
                    user.Email ?? "",
                openAt = evaluation.OpenAt,
                closesAt = evaluation.ClosedAt,

                Points = evaluation.Points,

                Questions = evaluation.EvaluationDetails
                    .Select(ed => new AnswerInputViewModel
                    {
                        EvaluationDetailId =
                            ed.EvaluationDetailId,

                        QuestionText =
                            ed.EvaluationQuestion!
                                .QuestionText,

                        AnswerType =
                            ed.EvaluationQuestion
                                .AnswerType
                    })
                    .ToList()
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeEvaluation(
    SubmitEvaluationViewModel model)
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

            // Make sure user is still a leader
            bool isLeader = await _groupService
                .IsMemberALeaderAsync(user.MemberId.Value);

            if (!isLeader)
            {
                return Forbid();
            }

            // IMPORTANT:
            // Check permission again during POST.
            var canTake =
                await _leaderEvaluationService.CanTakeAssignedEvaluationAsync(
                    model.EvaluationId,
                    user.MemberId.Value,
                    user.Id);

            if (!canTake)
            {
                TempData["ErrorMessage"] =
                    "This evaluation is no longer available.";

                return RedirectToAction(nameof(AssignedToMe));
            }

            var success =
                await _leaderEvaluationService.SaveEvaluationAsync(
                    user.Id,
                    model);

            if (!success)
            {
                TempData["ErrorMessage"] =
                    "The evaluation could not be saved.";

                return RedirectToAction(
                    nameof(TakeEvaluation),
                    new
                    {
                        evaluationId =
                            model.EvaluationId
                    });
            }

            // User clicked Submit Evaluation
            if (model.IsFinalSubmit)
            {
                TempData["SuccessMessage"] =
                    "Your evaluation has been submitted.";

                return RedirectToAction(nameof(ThankYou));
            }

            // User only saved their progress
            TempData["SuccessMessage"] =
                "Your progress has been saved.";

            return RedirectToAction(
                nameof(TakeEvaluation),
                new
                {
                    evaluationId =
                        model.EvaluationId
                });
        }

        [HttpGet]
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

            var vm = await _evaluationService.GetEvaluationByIdAsync(
                id.Value,
                user.MemberId.Value,
                false
            );

            vm.Submissions =
         await _evaluationService.GetSubmissionsForEvaluationAsync(id.Value);

            if (vm == null)
            {
                return NotFound();
            }

            return View(vm);
        }


        //GET
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

            var questions =
                await _evaluationService
                    .GetFrequentlyUsedQuestionsAsync();

            // false = Leader
            var completedEvents =
                await _eventService
                    .GetAllEventForEvaluationAsync(
                        user.MemberId.Value,
                        false);

            var vm = new CreateEvaluationViewModel
            {
                OpenAt = DateTime.Today,
                ClosedAt = DateTime.Today.AddDays(1),

                EventOptions = completedEvents
                    .Select(e =>
                        new EvaluationEventOptionViewModel
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
                    .ToList(),

                AvailableQuestions = questions
                    .Select(q =>
                        new AvailableQuestionViewModel
                        {
                            QuestionId = q.QuestionId,
                            QuestionText = q.QuestionText,
                            AnswerType = q.AnswerType
                        })
                    .ToList()
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEvaluationViewModel vm)
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (user.MemberId == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var questions =
                    await _evaluationService
                        .GetFrequentlyUsedQuestionsAsync();

                vm.AvailableQuestions = questions
                    .Select(q =>
                        new AvailableQuestionViewModel
                        {
                            QuestionId = q.QuestionId,
                            QuestionText = q.QuestionText,
                            AnswerType = q.AnswerType
                        })
                    .ToList();

                return View(vm);
            }

            var result = await _evaluationService.CreateEvaluationAsync(vm,user.MemberId.Value,false, user.Id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] =
                    result.Message;

                return View(vm);
            }

            TempData["SuccessMessage"] =
                result.Message;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
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

            var vm = await _evaluationService.GetUpdateEvaluationAsync(
                id.Value,
                user.MemberId.Value,
                false
            );

            if (vm == null)
            {
                return NotFound();
            }

            var questions =
                await _evaluationService.GetFrequentlyUsedQuestionsAsync();

            vm.AvailableQuestions = questions
                .Select(q => new AvailableQuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    AnswerType = q.AnswerType
                })
                .ToList();

            return View(vm);
        }

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

            if (!ModelState.IsValid)
            {
                var questions =
                    await _evaluationService.GetFrequentlyUsedQuestionsAsync();

                vm.AvailableQuestions = questions
                    .Select(q => new AvailableQuestionViewModel
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText,
                        AnswerType = q.AnswerType
                    })
                    .ToList();

                return View(vm);
            }

            var result =
                await _evaluationService.UpdateEvaluationAsync(
                    vm,
                    user.MemberId.Value,
                    false
                );

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;

                var questions =
                    await _evaluationService.GetFrequentlyUsedQuestionsAsync();

                vm.AvailableQuestions = questions
                    .Select(q => new AvailableQuestionViewModel
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText,
                        AnswerType = q.AnswerType
                    })
                    .ToList();

                return View(vm);
            }

            TempData["SuccessMessage"] = result.Message;

            return RedirectToAction(nameof(Index));
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

            var completedEvents =
                await _eventService.GetAllEventForEvaluationAsync(
                    user.MemberId.Value,
                    false);

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

        [HttpGet]
        public async Task<IActionResult> Archive(int? id)
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

            var evaluation =
                await _evaluationService
                    .GetArchiveEvaluationConfirmation(
                        id.Value,
                        user.MemberId.Value,
                        false);

            if (evaluation == null)
            {
                return NotFound();
            }

            return PartialView(
                "_ArchiveEvaluationForm",
                evaluation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
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

            var result =
                await _evaluationService.ArchiveEvaluationAsync(
                    id,
                    user.MemberId.Value,
                    false);

            if (!result.Success)
            {
                TempData["ErrorMessage"] =
                    result.Message;
            }
            else
            {
                TempData["SuccessMessage"] =
                    result.Message;
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetCloseEvaluation(int? id)
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

            var evaluation =
                await _evaluationService.GetCloseEvaluationAsync(
                    id.Value,
                    user.MemberId.Value,
                    false
                );

            if (evaluation == null)
            {
                return NotFound();
            }

            ViewBag.FormArea = "Leader";
            ViewBag.FormController = "Evaluations";

            return PartialView(
                "~/Views/Shared/_CloseEvaluationModal.cshtml",
                evaluation
            );
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseEvaluation(
              int evaluationId)
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

            var result =
                await _evaluationService.CloseEvaluationAsync(
                    evaluationId,
                    user.MemberId.Value,
                    false
                );

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

    }
}
