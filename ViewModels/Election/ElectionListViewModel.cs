namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class ElectionListViewModel
    {
        public int ElectionId { get; set; }
        public string ElectionName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string Status { get; set; } = string.Empty;


    }
}
