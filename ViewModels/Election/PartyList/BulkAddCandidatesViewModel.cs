using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Election.PartyList
{
    public class BulkAddCandidatesViewModel
    {
        public int partyListId { get; set; }
        public List<CandidateAssignmentItem> Assignments { get; set; } = new();
    }

    public class CandidateAssignmentItem
    {
        [Required]
        public string Position { get; set; }

        [Required]
        public int CandidateId { get; set; }
    }
}
