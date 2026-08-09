using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.EvaluationModels;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Implementation
{
    public class EvaluationService : IEvaluationService
    {
        private readonly AppDbContext _context;
        public EvaluationService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<User?> FindMemberForEvaluationAsync(string firstname, string email)
        {
            return await _context.Users.FirstOrDefaultAsync
                                (m => m.Email!.ToLower() == email.ToLower() 
                                && m.FirstName!.ToLower() == firstname.ToLower());
        }



        public async Task<Evaluation?> GetEvaluationForMemberAsync(string memberId)
        {
            // TEMP: just grab any evaluation for demo purposes.
            // TODO: replace with real logic once event/attendance linking is implemented.
            var evaluation = await _context.Evaluations
                .Where(e => !e.IsArchived)
                .Include(e => e.EvaluationDetails)
                    .ThenInclude(ed => ed.EvaluationQuestion)
                .FirstOrDefaultAsync();
            return evaluation;
        }

        public async Task<bool> SaveEvaluationAsync(string memberId, SubmitEvaluationViewModel model)
        {
            // 1. Find or create the submission (the "cover page")
            var submission = await _context.EvaluationSubmissions
                .Include(s => s.EvaluationResponse)
                .FirstOrDefaultAsync(s => s.EvaluationId == model.EvaluationId && s.UserId == memberId);

            if (submission == null)
            {
                submission = new EvaluationSubmission
                {
                    EvaluationId = model.EvaluationId,
                    UserId = memberId,
                    Status = SubmissionStatus.Draft,
                    CreatedAt = DateTime.Now
                };
                _context.EvaluationSubmissions.Add(submission);
                await _context.SaveChangesAsync(); // save now so submission.SubmissionId is generated
            }

            // 2. Block edits if already submitted
            if (submission.Status == SubmissionStatus.Submitted)
                return false;

            // 3. Save each answer — update if exists, insert if new
            foreach (var answerInput in model.Answers)
            {
                var existingAnswer = submission.EvaluationResponse
                    .FirstOrDefault(r => r.EvaluationDetailId == answerInput.EvaluationDetailId);

                if (existingAnswer != null)
                {
                    existingAnswer.Answer = answerInput.Answer; // update in place
                }
                else
                {
                    _context.EvaluationResponses.Add(new EvaluationResponse
                    {
                        SubmissionId = submission.SubmissionId,
                        EvaluationDetailId = answerInput.EvaluationDetailId,
                        Answer = answerInput.Answer
                    });
                }
            }

    
            if (model.IsFinalSubmit)
            {
                submission.Status = SubmissionStatus.Submitted;
                submission.SubmittedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Evaluation>> GetAvailableEvaluationsAsync()
        {
            return await _context.Evaluations
                .Where(e => !e.IsArchived)
                .ToListAsync();
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
