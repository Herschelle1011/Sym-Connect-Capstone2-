using Humanizer;

namespace Sym_Connect__Capstone2_.ViewModels.Election.PartyList
{
    public class ArchiveModalViewModel
    {
        public int partylistId { get; set; }
        public string partyListName { get; set; } = string.Empty;
        public string partyDescription { get; set; } = string.Empty;

        //public DateTime? DateCreated { get; set; } //To be added

        public List<string> CandidateNames { get; set; } = new();
        public int CandidateCount => CandidateNames.Count;

    }
}
