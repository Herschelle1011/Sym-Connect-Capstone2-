
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class WeeklyReportService : IWeeklyReportService
    {
        private readonly AppDbContext _context;

        public WeeklyReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WeeklyReportViewModel> GetWeeklyReportAsync(DateTime weekStart, DateTime weekEnd)
        {
            weekStart = weekStart.Date;
            weekEnd = weekEnd.Date.AddDays(1).AddTicks(-1); // make end-of-day inclusive

            var vm = new WeeklyReportViewModel { WeekStart = weekStart, WeekEnd = weekEnd };

            var eventsThisWeek = await _context.Events
                .AsNoTracking()
                .Where(e => !e.IsArchived && e.StartDate >= weekStart && e.StartDate <= weekEnd)
                .Select(e => new { e.EventId, e.StartDate, e.StartTime, e.EndDate, e.EndTime, e.ApprovalStatus })
                .ToListAsync();

            vm.TotalActivities = eventsThisWeek.Count;

            var now = DateTime.Now;
            foreach (var e in eventsThisWeek)
            {
                if (e.ApprovalStatus == ApprovalStatus.Rejected)
                {
                    vm.ActivitySummary.CancelledCount++;
                    continue;
                }

                var start = e.StartDate.Date + (e.StartTime ?? TimeSpan.Zero);
                var end = e.EndDate.Date + (e.EndTime ?? TimeSpan.Zero);

                if (now < start) vm.ActivitySummary.UpcomingCount++;
                else if (now <= end) vm.ActivitySummary.OngoingCount++;
                else vm.ActivitySummary.CompletedCount++;
            }

            // ---- Total attendance + participation points (Present/Late within this week's events) ----
            var eventIds = eventsThisWeek.Select(e => e.EventId).ToList();

            var attendanceRecords = await _context.Attendances
                .AsNoTracking()
                .Where(a => eventIds.Contains(a.EventId) && (a.Status == "Present" || a.Status == "Late"))
                .Select(a => new { a.EventId, a.TimeIn, a.Points })
                .ToListAsync();

            vm.TotalAttendance = attendanceRecords.Count;
            vm.ParticipationPoints = attendanceRecords.Sum(a => a.Points);

            // ---- Attendance by day (Mon–Sun line chart) ----
            var eventDateLookup = eventsThisWeek.ToDictionary(e => e.EventId, e => e.StartDate.Date);
            for (int i = 0; i < 7; i++)
            {
                var day = weekStart.AddDays(i);
                var count = attendanceRecords.Count(a => eventDateLookup.TryGetValue(a.EventId, out var d) && d == day);

                vm.AttendanceByDay.Add(new DayAttendanceViewModel
                {
                    Date = day,
                    DayLabel = day.ToString("ddd"),
                    Count = count
                });
            }

            // ---- Top participating groups ----
            // DECISION POINT: this uses each member's lifetime EarnedPoints summed per group, not points
            // earned specifically within this week. If you want a week-scoped leaderboard, this needs to
            // join against PointsLedger (per your "single source of truth" design) filtered to weekStart/weekEnd
            // instead of summing Member.EarnedPoints — let me know if that's what you actually want.
            vm.TopGroups = await _context.Groups
                .AsNoTracking()
                .Where(g => !g.IsArchived)
                .Select(g => new TopGroupViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    Points = g.GroupDetails.Sum(gd => (int?)gd.Member.EarnedPoints) ?? 0
                })
                .OrderByDescending(g => g.Points)
                .Take(5)
                .ToListAsync();

            // ---- Evaluations submitted this week ----
            // ASSUMPTION: Evaluation has DateCreated (DateTime) and Rating (int, 1-5) — I haven't seen this
            // model directly, only IEvaluationService.GetEventFinalRatingAsync referenced from EventService.
            // Adjust field names/types to match your actual Evaluation entity.
            var weeklyRatingAnswers = await _context.EvaluationResponses
              .AsNoTracking()
              .Where(r =>
                  !r.EvaluationSubmission.Evaluation.IsArchived &&
                  r.EvaluationSubmission.Status == SubmissionStatus.Submitted &&
                  r.EvaluationSubmission.SubmittedAt >= weekStart &&
                  r.EvaluationSubmission.SubmittedAt <= weekEnd &&
                  r.EvaluationDetail.EvaluationQuestion.AnswerType == AnswerType.RatingScale)
              .Select(r => r.Answer)
              .ToListAsync();

            var numericRatings = weeklyRatingAnswers
                .Where(a => int.TryParse(a, out _))
                .Select(a => int.Parse(a!))
                .ToList();

            vm.EvaluationSummary.FiveStar = numericRatings.Count(r => r == 5);
            vm.EvaluationSummary.FourStar = numericRatings.Count(r => r == 4);
            vm.EvaluationSummary.ThreeStar = numericRatings.Count(r => r == 3);
            vm.EvaluationSummary.TwoStar = numericRatings.Count(r => r == 2);
            vm.EvaluationSummary.OneStar = numericRatings.Count(r => r == 1);

            vm.EvaluationsSubmitted = await _context.EvaluationSubmissions
                .AsNoTracking()
                .CountAsync(s =>
                    !s.Evaluation.IsArchived &&
                    s.Status == SubmissionStatus.Submitted &&
                    s.SubmittedAt >= weekStart &&
                    s.SubmittedAt <= weekEnd);

            // ---- Announcements this week ----
            // ASSUMPTION: Announcement has DateCreated, Title, AnnouncementId — same caveat as above.
            vm.AnnouncementsCount = await _context.Announcements
                .CountAsync(a => a.CreatedDate >= weekStart && a.CreatedDate <= weekEnd);

            vm.RecentAnnouncements = await _context.Announcements
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedDate)
                .Take(4)
                .Select(a => new AnnouncementSummaryViewModel
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    DateCreated = a.CreatedDate
                })
                .ToListAsync();

            return vm;
        }
    }
}
