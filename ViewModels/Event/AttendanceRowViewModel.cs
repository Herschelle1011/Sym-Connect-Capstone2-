namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class AttendanceRowViewModel
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string GroupName { get; set; } = string.Empty;

        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }

        public int Points { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
