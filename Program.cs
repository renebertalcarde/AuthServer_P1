using AuthServer.Persistence;
using LogixSys.AuthServer.Persistence;
using Microsoft.AspNetCore.Identity;

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




//var certPath = Path.Combine(
//    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
//    "mkcert-authserver",
//    "localhost.pem");

//var keyPath = Path.Combine(
//    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
//    "mkcert-authserver",
//    "localhost-key.pem");

//var certificate =
//    System.Security.Cryptography.X509Certificates.X509Certificate2
//        .CreateFromPemFile(certPath, keyPath);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenLocalhost(7284, listenOptions =>
//    {
//        listenOptions.UseHttps(certificate);
//    });
//});
//Console.WriteLine($"Subject: {certificate.Subject}");
//Console.WriteLine($"Has private key: {certificate.HasPrivateKey}");
//Console.WriteLine($"Valid from: {certificate.NotBefore}");
//Console.WriteLine($"Valid until: {certificate.NotAfter}");




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await IdentitySeeder.SeedAsync(
      app.Services,
      app.Configuration);
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
