namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class ScannerViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int PresentCount { get; set; }
        public int TotalMembersCount { get; set; }
        public List<ParticipantCheckInViewModel> Participants { get; set; } = new();
    }
}
