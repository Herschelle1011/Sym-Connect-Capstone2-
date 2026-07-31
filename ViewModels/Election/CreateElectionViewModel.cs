using Sym_Connect__Capstone2_.ViewModels.Election.PartyList;

namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class CreateElectionViewModel : FormElectionViewModel
    {
        public List<int> SelectedPartyListIds { get; set; } = new();
        public List<PartyListAssignmentsViewModel> PartyListAssignments { get; set; } = new();
    }
}
