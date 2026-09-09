namespace Sym_Connect__Capstone2_.ViewModels.Facility
{
    public class FacilityReservationViewModel
    {
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public List<string> GroupName { get; set; }
    }
}
