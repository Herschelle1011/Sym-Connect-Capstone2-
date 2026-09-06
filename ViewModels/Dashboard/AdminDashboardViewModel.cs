using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public string AdminName { get; set; }
        public DateTime Today { get; set; }

        // Stat cards
        public int TotalMembers { get; set; }
        public int TotalGroups { get; set; }

        //EVENT SUMMARY
        public int TotalEventsThisPeriod { get; set; }
        public int EventsApprovedCount { get; set; }
        public int EventsPendingCount { get; set; }
        public int EventsRejectedCount { get; set; }


        public int UpcomingActivitiesCount { get; set; }
        public int PendingApprovalsCount { get; set; }

        // The three lists + chart
        public List<UpcomingEventViewModel> UpcomingEvents { get; set; } = new();
        public List<PendingActionViewModel> PendingActions { get; set; } = new();
        public List<ChartPointViewModel> WeeklySummary { get; set; } = new();
        public List<EventListItemViewModel> RecentEventRequests { get; set; } = new();
    }
}
