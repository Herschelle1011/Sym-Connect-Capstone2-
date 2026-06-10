using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Services
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
                 .Where(e => e.IsArchived == false)
                .ToListAsync();
        }

        public async Task<Evaluation?> GetEvaluationByIdAsync(int id)
        {
            return await _context.Evaluations
                .FirstOrDefaultAsync(e => e.EvaluationId == id && e.IsArchived == false);
        }

        //------PROCEED TO THE OTHER METHODS NEEDED -------//
        // SEARCH FOR INCLUDE, .SELECT , ENTITY FRAMEWORKDS // 

        public async Task CreateEvaluationAsync(Evaluation evaluation)
        {
            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateEvaluationAsync(Evaluation evaluation)
        {
            _context.Evaluations.Update(evaluation);
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

        public async Task ArchiveEvaluationAsync(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null) return;

            evaluation.Status = "Inactive";
            evaluation.IsArchived = true;
            evaluation.ArchivedDate = DateTime.Now;
             await _context.SaveChangesAsync();
        }
    }
}
