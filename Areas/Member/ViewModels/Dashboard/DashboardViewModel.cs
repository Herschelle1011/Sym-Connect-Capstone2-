using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Announcement;
namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        // Logged-in member
        public string MemberName { get; set; } = string.Empty;

        // Events happening today
        public List<TodayEventViewModel> TodayEvents { get; set; }
            = new List<TodayEventViewModel>();

        // Pinned announcements
        public List<VisibleAnnouncementViewModel> PinnedAnnouncement { get; set; }
            = new List<VisibleAnnouncementViewModel>();

        // Future events
        public List<UpcomingEventViewModel> UpcomingEvents { get; set; }
            = new List<UpcomingEventViewModel>();

        // Member's group information
        public MembershipSummaryViewModel Membership { get; set; }
            = new MembershipSummaryViewModel();

        // Member attendance progress
        public AttendanceProgressViewModel AttendanceProgress { get; set; }
            = new AttendanceProgressViewModel();
    }
}
