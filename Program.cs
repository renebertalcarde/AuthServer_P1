using AuthServer_P1.Persistence;
using LogixSys.AuthServer.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register database and OpenIddict Core services.
builder.Services.AddPersistence(builder.Configuration);

// Keep your existing service registrations here,
// including controllers and API services.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Register the initial OpenIddict client.
await OpenIddictSeeder.SeedAsync(app.Services);

// Keep your existing middleware and endpoint configuration here.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
