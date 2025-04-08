extern alias notesAlias;  // correspond à l'alias défini pour NotesService
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PatientService.Data;
using MongoDbSettings = notesAlias::NotesService.Settings.MongoDbSettings;
namespace NotesServiceTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                //Supprime l'enregistrement existant du client MongoDB (s'il existe)
                services.RemoveAll<IMongoClient>();

                // Configure le client MongoDB pour les tests
                services.AddSingleton<MongoDbSettings>();

                // Configure les paramètres MongoDB pour utiliser une base de données dédiée aux tests
                services.Configure<MongoDbSettings>(options =>
                {
                    options.ConnectionString = "mongodb://localhost:27017";
                    options.DatabaseName = "TestNotesDb";
                });

                //Nettoie la base de données de test avant d'exécuter les tests
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<MongoDbSettings>();
                }
            });
        }
    }
}
