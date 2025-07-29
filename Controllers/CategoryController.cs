using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class CategoryController : Controller
    {
        private readonly HttpClient _client;

        public CategoryController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _client.GetAsync("Category");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<CategoryModel>>(json);
                    return View(list);
                }

                TempData["Error"] = "Failed to load categories.";
                return View(new List<CategoryModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while fetching categories: " + ex.Message;
                return View(new List<CategoryModel>());
            }
        }

        public async Task<IActionResult> AddEdit(int id = 0)
        {
            try
            {
                if (id == 0)
                    return View(new CategoryModel());

                var response = await _client.GetAsync($"Category/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var category = JsonConvert.DeserializeObject<CategoryModel>(json);
                    return View(category);
                }

                TempData["Error"] = "Category not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading category: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(CategoryModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var jsonData = JsonConvert.SerializeObject(model);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (model.CategoryId == 0)
                {
                    response = await _client.PostAsync("Category", content);
                }
                else
                {
                    response = await _client.PutAsync($"Category/{model.CategoryId}", content);
                }

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "API operation failed.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Category/{id}");
                if (!response.IsSuccessStatusCode)
                    TempData["Error"] = "Failed to delete category.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
