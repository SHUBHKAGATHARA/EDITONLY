using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Security.Claims;

namespace IceCreame_MVC.Controllers
{
    public class CartController : Controller
    {
        private readonly HttpClient _client;

        public CartController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
        }

        public async Task<IActionResult> Index()
        {
            List<CartModel> carts = new();
            List<UserModel> users = new();
            List<ProductModel> products = new();

            try
            {
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

                var cartResponse = await _client.GetAsync("Cart");
                if (cartResponse.IsSuccessStatusCode)
                {
                    var cartData = await cartResponse.Content.ReadAsStringAsync();
                    carts = JsonConvert.DeserializeObject<List<CartModel>>(cartData)!;

                    foreach (var cart in carts)
                    {
                        cart.UserName = users.FirstOrDefault(u => u.UserId == cart.UserId)?.UserName ?? "";
                        cart.ProductName = products.FirstOrDefault(p => p.ProductId == cart.ProductId)?.ProductName ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading carts. " + ex.Message;
            }

            return View(carts);
        }

        public async Task<IActionResult> AddEdit(int? id)
        {
            CartModel cart = new();

            try
            {
                var userResponse = await _client.GetAsync("User");
                var productResponse = await _client.GetAsync("Product");

                if (userResponse.IsSuccessStatusCode && productResponse.IsSuccessStatusCode)
                {
                    var usersJson = await userResponse.Content.ReadAsStringAsync();
                    var productsJson = await productResponse.Content.ReadAsStringAsync();

                    ViewBag.Users = JsonConvert.DeserializeObject<List<UserModel>>(usersJson);
                    ViewBag.Products = JsonConvert.DeserializeObject<List<ProductModel>>(productsJson);
                }

                if (id != null)
                {
                    var response = await _client.GetAsync($"Cart/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var cartJson = await response.Content.ReadAsStringAsync();
                        cart = JsonConvert.DeserializeObject<CartModel>(cartJson)!;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load form data. " + ex.Message;
            }

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(CartModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var jsonData = JsonConvert.SerializeObject(model);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (model.CartId == 0)
                    response = await _client.PostAsync("Cart", content);
                else
                    response = await _client.PutAsync($"Cart/{model.CartId}", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                TempData["Error"] = "API call failed.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while saving the cart. " + ex.Message;
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Cart/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to delete cart.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the cart. " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> OrderFromCart(int cartId)
        {
            try
            {
                // Fetch cart details
                var response = await _client.GetAsync($"Cart/{cartId}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to retrieve cart item.";
                    return RedirectToAction("Index");
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                var cartItem = JsonConvert.DeserializeObject<CartModel>(jsonData);

                if (cartItem == null)
                {
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("Index");
                }

                // Convert cart to order model
                var order = new OrderModel
                {
                    UserId = cartItem.UserId,
                };

                var orderContent = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
                var orderResponse = await _client.PostAsync("Order", orderContent);
                if (!orderResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to create order.";
                    return RedirectToAction("Index");
                }

                var orderResult = JsonConvert.DeserializeObject<OrderModel>(await orderResponse.Content.ReadAsStringAsync());

                // Create order item
                var orderItem = new OrderItemModel
                {
                    OrderId = orderResult.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                };

                var itemContent = new StringContent(JsonConvert.SerializeObject(orderItem), Encoding.UTF8, "application/json");
                var itemResponse = await _client.PostAsync("OrderItem", itemContent);

                if (!itemResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Order created but failed to add item.";
                }
                else
                {
                    TempData["Success"] = "Order placed successfully!";
                }

                // Optional: delete cart
                await _client.DeleteAsync($"Cart/{cartId}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error while ordering: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddFromCart(int cartId)
        {
            try
            {
                var response = await _client.GetAsync($"Cart/{cartId}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to get cart item.";
                    return RedirectToAction("Index");
                }

                var jsonData = await response.Content.ReadAsStringAsync();
                var cartItem = JsonConvert.DeserializeObject<CartModel>(jsonData);

                if (cartItem == null)
                {
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("Index");
                }

                var wishlist = new WishListModel
                {
                    UserId = cartItem.UserId,
                    ProductId = cartItem.ProductId,
                    CreatedAt = DateTime.Now
                };

                var wishlistContent = new StringContent(JsonConvert.SerializeObject(wishlist), Encoding.UTF8, "application/json");
                var wishlistResponse = await _client.PostAsync("WishList", wishlistContent);

                if (wishlistResponse.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Moved to wishlist successfully!";
                    await _client.DeleteAsync($"Cart/{cartId}"); // Optional: remove from cart
                }
                else
                {
                    TempData["Error"] = "Failed to move to wishlist.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error while moving to wishlist: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


    }
}
