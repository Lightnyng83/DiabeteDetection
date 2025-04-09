using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NotesService.Models;
using NotesService.Services;
using PatientService.Models;
using PatientService.Repository;

namespace NotesService.Seed
{
    public static class SeedMongoData
    {
        public static async Task SeedNotesAsync(IServiceProvider services)
        {
            var noteService = services.GetRequiredService<NoteService>();

            var patientRepository = services.GetService<IPatientRepository>();
            if (patientRepository == null)
            {
                // Si vous ne pouvez pas récupérer le repository, vous pouvez lever une exception ou passer un fallback.
                throw new Exception("PatientRepository non disponible pour le seed.");
            }

            // Récupérer la liste des patients
            List<Patient> patients = await patientRepository.GetAllPatientsAsync();
            if (patients == null || patients.Count == 0)
            {
                // Vous pouvez choisir de logger l'absence de patients ou d'injecter des patients fictifs dans ce cas
                Console.WriteLine("Aucun patient trouvé dans la base SQL. Le seed des notes ne sera pas effectué.");
                return;
            }

            var patientNote = noteService.GetNoteAsync(patients[0].Id.ToString());
            if (patientNote == null)
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
            
            }
        }
    }
}
