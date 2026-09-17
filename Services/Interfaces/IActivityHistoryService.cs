using Sym_Connect__Capstone2_.ViewModels.ActivityHistoryViewModel;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IActivityHistoryService
    {
        Task<ActivityHistoryViewModel> GetActivityHistoryAsync(int memberId,string userId);
    }
}
