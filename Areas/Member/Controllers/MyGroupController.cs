using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class MyGroupController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IGroupService _groupService;

        public MyGroupController(UserManager<User> userManager, IGroupService groupService)
        {
            _userManager = userManager;
            _groupService = groupService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (user.MemberId == null)
                return Forbid();

            var model = await _groupService
                .GetGroupDetailsForMemberAsync(user.MemberId.Value);

            if (model == null)
                return NotFound();

            model.CurrentMemberId = user.MemberId.Value;

            return View(model);
        }
    }
}
