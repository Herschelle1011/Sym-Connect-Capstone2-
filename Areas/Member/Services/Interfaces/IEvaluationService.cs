using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.EvaluationModels;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces
{
    public interface IEvaluationService
    {
        Task<User?> FindMemberForEvaluationAsync(string fullname, string email);
        Task<Evaluation?> GetEvaluationForMemberAsync(string memberId);
        Task<bool> SaveEvaluationAsync(string memberId, SubmitEvaluationViewModel model);

        Task<List<Evaluation>> GetAvailableEvaluationsAsync();
        Task<Evaluation?> GetEvaluationByIdAsync(int evaluationId);
    }
}
