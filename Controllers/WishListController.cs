using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class WishListController : Controller
    {
        private readonly HttpClient _client;

        public WishListController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<WishListModel> wishlists = new();
                List<UserModel> users = new();
                List<ProductModel> products = new();

                var wishlistTask = _client.GetAsync("WishList");
                var userTask = _client.GetAsync("User");
                var productTask = _client.GetAsync("Product");

                await Task.WhenAll(wishlistTask, userTask, productTask);

                var wishlistResponse = await wishlistTask;
                var userResponse = await userTask;
                var productResponse = await productTask;

                if (userResponse.IsSuccessStatusCode)
                {
                    var userData = await userResponse.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<UserModel>>(userData)!;
                }

                if (productResponse.IsSuccessStatusCode)
                {
                    var productData = await productResponse.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<ProductModel>>(productData)!;
                }

                if (wishlistResponse.IsSuccessStatusCode)
                {
                    var wishlistData = await wishlistResponse.Content.ReadAsStringAsync();
                    wishlists = JsonConvert.DeserializeObject<List<WishListModel>>(wishlistData)!;

                    foreach (var item in wishlists)
                    {
                        item.UserName = users.FirstOrDefault(u => u.UserId == item.UserId)?.UserName ?? "Unknown User";
                        item.ProductName = products.FirstOrDefault(p => p.ProductId == item.ProductId)?.ProductName ?? "Unknown Product";
                    }

                    return View(wishlists);
                }

                TempData["Error"] = "Failed to load wishlist data.";
                return View(new List<WishListModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading data: " + ex.Message;
                return View(new List<WishListModel>());
            }
        }

        public async Task<IActionResult> AddEdit(int id = 0)
        {
            try
            {
                var users = new List<UserModel>();
                var products = new List<ProductModel>();

                var userResponse = await _client.GetAsync("User");
                if (userResponse.IsSuccessStatusCode)
                {
                    var userData = await userResponse.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<UserModel>>(userData)!;
                }

                var productResponse = await _client.GetAsync("Product");
                if (productResponse.IsSuccessStatusCode)
                {
                    var productData = await productResponse.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<ProductModel>>(productData)!;
                }

                ViewBag.Users = users;
                ViewBag.Products = products;

                if (id == 0)
                {
                    return View(new WishListModel());
                }

                var response = await _client.GetAsync($"WishList/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<WishListModel>(json);
                    return View(model);
                }

                TempData["Error"] = "Wishlist item not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading form: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(WishListModel model)
        {
            try
            {
                var users = new List<UserModel>();
                var products = new List<ProductModel>();

                var userResponse = await _client.GetAsync("User");
                if (userResponse.IsSuccessStatusCode)
                {
                    var userData = await userResponse.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<UserModel>>(userData)!;
                }

                var productResponse = await _client.GetAsync("Product");
                if (productResponse.IsSuccessStatusCode)
                {
                    var productData = await productResponse.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<ProductModel>>(productData)!;
                }

                ViewBag.Users = users;
                ViewBag.Products = products;

                if (!ModelState.IsValid)
                    return View(model);

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (model.WishListId == 0)
                    response = await _client.PostAsync("WishList", content);
                else
                    response = await _client.PutAsync($"WishList/{model.WishListId}", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "Failed to save wishlist item.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving data: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"WishList/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to delete wishlist item.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting item: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> AddFromCart(int id)
        {
            // Example: Fetch cart item and convert it to wishlist
            var response = await _client.GetAsync($"Cart/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to fetch cart item.";
                return RedirectToAction("Index", "Cart");
            }

            var cartItem = JsonConvert.DeserializeObject<CartModel>(await response.Content.ReadAsStringAsync());

            var wishlistItem = new WishListModel
            {
                UserId = cartItem.UserId,
                ProductId = cartItem.ProductId,
                CreatedAt = DateTime.Now
            };

            var json = JsonConvert.SerializeObject(wishlistItem);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var postResponse = await _client.PostAsync("WishList", content);
            if (postResponse.IsSuccessStatusCode)
            {
                TempData["Success"] = "Item moved to Wishlist successfully.";
                return RedirectToAction("Index", "WishList");
            }

            TempData["Error"] = "Failed to move item to Wishlist.";
            return RedirectToAction("Index", "Cart");
        }

    }
}
