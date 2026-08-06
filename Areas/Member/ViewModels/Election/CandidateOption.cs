namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election
{
    public class CandidateOption
    {
        public int CandidateId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }
}
