using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Leader.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Leader.Services.Implementation
{
    public class LeaderEvaluationService : ILeaderEvaluationService
    {
        private readonly AppDbContext _context;

        public LeaderEvaluationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MemberEvaluationListItemViewModel>>
      GetAssignedEvaluationsAsync(
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

                    // Hide evaluations already submitted by this leader
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

        public async Task<bool> CanTakeAssignedEvaluationAsync(
            int evaluationId,
            int memberId,
            string userId)
        {
            var now = DateTime.Now;

            return await _context.Evaluations
                .AnyAsync(e =>
                    e.EvaluationId == evaluationId &&
                    !e.IsArchived &&
                    e.OpenAt <= now &&
                    e.ClosedAt >= now &&

                    _context.Attendances.Any(a =>
                        a.EventId == e.EventId &&
                        a.MemberId == memberId &&
                        (
                            a.Status == "Present" ||
                            a.Status == "Late"
                        )) &&

                    !_context.EvaluationSubmissions.Any(s =>
                        s.EvaluationId == evaluationId &&
                        s.UserId == userId &&
                        s.Status == SubmissionStatus.Submitted)
                );
        }

        public async Task<bool> SaveEvaluationAsync(
    string userId,
    SubmitEvaluationViewModel model)
        {
      

            if (model.Answers == null ||
                !model.Answers.Any())
            {
                return false;
            }




            var evaluation =
                await _context.Evaluations
                    .FirstOrDefaultAsync(e =>
                        e.EvaluationId == model.EvaluationId &&
                        !e.IsArchived);

            if (evaluation == null)
            {
                return false;
            }

            var member = await _context.Members
          .FirstOrDefaultAsync(m =>
              m.User.Id == userId);

            if (member == null)
            {
                return false;
            }



            var submission =
                await _context.EvaluationSubmissions
                    .FirstOrDefaultAsync(s =>
                        s.EvaluationId == model.EvaluationId &&
                        s.UserId == userId);



            if (submission != null &&
                submission.Status ==
                    SubmissionStatus.Submitted)
            {
                return false;
            }



            if (submission == null)
            {
                submission =
                    new EvaluationSubmission
                    {
                        EvaluationId =
                            model.EvaluationId,

                        UserId =
                            userId,

                        Status =
                            SubmissionStatus.Draft,

                        CreatedAt =
                            DateTime.Now,

           
                        PointsEarned = 0
                    };

                _context.EvaluationSubmissions
                    .Add(submission);

                await _context.SaveChangesAsync();
            }


            foreach (var answerInput in model.Answers)
            {
                var existingAnswer =
                    await _context.EvaluationResponses
                        .FirstOrDefaultAsync(r =>
                            r.SubmissionId ==
                                submission.SubmissionId &&

                            r.EvaluationDetailId ==
                                answerInput.EvaluationDetailId);


                if (existingAnswer != null)
                {
                    existingAnswer.Answer =
                        answerInput.Answer;
                }
                else
                {
                    var response =
                        new EvaluationResponse
                        {
                            SubmissionId =
                                submission.SubmissionId,

                            EvaluationDetailId =
                                answerInput.EvaluationDetailId,

                            Answer =
                                answerInput.Answer
                        };

                    _context.EvaluationResponses
                        .Add(response);
                }
            }


            //  FINAL SUBMISSION

            if (model.IsFinalSubmit)
            {
                submission.Status =
                    SubmissionStatus.Submitted;

                submission.SubmittedAt =
                    DateTime.Now;

                member.EarnedPoints +=
                    evaluation.Points;
            }


            // SAVE EVERYTHING


            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<Evaluation?> GetEvaluationByIdAsync(
            int evaluationId)
        {
            return await _context.Evaluations
                .Include(e => e.EvaluationDetails)
                    .ThenInclude(ed => ed.EvaluationQuestion)
                .FirstOrDefaultAsync(e =>
                    e.EvaluationId == evaluationId &&
                    !e.IsArchived);
        }

    }
}
