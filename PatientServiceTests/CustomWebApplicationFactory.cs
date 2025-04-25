using Commons.Security.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ApplicationDbContext = PatientService.Data.ApplicationDbContext;

namespace PatientServiceTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Program.IsIntegrationTest = true;

            builder.ConfigureServices(services =>
            {
                // Remove EF Core configuration
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.AddScoped<ITokenService, TokenService>();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");

                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        }

    }
}