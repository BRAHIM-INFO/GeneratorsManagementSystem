using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // ✅ هذا هو Action الافتراضي
        public IActionResult Welcome()
        {
            return View();
        }
    }
}
