using Sym_Connect__Capstone2_.ViewModels.Dashboard;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IWeeklyReportService
    {
        Task<WeeklyReportViewModel> GetWeeklyReportAsync(DateTime weekStart, DateTime weekEnd);
    }
}
