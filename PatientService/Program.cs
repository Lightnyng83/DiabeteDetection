using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PatientService.Data;
using PatientService.Models;
using PatientService.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "wwwroot",
    ApplicationName = typeof(Program).Assembly.FullName,
});
builder.WebHost.UseUrls("http://*:8080");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!Program.IsIntegrationTest) // Si on est pas en test => donc en prod
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
    }
});


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

#region JWT Bearer

// Charger la configuration JWT
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings!.SecretKey);

// Configurer l'authentification JWT
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Essayer de récupérer le token depuis le cookie "token"
                var token = context.Request.Cookies["token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true
        };
    });
#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFront",
        policy =>
        {
            policy.WithOrigins("https://localhost:5002")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Autorise l'envoi des cookies
        });
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("keys"))
    .SetApplicationName("DiabeteDetection");
#endregion

builder.Services.AddRouting();
builder.Services.AddControllers();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting(); // D'abord le routage
app.UseCors("AllowFront"); // Puis CORS si nécessaire
app.UseAuthentication(); // Puis l'authentification
app.UseAuthorization();  // Puis l'autorisation
app.MapControllers();    // Enfin, mapper les contrôleurs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
#region SeedData

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Nouvelle version propre
        if (context.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("[INFO] Migrations en attente... Application des migrations.");
            context.Database.Migrate();
        }
        else
        {
            Console.WriteLine("[INFO] Pas de migration en attente.");
        }

        await SeedData.InitializeAsync(services, "Test@12345");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erreur lors du seed de données: " + ex.Message);
    }
}

#endregion

app.MapGet("/health", () => Results.Ok("Healthy"));


app.Run();

public partial class Program
{
    public static bool IsIntegrationTest { get; set; } = false;

}


