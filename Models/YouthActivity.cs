using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models
{
    public class YouthActivity
    {
        public enum ActivityCategory
        {
            Mass,
            Formation,
            Meeting,
            Service,
            Practice,
            Evaluation,
            Social,
            Other
        }

        public enum ActivityStatus
        {
            Pending,
            Approved,
            Rejected,
            Cancelled,
            Completed
        }

        public enum ActivityType
        {
            Event,
            Activity
        }


        [Key]
        public int ActivityId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string? Description { get; set; } = string.Empty;
        [Required]
        public int PointsAwarded { get; set; } = 0;

        //Activity Type = "Event" or "Activity"
        public ActivityType Type { get; set; }

        //category of the activity/event
        public ActivityCategory Category { get; set; }


        //planning approval workflow
        public ActivityStatus Status { get; set; } = ActivityStatus.Pending; //default to pending if leader

        //date and time of the activity
        [Required]
        public DateTime ScheduleDate { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }


        // FK to Facility (from facility table)
        public int? FacilityId { get; set; }
        [ForeignKey(nameof(FacilityId))]
        public Facility? Facility { get; set; }

        // FK to YouthGroup (which group assigned to this activity or event)
        public int? GroupId { get; set; }

        public Group? Group { get; set; }

        // Who submitted this for approval (could be ADMIN OR LEADER)
        public string SubmittedByUserId { get; set; }
        [ForeignKey(nameof(SubmittedByUserId))]
        public User? SubmittedBy { get; set; }
    }
}
