namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Calendar
{
    public class MemberCalendarPageViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public string ActiveTab { get; set; } = "MyGroupEvents"; 
        public string? GroupName { get; set; } 

        public List<CalendarDayViewModel> Days { get; set; } = new();

        public TodayEventPanelViewModel TodayPanel { get; set; } = new();
        public List<UpcomingEventViewModel> UpcomingEvents { get; set; } = new();
    }
}
