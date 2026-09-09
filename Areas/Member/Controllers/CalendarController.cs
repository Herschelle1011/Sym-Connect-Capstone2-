using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.Services.Implementation;
using Microsoft.AspNetCore.Authorization;


namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class CalendarController : Controller
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? year, int? month, string activeTab = "MyGroupEvents")
        {
            var memberId = HttpContext.Session.GetInt32("CurrentMemberId");
            if (memberId == null)
            {
                //to be change Login Part ----- 
                return RedirectToAction("DevSetSession", "Dev"); 
            }

            var today = DateTime.Today;
            var vm = await _calendarService.GetCalendarPageAsync(
                memberId.Value,
                year ?? today.Year,
                month ?? today.Month,
                activeTab
            );

            return View(vm);
        }
    }
}
