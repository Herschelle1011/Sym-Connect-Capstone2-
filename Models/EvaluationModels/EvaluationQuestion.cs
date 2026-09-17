using System.ComponentModel.DataAnnotations;
using Sym_Connect__Capstone2_.Models.Enums;


namespace Sym_Connect__Capstone2_.Models
{
    public class EvaluationQuestion
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public AnswerType AnswerType { get; set; }

        //public DateTime DateCreated { get; set; } = DateTime.Now;
        //public ICollection<EvaluationDetails> EvaluationDetails { get; set; } = new List<EvaluationDetails>();

    }
}