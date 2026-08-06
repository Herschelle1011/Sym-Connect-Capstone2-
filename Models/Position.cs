using Sym_Connect__Capstone2_.Models.ElectionModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models
{
    public class Position
    {
        public int PositionId { get; set; }

        [Required, MaxLength(100)]
        public string PositionName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public int MaxCandidates { get; set; } = 1;

    }
}
