namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class ArchivedElectionDetailsViewModel
    {
        public int ElectionId { get; set; }

        public string ElectionName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public List<string> PartyListNames { get; set; } = new();

        public int TotalPartyLists { get; set; }

        public int TotalCandidates { get; set; }

        public int TotalVotes { get; set; }

        public int VoteCount { get; set; }

        public DateTime? ArchiveDate { get; set; }
    }
}
