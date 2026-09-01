namespace Sym_Connect__Capstone2_.ViewModels.Notification
{
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }

        public string? Url { get; set; }
    }
}
