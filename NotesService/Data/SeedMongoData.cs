using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NotesService.Models;
using NotesService.Services;

namespace NotesService.Seed
{
    public static class SeedMongoData
    {
        public static async Task SeedNotesAsync(IServiceProvider services)
        {
            var noteService = services.GetRequiredService<NoteService>();

            // Définir le mapping entre l'identifiant du patient (int tel que dans Excel) et le GUID utilisé dans SQL
            var patientMapping = new Dictionary<int, Guid>
            {
                { 1, Guid.Parse("11111111-1111-1111-1111-111111111111") },
                { 2, Guid.Parse("22222222-2222-2222-2222-222222222222") },
                { 3, Guid.Parse("33333333-3333-3333-3333-333333333333") },
                { 4, Guid.Parse("44444444-4444-4444-4444-444444444444") }
            };

            // Chaque tuple contient : (PatientId venant de l'Excel, Contenu de la note)
            var excelData = new List<(int PatId, string Content)>
            {
                (1, "Le patient déclare qu'il 'se sent très bien' Poids égal ou inférieur au poids recommandé."),
                (2, "Le patient déclare qu'il ressent beaucoup de stress au travail Il se plaint également que son audition est anormale dernièrement."),
                (2, "Le patient déclare avoir fait une réaction aux médicaments au cours des 3 derniers mois Il remarque également que son audition continue d'être anormale"),
                (3, "Le patient déclare qu'il fume depuis peu"),
                (3, "Le patient déclare qu'il est fumeur et qu'il a cessé de fumer l'année dernière Il se plaint également de crises d’apnée respiratoire anormales Tests de laboratoire indiquant un taux de cholestérol LDL élevé"),
                (4, "Le patient déclare qu'il lui est devenu difficile de monter les escaliers Il se plaint également d’être essoufflé Tests de laboratoire indiquant que les anticorps sont élevés Réaction aux médicaments"),
                (4, "Le patient déclare qu'il a mal au dos lorsqu'il reste assis pendant longtemps"),
                (4, "Le patient déclare avoir commencé à fumer depuis peu Hémoglobine A1C supérieure au niveau recommandé"),
                (4, "Taille, Poids, Cholestérol, Vertige et Réaction"),
            };

            foreach (var (patId, content) in excelData)
            {
                if (patientMapping.TryGetValue(patId, out Guid patientGuid))
                {
                    var note = new Note
                    {
                        PatientId = patientGuid,
                        Content = content,
                        CreatedAt = DateTime.UtcNow 
                    };

                    await noteService.CreateNoteAsync(note);
                }

            }
        }
    }
}
