
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class EventCalendarViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public EventStatus Status { get; set; }



    }
}
