using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;

using System.Diagnostics;

namespace Sym_Connect__Capstone2_.Controllers
{
    //[Authorize]
    [AllowAnonymous] //TEMPORARY - REMOVE THIS LATER
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
