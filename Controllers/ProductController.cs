using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProductController> _logger;
        private const int PageSize = 5;

        public ProductController(IHttpClientFactory httpClientFactory, ILogger<ProductController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
            _logger = logger;
        }

        // GET: Product list with pagination
        public async Task<IActionResult> Index(int page = 1)
        {
            List<ProductModel> products = new();
            List<BrandModel> brands = new();

            try
            {
                // Fetch brands
                var brandResponse = await _client.GetAsync("Brand");
                if (brandResponse.IsSuccessStatusCode)
                {
                    var brandData = await brandResponse.Content.ReadAsStringAsync();
                    brands = JsonConvert.DeserializeObject<List<BrandModel>>(brandData);
                }

                // Fetch products
                var productResponse = await _client.GetAsync("Product");
                if (productResponse.IsSuccessStatusCode)
                {
                    var productData = await productResponse.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<ProductModel>>(productData);

                    foreach (var product in products)
                    {
                        product.BrandName = brands.FirstOrDefault(b => b.BrandId == product.BrandId)?.BrandName ?? "";
                    }
                }

                // Pagination logic
                int totalItems = products.Count;
                var itemsToShow = products
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / PageSize);

                return View(itemsToShow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product list.");
                ViewBag.Error = "An error occurred while loading products.";
                return View(new List<ProductModel>());
            }
        }

        // GET: Add or Edit
        public async Task<IActionResult> AddEdit(int id = 0)
        {
            var model = new ProductModel();

            try
            {
                var brandResponse = await _client.GetAsync("Brand");
                if (brandResponse.IsSuccessStatusCode)
                {
                    var brandJson = await brandResponse.Content.ReadAsStringAsync();
                    var brands = JsonConvert.DeserializeObject<List<BrandModel>>(brandJson);

                    model.BrandList = brands.Select(b => new SelectListItem
                    {
                        Value = b.BrandId.ToString(),
                        Text = b.BrandName
                    }).ToList();
                }

                if (id == 0)
                    return View(model);

                var response = await _client.GetAsync($"Product/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var productJson = await response.Content.ReadAsStringAsync();
                    var product = JsonConvert.DeserializeObject<ProductModel>(productJson);

                    if (product != null)
                    {
                        model.ProductId = product.ProductId;
                        model.ProductName = product.ProductName;
                        model.Price = product.Price;
                        model.Description = product.Description;
                        model.BrandId = product.BrandId;
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Add/Edit form for Product ID {Id}", id);
                ModelState.AddModelError("", "Something went wrong while loading the form.");
                return View(model);
            }
        }

        // POST: Add or Edit
        [HttpPost]
        public async Task<IActionResult> AddEdit(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateBrandList(model);
                return View(model);
            }

            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (model.ProductId == 0)
                    response = await _client.PostAsync("Product", content);
                else
                    response = await _client.PutAsync($"Product/{model.ProductId}", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                ModelState.AddModelError("", "Failed to save product. Try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving product.");
                ModelState.AddModelError("", "Unexpected error occurred.");
            }

            await PopulateBrandList(model);
            return View(model);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"Product/{id}");
                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning("Failed to delete product ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product ID {Id}", id);
            }

            return RedirectToAction("Index");
        }

        // Populate Brand List
        private async Task PopulateBrandList(ProductModel model)
        {
            try
            {
                var brandResponse = await _client.GetAsync("Brand");
                if (brandResponse.IsSuccessStatusCode)
                {
                    var brandJson = await brandResponse.Content.ReadAsStringAsync();
                    var brands = JsonConvert.DeserializeObject<List<BrandModel>>(brandJson);

                    model.BrandList = brands.Select(b => new SelectListItem
                    {
                        Value = b.BrandId.ToString(),
                        Text = b.BrandName
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating brand list.");
                model.BrandList = new List<SelectListItem>();
            }
        }
    }
}
