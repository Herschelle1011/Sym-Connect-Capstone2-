namespace Sym_Connect__Capstone2_.ViewModels.ActivityHistoryViewModel
{
    public class ActivityHistoryItemViewModel
    {
        public int Id { get; set; }

        // Event, Election, or Evaluation
        public string ActivityType { get; set; }
            = string.Empty;

        public string Title { get; set; }
            = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public string Location { get; set; }
            = string.Empty;

        // Present, Late, Absent, Voted, Submitted
        public string Status { get; set; }
            = string.Empty;

        public int Points { get; set; }
    }
}
