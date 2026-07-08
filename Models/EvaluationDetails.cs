using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models
{

    public class EvaluationDetails
    {
        [Key]
        public int EvaluationDetailId { get; set; }

        [ForeignKey(nameof(Evaluation))]
        public int EvaluationId { get; set; }

        [ForeignKey(nameof(EvaluationQuestion))]
        public int QuestionId { get; set; }

        //NAVIGATION PROPERTY
        public Evaluation? Evaluation { get; set; }
        public EvaluationQuestion? EvaluationQuestion { get; set; }

        //ARCHIVED SECTION
        public bool? IsArchived { get; set; } = false;  
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ArchiveDate { get; set; }
        
    }
}
