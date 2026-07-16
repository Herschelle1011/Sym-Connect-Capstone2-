using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation
{
    public class AnswerInputViewModel
    {
        public int EvaluationDetailId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public AnswerType AnswerType { get; set; }
    }
}
