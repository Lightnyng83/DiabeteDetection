using RiskReportService.Services;
using RiskReportService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurer les HttpClients
builder.Services.AddHttpClient<IPatientClient, PatientClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PatientApi"]);
});
builder.Services.AddHttpClient<INoteClient, NoteClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotesApi"]);
});

// Injection du service métier
builder.Services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();

var app = builder.Build();


app.UseAuthorization();
app.MapControllers();
app.Run();