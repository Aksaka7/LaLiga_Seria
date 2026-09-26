using System.Diagnostics;
using LaLiga.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LaLiga.WebUI.Controllers
{
    public class HomeController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
