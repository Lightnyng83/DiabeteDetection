using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotesService.Services;
using NotesService.Settings;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using NotesService.Security;
using Microsoft.EntityFrameworkCore;
using NotesService.Data;
using PatientService.Data;
using Utilitaires.Repository;

var builder = WebApplication.CreateBuilder(args);

#region Context

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

//Obligé d'ajouter le context de PatientService pour pouvoir faire le seed
//car IPatientRepository est injecté dans le service NoteService
//et nécessite le context SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // ou une autre chaîne pour SQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

#endregion
// Charger la config Mongo

builder.Services.AddScoped<IPatientRepository, PatientRepository>();

builder.Services.AddControllers();
builder.Services.AddSingleton<NoteService>();
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


#endregion

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFront");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

#region SeedData

await Task.Delay(5000);
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
await SeedMongoData.SeedNotesAsync(services);

#endregion

app.Run();
public partial class Program
{

}