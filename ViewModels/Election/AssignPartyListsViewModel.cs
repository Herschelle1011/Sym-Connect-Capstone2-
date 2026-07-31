namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class AssignPartyListsViewModel
    {
        public int electionId { get; set; }
        public string ElectionName { get; set; } = string.Empty;
        public List<PartyListsViewModel> AvailablePartyLists { get; set; } = new();
    }
}
