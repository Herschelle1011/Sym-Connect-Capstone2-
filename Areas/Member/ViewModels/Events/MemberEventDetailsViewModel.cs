namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Events
{
    public class MemberEventDetailsViewModel
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public string EventDescription { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsAllDay { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public List<string> FacilityNames { get; set; } = new();

        // One photo for each facility
        public List<string> FacilityPhotoUrls { get; set; } = new();

        public string PostedByName { get; set; } = string.Empty;

        public string PostedByRole { get; set; } = string.Empty;

        public int Points { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
