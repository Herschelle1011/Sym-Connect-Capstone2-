using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation
{

    public class TakeEvaluationViewModel
    {
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; } = string.Empty;
        public string MemberFullName { get; set; } = string.Empty; 
        public string MemberEmail { get; set; } = string.Empty;   
        public List<AnswerInputViewModel> Questions { get; set; } = new();
    }
   

  
}
