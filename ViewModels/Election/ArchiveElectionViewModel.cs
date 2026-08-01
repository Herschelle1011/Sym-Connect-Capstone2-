namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class ArchiveElectionViewModel
    {
        public int electionId { get; set; }
        public string electionTitle { get; set; } = string.Empty;

        public string electionStatus { get; set; } = string.Empty;  // e.g. "Open", "Closed"

        public int TotalVotes { get; set; }
        public int TotalCandidates { get; set; }
        public List<string> PositionsContested { get; set; } = new(); // e.g. ["President", "VP"]

        public bool HasVotes => TotalVotes > 0;

    }
}
