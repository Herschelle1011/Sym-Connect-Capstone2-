using Sym_Connect__Capstone2_.ViewModels.Dashboard.Leader;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface ILeaderDashboardService 
    {
        Task<LeaderDashboardViewModel?> GetDashboardAsync(int leaderMemberId);

    }
}
