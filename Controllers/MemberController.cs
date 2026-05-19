using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sym_Connect__Capstone2_.Controllers
{
    [AllowAnonymous] //TEMPORARY - REMOVE THIS LATER
                     //[Authorize(Roles = "Member")]

    public class MemberController : Controller
    {
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
