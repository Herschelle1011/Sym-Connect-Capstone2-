namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard
{
    public class TodayEventViewModel
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public bool IsAllDay { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string FacilityName { get; set; } = string.Empty;

        public string HostedBy { get; set; } = string.Empty;

        // Example:
        // "Started 45 mins ago"
        // "Starts in 20 mins"
        public string TimeStatus { get; set; } = string.Empty;
    }
}
