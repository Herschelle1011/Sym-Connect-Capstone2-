namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class RestoreEventViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        public bool CanRestore { get; set; }
    }
}
