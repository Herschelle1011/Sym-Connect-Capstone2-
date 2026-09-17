
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
namespace Sym_Connect__Capstone2_.Areas.Member.Services.Implementation
{
    public class MemberDashboardService : IMemberDashboardService
    {
        private readonly AppDbContext _context;

        public MemberDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetMemberDashboardAsync(int memberId)
        {
            var now = DateTime.Now;
            var today = now.Date;

            // =====================================================
            // 1. GET CURRENT MEMBER
            // =====================================================
            var member = await _context.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
            {
                return new DashboardViewModel
                {
                    MemberName = "Member"
                };
            }

            // =====================================================
            // 2. GET MEMBER'S GROUP IDS
            // =====================================================
            var memberGroupIds = await _context.GroupDetails
                .AsNoTracking()
                .Where(gd => gd.MemberId == memberId)
                .Select(gd => gd.GroupId)
                .ToListAsync();

            // =====================================================
            // 3. LIVE / HAPPENING TODAY
            // =====================================================
            var todayEventsData = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Approved &&
                    e.StartDate.Date <= today &&
                    e.EndDate.Date >= today &&

                    // Event must belong to one of this member's groups
                    e.ExpectedGroups.Any(eg =>
                        memberGroupIds.Contains(eg.GroupId))
                )
                .OrderBy(e => e.StartTime)
                .Select(e => new
                {
                    e.EventId,
                    e.EventTitle,
                    e.IsAllDay,
                    e.StartDate,
                    e.EndDate,
                    e.StartTime,
                    e.EndTime,

                    HostedBy = e.CreatedByUser != null
                        ? e.CreatedByUser.FirstName + " " +
                          e.CreatedByUser.LastName
                        : "Admin",

                    FacilityName = e.ExpectedFacilities
                        .Select(ef => ef.Facility.FacilityName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var todayEvents = todayEventsData
                .Select(e => new TodayEventViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    IsAllDay = e.IsAllDay,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,

                    FacilityName =
                        e.FacilityName ?? "No facility",

                    HostedBy = e.HostedBy,

                    TimeStatus = GetEventTimeStatus(
                        e.StartDate,
                        e.EndDate,
                        e.StartTime,
                        e.EndTime,
                        e.IsAllDay,
                        now)
                })
                .ToList();

            // =====================================================
            // 4. UPCOMING EVENTS
            // =====================================================
            var upcomingEventData = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Approved &&

                    (
                        e.StartDate.Date > today ||

                        (
                            e.StartDate.Date == today &&
                            (
                                e.IsAllDay ||
                                e.StartTime == null ||
                                e.StartTime > now.TimeOfDay
                            )
                        )
                    ) &&

                    e.ExpectedGroups.Any(eg =>
                        memberGroupIds.Contains(eg.GroupId))
                )
                .OrderBy(e => e.StartDate)
                .ThenBy(e => e.StartTime)
                .Take(3)
                .Select(e => new UpcomingEventViewModel
                {
                    EventId = e.EventId,

                    EventTitle = e.EventTitle,

                    IsAllDay = e.IsAllDay,

                    StartDate = e.StartDate,

                    EndDate = e.EndDate,

                    StartTime = e.StartTime,

                    EndTime = e.EndTime,

                    FacilityName = e.ExpectedFacilities
                        .Select(ef => ef.Facility.FacilityName)
                        .FirstOrDefault() ?? "No facility",

                    CreatedByName = e.CreatedByUser != null
                        ? e.CreatedByUser.FirstName + " " +
                          e.CreatedByUser.LastName
                        : "Admin"
                })
                .ToListAsync();

            // =====================================================
            // 5. MEMBERSHIP INFORMATION
            // =====================================================
            var membership = await _context.GroupDetails
                .AsNoTracking()
                .Where(gd =>
                    gd.MemberId == memberId &&
                    gd.Group != null &&
                    !gd.Group.IsArchived)
                .Select(gd => new MembershipSummaryViewModel
                {
                    GroupId = gd.GroupId,

                    GroupName = gd.Group!.GroupName,

                    GroupDescription = gd.Group.Description,

                    MemberCount = gd.Group.GroupDetails
                        .Count(x =>
                            x.Member != null &&
                            x.Member.Status == "Active")
                })
                .FirstOrDefaultAsync();

            membership ??= new MembershipSummaryViewModel
            {
                GroupName = "No Group",
                GroupDescription = "Not assigned to a group",
                MemberCount = 0
            };

            // =====================================================
            // 6. TOTAL EVENTS MEMBER SHOULD HAVE ATTENDED
            // =====================================================
            var totalCompletedEvents = await _context.Events
                .AsNoTracking()
                .CountAsync(e =>
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Approved &&

                    // Event already finished
                    (
                        e.EndDate.Date < today ||

                        (
                            e.EndDate.Date == today &&
                            e.EndTime != null &&
                            e.EndTime < now.TimeOfDay
                        )
                    ) &&

                    e.ExpectedGroups.Any(eg =>
                        memberGroupIds.Contains(eg.GroupId))
                );

            // =====================================================
            // 7. EVENTS ATTENDED
            // =====================================================
            var attendedEvents = await _context.Attendances
                .AsNoTracking()
                .Where(a =>
                    a.MemberId == memberId &&
                    (
                        a.Status == "Present" ||
                        a.Status == "Late"
                    ))
                .Select(a => a.EventId)
                .Distinct()
                .CountAsync();

            // =====================================================
            // 8. ATTENDANCE PERCENTAGE
            // =====================================================
            var attendancePercentage = 0;

            if (totalCompletedEvents > 0)
            {
                attendancePercentage = (int)Math.Round(
                    (double)attendedEvents /
                    totalCompletedEvents * 100
                );
            }

            // Protect against percentage accidentally going over 100
            attendancePercentage =
                Math.Clamp(attendancePercentage, 0, 100);

            var attendanceProgress =
                new AttendanceProgressViewModel
                {
                    AttendedEvents = attendedEvents,

                    TotalEvents = totalCompletedEvents,

                    AttendancePercentage = attendancePercentage,

                    RemainingForBadge = Math.Max(
                        totalCompletedEvents - attendedEvents,
                        0)
                };

            // =====================================================
            // 9. RETURN DASHBOARD
            // =====================================================
            return new DashboardViewModel
            {
                MemberName = member.FirstName,

                TodayEvents = todayEvents,

                UpcomingEvents = upcomingEventData,

                Membership = membership,

                AttendanceProgress = attendanceProgress,

                // We'll connect this next once we use your
                // announcement service/model.
                PinnedAnnouncement = new()
            };
        }

