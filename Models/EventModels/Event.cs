using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.FacilityModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models.EventModels
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

        [Range(1, 100)]
        public int Points { get; set; } = 3;

        //public int? FacilityId { get; set; }
        //public Facility? Facility { get; set; } = null!;

        public EventStatus Status { get; set; } = EventStatus.Upcoming;


        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public bool IsArchived { get; set; }
        public DateTime? ArchiveDate { get; set; }



        //TIME SCANNER OPEN/OUT MODIFY
        public TimeSpan? TimeInScannerOpensOffset { get; set; }  
        public TimeSpan? TimeInScannerClosesOffset { get; set; }

        public TimeSpan? TimeOutScannerOpensOffset { get; set; }
        public TimeSpan? TimeOutScannerClosesOffset { get; set; }



        [ForeignKey(nameof(CreatedByUser))]
        public int? CreatedByUserId { get; set; }
        public Member? CreatedByUser { get; set; }


        //APPROVAL PART --- LEADERS PART
        public string? RejectionReason { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public DateTime? ApprovedDate { get; set; }

        [ForeignKey(nameof(ApprovedByUser))]
        public int? ApprovedByUserId { get; set; }
        public Member? ApprovedByUser { get; set; }




        public ICollection<EventFacility> ExpectedFacilities { get; set; } = new List<EventFacility>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<EventGroup> ExpectedGroups { get; set; } = new List<EventGroup>();

        public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();

    }
}
