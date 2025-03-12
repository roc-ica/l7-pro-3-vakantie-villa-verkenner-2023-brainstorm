using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MySql.EntityFrameworkCore.Extensions;
using VillaVerkenerAPI.Models.DB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not found.");
}

// Add DbContext and MySQL setup
builder.Services.AddDbContext<DBContext>(options =>
    options.UseMySQL(connectionString));

var app = builder.Build();

// Allow localhost
app.UseCors(policy =>
    policy.WithOrigins("http://localhost", "http://villaverkenner.local")
          .AllowAnyMethod()
          .AllowAnyHeader());

app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin))
    {
        Console.WriteLine($"Request from origin: {origin}");
    }
    await next();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
