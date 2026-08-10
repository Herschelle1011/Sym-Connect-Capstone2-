namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Calendar
{
    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; } 
        public bool IsToday { get; set; }

        public List<CalendarEventsViewModel> Events { get; set; } = new();
    }
}
