namespace Sym_Connect__Capstone2_.Models.Enums
{
    public class EventResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public string? ConflictingEventTitle { get; set; }

        public static EventResult Ok()
            => new EventResult { Success = true };

        public static EventResult Fail(string message)
            => new EventResult { Success = false, ErrorMessage = message };
    }
}
