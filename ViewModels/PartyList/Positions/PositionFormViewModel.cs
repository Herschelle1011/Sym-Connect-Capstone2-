using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.PartyList.Positions
{
    public class PositionFormViewModel
    {
   
        [Required(ErrorMessage = "Position Name is required.")]
        public string PositionName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "DisplayOrder is required.")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public int MaxCandidate { get; set; } = 1;
    }
}
