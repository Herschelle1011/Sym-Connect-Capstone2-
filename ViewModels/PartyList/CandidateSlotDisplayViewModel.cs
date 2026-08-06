namespace Sym_Connect__Capstone2_.ViewModels.PartyList
{
    public class CandidateSlotDisplayViewModel
    {
        public CandidateViewModel? Candidate { get; set; }
        public bool IsEmpty => Candidate == null;
    }
}
