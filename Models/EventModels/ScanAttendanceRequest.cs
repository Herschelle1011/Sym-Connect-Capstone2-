namespace Sym_Connect__Capstone2_.Models.EventModels
{
    public class ScanAttendanceRequest
    {
        public string QrToken { get; set; } = string.Empty;
        public int EventId { get; set; }
    }

}
