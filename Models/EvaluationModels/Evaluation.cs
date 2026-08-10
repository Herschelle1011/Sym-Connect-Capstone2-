using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models
{
    public class Evaluation
    {
        [Key]
        public int EvaluationId { get; set; }
        [Required]
        public string EvaluationTitle { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public DateTime OpenAt { get; set; }
        public DateTime ClosedAt { get; set; }

        public bool IsArchived { get; set; } = false;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ArchivedDate { get; set; }

        public ICollection<EvaluationDetails> EvaluationDetails { get; set; } =
                            new List<EvaluationDetails>();
    }
}