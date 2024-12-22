using Microsoft.AspNetCore.Mvc;

namespace distant.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
