using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Controllers
{
    [Authorize(Roles = "Admin")]

    public class WeeklySummaryReportController : Controller
    {
        private readonly IWeeklyReportService _weeklyReportService;

        public WeeklySummaryReportController(IWeeklyReportService weeklyReportService)
        {
            _weeklyReportService = weeklyReportService;
        }

        // GET: /WeeklySummaryReport?startDate=2026-09-07&endDate=2026-09-13
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var (start, end) = ResolveRange(startDate, endDate);

            if (start > end)
            {
                (start, end) = (end, start); // swap if the user picked them backwards
            }

            var report = await _weeklyReportService.GetWeeklyReportAsync(start, end);

            // So the view's date inputs show what's actually being displayed,
            // whether the user picked it or it fell back to the default.
            ViewBag.SelectedStartDate = start;
            ViewBag.SelectedEndDate = end;

            return View(report);
        }

        // No selection yet → default to the current Monday–Sunday week.
        // Any explicit selection (even just one side) is respected as-is.
        private static (DateTime start, DateTime end) ResolveRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return (startDate.Value.Date, endDate.Value.Date);
            }

            var today = DateTime.Today;
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7;
            var monday = today.AddDays(-diffToMonday);
            var sunday = monday.AddDays(6);

            return (startDate?.Date ?? monday, endDate?.Date ?? sunday);
        }
    }
}