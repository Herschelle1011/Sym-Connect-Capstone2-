using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class ActivityHistoryController : Controller
    {
        private readonly IActivityHistoryService _activityHistoryService;

        private readonly UserManager<User> _userManager;

        public ActivityHistoryController(IActivityHistoryService activityHistoryService,UserManager<User> userManager)
        {
            _activityHistoryService = activityHistoryService;

            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user =
                await _userManager.GetUserAsync(User);

            if (user == null ||
                !user.MemberId.HasValue)
            {
                return Unauthorized();
            }


            var model =
                await _activityHistoryService
                    .GetActivityHistoryAsync(
                        user.MemberId.Value,
                        user.Id);


            return View(model);
        }
    }
}
