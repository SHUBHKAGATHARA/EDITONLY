using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class BrandController : Controller
    {
        private readonly HttpClient _client;

        public BrandController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<BrandModel> brands = new();
                var response = await _client.GetAsync("Brand");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    brands = JsonConvert.DeserializeObject<List<BrandModel>>(data)!;
                }

                return View(brands);
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message
                ViewBag.Error = "Error loading brand list.";
                return View(new List<BrandModel>());
            }
        }

        public async Task<IActionResult> AddEdit(int? id)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return View(new BrandModel()); // Create
                }

                var response = await _client.GetAsync($"Brand/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var brand = JsonConvert.DeserializeObject<BrandModel>(data)!;
                    return View(brand); // Edit
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message
                ViewBag.Error = "Error loading brand details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(BrandModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                    HttpResponseMessage response;

                    if (model.BrandId == 0)
                    {
                        response = await _client.PostAsync("Brand", content); // Create
                    }
                    else
                    {
                        response = await _client.PutAsync($"Brand/{model.BrandId}", content); // Edit
                    }

                    if (response.IsSuccessStatusCode)
                        return RedirectToAction("Index");

                    ViewBag.Error = "Failed to save brand.";
                }
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message
                ViewBag.Error = "An unexpected error occurred.";
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Brand/{id}");

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ViewBag.Error = "Failed to delete brand.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message
                ViewBag.Error = "Error deleting brand.";
                return RedirectToAction("Index");
            }
        }
    }
}
