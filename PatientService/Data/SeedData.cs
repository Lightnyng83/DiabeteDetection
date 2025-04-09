using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatientService.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace PatientService.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, string testUserPassword)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                int retries = 10; // 10 essais
                while (retries > 0)
                {
                    try
                    {
                        Console.WriteLine("Tentative de connexion à SQL Server...");
                        await context.Database.MigrateAsync();
                        Console.WriteLine("Connexion réussie à SQL Server.");
                        break; // Succès
                    }
                    catch (Exception ex)
                    {
                        retries--;
                        Console.WriteLine($"Erreur de connexion SQL : {ex.Message}. Tentatives restantes : {retries}");
                        await Task.Delay(3000); // Attendre 3 secondes avant d'essayer à nouveau
                    }
                }

                if (retries == 0)
                {
                    throw new Exception("Impossible de se connecter à SQL Server après plusieurs tentatives.");
                }

                #region Create User

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                if (!await context.Roles.AnyAsync())
                {
                    var doctorRole = "Doctor";
                    if (!await roleManager.RoleExistsAsync(doctorRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(doctorRole));
                    }

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
                            await userManager.AddToRoleAsync(testUser, doctorRole);
                        }
                        else
                        {
                            throw new Exception("La création de l'utilisateur de test a échoué : " + string.Join(", ", result.Errors));
                        }
                    }
                }

               

                #endregion

                #region Add Patients

                var nom = context.Patients.FirstOrDefault(p => p.Nom == "Test");
                if (nom == null)
                {
                    context.Patients.Add(new Patient
                    {
                        Nom = "Test",
                        Prenom = "TestNone",
                        DateNaissance = new DateOnly(1996, 12, 31),
                        Adresse = "1 Brookside St",
                        Telephone = "100-222-3333",
                        Genre = 0
                    });
                    context.Patients.Add(new Patient
                    {
                        Nom = "Test",
                        Prenom = "TestBorderline",
                        DateNaissance = new DateOnly(1945, 06, 24),
                        Adresse = "2 High St",
                        Telephone = "200-333-4444",
                        Genre = 1
                    });
                    context.Patients.Add(new Patient
                    {
                        Nom = "Test",
                        Prenom = "TestInDanger",
                        DateNaissance = new DateOnly(2004, 06, 18),
                        Adresse = "3 Club Road",
                        Telephone = "300-444-5555",
                        Genre = 1
                    });
                    context.Patients.Add(new Patient
                    {
                        Nom = "Test",
                        Prenom = "TestEarlyOnset",
                        DateNaissance = new DateOnly(2002, 06, 28),
                        Adresse = "4 Valley Dr",
                        Telephone = "400-555-6666",
                        Genre = 0
                    });
                    await context.SaveChangesAsync();
                }

                #endregion
            }
        }
    }
}
