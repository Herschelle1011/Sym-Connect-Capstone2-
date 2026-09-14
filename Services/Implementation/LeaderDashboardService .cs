using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Dashboard.Leader;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class LeaderDashboardService : ILeaderDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IEventService _eventService;
        private readonly IAnnouncementService _announcementService;

        public LeaderDashboardService(
            AppDbContext context,
            IEventService eventService,
            IAnnouncementService announcementService)
        {
            _context = context;
            _eventService = eventService;
            _announcementService = announcementService;
        }

        public async Task<LeaderDashboardViewModel?> GetDashboardAsync(int leaderMemberId)
        {
            var leaderGroup = await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.LeaderId == leaderMemberId && !g.IsArchived);

            // Not currently leading any group — dashboard has nothing group-scoped to show.
            if (leaderGroup == null) return null;

            var leader = await _context.Members
                .AsNoTracking()
                .Where(m => m.MemberId == leaderMemberId)
                .Select(m => new { m.FirstName })
                .FirstOrDefaultAsync();

            // ---- Total Members ----
            var totalMembers = await _context.GroupDetails
                .CountAsync(gd => gd.GroupId == leaderGroup.GroupId);

            // ---- Participation Points: sum of EarnedPoints across this group's members ----
            var participationPoints = await _context.GroupDetails
                .Where(gd => gd.GroupId == leaderGroup.GroupId)
                .SumAsync(gd => gd.Member.EarnedPoints);

            // ---- Upcoming Events (reuses existing method — already excludes cancelled/archived) ----
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync(isAdmin: false, currentUserId: leaderMemberId);

            // ---- Pending Request: leader's own event submissions awaiting approval ----
            var pendingRequestCount = await _context.Events
                .CountAsync(e => e.CreatedByUserId == leaderMemberId
                               && !e.IsArchived
                               && !e.IsCancelled
                               && e.ApprovalStatus == ApprovalStatus.Pending);

            // ---- Pinned Announcements (Admin pin + this group's own Leader pin) ----
            var pinnedAnnouncements = await _announcementService.GetPinnedAnnouncementsAsync(
                isLeader: true, groupId: leaderGroup.GroupId);

            // ---- Group Attendance chart ----
            var groupAttendanceChart = await GetGroupAttendanceChartAsync(leaderGroup.GroupId);

            // ---- Submission Status: Event half only for now (see note on Media) ----
            var eventSubmissions = await _context.Events
                .Where(e => e.CreatedByUserId == leaderMemberId && !e.IsArchived)
                .OrderByDescending(e => e.DateCreated)
                .Take(5)
                .Select(e => new SubmissionStatusItemViewModel
                {
                    Name = e.EventTitle,
                    Category = "Event",
                    Status = e.ApprovalStatus.ToString()
                })
                .ToListAsync();

            // TODO: append Media submissions here once the Media model/service is confirmed —
            // same shape (Name, Category = "Media", Status), then re-sort the combined list
            // by date if you want interleaving rather than Events-then-Media.

            return new LeaderDashboardViewModel
            {
                LeaderFirstName = leader?.FirstName ?? "",
                TotalMembers = totalMembers,
                ParticipationPoints = participationPoints,
                UpcomingActivitiesCount = upcomingEvents.Count,
                PendingRequestCount = pendingRequestCount,
                PinnedAnnouncements = pinnedAnnouncements,
                UpcomingEvents = upcomingEvents.Take(3).ToList(),
                GroupAttendanceChart = groupAttendanceChart,
                SubmissionStatuses = eventSubmissions
            };
        }

        private async Task<List<GroupAttendanceChartItemViewModel>> GetGroupAttendanceChartAsync(int groupId)
        {
            var now = DateTime.Now;

            // Last 5 events this group was expected at, that have already ended.
            // NOTE: this check is date-only (EndDate < today), not time-of-day —
            // an event ending later today would still count as "not yet completed"
            // and be excluded, which is the safer direction for a chart, but flag
            // if you want end-time precision instead.
            var recentEvents = await _context.EventGroups
                .Where(eg => eg.GroupId == groupId
                          && eg.Event.EndDate.Date < now.Date
                          && !eg.Event.IsArchived
                          && !eg.Event.IsCancelled)
                .OrderByDescending(eg => eg.Event.EndDate)
                .Select(eg => new { eg.EventId, eg.Event.EventTitle, eg.Event.EndDate })
                .Take(5)
                .ToListAsync();

            var groupMemberIds = await _context.GroupDetails
                .Where(gd => gd.GroupId == groupId)
                .Select(gd => gd.MemberId)
                .ToListAsync();

            var chartData = new List<GroupAttendanceChartItemViewModel>();

            // Re-sort chronologically (oldest → newest) so the chart reads left-to-right correctly
            foreach (var evt in recentEvents.OrderBy(e => e.EndDate))
            {
                var presentCount = await _context.Attendances
                    .CountAsync(a => a.EventId == evt.EventId
                                   && groupMemberIds.Contains(a.MemberId)
                                   && (a.Status == "Present" || a.Status == "Late"));

                chartData.Add(new GroupAttendanceChartItemViewModel
                {
                    EventTitle = evt.EventTitle,
                    PresentCount = presentCount
                });
            }

            return chartData;
        }
    }
}
