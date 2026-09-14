using Sym_Connect__Capstone2_.ViewModels.Announcement;
using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.ViewModels.Dashboard.Leader
{
    public class LeaderDashboardViewModel
    {
        public string LeaderFirstName { get; set; } = string.Empty;

        public int TotalMembers { get; set; }
        public int ParticipationPoints { get; set; }
        public int UpcomingActivitiesCount { get; set; }
        public int PendingRequestCount { get; set; }

        public List<VisibleAnnouncementViewModel> PinnedAnnouncements { get; set; } = new();
        public List<EventListItemViewModel> UpcomingEvents { get; set; } = new();
        public List<GroupAttendanceChartItemViewModel> GroupAttendanceChart { get; set; } = new();
        public List<SubmissionStatusItemViewModel> SubmissionStatuses { get; set; } = new();
    }

    public class GroupAttendanceChartItemViewModel
    {
        public string EventTitle { get; set; } = string.Empty;
        public int PresentCount { get; set; }
    }

    public class SubmissionStatusItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Event" or "Media"
        public string Status { get; set; } = string.Empty;   // Approved / Pending / Rejected
    }
}

