namespace Sym_Connect__Capstone2_.ViewModels.Facility
{
    public class FacilityScheduleEventViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsAllDay { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
