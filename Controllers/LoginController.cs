using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7106/api/");

            var loginData = new
            {
                UserName = userName,
                Password = password
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("User/Login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize response without using your UserModel
                var user = JsonConvert.DeserializeObject<dynamic>(responseBody);

                string role = user?.role;
                string email = user?.email?.ToString() ?? "";

                if (role == "Admin" || role == "Shopkeeper")
                {
                    HttpContext.Session.SetString("UserName", userName);
                    HttpContext.Session.SetString("Role", role);
                    HttpContext.Session.SetString("Email", email);

                    return RedirectToAction("Index", "Home");
                }

                else
                {
                    ViewBag.Error = "Only Admin or Shopkeeper can login.";
                    return View();
                }
            }

            ViewBag.Error = "Invalid credentials.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
