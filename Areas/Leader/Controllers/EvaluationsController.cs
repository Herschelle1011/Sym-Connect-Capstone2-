using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class EvaluationsController : Controller
    {
        private readonly IEvaluationService _evaluationService;
        private readonly IEventService _eventService;
        private readonly UserManager<User> _userManager;
        private readonly IGroupService _groupService;

        public EvaluationsController(IEvaluationService evaluationService, IEventService eventService, UserManager<User> userManager, IGroupService groupService)
        {
            _evaluationService = evaluationService;
            _eventService = eventService;
            _userManager = userManager;
            _groupService = groupService;
        }

        //GET
        public async Task<IActionResult> Index()
        {
         
            var user = await _userManager.GetUserAsync(User);

            var roles = await _userManager.GetRolesAsync(user);


            bool isLeader =
         await _groupService.IsMemberALeaderAsync(
           user.MemberId.Value
       );

            if (!isLeader)
            {
                return Forbid();
            }



            Console.WriteLine("==============================");
            Console.WriteLine($"USERNAME: {user.UserName}");
            Console.WriteLine($"MEMBER ID: {user.MemberId}");
            Console.WriteLine($"ROLES: {string.Join(", ", roles)}");
            Console.WriteLine("==============================");

            if (user.MemberId == null)
            {
                return Forbid();
            }

          

            Console.WriteLine($"IS LEADER: {isLeader}");

  

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
                    EventTitle = e.EventTitle
                })
                .ToList();

            return View(vm);
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

            var result = await _evaluationService.CreateEvaluationAsync(vm,user.MemberId.Value,false);

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
    }
}
