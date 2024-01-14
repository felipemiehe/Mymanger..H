using Microsoft.AspNetCore.Mvc;

namespace front.Controllers
{
    public class DashBoardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
