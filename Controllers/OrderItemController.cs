using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<OrderItemController> _logger;

        public OrderItemController(IHttpClientFactory httpClientFactory, ILogger<OrderItemController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
            _logger = logger;
        }

        // GET: /OrderItem
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _client.GetAsync("OrderItem");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var items = JsonConvert.DeserializeObject<List<OrderItemModel>>(json);
                    return View(items);
                }
                ModelState.AddModelError("", "API returned failure status.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching OrderItem list.");
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
            }

            return View(new List<OrderItemModel>());
        }

        // GET: /OrderItem/AddEdit/{id}
        public async Task<IActionResult> AddEdit(int id = 0)
        {
            try
            {
                if (id == 0)
                    return View(new OrderItemModel());

                var response = await _client.GetAsync($"OrderItem/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var item = JsonConvert.DeserializeObject<OrderItemModel>(json);
                    return View(item);
                }

                ModelState.AddModelError("", "API returned failure for detail request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading OrderItem for edit.");
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
            }

            return NotFound();
        }

        // POST: /OrderItem/AddEdit
        [HttpPost]
        public async Task<IActionResult> AddEdit(OrderItemModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var jsonData = JsonConvert.SerializeObject(model);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (model.OrderItemId == 0)
                    response = await _client.PostAsync("OrderItem", content);
                else
                    response = await _client.PutAsync($"OrderItem/{model.OrderItemId}", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "API operation failed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Add/Edit OrderItem.");
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
            }

            return View(model);
        }

        // GET: /OrderItem/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"OrderItem/{id}");
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Failed to delete OrderItem with ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting OrderItem with ID {Id}", id);
            }

            return RedirectToAction("Index");
        }
    }
}
