using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sym_Connect__Capstone2_.Controllers
{

    [Authorize]
    public class LeaderController : Controller
    {
        [AllowAnonymous] //TEMPORARY - REMOVE THIS LATER
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
