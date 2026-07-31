using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class AddCandidateViewModel
    {
        public int partyListId { get; set; }
        public int memberId { get; set; }
        public int electionId { get; set; }
        public Positions position;
    }
}
