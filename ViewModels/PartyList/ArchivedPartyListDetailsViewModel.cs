namespace Sym_Connect__Capstone2_.ViewModels.PartyList
{
    public class ArchivedPartyListDetailsViewModel
    {
        public int PartyListId { get; set; }

        public string PartyName { get; set; } = string.Empty;

        public string PartyListDescription { get; set; } = string.Empty;

        public int CandidateCount { get; set; }

        public List<ArchivedPartyListCandidateViewModel> Candidates { get; set; }
            = new();

        public DateTime? ArchiveDate { get; set; }
    }

    public class ArchivedPartyListCandidateViewModel
    {
        public int CandidateId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
    }
}
