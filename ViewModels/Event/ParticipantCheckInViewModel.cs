namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class ParticipantCheckInViewModel
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime? CheckedInAt { get; set; }
    }
}
