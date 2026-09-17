namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Events
{
    public class MemberEventItemViewModel
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;


        // Date
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        // Time
        public bool IsAllDay { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }


        // Facility
        public string FacilityName { get; set; } = string.Empty;

        public string? FacilityPhotoUrl { get; set; }

        public List<string> FacilityPhotoUrls { get; set; } = new();


        // Who created the event
        public string HostedByName { get; set; } = string.Empty;

        public string HostedByRole { get; set; } = string.Empty;


        // Used for the badge:
        // Upcoming / Ongoing / Completed
        public string Status { get; set; } = string.Empty;
    }
}
