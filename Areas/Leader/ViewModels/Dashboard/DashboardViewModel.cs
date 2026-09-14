using Sym_Connect__Capstone2_.ViewModels.Announcement;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Group;


namespace Sym_Connect__Capstone2_.Areas.Leader.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public List<VisibleAnnouncementViewModel?> PinnedAnnouncement { get; set; }
        public List<VisibleAnnouncementViewModel> Announcements { get; set; } = new();

        // New — stat cards
        public int TotalMembers { get; set; }
        public int ParticipationPoints { get; set; }
        public int UpcomingActivitiesCount { get; set; }
        public int PendingRequestCount { get; set; }

        // New — Upcoming Events cards
        public List<EventListItemViewModel> UpcomingEvents { get; set; } = new();

        // New — Group Attendance chart
        public List<GroupAttendanceChartItemViewModel> GroupAttendanceChart { get; set; } = new();

        // New — Submission Status list
        public List<SubmissionStatusItemViewModel> SubmissionStatuses { get; set; } = new();
    }

    public class SubmissionStatusItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Event" or "Media"
        public string Status { get; set; } = string.Empty;
    }
}