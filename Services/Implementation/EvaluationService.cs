using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Helpers;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EvaluationService : IEvaluationService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public EvaluationService(AppDbContext context, INotificationService notificationService, UserManager<User> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<List<MemberEvaluationListItemViewModel>>GetAvailableEvaluationsAsync(int currentMemberId,bool isAdmin)
        {
            var query = _context.Evaluations
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived &&
                    e.OpenAt <= DateTime.Now &&
                    e.ClosedAt >= DateTime.Now);
            // Leader only sees evaluations for events they created
            if (!isAdmin)
            {
                query = query.Where(e =>
                    e.Event.CreatedByUserId == currentMemberId);
            }

            return await query
                .Select(e => new MemberEvaluationListItemViewModel
                {
                    EvaluationId = e.EvaluationId,
                    EvaluationTitle = e.EvaluationTitle,
                    Description = e.Description,
                    DateCreated = e.DateCreated,
                    OpenAt = e.OpenAt,
                    ClosedAt = e.ClosedAt,
                    EventTitle = e.Event.EventTitle,
                    Points = e.Points
                })
                .ToListAsync();
        }


        public async Task<List<EvaluationQuestion>> GetAllQuestionsAsync()
        {
            var questions = await _context.EvaluationQuestions.ToListAsync();
            return questions.GroupBy(q => q.QuestionText.Trim()
                             .ToLower()).Select(g => g.First())
                              .ToList();

        }

        public async Task<Evaluation?> GetEvaluationByIdAsync(int id)
        {
            return await _context.Evaluations
                .Include(d => d.EvaluationDetails)
                    .ThenInclude(q => q.EvaluationQuestion)
                        .FirstOrDefaultAsync(e => e.EvaluationId == id && e.IsArchived == false);
        }



        public async Task<DetailsEvaluationViewModel?> GetEvaluationByIdAsync(
    int evaluationId,
    int currentMemberId,
    bool isAdmin)
        {
            var query = _context.Evaluations
                .AsNoTracking()
                .Where(e =>
                    e.EvaluationId == evaluationId &&
                    !e.IsArchived);

            // If this is a Leader, only allow evaluations
            // for events created by that Leader.
            if (!isAdmin)
            {
                query = query.Where(e =>
                    e.Event.CreatedByUserId == currentMemberId);
            }

            return await query
                .Select(e => new DetailsEvaluationViewModel
                {
                    EvaluationId = e.EvaluationId,

                    EvaluationTitle = e.EvaluationTitle,

                    Description = e.Description,

                    OpenAt = e.OpenAt,

                    ClosesAt = e.ClosedAt,

                    DateCreated = e.DateCreated,

                    EventTitle = e.Event.EventTitle,
                    Points = e.Points,

                    Questions = e.EvaluationDetails
                        .Select(d => new EvaluationQuestionViewModel
                        {
                            EvaluationDetailId =
                                d.EvaluationDetailId,

                            QuestionId =
                                d.QuestionId,

                            QuestionText =
                                d.EvaluationQuestion.QuestionText,

                            AnswerType =
                                d.EvaluationQuestion.AnswerType
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }








        //------PROCEED TO THE OTHER METHODS NEEDED -------//
        // SEARCH FOR INCLUDE, .SELECT , ENTITY FRAMEWORKDS //

        public async Task CreateOrReuseQuestionAsync(int evaluationId, string questionText, AnswerType answerType)
        {
            // Search if the exact question already exists.
            var question = await _context.EvaluationQuestions
                .FirstOrDefaultAsync(q =>
                    q.QuestionText == questionText &&
                    q.AnswerType == answerType);

            // If it does not exist, create it.
            if (question == null)
            {
                question = new EvaluationQuestion
                {
                    QuestionText = questionText,
                    AnswerType = answerType
                };

                _context.EvaluationQuestions.Add(question);
                await _context.SaveChangesAsync();
            }

            // Check if this question is already connected
            // to this evaluation.
            var alreadyExists = await _context.EvaluationDetail
                .AnyAsync(d =>
                    d.EvaluationId == evaluationId &&
                    d.QuestionId == question.QuestionId);

            if (alreadyExists)
            {
                return;
            }

            // Create the bridge between Evaluation and Question.
            var evaluationDetail = new EvaluationDetails
            {
                EvaluationId = evaluationId,
                QuestionId = question.QuestionId
            };

            _context.EvaluationDetail.Add(evaluationDetail);

            await _context.SaveChangesAsync();
        }


        //GET MOST REUSED QUESTIOSN 
        public async Task<List<ReusableQuestionViewModel>> GetFrequentlyUsedQuestionsAsync()
        {
            var questions = await _context.EvaluationQuestions
                .Select(q => new ReusableQuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    AnswerType = q.AnswerType,

                    UsageCount = _context.EvaluationDetail
                        .Count(d => d.QuestionId == q.QuestionId)
                })
                .Where(q => q.UsageCount >= 5)
                .OrderByDescending(q => q.UsageCount)
                .Take(10)
                .ToListAsync();

            return questions;
        }

        public async Task<EditQuestionViewModel> GetQuestionAsync(int id)
        {
            var evaluationDetail = await _context.EvaluationDetail
                                    .Include(e => e.EvaluationQuestion)
                                    .FirstOrDefaultAsync(e => e.EvaluationDetailId == id);
            if (evaluationDetail == null) return null;
            return new EditQuestionViewModel
            {
                EvaluationDetailId = evaluationDetail.EvaluationDetailId,
                QuestionText = evaluationDetail.EvaluationQuestion.QuestionText,
                answerType = evaluationDetail.EvaluationQuestion.AnswerType
            };

        }

        public async Task EditQuestionAsync(EditQuestionViewModel vm)
        {
            // -- June 9, 2026 current PROGRESS

            //Check if the EvaluationDetail exist
            var evaluationDetail = await _context.EvaluationDetail
                                .FirstOrDefaultAsync(d => d.EvaluationDetailId == vm.EvaluationDetailId);

            if (evaluationDetail == null) return;

            //Check if the EvaluationQuestions exist < 
            var evaluationQuestions = await _context.EvaluationQuestions
                                .FirstOrDefaultAsync(q => q.QuestionText == vm.QuestionText
                                                        && q.AnswerType == vm.answerType);
            if (evaluationQuestions == null) //if QUESTION is empty < 
            {
                // ADD new Question < 
                evaluationQuestions = new EvaluationQuestion
                {
                    QuestionText = vm.QuestionText,
                    AnswerType = vm.answerType,
                };
                _context.EvaluationQuestions.Add(evaluationQuestions);
                await _context.SaveChangesAsync();
            }
            //if question already exist USE QUESTION ID < 
            // by changing its bridge As known as : Model > Evaluationdetails.QuestionId according to the QuestionId in table 
            evaluationDetail.QuestionId = evaluationQuestions.QuestionId;
            await _context.SaveChangesAsync();
        }


        public async Task<OperationalResult> CreateEvaluationAsync(
    CreateEvaluationViewModel vm, int currentMemberId,
    bool isAdmin)
        {
            // Make sure an event was selected
            if (!vm.EventId.HasValue)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Please select an event."
                };
            }

     


            // Make sure the selected event really exists
            var selectedEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.EventId == vm.EventId.Value && e.ApprovalStatus != ApprovalStatus.Pending && e.ApprovalStatus != ApprovalStatus.Rejected &&
                    !e.IsArchived);

            if (selectedEvent == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "The selected event was not found."
                };
            }

            var alreadyHasEvaluation =
           await _context.Evaluations
         .AnyAsync(e =>
             e.EventId == selectedEvent.EventId &&
             !e.IsArchived);

            if (alreadyHasEvaluation)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "This event already has an active evaluation."
                };
            }

            if (!isAdmin &&
               selectedEvent.CreatedByUserId != currentMemberId)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "You can only create an evaluation for an event you created."
                };
            }


            // Create the evaluation
            var evaluation = new Evaluation
            {
                EvaluationTitle = vm.EvaluationTitle,
                Description = vm.Description,

                // Connect evaluation to event
                EventId = selectedEvent.EventId,
                Points = 2,
                OpenAt = vm.OpenAt,
                ClosedAt = vm.ClosedAt,
                DateCreated = DateTime.Now,
                IsArchived = false,
                ArchivedDate = null,
            };

            _context.Evaluations.Add(evaluation);

            // Save first so EvaluationId is generated
            await _context.SaveChangesAsync();


            // Add questions
            foreach (var question in vm.RatingQuestions)
            {
                await CreateOrReuseQuestionAsync(
                    evaluation.EvaluationId,
                    question.QuestionText,
                    AnswerType.RatingScale
                );
            }


            // ================= NOTIFICATION =================

            var expectedGroupIds = await _context.Events
        .Where(e => e.EventId == selectedEvent.EventId)
        .SelectMany(e => e.ExpectedGroups)
        .Select(eg => eg.GroupId)
        .ToListAsync();


            // Get members who:
            // 1. belong to one of the expected groups
            // 2. actually attended this event
            var attendedMemberIds = await _context.Attendances
                .Where(a =>
                    a.EventId == selectedEvent.EventId &&
                    (a.Status == "Present" || a.Status == "Late") &&
                    a.Member.GroupDetails.Any(gd =>
                        expectedGroupIds.Contains(gd.GroupId)))
                .Select(a => a.MemberId)
                .Distinct()
                .ToListAsync();

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            var adminUserIds = adminUsers
                .Select(u => u.Id)
                .ToList();


            // Convert MemberId into ASP.NET Identity UserId
            var userIds = await _context.Users
                .AsNoTracking()
                .Where(u =>
                    u.MemberId.HasValue &&
                    attendedMemberIds.Contains(u.MemberId.Value) && !adminUserIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();



            // Send notification only to those users
            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    continue;
                }

                bool isLeader =
                    await _userManager.IsInRoleAsync(user, "Leader");


                string notificationUrl;

                if (isLeader)
                {
                    notificationUrl =
                        $"/Leader/Evaluations/TakeEvaluation?evaluationId={evaluation.EvaluationId}";
                }
                else
                {
                    notificationUrl =
                        $"/Member/Evaluation/TakeEvaluation?evaluationId={evaluation.EvaluationId}";
                }


                await _notificationService.NotifyUserAsync(
                    userId,
                    "Event Evaluation Required",
                    $"You attended \"{selectedEvent.EventTitle}\". Please complete the required evaluation.",
                    notificationUrl
                );
            }


            return new OperationalResult
            {
                Success = true,
                Message = "Evaluation Created Successfully"
            };
        }



        public async Task<UpdateEvaluationViewModel?> GetUpdateEvaluationAsync(
                int id,
                int currentMemberId,
                bool isAdmin)
        {

            var query = _context.Evaluations
           .AsNoTracking()
           .Include(e => e.Event)
           .Include(e => e.EvaluationDetails)
               .ThenInclude(d => d.EvaluationQuestion)
           .Where(e =>
               e.EvaluationId == id &&
               !e.IsArchived);

            if (!isAdmin)
            {
                query = query.Where(e =>
                    e.Event.CreatedByUserId == currentMemberId);
            }
            var evaluation = await query.FirstOrDefaultAsync();

            if (evaluation == null)
            {
                return null;
            }

            return new UpdateEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,
                Description = evaluation.Description,
                ClosedAt = evaluation.ClosedAt,
                OpenAt = evaluation.OpenAt,

                RatingScale = evaluation.EvaluationDetails
                    .Where(e => e.EvaluationQuestion!.AnswerType == AnswerType.RatingScale)
                    .Select(e => new QuestionRowViewModel
                    {
                        QuestionId = e.QuestionId,
                        QuestionText = e.EvaluationQuestion!.QuestionText,
                        AnswerType = e.EvaluationQuestion.AnswerType,
                    }).ToList(),


            };
        }




        public async Task<OperationalResult> UpdateEvaluationAsync(UpdateEvaluationViewModel vm,int currentMemberId,
                                                            bool isAdmin)
        {
            // Find evaluation
            var existingEvaluation = await _context.Evaluations
                .FindAsync(vm.EvaluationId);

            if (existingEvaluation == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Evaluation not found."
                };
            }


            // Make sure an event was selected
            if (!vm.EventId.HasValue)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Please select an event."
                };
            }

            var selectedEvent = await _context.Events
        .AsNoTracking()
        .FirstOrDefaultAsync(e =>
            e.EventId == vm.EventId.Value &&
            !e.IsArchived &&
            e.ApprovalStatus == ApprovalStatus.Approved);

            if (selectedEvent == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "The selected event was not found."
                };
            }


            if (!isAdmin &&
              selectedEvent.CreatedByUserId != currentMemberId)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "You can only update an evaluation for an event you created."
                };
            }

            // Update evaluation
            existingEvaluation.EvaluationTitle = vm.EvaluationTitle;
            existingEvaluation.Description = vm.Description;

            // Update its event
            existingEvaluation.EventId = selectedEvent.EventId;

            existingEvaluation.OpenAt = vm.OpenAt;
            existingEvaluation.ClosedAt = vm.ClosedAt;
            existingEvaluation.UpdatedAt = DateTime.Now;


            // Remove existing EvaluationDetail bridges
            var existingQuestions = await _context.EvaluationDetail
                .Where(e => e.EvaluationId == vm.EvaluationId)
                .ToListAsync();

            _context.EvaluationDetail.RemoveRange(existingQuestions);

            await _context.SaveChangesAsync();


            // Add the updated questions
            foreach (var question in vm.RatingScale)
            {
                await CreateOrReuseQuestionAsync(
                    vm.EvaluationId,
                    question.QuestionText,
                    question.AnswerType
                );
            }


            // ================= NOTIFICATION =================

            var userIds = await _context.Users
                .AsNoTracking()
                .Where(u => u.MemberId != null)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                await _notificationService.NotifyUserAsync(
                    userId,
                    "Evaluation Updated",
                    $"The evaluation for \"{selectedEvent.EventTitle}\" has been updated.",
                    $"/Member/Evaluation/TakeEvaluation?evaluationId={existingEvaluation.EvaluationId}"
                );
            }


            return new OperationalResult
            {
                Success = true,
                Message = "Evaluation updated successfully."
            };
        }

        public async Task<OperationalResult> DeleteEvaluationAsync(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null)
            {
                return new OperationalResult { Success = false, Message = "Evaluation not found." };
            }

            _context.Evaluations.Remove(evaluation);
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"\"{evaluation.EvaluationTitle}\" deleted successfully." };
        }
        public async Task<bool> EvaluationExist(int id)
        {
            return await _context.Evaluations.AnyAsync(e => e.EvaluationId == id);
        }

        public async Task<ArchiveEvaluationViewModel?> GetArchiveEvaluationConfirmation(int id,int currentMemberId,bool isAdmin)
        {
            var query = _context.Evaluations
                .AsNoTracking()
                .Include(e => e.Event)
                .Where(e =>
                    e.EvaluationId == id &&
                    !e.IsArchived);

            if (!isAdmin)
            {
                query = query.Where(e =>
                    e.Event.CreatedByUserId == currentMemberId);
            }

            var evaluation =
                await query.FirstOrDefaultAsync();

            if (evaluation == null)
                return null;

            return new ArchiveEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,
                Description = evaluation.Description
            };
        }

        public async Task<OperationalResult> ArchiveEvaluationAsync(int id,int currentMemberId,bool isAdmin)
        {
            var evaluation = await _context.Evaluations
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.EvaluationId == id &&
                    !e.IsArchived);

            if (evaluation == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Evaluation not found."
                };
            }

            if (!isAdmin &&
                evaluation.Event.CreatedByUserId != currentMemberId)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        "You can only archive evaluations for events you created."
                };
            }

            evaluation.IsArchived = true;
            evaluation.ArchivedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return new OperationalResult
            {
                Success = true,
                Message = "Evaluation archived successfully."
            };
        }

        public async Task<SummaryResponseReportViewModel> summaryReport(int? evaluationId)
        {
            var evaluation = await _context.Evaluations
        .Include(e => e.EvaluationDetails)
            .ThenInclude(d => d.EvaluationQuestion)
        .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId);

            if (evaluation == null)
                throw new Exception("Evaluation not found.");

            var submissions = await _context.EvaluationSubmissions
                .Where(s => s.EvaluationId == evaluationId)
                .ToListAsync();

            int totalResponses = submissions.Count;

            int completedResponses = submissions.Count(s =>
                s.Status == SubmissionStatus.Submitted);

            double completionRate = totalResponses == 0
                ? 0
                : (double)completedResponses / totalResponses * 100;

            var responseList = await _context.EvaluationResponses
                .Where(r => r.EvaluationSubmission.EvaluationId == evaluationId)
                .Include(r => r.EvaluationDetail)
                    .ThenInclude(d => d.EvaluationQuestion)
                .ToListAsync();

            var numericRatings = responseList
                .Where(r => int.TryParse(r.Answer, out _))
                .Select(r => int.Parse(r.Answer!))
                .ToList();

            double overallAverage = numericRatings.Any()
                ? Math.Round(numericRatings.Average(), 1)
                : 0;

            var questionSummaries = new List<QuestionSummaryViewModel>();

            foreach (var detail in evaluation.EvaluationDetails)
            {
                var responses = responseList
                    .Where(r => r.EvaluationDetailId == detail.EvaluationDetailId)
                    .ToList();

                int veryExcellent = responses.Count(r => r.Answer == "5");
                int excellent = responses.Count(r => r.Answer == "4");
                int neutral = responses.Count(r => r.Answer == "3");
                int poor = responses.Count(r => r.Answer == "2");
                int veryPoor = responses.Count(r => r.Answer == "1");

                var ratings = responses
                    .Where(r => int.TryParse(r.Answer, out _))
                    .Select(r => int.Parse(r.Answer!))
                    .ToList();

                questionSummaries.Add(new QuestionSummaryViewModel
                {
                    EvaluationDetailId = detail.EvaluationDetailId,
                    QuestionId = detail.QuestionId,
                    QuestionText = detail.EvaluationQuestion?.QuestionText ?? "",

                    AverageRating = ratings.Any()
                        ? Math.Round(ratings.Average(), 1)
                        : 0,

                    VeryExcellent = veryExcellent,
                    Excellent = excellent,
                    Neutral = neutral,
                    Poor = poor,
                    VeryPoor = veryPoor
                });
            }

            return new SummaryResponseReportViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,
                Description = evaluation.Description,

                TotalResponses = totalResponses,
                CompletionRate = Math.Round(completionRate, 0),
                AverageRating = overallAverage,

                Status = evaluation.ClosedAt <= DateTime.Now
                    ? "Closed"
                    : "Open",

                Questions = questionSummaries
            };
        }

        public async Task<List<SubmissionListItemViewModel>> GetSubmissionsForEvaluationAsync(int evaluationId)
        {
            return await _context.EvaluationSubmissions
       .Include(s => s.User)
       .Where(s => s.EvaluationId == evaluationId)
       .Select(s => new SubmissionListItemViewModel
       {
           SubmissionId = s.SubmissionId,
           Email = s.User.Email ?? "N/A",
           SubmittedAt = s.SubmittedAt,
           Status = s.Status
       })
       .ToListAsync();
        }
        public async Task<SubmissionDetailsViewModel?> GetSubmissionDetailsAsync(int submissionId)
        {
            var submission = await _context.EvaluationSubmissions
        .Include(s => s.User)
        .Include(s => s.EvaluationResponse)
            .ThenInclude(r => r.EvaluationDetail)
                .ThenInclude(d => d.EvaluationQuestion)
        .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null) return null;

            return new SubmissionDetailsViewModel
            {
                SubmittedAt = submission.SubmittedAt,
                Answers = submission.EvaluationResponse.Select(r => new AnswerReviewItem
                {
                    QuestionText = r.EvaluationDetail!.EvaluationQuestion!.QuestionText,
                    Answer = ConvertRating.Convert(r.Answer)
                }).ToList()
            };
        }


        public async Task<string?> GetCurrentStatusAsync(int evaluationId)
        {
            var evaluation = await _context.Evaluations
                .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId);

            if (evaluation == null) return null;

            return EvaluationStatusHelper.GetStatus(evaluation.OpenAt, evaluation.ClosedAt);
        }


        public async Task<double?> GetEventFinalRatingAsync(int eventId)
        {
            var ratings = await _context.EvaluationResponses
                .Where(r =>
                    r.EvaluationSubmission.Evaluation.EventId == eventId &&

                    // Ignore archived evaluations
                    !r.EvaluationSubmission.Evaluation.IsArchived &&

                    // Only completed submissions
                    r.EvaluationSubmission.Status ==
                        SubmissionStatus.Submitted &&

                    // Only rating questions
                    r.EvaluationDetail.EvaluationQuestion.AnswerType ==
                        AnswerType.RatingScale)
                .Select(r => r.Answer)
                .ToListAsync();

            var numericRatings = ratings
                .Where(r => int.TryParse(r, out _))
                .Select(r => int.Parse(r!))
                .ToList();

            if (!numericRatings.Any())
            {
                return null;
            }

            return Math.Round(
                numericRatings.Average(),
                1);
        }
        public async Task<int> GetEventResponseCountAsync(int eventId)
        {
            return await _context.EvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s =>
                    s.Evaluation.EventId == eventId &&
                    !s.Evaluation.IsArchived &&
                    s.Status == SubmissionStatus.Submitted);
        }



        public async Task<List<MemberEvaluationListItemViewModel>> GetAssignedEvaluationsAsync(
          int currentMemberId,
          string userId)
        {
            var now = DateTime.Now;

            return await _context.Evaluations
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived &&
                    e.EventId != null &&

                    // Leader attended the event
                    _context.Attendances.Any(a =>
                        a.EventId == e.EventId &&
                        a.MemberId == currentMemberId &&
                        (
                            a.Status == "Present" ||
                            a.Status == "Late"
                        )) &&

                    // IMPORTANT:
                    // Hide evaluation after this user submits it
                    !_context.EvaluationSubmissions.Any(s =>
                        s.EvaluationId == e.EvaluationId &&
                        s.UserId == userId &&
                        s.Status == SubmissionStatus.Submitted)
                )
                .OrderByDescending(e => e.DateCreated)
                .Select(e => new MemberEvaluationListItemViewModel
                {
                    EvaluationId = e.EvaluationId,
                    EvaluationTitle = e.EvaluationTitle,
                    Description = e.Description,
                    DateCreated = e.DateCreated,
                    OpenAt = e.OpenAt,
                    ClosedAt = e.ClosedAt,
                    EventTitle = e.Event.EventTitle,

                    Points = e.Points,

                    Status = e.OpenAt > now
                        ? "Upcoming"
                        : e.ClosedAt < now
                            ? "Closed"
                            : "Open"
                })
                .ToListAsync();
        }

    }
}