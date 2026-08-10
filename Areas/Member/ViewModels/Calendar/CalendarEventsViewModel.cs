using Sym_Connect__Capstone2_.Models.Enums;
namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Calendar
{
    public class CalendarEventsViewModel
    {
        public int eventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string? GroupName { get; set; }

        public bool isAllDay { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool IsMyGroupEvent { get; set; }

        public MemberEventStatus Status { get; set; }

    }
}
