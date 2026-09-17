namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class EventStatusSummaryViewModel
    {
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
        public int Expired { get; set; }

        public int Total =>
            Approved + Pending + Rejected + Expired;
    }
}
