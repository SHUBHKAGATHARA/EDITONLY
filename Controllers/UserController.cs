using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _client;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
        }

        // GET: /User/
        public async Task<IActionResult> Index(int page = 1, string? searchTerm = null)
        {
            try
            {
                int pageSize = 5;
                var response = await _client.GetAsync("User");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to fetch user data.";
                    return View(new List<UserModel>());
                }

                var allUsers = JsonConvert.DeserializeObject<List<UserModel>>(await response.Content.ReadAsStringAsync()) ?? new();

                // Filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    allUsers = allUsers
                        .Where(u => u.UserName != null && u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // Pagination
                int totalUsers = allUsers.Count;
                int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
                var usersOnPage = allUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                ViewBag.SearchTerm = searchTerm;

                return View(usersOnPage);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return View(new List<UserModel>());
            }
        }

        // GET: /User/AddEdit/5 or /User/AddEdit
        public async Task<IActionResult> AddEdit(int? id)
        {
            try
            {
                if (id == null || id == 0)
                    return View(new UserModel()); // Create

                var response = await _client.GetAsync($"User/{id}");
                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(json);
                return View(user); // Edit
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /User/AddEdit
        [HttpPost]
        public async Task<IActionResult> AddEdit(UserModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var jsonData = JsonConvert.SerializeObject(model);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = model.UserId == 0
                    ? await _client.PostAsync("User", content)        // Create
                    : await _client.PutAsync($"User/{model.UserId}", content); // Update

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"API Error: {response.StatusCode} - {error}");
                    return View(model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                return View(model);
            }
        }

        // GET: /User/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"User/{id}");

                if (!response.IsSuccessStatusCode)
                    TempData["Error"] = "Failed to delete user.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while deleting: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
