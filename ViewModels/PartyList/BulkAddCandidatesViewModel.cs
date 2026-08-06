using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.PartyList
{
    public class BulkAddCandidatesViewModel
    {
        public int partyListId { get; set; }
        public List<CandidateAssignmentItem> Assignments { get; set; } = new();
    }

    public class CandidateAssignmentItem
    {
        [Required]
        public int PositionId { get; set; }

        [Required]
        public int MemberId { get; set; }

        public int? ExistingCandidateId { get; set; }
    }
}
