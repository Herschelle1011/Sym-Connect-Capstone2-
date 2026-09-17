using Sym_Connect__Capstone2_.ViewModels.Election;
namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election
{
    public class ElectionIndexViewModel
    {
        public List<GetAllElectionListViewModel> ActiveElections { get; set; } = new();
        public List<VotingHistoryViewModel> VotingHistory { get; set; } = new();
    }
}
