using Microsoft.AspNetCore.Authentication.JwtBearer;
using RiskReportService.Services;
using RiskReportService.Services.Interfaces;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RiskReportService.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurer les HttpClients
builder.Services.AddHttpClient<IPatientClient, PatientClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PatientApi"]!);
});
builder.Services.AddHttpClient<INoteClient, NoteClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotesApi"]!);
});

// Injection du service métier
builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
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
app.Run();