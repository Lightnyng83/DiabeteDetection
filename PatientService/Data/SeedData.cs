using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatientService.Models;
using System;
using System.Threading.Tasks;

namespace PatientService.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, string testUserPassword)
        {
            // Créer un scope pour obtenir les services
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                context.Database.Migrate();
                #region Create User

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                //Création d'un rôle 'Doctor' s'il n'existe pas déjà
                var doctorRole = "Doctor";
                if (!await roleManager.RoleExistsAsync(doctorRole))
                {
                    await roleManager.CreateAsync(new IdentityRole(doctorRole));
                }

                // Vérifier si l'utilisateur de test existe déjà
                var testUserName = "testuser";
                var testUser = await userManager.FindByNameAsync(testUserName);
                if (testUser == null)
                {
                    testUser = new ApplicationUser
                    {
                        UserName = testUserName,
                        Email = "testuser@example.com"
                    };

                    var result = await userManager.CreateAsync(testUser, testUserPassword);
                    if (result.Succeeded)
                    {
                        // Ajouter l'utilisateur au rôle 'Doctor'
                        await userManager.AddToRoleAsync(testUser, doctorRole);
                    }
                    else
                    {
                        //La création de l'utilisateur a échoué. Vérifie les messages d'erreur.
                        throw new Exception("La création de l'utilisateur de test a échoué : " + string.Join(", ", result.Errors));
                    }
                }

                #endregion

                #region Add Patients
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var nom = dbContext.Patients.FirstOrDefault(p => p.Nom == "Test");
                if (nom == null)
                {
                    dbContext.Patients.Add(new Patient
                    {
                        Nom = "Test",
                        Prenom = "TestNone",
                        DateNaissance = new DateOnly(1996, 12, 31),
                        Adresse = "1 Brookside St",
                        Telephone = "100-222-3333",
                        Genre = 0
                    });
                    dbContext.Patients.Add(new Patient
                    {
                        Nom = "Test",
                        Prenom = "TestBorderline",
                        DateNaissance = new DateOnly(1945, 06, 24),
                        Adresse = "2 High St",
                        Telephone = "200-333-4444",
                        Genre = 1
                    });
                    dbContext.Patients.Add(new Patient
                    {
                        Nom = "Test",
                        Prenom = "TestInDanger",
                        DateNaissance = new DateOnly(2004, 06, 18),
                        Adresse = "3 Club Road",
                        Telephone = "300-444-5555",
                        Genre = 1
                    });
                    dbContext.Patients.Add(new Patient
                    {
                        Nom = "Test",
                        Prenom = "TestEarlyOnset",
                        DateNaissance = new DateOnly(2002, 06, 28),
                        Adresse = "4 Valley Dr",
                        Telephone = "400-555-6666",
                        Genre = 0
                    });
                    await dbContext.SaveChangesAsync();
                }

                #endregion
            }
        }
    }
}
