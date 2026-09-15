namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Points
{
    public class MemberPointActivityViewModel
    {
        public int AttendanceId { get; set; }

        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string FacilityName { get; set; } 

        public string Status { get; set; } = string.Empty;

        public int Points { get; set; }
    }
}
