using Microsoft.AspNetCore.Mvc;

namespace GeneratorsManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
