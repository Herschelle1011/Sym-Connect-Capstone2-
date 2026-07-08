namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class DeleteEvaluationViewModel
    {
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }
}
