using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Ajoute le fichier ocelot.json � la configuration
builder.Configuration.AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true);

// Enregistre Ocelot avec la configuration
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
app.Use(async (context, next) =>
{
    Console.WriteLine($"Requ�te re�ue par Ocelot : {context.Request.Path}");
    await next();
});

await app.UseOcelot();

app.Run();