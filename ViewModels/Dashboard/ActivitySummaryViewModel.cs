namespace Sym_Connect__Capstone2_.ViewModels.Dashboard
{
    public class ActivitySummaryViewModel
    {
        public int CompletedCount { get; set; }
        public int OngoingCount { get; set; }
        public int UpcomingCount { get; set; }
        public int CancelledCount { get; set; }
        public int TotalCount => CompletedCount + OngoingCount + UpcomingCount + CancelledCount;

        public int CompletedPercent => TotalCount == 0 ? 0 : (int)Math.Round(CompletedCount * 100.0 / TotalCount);
        public int OngoingPercent => TotalCount == 0 ? 0 : (int)Math.Round(OngoingCount * 100.0 / TotalCount);
        public int UpcomingPercent => TotalCount == 0 ? 0 : (int)Math.Round(UpcomingCount * 100.0 / TotalCount);
        public int CancelledPercent => TotalCount == 0 ? 0 : (int)Math.Round(CancelledCount * 100.0 / TotalCount);
    }

}
