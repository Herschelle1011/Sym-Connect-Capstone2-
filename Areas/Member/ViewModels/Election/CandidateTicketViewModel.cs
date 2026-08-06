namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election
{
    public class CandidateTicketViewModel
    {
        public int PartyListId { get; set; }
        public string PartyListName { get; set; } = string.Empty;
        public List<CandidateOption> candidates { get; set; } = new();

    }
}
