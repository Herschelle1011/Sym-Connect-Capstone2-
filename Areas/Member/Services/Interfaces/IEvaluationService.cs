using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Models;
using MemberModel = Sym_Connect__Capstone2_.Models.Member;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces
{
    public interface IEvaluationService
    {
        Task<MemberModel?> FindMemberForEvaluationAsync(string firstname, string email);
        Task<bool> SaveEvaluationAsync(string memberId, SubmitEvaluationViewModel model);

        Task<List<MemberEvaluationListItemViewModel>>GetAvailableEvaluationsAsync(int memberId,string userId);
        
        Task<Evaluation?> GetEvaluationByIdAsync(int evaluationId);

        Task<bool> CanTakeEvaluationAsync(int evaluationId, int memberId, string userId);
    }
}
