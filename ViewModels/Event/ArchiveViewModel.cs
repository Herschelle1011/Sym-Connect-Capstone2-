namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class ArchiveViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public List<int>? FacilityId { get; set; } = new List<int>();


        
    }
}
