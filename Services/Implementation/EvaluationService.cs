using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Helpers;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EvaluationService : IEvaluationService
    {
        private readonly AppDbContext _context;
        public EvaluationService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Evaluation>> GetAllEvaluationsAsync()
        {
            return await _context.Evaluations

                    .Include(d => d.EvaluationDetails)
                     .ThenInclude(q => q.EvaluationQuestion)
                      .Where(e => e.IsArchived == false)
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

        //------PROCEED TO THE OTHER METHODS NEEDED -------//
        // SEARCH FOR INCLUDE, .SELECT , ENTITY FRAMEWORKDS //

        public async Task CreateOrReuseQuestionAsync(int evaluationid, string questionText,
            AnswerType answertype)
        {
            //first find/search question along with its answertype using first to get first question && answertype
            var question = await _context.EvaluationQuestions.FirstOrDefaultAsync(e => e.QuestionText == questionText && e.AnswerType == answertype);
            if (question == null)
            {
                //if question doesnt exist yet create a new question 
                // REUSE the same variable after the search and fill it up if question if empty create New
                question = new EvaluationQuestion
                {
                    QuestionText = questionText,
                    AnswerType = answertype,
                };
                _context.EvaluationQuestions.Add(question);
                await _context.SaveChangesAsync();
            }
            //Avoid duplicate remove BRIDGE if it is already linked like evaluation == questionId already 
            var AlreadyExist = await _context.EvaluationDetail.AnyAsync(e => e.EvaluationId == evaluationid && e.QuestionId == question.QuestionId);
            if (AlreadyExist) return; //IF true STOP !!

            // no search was invented here so create a New variable
            var evaluationDetail = new EvaluationDetails
            {
                QuestionId = question.QuestionId,
                EvaluationId = evaluationid,

            };
            _context.EvaluationDetail.Add(evaluationDetail); //this continue to add bridges 
            await _context.SaveChangesAsync();
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


        public async Task CreateEvaluationAsync(CreateEvaluationViewModel vm)
        {
            //follow this method and POLISH everything ------STUCK HERE RESEARCH 7/6/2026
            var evaluation = new Evaluation
            {
                EvaluationTitle = vm.EvaluationTitle,
                Description = vm.Description,
                OpenAt = vm.OpenAt,
                ClosedAt = vm.ClosedAt,
                DateCreated = DateTime.Now,
                IsArchived = false,
                ArchivedDate = null,
            };
            //add evaluation
            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();
            //add question to evaluationdetails
            foreach (var question in vm.RatingQuestions)
            {
                await CreateOrReuseQuestionAsync(
                    evaluation.EvaluationId,
                    question.QuestionText,
                    AnswerType.RatingScale
                    );
            }


        }

        public async Task<UpdateEvaluationViewModel?> GetUpdateEvaluationAsync(int id)
        {
            var evaluation = await _context.Evaluations.
                Include(ed => ed.EvaluationDetails)
                .ThenInclude(q => q.EvaluationQuestion).
                FirstOrDefaultAsync(e => e.EvaluationId == id);

            if (evaluation == null) return null;
            return new UpdateEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,
                Description = evaluation.Description,
                ClosedAt = evaluation.ClosedAt,
                OpenAt = evaluation.OpenAt,

                // FIX: filter each list by AnswerType instead of selecting all details into both
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




        public async Task UpdateEvaluationAsync(UpdateEvaluationViewModel vm)
        {
            var Exist = await _context.Evaluations.FindAsync(vm.EvaluationId);
            if (Exist == null) return;

            Exist.EvaluationTitle = vm.EvaluationTitle;
            Exist.Description = vm.Description;
            Exist.OpenAt = vm.OpenAt;
            Exist.ClosedAt = vm.ClosedAt;
            Exist.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();


            //Remove the Bridge which is EvaluationDetail where evaluationid == evaluationId(EditedbyUser) then convert to LIST();
            var ExistingQuestions = _context.EvaluationDetail.Where(e => e.EvaluationId == vm.EvaluationId).ToList();
            _context.EvaluationDetail.RemoveRange(ExistingQuestions);

            await _context.SaveChangesAsync();

            foreach (var question in vm.RatingScale)
            {
                await CreateOrReuseQuestionAsync(
                    vm.EvaluationId,
                    question.QuestionText,
                    question.AnswerType
                   );
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteEvaluationAsync(int id)
        {
            var getEvaluation = await _context.Evaluations.FindAsync(id);
            if (getEvaluation == null) return;
            _context.Evaluations.Remove(getEvaluation);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EvaluationExist(int id)
        {
            return await _context.Evaluations.AnyAsync(e => e.EvaluationId == id);
        }

        public async Task<ArchiveEvaluationViewModel?> GetArchiveEvaluationConfirmation(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null) return null;

            return new ArchiveEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,
                Description = evaluation.Description,
            };
        }
        public async Task ArchiveEvaluation(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null) return;
            evaluation.IsArchived = true;
            evaluation.ArchivedDate = DateTime.Now;
            await _context.SaveChangesAsync();
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
    }
}