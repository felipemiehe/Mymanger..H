using Microsoft.AspNetCore.Mvc;

namespace front.Controllers
{
    public class EdificiosAtivosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
