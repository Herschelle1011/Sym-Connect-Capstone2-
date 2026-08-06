namespace Sym_Connect__Capstone2_.ViewModels.PartyList
{
    public class GetPartyListPositionViewModel
    {
        public int partyListId { get; set; }
        public string PartyListName { get; set; }
        public List<PartyListPositionItemViewModel> Positions { get; set; }
    }
}
