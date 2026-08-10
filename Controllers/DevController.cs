using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Sym_Connect__Capstone2_.Data;

namespace Sym_Connect__Capstone2_.Controllers
{
#if DEBUG
    [Route("dev")]  // this overrides the default area routing pattern
    public class DevController : Controller
    {
        private readonly AppDbContext _context;

        public DevController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("set-session")]
        public IActionResult DevSetSession(int memberId)
        {
            HttpContext.Session.SetInt32("CurrentMemberId", memberId);
            return RedirectToAction("Index", "Profile", new {area = "Member"});
        }

        [HttpGet("set-session-leader")]
        public async Task<IActionResult> DevSetSessionLeader(int memberId)
        {
            bool isLeader = await _context.Groups
                .AnyAsync(g => g.LeaderId == memberId && !g.IsArchived);

            if (!isLeader)
            {
                return Content($"MemberId {memberId} is not a Leader of any active Group.");
            }

            HttpContext.Session.SetInt32("CurrentMemberId", memberId);

            return RedirectToAction("Index", "Profile", new { area = "Member" });
        }
    }
#endif
}