using Microsoft.AspNetCore.Mvc;

namespace PureFitness.Controllers
{
    public class LandingPageController : Controller
    {
        private readonly ILogger<LandingPageController> _logger;

        public LandingPageController(ILogger<LandingPageController> logger)
        {
            _logger = logger;
        }
        public IActionResult LandingView()
        {
            return View();
        }

        public ActionResult Login()
        {
            return RedirectToAction("LoginView", "Login");
        }
    }
}
