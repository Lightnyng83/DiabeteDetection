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
        private readonly HttpClient _notesServiceClient;


        public PatientsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _notesServiceClient = httpClientFactory.CreateClient("NotesService");
        }

        public async Task<IActionResult> Index()
        {
            if (!TryAddTokenToHeader())
            {
                return RedirectToAction("Login", "Account");
            }

            // Appel à l'API pour récupérer la liste des patients
            List<PatientViewModel>? patients = await _httpClient.GetFromJsonAsync<List<PatientViewModel>>("patients");

            return View(patients);
        }

        #region Details & Update

        // GET: Patients/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (!TryAddTokenToHeader())
            {
                return RedirectToAction("Login", "Account");
            }

            // Récupère le patient (API Patients)
            var patient = await _httpClient.GetFromJsonAsync<PatientViewModel>($"patients/{id}");
            if (patient == null)
            {
                return NotFound();
            }

            // Récupère les notes du patient (API NotesService)
            // L'API NotesService expose l'endpoint GET /api/notes/patient/{patientId}.
            // Vu la configuration de HttpClient ("NotesService" avec BaseAddress https://localhost:7041/api/),
            // nous appelons directement l'URL "notes/patient/{id}"
            var notes = await _notesServiceClient.GetFromJsonAsync<List<NoteViewModel>>($"notes/patient/{id}");

            var viewModel = new PatientDetailViewModel
            {
                Patient = patient,
                Notes = notes ?? new List<NoteViewModel>()
            };

            return View(viewModel);
        }

        // POST: Patients/Details
        [HttpPost]
        public async Task<IActionResult> Details(PatientViewModel model)
        {
            if (!TryAddTokenToHeader())
            {
                return RedirectToAction("Login", "Account");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Envoi d'une requête PUT à l'API pour mettre à jour le patient
            var response = await _httpClient.PutAsJsonAsync($"patients/{model.Id}", model);
            if (response.IsSuccessStatusCode)
            {
                // Redirige vers l'index ou affiche un message de succès
                return RedirectToAction("Index");
            }
            // En cas d'erreur, on réaffiche le formulaire avec un message d'erreur (à compléter selon vos besoins)
            ModelState.AddModelError(string.Empty, "Erreur lors de la mise à jour du patient.");
            return View(model);
        }



        #endregion

        #region Create

        [HttpGet]
        public IActionResult Create()
        {
            if (!TryAddTokenToHeader())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PatientViewModel patient)
        {
            if (!TryAddTokenToHeader())
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                // Appel à l'API pour créer un nouveau patient
                var response = await _httpClient.PostAsJsonAsync("patients", patient);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }

            return View(patient);
        }

        #endregion

        #region Delete

        [HttpPost]
        public async Task<IActionResult> DeletePatient(Guid id)
        {
            if (!TryAddTokenToHeader())
            {
                return RedirectToAction("Login", "Account");
            }
            // Appel à l'API pour supprimer un patient
            var response = await _httpClient.DeleteAsync($"patients/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index", "Patients");
        }

        #endregion

        #region Privates Methods

        private bool TryAddTokenToHeader()
        {
            // Récupérer le token depuis la Session
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                // Rediriger vers la page de connexion si aucun token n'est disponible
                return false;
            }

            // Ajouter le token dans le header Authorization
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _notesServiceClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        #endregion

    }
}

