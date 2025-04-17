using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Ajoute le fichier ocelot.json à la configuration
builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);

// Enregistre Ocelot avec la configuration
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
app.Use(async (context, next) =>
{
    Console.WriteLine($"Requête reçue par Ocelot : {context.Request.Path}");
    await next();
});

await app.UseOcelot();

app.Run();