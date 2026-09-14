using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.ViewModels.Group;
namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class GroupAttendanceDetailsViewModel
    {
        public int groupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int eventId { get; set; }
        public string eventTitle { get; set; } = string.Empty;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public DateTime EventDate { get; set; }
        public string GroupLeader { get; set; } = string.Empty;
        public int TotalMembers { get; set; }

        public string EventFacility { get; set; }
        public string? EventPhotoUrl { get; set; }
        public int PresentCount { get; set; }
        public int PendingCount { get; set; }
        public int AbsentCount { get; set; }
        public int AttendancePercentage { get; set; }


        public GroupAttendanceStatus GroupAttendanceStatus { get; set; }

        public List<AttendanceRowViewModel> attendanceRowViewModels { get; set; } = new();



    }
}
