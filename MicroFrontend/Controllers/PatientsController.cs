using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MicroFrontend.Models;
using System.Net;

namespace MicroFrontend.Controllers
{
    public class PatientsController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index()
        {
            // Récupérer le token depuis la Session
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                // Rediriger vers la page de connexion si aucun token n'est disponible
                return RedirectToAction("Login", "Account");
            }

            // Ajouter le token dans le header Authorization
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Appel à l'API pour récupérer la liste des patients
            List<PatientViewModel> patients = await _httpClient.GetFromJsonAsync<List<PatientViewModel>>("patients");

            return View(patients);
        }

        // Action pour afficher un patient spécifique
        public async Task<IActionResult> Details(Guid id)
        {
            // Récupérer le token depuis la Session
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                // Rediriger vers la page de connexion si aucun token n'est disponible
                return RedirectToAction("Login", "Account");
            }

            // Ajouter le token dans le header Authorization
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Appel à l'API pour récupérer les détails d'un patient
            PatientViewModel? patient = await _httpClient.GetFromJsonAsync<PatientViewModel>($"patients/{id}");
            return View(patient);
        }

        // Action pour créer un nouveau patient
        [HttpGet]
        public IActionResult Create(string token)
        {
            // Ajouter le token à l'en-tête Authorization
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PatientViewModel patient, string token)
        {
            // Ajouter le token à l'en-tête Authorization
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            if (ModelState.IsValid)
            {
                // Appel à l'API pour créer un nouveau patient
                var response = await _httpClient.PostAsJsonAsync("patients", patient);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", new { token });
                }
            }

            return View(patient);


        }

        // Action pour supprimer un patient
        [HttpGet]
        public IActionResult Delete(Guid id, string token)
        {
            // Ajouter le token à l'en-tête Authorization
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeletePatient(Guid id, string token)
        {
            // Ajouter le token à l'en-tête Authorization
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Appel à l'API pour supprimer un patient
            var response = await _httpClient.DeleteAsync($"patients/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { token });
            }
            return RedirectToAction("Index","Patients");
        }
    }
}

