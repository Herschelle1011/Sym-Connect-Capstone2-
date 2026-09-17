using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class PointsController : Controller
    {
        private readonly IMemberPointsService _memberPointsService;
        private readonly UserManager<User> _userManager;


        public PointsController(
            IMemberPointsService memberPointsService,
            UserManager<User> userManager)
        {
            _memberPointsService = memberPointsService;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }


            // A Member account should have a MemberId
            if (user.MemberId == null)
            {
                return Forbid();
            }


            // Get this member's Points page data
            var model =
                await _memberPointsService
                    .GetMemberPointsAsync(user.MemberId.Value);


            // Send the ViewModel to Index.cshtml
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int attendanceId)
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Make sure this account is connected to a Member
            if (user.MemberId == null)
            {
                return Forbid();
            }

            // Get this attendance record.
            // We also pass MemberId so members cannot
            // view another member's attendance.
            var model = await _memberPointsService
                .GetMemberPointDetailsAsync(
                    attendanceId,
                    user.MemberId.Value);

            if (model == null)
            {
                return NotFound();
            }

            // This will be the modal partial we create next
            return PartialView("_PointDetailsModal", model);
        }
    }
}