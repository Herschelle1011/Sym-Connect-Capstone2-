using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.ViewModels.Attendance;
using System.ComponentModel.DataAnnotations;

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
        public DateTime? EndDate { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int Points { get; set; }

        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        // Facility
        public int? FacilityId { get; set; }
        public List<string> FacilityName { get; set; } = new List<string>();
        public string FacilityType { get; set; } = string.Empty;
        public string FacilityDescription { get; set; } = string.Empty;
        public int FacilityCapacity { get; set; }


        public TimeSpan? TimeInScannerOpenTime { get; set; }
        public TimeSpan? TimeInScannerClosesTime { get; set; }

        public double? EvaluationRating { get; set; }


        public TimeSpan? TimeOutScannerOpenTime { get; set; }
        public TimeSpan? TimeOutScannerClosesTime { get; set; }

        public List<FacilityPhotoVM> FacilityPhotos { get; set; } = new();

        //Created by Who?
        public ApprovalStatus ApprovalStatus { get; set; }

        public string? RejectionReason { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }


        public string? RequestedByGroupName { get; set; }


        // Statistics
        public int ExpectedGroupCount { get; set; }
        public int AttendanceCount { get; set; }
        public int EvaluationCount { get; set; }

        // Cards
        public List<ExpectedFacilityCardVM> Facilities { get; set; } = new();

        public List<ExpectedGroupCardVM> ExpectedGroups { get; set; } = new();

        public List<ScheduleTimelineVM> ScheduleTimeline { get; set; } = new();

        // Flags
        public bool HasEvaluation => EvaluationCount > 0;
        public int EvaluationResponseCount { get; set; }


        public bool HasAttendance => AttendanceCount > 0;


        public AttendanceTabViewModel AttendanceRecords { get; set; }






    }





}