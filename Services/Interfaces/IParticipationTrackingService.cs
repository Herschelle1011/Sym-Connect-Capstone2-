using Sym_Connect__Capstone2_.ViewModels.Participation;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IParticipationTrackingService
    {
        Task<ParticipationTrackingViewModel> GetParticipationTrackingAsync();

    }
}
