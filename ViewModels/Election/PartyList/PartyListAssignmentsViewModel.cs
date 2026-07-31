namespace Sym_Connect__Capstone2_.ViewModels.Election.PartyList
{
    public class PartyListAssignmentsViewModel
    {
            public int PartyListId { get; set; }
            public List<CandidateViewModel> Candidates { get; set; } = new();
        
    }
}
