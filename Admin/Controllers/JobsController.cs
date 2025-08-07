using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class JobsController : Controller
    {
        // sadece Jobs tablosunu döner (sayfa yenilemeden)
        public IActionResult Partial()
        {
            return PartialView("_JobsPartial");
        }

    }
}
