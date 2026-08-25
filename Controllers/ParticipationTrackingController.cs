using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Index()
        {
            var vm = await _participationService
                    .GetParticipationTrackingAsync();

            return View(vm);
        }
    }
}
