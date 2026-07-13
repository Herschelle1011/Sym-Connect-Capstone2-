namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class EventListViewModel
    {
        public List<EventCalendarViewModel> Events { get; set; } = new();

        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthLabel => new DateTime(Year, Month, 1).ToString("MMMM yyyy");

        public int PreviousYear => Month == 1 ? Year - 1 : Year;
        public int PreviousMonth => Month == 1 ? 12 : Month - 1;

        public int NextYear => Month == 12 ? Year + 1 : Year;
        public int NextMonth => Month == 12 ? 1 : Month + 1;

    }
}
