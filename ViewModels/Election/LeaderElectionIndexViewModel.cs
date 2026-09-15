using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election;

namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class LeaderElectionIndexViewModel
    {
        public List<GetAllElectionListViewModel> ActiveElections { get; set; }
        public List<VotingHistoryViewModel> VotingHistory { get; set; }
    }
}
