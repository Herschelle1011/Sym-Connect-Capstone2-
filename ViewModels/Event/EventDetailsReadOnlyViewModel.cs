using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class EventDetailsReadOnlyViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<string> FacilityNames { get; set; } = new();
        public string? FacilityPhotoUrl { get; set; }
        public List<string> GroupNames { get; set; } = new();
        public string CreatedByName { get; set; }
        public string CreatedByRole { get; set; }
    }
}
