namespace Sym_Connect__Capstone2_.ViewModels.Dashboard
{
    public class WeeklyReportViewModel
    {

        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }

        public int TotalActivities { get; set; }
        public int TotalAttendance { get; set; }
        public int ParticipationPoints { get; set; }
        public int EvaluationsSubmitted { get; set; }
        public int AnnouncementsCount { get; set; }

        public ActivitySummaryViewModel ActivitySummary { get; set; } = new();
        public List<DayAttendanceViewModel> AttendanceByDay { get; set; } = new();
        public List<TopGroupViewModel> TopGroups { get; set; } = new();
        public EvaluationSummaryViewModel EvaluationSummary { get; set; } = new();
        public List<AnnouncementSummaryViewModel> RecentAnnouncements { get; set; } = new();
    }
}