        // =========================================================
        // EVENT TIME TEXT
        // =========================================================
        private static string GetEventTimeStatus(
            DateTime startDate,
            DateTime endDate,
            TimeSpan? startTime,
            TimeSpan? endTime,
            bool isAllDay,
            DateTime now)
        {
            if (isAllDay)
            {
                return "All day";
            }

            if (!startTime.HasValue)
            {
                return string.Empty;
            }

            var eventStart =
                startDate.Date.Add(startTime.Value);

            DateTime? eventEnd = null;

            if (endTime.HasValue)
            {
                eventEnd =
                    endDate.Date.Add(endTime.Value);
            }

            // Event hasn't started yet
            if (now < eventStart)
            {
                var difference = eventStart - now;

                if (difference.TotalMinutes < 60)
                {
                    var minutes =
                        Math.Max(1, (int)difference.TotalMinutes);

                    return $"Starts in {minutes} min" +
                           (minutes == 1 ? "" : "s");
                }

                if (difference.TotalHours < 24)
                {
                    var hours =
                        Math.Max(1, (int)difference.TotalHours);

                    return $"Starts in {hours} hr" +
                           (hours == 1 ? "" : "s");
                }

                return "Starts later";
            }

            // Event is currently happening
            if (!eventEnd.HasValue || now <= eventEnd.Value)
            {
                var difference = now - eventStart;

                if (difference.TotalMinutes < 60)
                {
                    var minutes =
                        Math.Max(1, (int)difference.TotalMinutes);

                    return $"Started {minutes} min" +
                           (minutes == 1 ? "" : "s") +
                           " ago";
                }

                var hours =
                    Math.Max(1, (int)difference.TotalHours);

                return $"Started {hours} hr" +
                       (hours == 1 ? "" : "s") +
                       " ago";
            }

            return "Ended";
        }
    }
}

