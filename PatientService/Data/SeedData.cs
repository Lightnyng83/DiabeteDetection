using Microsoft.AspNetCore.Identity;
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
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Optionnel : Création d'un rôle 'Admin' s'il n'existe pas déjà
                var adminRole = "Admin";
                if (!await roleManager.RoleExistsAsync(adminRole))
                {
                    await roleManager.CreateAsync(new IdentityRole(adminRole));
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
                        // Ajouter l'utilisateur au rôle 'Admin'
                        await userManager.AddToRoleAsync(testUser, adminRole);
                    }
                    else
                    {
                        // <span style="color: red;">Erreur :</span> La création de l'utilisateur a échoué. Vérifie les messages d'erreur.
                        throw new Exception("La création de l'utilisateur de test a échoué : " + string.Join(", ", result.Errors));
                    }
                }
            }
        }
    }
}
