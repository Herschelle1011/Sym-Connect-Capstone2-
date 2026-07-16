namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class DetailsEvaluationViewModel
    {
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }
        public DateTime OpenAt { get; set; }
        public DateTime ClosesAt { get; set; }

        public List<SubmissionListItemViewModel> Submissions { get; set; } = new();

        public List<EvaluationQuestionViewModel> Questions { get; set; } = new List<EvaluationQuestionViewModel>();

    }
}
