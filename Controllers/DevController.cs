using Microsoft.AspNetCore.Mvc;

namespace Sym_Connect__Capstone2_.Controllers
{
#if DEBUG
    [Route("dev")]  // this overrides the default area routing pattern
    public class DevController : Controller
    {
        [HttpGet("set-session")]
        public IActionResult DevSetSession()
        {
            HttpContext.Session.SetInt32("CurrentMemberId", 1);
            return Content("Session set: CurrentMemberId = 1");
        }
    }
#endif
}