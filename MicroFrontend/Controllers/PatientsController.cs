using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MicroFrontend.Models;

namespace MicroFrontend.Controllers
{
    public class PatientsController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        // Action pour afficher la liste des patients
        public async Task<IActionResult> Index(string token)
        {
            // Appel à l'API pour récupérer la liste des patients.
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            List<PatientViewModel> patients = await _httpClient.GetFromJsonAsync<List<PatientViewModel>>("patients");

            return View(patients);
        }
    }
}