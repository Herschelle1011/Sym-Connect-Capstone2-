using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<int> GetAnnouncementCountThisWeekAsync(DateTime start, DateTime end)
        {
            return await _context.Announcements
                .CountAsync(a => a.PublishDate >= start && a.PublishDate < end);
        }
        public async Task<int> GetEvaluationCountThisWeekAsync(DateTime start, DateTime end)
        {
            return await _context.EvaluationSubmissions
                .CountAsync(s => s.SubmittedAt >= start && s.SubmittedAt < end);
        }

        public async Task<int> GetParticipationPointsThisWeekAsync(DateTime start, DateTime end)
        {
            return await _context.Attendances
                .Where(a => a.TimeOut.HasValue
                         && a.TimeIn.HasValue
                         && a.TimeIn.Value >= start
                         && a.TimeIn.Value < end)
                .SumAsync(a => a.Points);
        }


        public async Task<List<UpcomingEventViewModel>> GetUpcomingEventsAsync(int take = 3)
        {
            var now = DateTime.Now;

            var events = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived &&
                    e.ApprovalStatus == ApprovalStatus.Approved &&
                    e.StartDate.Date >= now.Date)
                .OrderBy(e => e.StartDate)
                .ThenBy(e => e.StartTime)
                .Take(take)
                .Select(e => new
                {
                    e.StartDate,
                    e.StartTime,
                    e.EventTitle,

                    Location = e.ExpectedFacilities
                        .Select(f => f.Facility.FacilityName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return events
                .Select(e => new UpcomingEventViewModel
                {
                    Day = e.StartDate.Day,

                    Month = e.StartDate.ToString("MMM"),

                    Title = e.EventTitle,

                    Time = e.StartTime.HasValue
                        ? DateTime.Today
                            .Add(e.StartTime.Value)
                            .ToString("h:mm tt")
                        : "All Day",

                    Location = e.Location ?? "TBA"
                })
                .ToList();
        }

        public async Task<int> GetActivitiesCountThisWeekAsync(DateTime start, DateTime end)
        {
            return await _context.Events
                .CountAsync(e => !e.IsArchived
                               && e.ApprovalStatus == ApprovalStatus.Approved
                               && e.StartDate >= start && e.StartDate < end);
        }

        public async Task<int> GetAttendanceCountThisWeekAsync(DateTime start, DateTime end)
        {
            return await _context.Attendances
                .CountAsync(a => a.TimeOut.HasValue
                               && a.TimeIn.HasValue
                               && a.TimeIn.Value >= start
                               && a.TimeIn.Value < end);
        }


        public async Task<(int Count, DateTime? LatestDate)> GetPendingMembershipApplicationsAsync()
        {
            var query = _context.Members
                .Where(a => a.ApplicationStatus == MembershipApplicationStatus.Pending);

            var count = await query.CountAsync();
            var latest = await query.OrderByDescending(a => a.DateApplied)
                .Select(a => (DateTime?)a.DateRegistered)
                .FirstOrDefaultAsync();

            return (count, latest);
        }

        public async Task<(int Count, DateTime? LatestDate)> GetPendingEventRequestsAsync()
        {
            var query = _context.Events.Where(e => e.ApprovalStatus == ApprovalStatus.Pending && !e.IsArchived);

            var count = await query.CountAsync();
            var latest = await query.OrderByDescending(e => e.DateCreated)
                .Select(e => (DateTime?)e.DateCreated)
                .FirstOrDefaultAsync();

            return (count, latest);
        }

        // TODO: Media/Documents module isn't built yet — placeholder returning 0
        // until that feature exists. Wire this up to the real entity once it's ready.
        public async Task<(int Count, DateTime? LatestDate)> GetPendingMediaFilesAsync()
        {
            var query = _context.MediaFiles.Where(m => m.MediaFileStatus == MediaFileStatus.Pending);

            var count = await query.CountAsync();
            var latest = await query.OrderByDescending(m => m.UploadDate)
                .Select(m => (DateTime?)m.UploadDate)
                .FirstOrDefaultAsync();

            return (count, latest);
        }

        public async Task<List<RecentRegistrationViewModel>> GetRecentApprovedMembersAsync(int take = 5)
        {
            return await _context.Members
                .Where(a => a.ApplicationStatus == MembershipApplicationStatus.Approved)
                .OrderByDescending(a => a.DateApplied)
                .Take(take)
                .Select(a => new RecentRegistrationViewModel
                {
                    MemberId = a.MemberId,
                    FullName = a.FirstName + " " + a.LastName,
                    PhotoUrl = a.User.ProfilePictureUrl,
                    ApprovedDate = a.DateApplied ?? a.DateRegistered
                })
                .ToListAsync();
        }


        public async Task<List<EventListItemViewModel>> GetRecentEventRequestsAsync(int take = 5)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => !e.IsArchived)
                .OrderByDescending(e => e.DateCreated)
                .Take(take)
                .Select(e => new EventListItemViewModel
                {
                    EventTitle = e.EventTitle,
                    GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList(), 
                    SubmittedByName = e.CreatedByUser.FullName ?? "",           
                    DateCreated = e.DateCreated,
                    ApprovalStatus = e.ApprovalStatus
                })
                .ToListAsync();
        }

        public async Task<int> GetExpiredEventCountAsync()
        {
            return await _context.Events
                .CountAsync(e =>
                    !e.IsArchived &&
                    e.ApprovalStatus == ApprovalStatus.Expired);
        }


    }
}
