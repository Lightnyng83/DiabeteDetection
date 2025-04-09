using MongoDB.Driver;
using NotesService.Models;
using NotesService.Services;
using System.Net.Http;


namespace NotesService.Data
{
    public class SeedMongoData
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClient _notesServiceClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SeedMongoData(HttpClient httpClient, HttpClient notesServiceClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _notesServiceClient = notesServiceClient;
            this._httpContextAccessor = httpContextAccessor;
        }
        public async Task SeedNotesAsync(IServiceProvider services)
        {
            var noteService = services.GetRequiredService<NoteService>();

            var patientApiService = services.GetRequiredService<PatientApiService>();

            var httpClient = services.GetRequiredService<HttpClient>();
            if (!TryAddTokenToHeader())
            {
                Console.WriteLine("Token non valide ou absent. Impossible d'ajouter le token dans l'en-tête.");
                return;
            }

            List<PatientDto> patients = await patientApiService.GetAllPatientsAsync();

            if (patients == null! || patients.Count == 0)
            {
                // Vous pouvez choisir de logger l'absence de patients ou d'injecter des patients fictifs dans ce cas
                Console.WriteLine("Aucun patient trouvé dans la base SQL. Le seed des notes ne sera pas effectué.");
                return;
            }
            var mongoClient = new MongoClient("mongodb://mongodb:27017"); // Remplacer par ta chaîne de connexion Mongo
            var database = mongoClient.GetDatabase("DiabeteNotesDb");

            // Vérifie si la collection "Notes" existe déjà
            var collectionNames = await database.ListCollectionNamesAsync();
            var collections = await collectionNames.ToListAsync();

            if (!collections.Contains("Notes"))
            {
                // Si la collection n'existe pas, on la crée manuellement
                await database.CreateCollectionAsync("Notes");
                Console.WriteLine("Collection 'Notes' créée.");
            }
            var patientNote = noteService.GetNoteAsync(patients[0].Id.ToString());
            if (patientNote == null!)
            {
                foreach (var patient in patients)
                {
                    switch (patient.Prenom)
                    {
                        case "TestNone":
                            var note1 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Le patient déclare qu'il 'se sent très bien' Poids égal ou inférieur au poids recommandé.",
                                CreatedAt = DateTime.UtcNow
                            };
                            await noteService.CreateNoteAsync(note1);
                            break;
                        case "TestBorderline":
                            var note2 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Le patient déclare qu'il ressent beaucoup de stress au travail Il se plaint également que son audition est anormale dernièrement.",
                                CreatedAt = DateTime.UtcNow
                            };
                            await noteService.CreateNoteAsync(note2);
                            var note3 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Le patient déclare avoir fait une réaction aux médicaments au cours des 3 derniers mois Il remarque également que son audition continue d'être anormale",
                                CreatedAt = DateTime.UtcNow
                            };
                            break;
                        case "TestInDanger":
                            var note4 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Le patient déclare qu'il fume depuis peu",
                                CreatedAt = DateTime.UtcNow
                            };
                            await noteService.CreateNoteAsync(note4);
                            var note5 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Le patient déclare qu'il est fumeur et qu'il a cessé de fumer l'année dernière Il se plaint également de crises d’apnée respiratoire anormales Tests de laboratoire indiquant un taux de cholestérol LDL élevé",
                                CreatedAt = DateTime.UtcNow
                            };
                            await noteService.CreateNoteAsync(note5);
                            break;
                        case "TestEarlyOnset":
                            var note6 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Le patient déclare qu'il lui est devenu difficile de monter les escaliers Il se plaint également d’être essoufflé Tests de laboratoire indiquant que les anticorps sont élevés Réaction aux médicaments",
                                CreatedAt = DateTime.UtcNow
                            };
                            await noteService.CreateNoteAsync(note6);
                            var note7 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Le patient déclare qu'il a mal au dos lorsqu'il reste assis pendant longtemps",
                                CreatedAt = DateTime.UtcNow
                            };
                            await noteService.CreateNoteAsync(note7);
                            var note8 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Le patient déclare avoir commencé à fumer depuis peu Hémoglobine A1C supérieure au niveau recommandé",
                                CreatedAt = DateTime.UtcNow
                            };
                            await noteService.CreateNoteAsync(note8);
                            var note9 = new Note
                            {
                                PatientId = patient.Id,
                                Content = "Taille, Poids, Cholestérol, Vertige et Réaction",
                                CreatedAt = DateTime.UtcNow
                            };
                            await noteService.CreateNoteAsync(note9);
                            break;
                    }
                }

                Console.WriteLine("[DEBUG]SeedData Correctement initialisé");
            
            }
        }


        

        private bool TryAddTokenToHeader()
        {
            // Récupérer le token depuis la Session
            var token = _httpContextAccessor.HttpContext.Session.GetString("Token");

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
    }
}
