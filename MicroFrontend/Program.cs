using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Ajoute les services MVC
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("ApiClient", client =>
{
    // On récupère la valeur depuis la configuration,
    // qui sera surchargée par les variables d'environnement en Docker.
    var baseUrl = builder.Configuration["AccountApi:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        baseUrl = "https://localhost:7090/api/"; // Valeur par défaut en local
    }
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient("NotesService", client =>
{
    var baseUrl = builder.Configuration["NotesApi:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        baseUrl = "https://localhost:7041/api/"; // Valeur par défaut en local
    }
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient("RiskService", client =>
{
    var baseUrl = builder.Configuration["RiskReportApi:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        baseUrl = "https://localhost:7089/api/"; // Valeur par défaut en local
    }
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("keys"))
    .SetApplicationName("DiabeteDetection");

// Configuration de la session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuration de l'authentification par cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";  // Redirection vers /Account/Login
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();