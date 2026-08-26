using Sym_Connect__Capstone2_.ViewModels.Participation;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IParticipationTrackingService
    {
        Task<ParticipationTrackingViewModel> GetParticipationTrackingAsync(DateTime? fromDate = null, DateTime? toDate = null);

        Task<List<MemberParticipationViewModel>> GetMembersAsync(DateTime? fromDate, DateTime? toDate, int groupId);
            }
}
