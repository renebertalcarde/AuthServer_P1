using AuthServer.Persistence;
using LogixSys.AuthServer.Persistence;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register database and OpenIddict Core services.
builder.Services.AddPersistence(builder.Configuration);

// Keep your existing service registrations here,
// including controllers and API services.

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Load PFX securely (password from env or secrets)
var pfxPath = @"C:\dev\authserver-certs\authserver.pfx";
var pfxPassword = builder.Configuration["AuthServer:CertificatePassword"]
                  ?? Environment.GetEnvironmentVariable("AUTHSERVER_CERT_PASSWORD");

var certificate = new X509Certificate2(
    pfxPath,
    pfxPassword,
    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7284, listen =>
    {
        listen.UseHttps(certificate);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //await IdentitySeeder.SeedAsync(
    //  app.Services,
    //  app.Configuration);
}

// Register the initial OpenIddict client.
await OpenIddictSeeder.SeedAsync(app.Services);

// Keep your existing middleware and endpoint configuration here.

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
