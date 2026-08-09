using Microsoft.AspNetCore.Mvc;

namespace Sym_Connect__Capstone2_.Controllers
{
#if DEBUG
    [Route("dev")]  // this overrides the default area routing pattern
    public class DevController : Controller
    {
        [HttpGet("set-session")]
        public IActionResult DevSetSession(int memberId)
        {
            HttpContext.Session.SetInt32("CurrentMemberId", memberId);
            return RedirectToAction("Index", "Profile", new {area = "Member"});
        }
    }
#endif
}