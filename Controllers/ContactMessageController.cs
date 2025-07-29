using IceCreame_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace IceCreame_MVC.Controllers
{
    public class ContactMessageController : Controller
    {
        private readonly HttpClient _client;

        public ContactMessageController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7106/api/");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _client.GetAsync("ContactMessage");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<ContactMessageModel>>(json);
                    return View(list);
                }

                TempData["Error"] = "Failed to load contact messages.";
                return View(new List<ContactMessageModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while fetching messages: " + ex.Message;
                return View(new List<ContactMessageModel>());
            }
        }

        public async Task<IActionResult> AddEdit(int id = 0)
        {
            try
            {
                if (id == 0)
                    return View(new ContactMessageModel());

                var response = await _client.GetAsync($"ContactMessage/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<ContactMessageModel>(json);
                    return View(model);
                }

                TempData["Error"] = "Message not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading message: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(ContactMessageModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var jsonData = JsonConvert.SerializeObject(model);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (model.MessageId == 0)
                {
                    response = await _client.PostAsync("ContactMessage", content);
                }
                else
                {
                    response = await _client.PutAsync($"ContactMessage/{model.MessageId}", content);
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
                var response = await _client.DeleteAsync($"ContactMessage/{id}");
                if (!response.IsSuccessStatusCode)
                    TempData["Error"] = "Failed to delete the message.";

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
