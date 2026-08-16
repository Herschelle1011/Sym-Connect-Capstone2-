namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class EventSearchResultViewModel
    {
        public int EventId { get;  set; }
        public string EventTitle { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsAllDay { get; set; }

        public List<string> Facilities { get; set; } = new List<string>();

    }
}
