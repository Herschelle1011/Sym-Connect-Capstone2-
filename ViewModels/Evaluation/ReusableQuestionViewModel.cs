using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class ReusableQuestionViewModel
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; }

        public AnswerType AnswerType { get; set; }

        public int UsageCount { get; set; }
    }
}
