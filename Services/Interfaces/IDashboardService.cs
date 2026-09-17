using Sym_Connect__Capstone2_.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<int> GetAnnouncementCountThisWeekAsync(DateTime start, DateTime end);
        Task<int> GetEvaluationCountThisWeekAsync(DateTime start, DateTime end);
        Task<int> GetParticipationPointsThisWeekAsync(DateTime start, DateTime end);
        Task<List<UpcomingEventViewModel>> GetUpcomingEventsAsync(int take = 3);
        Task<int> GetActivitiesCountThisWeekAsync(DateTime start, DateTime end);
        Task<int> GetAttendanceCountThisWeekAsync(DateTime start, DateTime end);

        Task<List<ChartPointViewModel>>GetWeeklyActivityOverviewAsync();

        Task<int> GetExpiredEventCountAsync();

        // NEW
        Task<(int Count, DateTime? LatestDate)> GetPendingMembershipApplicationsAsync();
        Task<(int Count, DateTime? LatestDate)> GetPendingEventRequestsAsync();
        Task<(int Count, DateTime? LatestDate)> GetPendingMediaFilesAsync();
        Task<List<RecentRegistrationViewModel>> GetRecentApprovedMembersAsync(int take = 5);
        Task<List<EventListItemViewModel>> GetRecentEventRequestsAsync(int take = 5);


        Task<EventStatusSummaryViewModel>GetEventStatusSummaryAsync(
        DateTime start,
        DateTime end);

    }
}
