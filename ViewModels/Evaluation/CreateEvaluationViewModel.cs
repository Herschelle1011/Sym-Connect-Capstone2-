using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class CreateEvaluationViewModel : EvaluationFormViewModel
    {
        public List<QuestionRowViewModel> RatingQuestions { get; set; } = new();
        public List<AvailableQuestionViewModel> AvailableQuestions { get; set; } = new();
    }

    public class QuestionRowViewModel
    {
        public int? QuestionId { get; set; }   // NEW hidden field — null if it's a brand-new question
        public string QuestionText { get; set; } = string.Empty;

        public AnswerType AnswerType { get; set; }
    }
    public class AvailableQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;

        public AnswerType AnswerType { get; set; }
    }
}
