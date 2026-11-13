using Microsoft.AspNetCore.Mvc;

namespace SanjeshP.Payeh.Api.Controllers.v1
{
    public class MenusController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
