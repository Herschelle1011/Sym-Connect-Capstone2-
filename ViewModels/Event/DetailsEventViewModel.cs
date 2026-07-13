using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class DetailsEventViewModel
    {
        // Header
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string? EventDescription { get; set; }

        public EventStatus Status { get; set; }

        public bool IsAllDay { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsArchived { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        // Facility
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty;
        public string FacilityDescription { get; set; } = string.Empty;
        public int FacilityCapacity { get; set; }

        public List<string> FacilityPhotos { get; set; } = new();

        // Statistics
        public int ExpectedGroupCount { get; set; }
        public int AttendanceCount { get; set; }
        public int EvaluationCount { get; set; }

        // Cards
        public List<ExpectedGroupCardVM> ExpectedGroups { get; set; } = new();

        public List<ScheduleTimelineVM> ScheduleTimeline { get; set; } = new();

        // Flags
        public bool HasEvaluation => EvaluationCount > 0;
        public bool HasAttendance => AttendanceCount > 0;
    }

    public class ExpectedGroupCardVM
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public int MemberCount { get; set; }
    }

    public class ScheduleTimelineVM
    {
        public string Time { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}