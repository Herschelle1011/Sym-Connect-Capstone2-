
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class EditQuestionViewModel
    {
        public int EvaluationId { get; set; }
        public int EvaluationDetailId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public AnswerType answerType { get; set; }
    }
}
