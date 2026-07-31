using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels
{
    public class TakeVoteViewModel
    {
        public string memberId { get; set; }

        public int electionId { get; set; }
        public Positions position { get; set; }
        public int candidateId { get; set; }

        public List<SelectedCandidateViewModel> selectedVotes { get; set; } = new List<SelectedCandidateViewModel>();
    }
}
