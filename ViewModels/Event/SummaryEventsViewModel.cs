using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class SummaryEventsViewModel
    {
        public int TotalRequests { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int ExpiredCount { get; set; }

        public List<EventListItemViewModel> Events { get; set; } = new();
    }

}
