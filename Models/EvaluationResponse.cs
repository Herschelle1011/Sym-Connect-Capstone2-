using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models
{
    public class EvaluationResponse
    {
        [Key]
        public int ResponseId { get; set; }


        [ForeignKey(nameof(EvaluationSubmission))]
        public int SubmissionId { get; set; }

        [ForeignKey(nameof(EvaluationDetail))]
        public int EvaluationDetailId { get; set; }

        public string? Answer { get; set; }


        // Navigation Properties
        public EvaluationDetails? EvaluationDetail { get; set; }
        public EvaluationSubmission? EvaluationSubmission { get; set; }

    }
}

