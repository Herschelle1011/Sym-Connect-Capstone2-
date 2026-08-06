using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.PartyList
{
    public class PartyListDetailsViewModel
    {
        public int partyListid { get; set; }
        public string partyListName { get; set; } = string.Empty;
        public string partyDescription { get; set; } = string.Empty;

        public int? ElectionId { get; set; }
        public string? ElectionName { get; set; }   //"Unassigned first"
        public List<CandidateViewModel> Candidates { get; set; } = new List<CandidateViewModel>();

    }
}
