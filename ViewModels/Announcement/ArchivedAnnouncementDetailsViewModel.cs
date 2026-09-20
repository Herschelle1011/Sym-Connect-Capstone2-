namespace Sym_Connect__Capstone2_.ViewModels.Announcement
{
    public class ArchivedAnnouncementDetailsViewModel
    {
        public int AnnouncementId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public string TargetAudience { get; set; } = string.Empty;

        public string? GroupName { get; set; }

        public bool IsPinned { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public DateTime? ArchiveDate { get; set; }
    }
}
