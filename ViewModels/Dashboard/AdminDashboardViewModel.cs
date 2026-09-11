using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public string AdminName { get; set; }
        public DateTime Today { get; set; }

        public int TotalMembers { get; set; }
        public int TotalGroups { get; set; }

        public int TotalEventsThisPeriod { get; set; }
        public int EventsApprovedCount { get; set; }
        public int EventsPendingCount { get; set; }
        public int EventsRejectedCount { get; set; }

        public int UpcomingActivitiesCount { get; set; }
        public int PendingApprovalsCount { get; set; }

        public List<UpcomingEventViewModel> UpcomingEvents { get; set; } = new();
        public List<PendingApprovalItemViewModel> PendingApprovals { get; set; } = new();
        public List<RecentRegistrationViewModel> RecentRegistrations { get; set; } = new();
        public List<ChartPointViewModel> WeeklySummary { get; set; } = new();
        public List<EventListItemViewModel> RecentEventRequests { get; set; } = new();
    }

    public class PendingApprovalItemViewModel
    {
        public string Type { get; set; } = string.Empty; // "Membership", "EventRequest", "Media"
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime? LatestDate { get; set; }
        public string Url { get; set; } = string.Empty;
        public string IconClass { get; set; } = "bi-info-circle";
    }

    public class RecentRegistrationViewModel
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime ApprovedDate { get; set; }
    }
}