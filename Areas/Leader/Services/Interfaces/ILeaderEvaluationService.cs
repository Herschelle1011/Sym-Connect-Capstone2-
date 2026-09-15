using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Areas.Leader.Services.Interfaces
{
    public interface ILeaderEvaluationService 
    {
       Task<List<MemberEvaluationListItemViewModel>>
      GetAssignedEvaluationsAsync(
          int currentMemberId,
          string userId);

        Task<bool> CanTakeAssignedEvaluationAsync(
            int evaluationId,
            int memberId,
            string userId);

        Task<Evaluation?> GetEvaluationByIdAsync(
            int evaluationId);

        Task<bool> SaveEvaluationAsync(
            string userId,
            SubmitEvaluationViewModel model);
    }
}
