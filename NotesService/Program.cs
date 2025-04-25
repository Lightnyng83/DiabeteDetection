using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotesService.Services;
using NotesService.Settings;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using NotesService.Security;
using NotesService.Data;
using Commons.Security.Service;


var builder = WebApplication.CreateBuilder(args);

#region Context

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));


#endregion
var gatewayUrl = builder.Configuration["Gateway:BaseUrl"];
builder.Services.AddHttpClient<PatientApiService>(client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
});

builder.Services.Configure<Commons.Security.Model.JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddControllers();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<NoteService>();
builder.Services.AddTransient<SeedMongoData>();
builder.Services.AddHttpContextAccessor();
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
        options.RequireHttpsMetadata = false;
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var seedMongoData = services.GetRequiredService<SeedMongoData>();
        await seedMongoData.SeedNotesAsync(services);

    }
    catch (Exception ex)
    {
        Console.WriteLine("Erreur lors du seed de données: " + ex.Message);
    }
}

#endregion

app.Run();
public partial class Program
{

}