using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class EvaluationFormViewModel
    {
        [Required(ErrorMessage = "Evaluation Title is required")]
        [StringLength(100)]
        public string EvaluationTitle { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required")]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Close Date is required")]
        public DateTime ClosedAt { get; set; }
        [Required(ErrorMessage = "Open Date is required")]
        public DateTime OpenAt { get; set; }

        [Required(ErrorMessage = "Please select an event.")]
        public int? EventId { get; set; }

        public List<EvaluationEventOptionViewModel> EventOptions { get; set; }
            = new();

        public List<EvaluationQuestionViewModel> Questions { get; set; }
                                                   = new();

    }
}
