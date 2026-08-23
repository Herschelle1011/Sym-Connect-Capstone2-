namespace Sym_Connect__Capstone2_.ViewModels.Attendance
{
    public class AttendanceTabViewModel
    {
        // === HEADER === 

        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        //=== SUMMARY ==== 

        public int TotalGroups { get; set; }
        public int FullyPresentCount { get; set; }
        public int PartiallyPresentCount { get; set; }
        public int AbsentCount { get; set; }

        public List<GroupAttendanceRowViewModel> GroupAttendance { get; set; } = new();
    }
}
