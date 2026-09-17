namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard
{
    public class UpcomingEventViewModel
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public bool IsAllDay { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string FacilityName { get; set; } = string.Empty;

        public string CreatedByName { get; set; } = string.Empty;
    }
}
