using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class EventListItemViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public DateTime EndDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? RejectionReason { get; set; }
        public string? ApproveByName { get; set; }

        public List<string> FacilityNames { get; set; } = new();
        public List<string> GroupNames { get; set; } = new();

        public string? SubmittedByName { get; set; }

   
    }
}
