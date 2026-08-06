using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.ViewModels.PartyList;

namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class PartyListsViewModel
    {
        public int Id { get; set; }
        public string PartyListName { get; set; } = string.Empty;
        public string PartyListDescription { get; set; } = string.Empty;

        public List<CandidateViewModel> candidates { get; set; } = new List<CandidateViewModel>();

        public DateTime? DateCreated { get; set; }

    }
}
