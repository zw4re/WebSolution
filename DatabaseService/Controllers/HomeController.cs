using Microsoft.AspNetCore.Mvc;

namespace DatabaseService.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Views/Home/Index.cshtml dosyasını açar
        }

        public IActionResult Privacy()
        {
            return View(); // Views/Home/Privacy.cshtml dosyasını açar
        }
    }
}
