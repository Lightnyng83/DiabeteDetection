using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MicroFrontend.Models;

namespace MicroFrontend.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            Console.WriteLine($"[DEBUG] BaseAddress: {_httpClient.BaseAddress}");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Appel à l'API de connexion
            Console.WriteLine($"[DEBUG] BaseAddress : {_httpClient.BaseAddress}");
            Console.WriteLine($"[DEBUG] Appel de l'API: {_httpClient.BaseAddress}account/login");

            var response = await _httpClient.PostAsJsonAsync("account/login", model);
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                var token = loginResponse!.Token;

                // Stocker le token dans la Session 
                HttpContext.Session.SetString("Token", token!);

                // Redirection vers la page des patients
                return RedirectToAction("Index", "Patients");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Échec de l'authentification. Vérifiez vos identifiants.");
                return View(model);
            }
        }




       
    }
}
