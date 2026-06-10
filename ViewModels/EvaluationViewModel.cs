using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels
{
    public class EvaluationViewModel
    {


        public class ListEvaluationViewModel
        {
            public int EvaluationId { get; set; }
            public string EvaluationTitle { get; set; } = string.Empty;
 
            public string Description { get; set; } = string.Empty;

            public DateTime DateCreated { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        public class DetailsEvaluationViewModel
        {
            public int EvaluationId { get; set; }
            public string EvaluationTitle { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public DateTime DateCreated { get; set; }
            public string Status { get; set; } = string.Empty;
        }


        public class CreateEvaluationViewModel
        {
    
            [Required(ErrorMessage = "Evaluation Title is required")]
            [StringLength(100)]
            public string EvaluationTitle { get; set; } = string.Empty;
            [Required(ErrorMessage = "Description is required")]
            [StringLength(250)]
            public string Description { get; set; } = string.Empty;
  
            public DateTime DateSubmitted { get; set; } = DateTime.Now;
            [Required(ErrorMessage = "Status is required")]
            public string Status { get; set; } = "Active";
        }

        public class UpdateEvaluationViewModel
        {
            public int EvaluationId { get; set; }

            [Required(ErrorMessage = "Evaluation Title is required")]
            [StringLength(100)]
            public string EvaluationTitle { get; set; } = string.Empty;
            [Required(ErrorMessage = "Description is required")]
            [StringLength(250)]
            public string Description { get; set; } = string.Empty;

            public DateTime DateUpdated { get; set; } = DateTime.Now;
            [Required(ErrorMessage = "Status is required")]
            public string Status { get; set; } = "Active";
        }

        public class ArchiveEvaluationViewModel
        {
            public int EvaluationId { get; set; }
            public string EvaluationTitle { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime DateCreated { get; set; }
        }

        public class DeleteEvaluationViewModel
        {
            public int EvaluationId { get; set; }
            public string EvaluationTitle { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime DateCreated { get; set; }

        }

    }
}
