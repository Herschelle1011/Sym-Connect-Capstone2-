using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels
{
    public class AnswerInputViewModel
    {
        public int EvaluationDetailId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public AnswerType AnswerType { get; set; } 
        public string? Answer { get; set; }
    }

    public class TakeEvaluationViewModel
    {
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; } = string.Empty;
        public string MemberFullName { get; set; } = string.Empty; 
        public string MemberEmail { get; set; } = string.Empty;   
        public List<AnswerInputViewModel> Questions { get; set; } = new();
    }
    public class SubmitEvaluationViewModel
    {
        public int EvaluationId { get; set; }
        public List<AnswerSubmitDto> Answers { get; set; } = new();
        public bool IsFinalSubmit { get; set; } // true = Submit, false = Save for Later
    }

    public class AnswerSubmitDto
    {
        public int EvaluationDetailId { get; set; }
        public string Answer { get; set; } = string.Empty;
    }
}
