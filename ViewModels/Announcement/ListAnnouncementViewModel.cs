using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;

namespace Sym_Connect__Capstone2_.ViewModels.Announcement
{
    public class ListAnnouncementViewModel
    {
        public int AnnouncementId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public TargetAudience TargetType { get; set; }

        public string? GroupName { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public PublishOption PublishOption { get; set; }
        public DateTime? PublishDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsArchived { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public bool IsPinned { get; set; } = false;
    }
}
