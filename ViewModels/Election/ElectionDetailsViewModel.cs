namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class ElectionDetailsViewModel
    {
        public int id { get; set; }
        public string ElectionName { get; set; } = string.Empty;


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public List<PartyListsViewModel> partyLists { get; set; } = new List<PartyListsViewModel>();


    }
}
