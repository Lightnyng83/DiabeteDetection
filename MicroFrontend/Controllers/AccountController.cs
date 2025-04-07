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

        // L'HttpClient est injecté via DI (voir configuration dans Program.cs)
        public AccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Appel à l'API de connexion (assure-toi que l'URL correspond à ton API, par exemple via Ocelot)
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7090/api/account/login", model);
            if (response.IsSuccessStatusCode)
            {
                // Récupère la réponse (le token)
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                // Pour tester, on peut stocker le token dans TempData ou dans la session
                var token = loginResponse.Token;

                // Redirection vers la page d'accueil ou une page sécurisée
                return RedirectToAction("Index", "Patients",token);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Échec de l'authentification. Vérifiez vos identifiants.");
                return View(model);
            }
        }
    }
}
