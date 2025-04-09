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
        public async Task<IActionResult> Details(PatientDetailViewModel model)
        {
            if (!TryAddTokenToHeader())
            {
                return RedirectToAction("Login", "Account");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Mettre à jour le patient
            var patient = new PatientViewModel
            {
                Nom = model.Patient.Nom,
                Prenom = model.Patient.Prenom,
                DateNaissance = model.Patient.DateNaissance,
                Genre = model.Patient.Genre,
                Adresse = model.Patient.Adresse,
                Telephone = model.Patient.Telephone,
                Id = model.Patient.Id
            };

            var responsePatient = await _httpClient.PutAsJsonAsync($"patients/{patient.Id}", patient);

            // On initialise la variable pour la réponse de l'appel aux notes
            HttpResponseMessage responseNote = null;

            // Vérifier si le médecin a ajouté du contenu pour une note
            if (!string.IsNullOrWhiteSpace(model.Content))
            {
                var note = new NoteViewModel
                {
                    PatientId = model.Patient.Id,
                    Content = model.Content,
                    CreatedAt = DateTime.Now
                };

                // Appeler l'API notes uniquement si du contenu a été fourni
                responseNote = await _notesServiceClient.PostAsJsonAsync("notes", note);
            }

            // Si l'appel à l'API patient a réussi et que, soit l'appel aux notes n'a pas été effectué
            // (car aucun contenu n'a été fourni), soit il a réussi, alors redirige
            if (responsePatient.IsSuccessStatusCode && (responseNote == null || responseNote.IsSuccessStatusCode))
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "Erreur lors de la mise à jour du patient ou de l'ajout de la note.");
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

