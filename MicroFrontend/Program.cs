using Commons.Security.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Ajoute les services MVC
builder.Services.AddControllersWithViews();
var gatewayUrl = builder.Configuration["Gateway:BaseUrl"];
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
});
builder.Services.Configure<Commons.Security.Model.JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
// Configuration de la session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<ITokenService, TokenService>();

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