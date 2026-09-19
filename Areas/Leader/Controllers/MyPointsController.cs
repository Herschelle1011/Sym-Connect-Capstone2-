using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class MyPointsController : Controller
    {
        private readonly IMemberPointsService _memberPointsService;
        private readonly UserManager<User> _userManager;

        public MyPointsController(
            IMemberPointsService memberPointsService,
            UserManager<User> userManager)
        {
            _memberPointsService = memberPointsService;
            _userManager = userManager;
        }


        // =========================================
        // MY POINTS PAGE
        // =========================================

        public async Task<IActionResult> Index()
        {
            // Get the currently logged-in Leader account
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }


            // Leaders are also connected to a Member record.
            // We need the MemberId to get their points.
            if (user.MemberId == null)
            {
                return Forbid();
            }


            // Reuse the same Member Points service
            var model = await _memberPointsService
                .GetMemberPointsAsync(user.MemberId.Value);


            return View(model);
        }


        // =========================================
        // POINT DETAILS MODAL
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Details(int attendanceId)
        {
            // Get the currently logged-in Leader account
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }


            // Make sure the Leader has a Member record
            if (user.MemberId == null)
            {
                return Forbid();
            }



            var model = await _memberPointsService
                .GetMemberPointDetailsAsync(
                    attendanceId,
                    user.MemberId.Value);


            if (model == null)
            {
                return NotFound();
            }


            return PartialView("_PointDetailModal", model);
        }
    }
}