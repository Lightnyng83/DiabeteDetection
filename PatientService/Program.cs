using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;

var builder = WebApplication.CreateBuilder(args);

// Utilise builder.Configuration directement
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


