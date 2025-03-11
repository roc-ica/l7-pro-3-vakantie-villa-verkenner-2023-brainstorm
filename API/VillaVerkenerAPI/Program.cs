using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;
using VillaVerkenerAPI.Models.DB;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not found.");
}

// Add DbContext and MySQL setup
builder.Services.AddDbContext<DBContext>(options =>
    options.UseMySQL(connectionString));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use HTTPS redirection
app.UseHttpsRedirection();

// Simple Hello World endpoint
app.MapGet("/hello", () => "Hello, World!");

// Endpoint to get all villas
app.MapGet("/villaList", async (DBContext dbContext) =>
{
    List<Villa> villaList = await dbContext.Villas.ToListAsync();
    return villaList;
});
// Endpoint to show Json of hardcoded Villa
app.MapGet("/villa", () => new Villa
{
    VillaId = 1,
    VillaImageId = 1,
    Naam = "Villa",
    Omschrijving = "Villa is een prachtige villa in het hart van de Ardennen.",
    Prijs = 1000,
    Locatie = "Ardennen",
    Capaciteit = 10,
    Slaapkamers = 5,
    Badkamers = 3,
    Verkocht = 0,
    IsDeleted = 0,
    DeletedAt = null,
    Images = new List<Image>
    {
        new Image
        {
            VillaImageId = 1,
            ImageLocation = "https://www.villaverkener.be/images/villa.jpg",
            VillaId = 1
        }
    }
});

app.Run();

