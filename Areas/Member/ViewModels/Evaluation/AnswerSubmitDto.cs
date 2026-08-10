using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation
{
    public class AnswerSubmitDto
    {
        public int EvaluationDetailId { get; set; }
        [Required(ErrorMessage = "please answer all the questions")]
        public string Answer { get; set; } = string.Empty;
    }
}
