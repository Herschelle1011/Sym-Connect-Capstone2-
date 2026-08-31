using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;

namespace Sym_Connect__Capstone2_.ViewModels.Announcement
{
    public class DetailsAnnouncementViewModel
    {
        public int AnnouncementId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public TargetAudience TargetAudience { get; set; }

        public string? GroupName { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
