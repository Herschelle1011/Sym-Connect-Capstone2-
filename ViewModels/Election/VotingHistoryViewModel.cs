namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class VotingHistoryViewModel
    {
        public int ElectionId { get; set; }
        public string ElectionName { get; set; } = "";

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public DateTime VotedDate { get; set; }   
        public string Status { get; set; } = "";  
        public bool CanViewResults { get; set; }
    }
}
