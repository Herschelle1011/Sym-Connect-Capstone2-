using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Data;
using MemberModel = Sym_Connect__Capstone2_.Models.Member;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Implementation
{
    public class EvaluationService : IEvaluationService
    {
        private readonly AppDbContext _context;
        public EvaluationService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<MemberModel?> FindMemberForEvaluationAsync(string firstname, string email)
        {
            return await _context.Members.FirstOrDefaultAsync
                                (m => m.User.Email!.ToLower() == email.ToLower() 
                                && m.FirstName!.ToLower() == firstname.ToLower());
        }



        public async Task<List<MemberEvaluationListItemViewModel>>GetAvailableEvaluationsAsync(int memberId,string userId)
        {
            return await _context.Evaluations
                .AsNoTracking()

                .Where(e =>
                    // Evaluation must still be available
                    !e.IsArchived &&
                    e.OpenAt <= DateTime.Now &&
                    e.ClosedAt >= DateTime.Now &&

                    // Member must have attended this event
                    _context.Attendances.Any(a =>
                        a.EventId == e.EventId &&
                        a.MemberId == memberId &&
                        (
                            a.Status == "Present" ||
                            a.Status == "Late"
                        )) &&

                    // Member must NOT have already submitted it
                    !_context.EvaluationSubmissions.Any(s =>
                        s.EvaluationId == e.EvaluationId &&
                        s.UserId == userId &&
                        s.Status == SubmissionStatus.Submitted)
                )

                .Select(e => new MemberEvaluationListItemViewModel
                {
                    EvaluationId = e.EvaluationId,
                    EvaluationTitle = e.EvaluationTitle,
                    Description = e.Description,
                    DateCreated = e.DateCreated,
                    OpenAt = e.OpenAt,
                    ClosedAt = e.ClosedAt,
                    EventTitle = e.Event.EventTitle
                })

                .ToListAsync();
        }


        public async Task<bool> CanTakeEvaluationAsync(int evaluationId,int memberId,string userId)
        {
            var canTake = await _context.Evaluations
                .AnyAsync(e =>
                    e.EvaluationId == evaluationId &&

                    !e.IsArchived &&
                    e.OpenAt <= DateTime.Now &&
                    e.ClosedAt >= DateTime.Now &&

                    // Must have attended
                    _context.Attendances.Any(a =>
                        a.EventId == e.EventId &&
                        a.MemberId == memberId &&
                        (
                            a.Status == "Present" ||
                            a.Status == "Late"
                        )) &&

                    // Must not already be submitted
                    !_context.EvaluationSubmissions.Any(s =>
                        s.EvaluationId == evaluationId &&
                        s.UserId == userId &&
                        s.Status == SubmissionStatus.Submitted)
                );

            return canTake;
        }

        public async Task<bool> SaveEvaluationAsync(
      string userId,
      SubmitEvaluationViewModel model)
        {
            var submission = await _context.EvaluationSubmissions
                .FirstOrDefaultAsync(s =>
                    s.EvaluationId == model.EvaluationId &&
                    s.UserId == userId);

            // 2. Create one if this is the first attempt
            if (submission == null)
            {
                submission = new EvaluationSubmission
                {
                    EvaluationId = model.EvaluationId,
                    UserId = userId,
                    Status = SubmissionStatus.Draft,
                    CreatedAt = DateTime.Now
                };

                _context.EvaluationSubmissions.Add(submission);

                // Save first so SubmissionId is generated
                await _context.SaveChangesAsync();
            }

            // 3. Do not allow another submission
            if (submission.Status == SubmissionStatus.Submitted)
            {
                return false;
            }

            // 4. Make sure answers actually arrived from the form
            if (model.Answers == null || !model.Answers.Any())
            {
                return false;
            }

            // 5. Save every answer
            foreach (var answerInput in model.Answers)
            {
                var existingAnswer =
                    await _context.EvaluationResponses
                        .FirstOrDefaultAsync(r =>
                            r.SubmissionId == submission.SubmissionId &&
                            r.EvaluationDetailId ==
                                answerInput.EvaluationDetailId);

                if (existingAnswer != null)
                {
                    // Existing draft answer → update it
                    existingAnswer.Answer =
                        answerInput.Answer;
                }
                else
                {
                    // First answer → create it
                    var response = new EvaluationResponse
                    {
                        SubmissionId = submission.SubmissionId,
                        EvaluationDetailId =
                            answerInput.EvaluationDetailId,

                        Answer = answerInput.Answer
                    };

                    _context.EvaluationResponses.Add(response);
                }
            }

            // 6. Final submission
            if (model.IsFinalSubmit)
            {
                submission.Status =
                    SubmissionStatus.Submitted;

                submission.SubmittedAt =
                    DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<Evaluation?> GetEvaluationByIdAsync(int evaluationId)
        {
            return await _context.Evaluations
                .Include(e => e.EvaluationDetails)
                    .ThenInclude(ed => ed.EvaluationQuestion)
                .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId && !e.IsArchived);
        }

    }
}
