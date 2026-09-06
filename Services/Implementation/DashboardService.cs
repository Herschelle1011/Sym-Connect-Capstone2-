using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Dashboard;

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



       
    }
}
