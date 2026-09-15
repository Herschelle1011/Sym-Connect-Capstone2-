namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Points
{
    public class MemberPointDetailsViewModel
    {
        public int AttendanceId { get; set; }

        public int EventId { get; set; }


        // =========================
        // EVENT INFORMATION
        // =========================

        public string EventTitle { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public List<string> FacilityNames { get; set; } = new();


        // =========================
        // ATTENDANCE
        // =========================

        // Present / Late / Absent
        public string Status { get; set; } = string.Empty;

        public int Points { get; set; }

        public DateTime? TimeIn { get; set; }

        public DateTime? TimeOut { get; set; }


        // =========================
        // SCANNER INFORMATION
        // =========================

        public string ScannedByName { get; set; } = string.Empty;

        public string ScannedByRole { get; set; } = string.Empty;


        // =========================
        // REMARKS
        // =========================

        public string Remarks { get; set; } = string.Empty;
    }
}
