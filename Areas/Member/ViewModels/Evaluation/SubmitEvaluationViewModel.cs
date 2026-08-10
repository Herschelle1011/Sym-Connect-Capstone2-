namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation
{
    public class SubmitEvaluationViewModel
    {
        public int EvaluationId { get; set; }
        public List<AnswerSubmitDto> Answers { get; set; } = new();
        public bool IsFinalSubmit { get; set; }
    }
}
