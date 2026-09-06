namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation
{
    public class MemberEvaluationListItemViewModel
    {
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; } = string.Empty;
        public DateTime OpenAt { get; set; }
        public DateTime ClosedAt { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;   // "Upcoming" / "Open" / "Closed"
        public bool IsAnswerable => Status == "Open";
    }
}
