using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class GroupController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        public GroupController(IGroupService groupService, UserManager<User> userManager, AppDbContext context)
        {
            _groupService = groupService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? sort = "points_desc")
        {
            var leaderId = await GetCurrentLeaderIdAsync();
            if (leaderId == null) return Forbid();

            var group = await _groupService.GetGroupForLeaderAsync(leaderId.Value, search, sort);
            if (group == null) return NotFound("You are not currently assigned as a leader of any group.");

            return View(group);
        }

        [HttpGet]
        public async Task<IActionResult> RefreshMembers(string? search, string? sort = "points_desc")
        {
            var leaderId = await GetCurrentLeaderIdAsync();
            if (leaderId == null) return Forbid();

            var group = await _groupService.GetGroupForLeaderAsync(leaderId.Value, search, sort);
            if (group == null) return NotFound();

            return PartialView("_LeaderMembersTable", group);
        }

        private async Task<int?> GetCurrentLeaderIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var member = await _context.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == user.Id);

            return member?.MemberId;
        }
    }
}

