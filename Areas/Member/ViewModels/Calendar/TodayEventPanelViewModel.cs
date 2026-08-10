namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Calendar
{
    public class TodayEventPanelViewModel
    {
        public bool HasEventToday { get; set; }

        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string? GroupName { get; set; }
        public bool IsLiveNow { get; set; }
        public DateTime EndTime { get; set; }
    }
}
