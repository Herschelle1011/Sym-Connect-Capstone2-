namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class ArchivedEvaluationDetailsViewModel
    {
        public int EvaluationId { get; set; }

        public string EvaluationTitle { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string EventName { get; set; } = "No Event";

        public DateTime OpenAt { get; set; }

        public DateTime ClosedAt { get; set; }

        public int QuestionCount { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? ArchiveDate { get; set; }
    }
}
