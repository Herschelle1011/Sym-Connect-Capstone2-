using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;


namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class EvaluationQuestionViewModel
    {
        public int QuestionId { get; set; }
        [Required(ErrorMessage = "Enter any question")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose an AnswerType")]
        public AnswerType AnswerType { get; set; }

    }
}
