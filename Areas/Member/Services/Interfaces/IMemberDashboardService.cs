using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard;
namespace Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces
{
    public interface IMemberDashboardService
    {
        Task<DashboardViewModel> GetMemberDashboardAsync(int memberId);
    }
}
