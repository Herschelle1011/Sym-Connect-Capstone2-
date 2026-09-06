namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class EvaluationEventOptionViewModel
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public List<string> GroupNames { get; set; } = new();

        public List<string> FacilityNames { get; set; } = new();
    }
}
