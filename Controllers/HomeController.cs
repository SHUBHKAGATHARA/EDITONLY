using System.Diagnostics;
using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace IceCreame_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "admin")
                return RedirectToAction("AdminLogin", "Login");

            return View(); // Admin dashboard
        }

        // Profile Settings Page
        public IActionResult ProfileSettings()
        {
            if (HttpContext.Session.GetString("UserName") == null)
                return RedirectToAction("Login", "Login");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.Email = HttpContext.Session.GetString("Email");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            return View(); // Create a corresponding view
        }

        // Logout method
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear all session values
            return RedirectToAction("Login", "Login"); // Redirect to login page
        }
    }
}
