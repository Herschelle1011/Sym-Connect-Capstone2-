using Sym_Connect__Capstone2_.ViewModels.Announcement;

namespace Sym_Connect__Capstone2_.Areas.Leader.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public VisibleAnnouncementViewModel? PinnedAnnouncement { get; set; }
        public List<VisibleAnnouncementViewModel> Announcements { get; set; } = new();
    }
}
