namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class CancelledEventDetailsViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public List<string> FacilityNames { get; set; } = new();
        public List<string> AssignedGroupNames { get; set; } = new();

        public string CreatedByName { get; set; }

        public string CancellationReason { get; set; }
        public DateTime? CancelledDate { get; set; }
    }
}
