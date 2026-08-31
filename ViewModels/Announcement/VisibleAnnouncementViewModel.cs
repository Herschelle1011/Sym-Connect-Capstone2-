using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;

namespace Sym_Connect__Capstone2_.ViewModels.Announcement
{
    public class VisibleAnnouncementViewModel
    {
        public int AnnouncementId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime? PublishedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public TargetAudience TargetType { get; set; }

        public string? GroupName { get; set; }

        public string PostedByName { get; set; } = string.Empty;

        public string PhotoUrl { get; set; } = string.Empty;
    }
}
