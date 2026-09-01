using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models.Notification
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public string? UserId { get; set; }

        public string? Url { get; set; }
    }
}
