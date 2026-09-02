namespace Sym_Connect__Capstone2_.ViewModels.Announcement
{
    public class ArchiveAnnouncementViewModel
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
