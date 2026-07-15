using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Models
{
    public class EvaluationSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [ForeignKey(nameof(Evaluation))]
        public int EvaluationId { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;

        public SubmissionStatus Status { get; set; } 
        public DateTime? SubmittedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        public Evaluation? Evaluation { get; set; }
        public User? User { get; set; }
        public ICollection<EvaluationResponse> EvaluationResponse { get; set; } = new List<EvaluationResponse>();
    }
}
