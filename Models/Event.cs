using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;
        public string? EventDescription { get; set; } = string.Empty;

        public bool IsAllDay { get; set; }

        //time if its not ALLDAY
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public int FacilityId { get; set; }
        public Facility Facility { get; set; } = null!;

        public EventStatus Status { get; set; } = EventStatus.Upcoming;

        public string? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsArchived { get; set; }
        public DateTime? ArchiveDate { get; set; }

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<EventGroup> ExpectedGroups { get; set; } = new List<EventGroup>();

        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    }
}
