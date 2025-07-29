using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _client;

        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
        }

        public async Task<IActionResult> Index(string searchString = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var orderResponse = await _client.GetAsync($"Order?searchString={searchString}&page={page}&pageSize={pageSize}");
                var userResponse = await _client.GetAsync("User");

                if (!orderResponse.IsSuccessStatusCode || !userResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to load data from API.";
                    return View(new List<OrderModel>());
                }

                var orderJson = await orderResponse.Content.ReadAsStringAsync();
                var userJson = await userResponse.Content.ReadAsStringAsync();

                var orders = JsonConvert.DeserializeObject<List<OrderModel>>(orderJson);
                var users = JsonConvert.DeserializeObject<List<UserModel>>(userJson);

                // Attach UserName to each Order
                foreach (var order in orders)
                {
                    var user = users.FirstOrDefault(u => u.UserId == order.UserId);
                    order.UserName = user?.UserName ?? "Unknown";
                }

                ViewBag.SearchString = searchString;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)orders.Count / pageSize);

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading orders: {ex.Message}";
                return View(new List<OrderModel>());
            }
        }

        public IActionResult AddEdit(int id = 0)
        {
            return View(new OrderModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(int id, OrderModel model)
        {
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (id == 0)
                    response = await _client.PostAsync("Order", jsonContent);
                else
                    response = await _client.PutAsync($"Order/{id}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Order saved successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Failed to save order.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error saving order: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Order/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Order deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete the order.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderFromCart(int cartId)
        {
            try
            {
                var cartResponse = await _client.GetAsync($"Cart/{cartId}");
                if (!cartResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("Index", "Cart");
                }

                var cartItem = JsonConvert.DeserializeObject<CartModel>(await cartResponse.Content.ReadAsStringAsync());

                var productResponse = await _client.GetAsync($"Product/{cartItem.ProductId}");
                if (!productResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Product details not found.";
                    return RedirectToAction("Index", "Cart");
                }

                var product = JsonConvert.DeserializeObject<ProductModel>(await productResponse.Content.ReadAsStringAsync());

                var order = new OrderModel
                {
                    UserId = cartItem.UserId ?? 0,
                    OrderName = $"{product.ProductName} (x{cartItem.Quantity})",
                    TotalAmount = product.Price * (cartItem.Quantity ?? 1),
                    OrderDate = DateTime.UtcNow,
                };

                var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
                var orderResponse = await _client.PostAsync("Order", content);

                if (orderResponse.IsSuccessStatusCode)
                {
                    await _client.DeleteAsync($"Cart/{cartId}");
                    TempData["Success"] = "Order placed successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Failed to place the order.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}
