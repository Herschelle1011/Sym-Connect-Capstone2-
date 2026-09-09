using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Announcement;
namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public List<VisibleAnnouncementViewModel?> PinnedAnnouncement { get; set; }
        public List<VisibleAnnouncementViewModel> Announcements { get; set; } = new();
    }
}
