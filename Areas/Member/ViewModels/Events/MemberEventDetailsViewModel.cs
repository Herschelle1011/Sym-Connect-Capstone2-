namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Events
{
    public class MemberEventDetailsViewModel
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public string EventDescription { get; set; } = string.Empty;


        // Date
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        // Time
        public bool IsAllDay { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }


        // Facility
        public List<string> FacilityNames { get; set; } = new();
        public string? FacilityPhotoUrl { get; set; }


        // Creator
        public string PostedByName { get; set; } = string.Empty;

        public string PostedByRole { get; set; } = string.Empty;


        // Points member can earn
        public int Points { get; set; }


        // Upcoming / Ongoing / Completed
        public string Status { get; set; } = string.Empty;
    }
}
