﻿using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Appel à l'API de connexion
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7090/api/account/login", model);
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                var token = loginResponse.Token;

                // Stocker le token dans la Session (assurez-vous d'avoir activé la Session dans Program.cs)
                HttpContext.Session.SetString("Token", token);

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
