using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
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

        [Required]
        public string Status { get; set; } = string.Empty;

        public bool? IsArchived { get; set; } = false;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ArchivedDate { get; set; }


    }
}
