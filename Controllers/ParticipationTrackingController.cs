using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Controllers
{
    public class ParticipationTrackingController : Controller
    {
        private readonly IParticipationTrackingService
            _participationService;

        public ParticipationTrackingController(
            IParticipationTrackingService participationService)
        {
            _participationService =
                participationService;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
        {
            var vm = await _participationService
                    .GetParticipationTrackingAsync(fromDate, toDate);

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupMembers(int groupId, DateTime? 
                                                    fromDate = null, DateTime? toDate = null)
        {
            var members = await _participationService
                .GetMembersAsync(fromDate, toDate, groupId);

            var groupName = members.FirstOrDefault()?.GroupName ?? "Group";

            ViewBag.GroupName = groupName;
            return PartialView("_ViewGetMemberTab", members);
        }
    }
}
