using Sym_Connect__Capstone2_.Models.Enums;
namespace Sym_Connect__Capstone2_.ViewModels.Attendance
{
    public class GroupAttendanceRowViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? AvatarColor { get; set; } // hex or CSS class for the colored circle icon

        public int TotalMembers { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }

        public double AttendanceRate { get; set; } 

        public GroupAttendanceStatus Status { get; set; }
    }
}
