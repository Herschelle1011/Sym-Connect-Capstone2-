using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Calendar;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Implementation
{
    public class CalendarService : ICalendarService
    {
        private readonly AppDbContext _context;

        public CalendarService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MemberCalendarPageViewModel> GetCalendarPageAsync(
            int memberId, int year, int month, string activeTab)
        {
            var myGroupId = await _context.GroupDetails.Where(m => m.MemberId == memberId)
                                .Select(gd => gd.GroupId)
                                .ToListAsync();

            var myGroupName = await _context.Groups
                .Where(gd => myGroupId.Contains(gd.GroupId) && !gd.IsArchived)
                .Select(g => g.GroupName).FirstOrDefaultAsync();

            var firstOfMonth = new DateTime(year, month, 1);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
            var gridStart = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
            var gridEnd = lastOfMonth.AddDays(6 - (int)lastOfMonth.DayOfWeek);

            //my group event Tab --- 
            var myGroupEvents = await _context.Events
               .AsNoTracking()
               .Where(e => !e.IsArchived
                   && e.StartDate.Date <= gridEnd && e.EndDate.Date >= gridStart
                   && e.ExpectedGroups.Any(eg => myGroupId.Contains(eg.GroupId)))
               .Select(e => new
               {
                   e.EventId,
                   e.EventTitle,
                   e.IsAllDay,
                   e.StartDate,
                   e.EndDate,
                   e.StartTime,
                   e.EndTime,
                   FacilityName = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                   GroupName = e.ExpectedGroups
                       .Where(eg => myGroupId.Contains(eg.GroupId))
                       .Select(eg => eg.Group.GroupName)
                       .FirstOrDefault()
               })
               .ToListAsync();

            //attended events Tab --- 

            var attendedEvents = await _context.Attendances
               .AsNoTracking()
               .Where(a => a.MemberId == memberId
                   && a.Event.StartDate.Date <= gridEnd && a.Event.EndDate.Date >= gridStart)
               .Select(a => new
               {
                   a.Event.EventId,
                   a.Event.EventTitle,
                   a.Event.IsAllDay,
                   a.Event.StartDate,
                   a.Event.EndDate,
                   a.Event.StartTime,
                   a.Event.EndTime
               })
               .ToListAsync();

            var now = DateTime.Now;

            List<CalendarEventsViewModel> tabEvents = activeTab == "Attended"
                ? attendedEvents.Select(e => new CalendarEventsViewModel
                {
                    eventId = e.EventId,
                    EventTitle = e.EventTitle,
                    GroupName = null,
                    isAllDay = e.IsAllDay,
                    StartTime = e.StartDate.Date + (e.StartTime ?? TimeSpan.Zero),
                    EndTime = e.EndDate.Date + (e.EndTime ?? TimeSpan.Zero),
                    IsMyGroupEvent = false,
                    Status = ComputeMemberEventStatus(e.StartDate, e.EndDate, e.StartTime, e.EndTime, now)
                }).ToList()
                : myGroupEvents.Select(e => new CalendarEventsViewModel
                {
                    eventId = e.EventId,
                    EventTitle = e.EventTitle,
                    GroupName = e.GroupName,
                    isAllDay = e.IsAllDay,
                    StartTime = e.StartDate.Date + (e.StartTime ?? TimeSpan.Zero),
                    EndTime = e.EndDate.Date + (e.EndTime ?? TimeSpan.Zero),
                    IsMyGroupEvent = true,
                    Status = ComputeMemberEventStatus(e.StartDate, e.EndDate, e.StartTime, e.EndTime, now)
                }).ToList();

            var days = new List<CalendarDayViewModel>();
            for (var date = gridStart; date <= gridEnd; date = date.AddDays(1))
            {
                days.Add(new CalendarDayViewModel
                {
                    Date = date,
                    IsCurrentMonth = date.Month == month,
                    IsToday = date.Date == DateTime.Today,
                    Events = tabEvents.Where(ev => ev.StartTime.Date <= date && ev.EndTime.Date >= date).ToList()
                });
            }

            var todayEvent = myGroupEvents
               .Where(e => e.StartDate.Date <= DateTime.Today && e.EndDate.Date >= DateTime.Today)
               .OrderBy(e => e.StartDate.Date + (e.StartTime ?? TimeSpan.Zero))
               .FirstOrDefault();

            var todayPanel = new TodayEventPanelViewModel { HasEventToday = false };
            if (todayEvent != null)
            {
                var eventStart = todayEvent.StartDate.Date + (todayEvent.StartTime ?? TimeSpan.Zero);
                var eventEnd = todayEvent.EndDate.Date + (todayEvent.EndTime ?? new TimeSpan(23, 59, 59));

                todayPanel = new TodayEventPanelViewModel
                {
                    HasEventToday = true,
                    EventId = todayEvent.EventId,
                    EventTitle = todayEvent.EventTitle,
                    GroupName = todayEvent.GroupName,
                    IsLiveNow = now >= eventStart && now <= eventEnd,
                    EndTime = eventEnd
                };
            }

            //upcoming related only to my groupEvents
            var upcoming = myGroupEvents
               .Where(e => e.StartDate.Date + (e.StartTime ?? TimeSpan.Zero) > now)
               .OrderBy(e => e.StartDate.Date + (e.StartTime ?? TimeSpan.Zero))
               .Take(3)
               .Select(e => new UpcomingEventViewModel
               {
                   EventId = e.EventId,
                   EventTitle = e.EventTitle,
                   FacilityName = e.FacilityName,
               })
               .ToList();

            return new MemberCalendarPageViewModel
            {
                Year = year,
                Month = month,
                ActiveTab = activeTab,
                GroupName = myGroupName,
                Days = days,
                TodayPanel = todayPanel,
                UpcomingEvents = upcoming
            };

        }

        private MemberEventStatus ComputeMemberEventStatus(
           DateTime startDate, DateTime endDate, TimeSpan? startTime, TimeSpan? endTime, DateTime now)
        {
            var start = startDate.Date + (startTime ?? TimeSpan.Zero);
            var end = endDate.Date + (endTime ?? new TimeSpan(23, 59, 59));

            if (now < start) return MemberEventStatus.Upcoming;
            if (now <= end) return MemberEventStatus.Live;
            return MemberEventStatus.Completed;
        }

    }
}
