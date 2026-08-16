namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Calendar
{
    public class UpcomingEventViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public List<string>? FacilityName { get; set; } = new List<string>();
    }
}
