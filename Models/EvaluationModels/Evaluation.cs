using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sym_Connect__Capstone2_.Models.EventModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int Points { get; set; } = 2;

        public bool IsArchived { get; set; } = false;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ArchivedDate { get; set; }

        public ICollection<EvaluationDetails> EvaluationDetails { get; set; } =
                            new List<EvaluationDetails>();

        [ForeignKey(nameof(Event))]
        public int? EventId { get; set; }

        // Navigation property
        public Event? Event { get; set; }
    }
}