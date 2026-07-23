using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class UpdateEvaluationViewModel : EvaluationFormViewModel
    {
        public int EvaluationId { get; set; }

        public List<QuestionRowViewModel> RatingScale { get; set; } = new();
        public List<QuestionRowViewModel> ShortAnswerQuestion { get; set; } = new();

        public List<AvailableQuestionViewModel> AvailableQuestions { get; set; } = new();

 
    }
}
