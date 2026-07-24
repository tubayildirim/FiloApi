using Filo.Api.Endpoints;
using Filo.Api.Middlewares;
using Scalar.AspNetCore;
using Filo.Application;
using Filo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenApi();
builder.Services.AddApiServices(builder.Configuration);

builder.Services.Configure<Filo.Application.Common.Settings.CacheSettings>(
    builder.Configuration.GetSection(Filo.Application.Common.Settings.CacheSettings.SectionName));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Auto-migrate and seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Filo.Infrastructure.Persistence.AppDbContext>();
    try
    {
        context.Database.EnsureCreated();
        
        if (!context.Person.Any())
        {
            var persons = new List<Filo.Domain.Entities.Person>
            {
                new() { Name = "Ahmet", Surname = "Yıldırım", Tckn = "12345678901", Age = 34, Gender = "Erkek", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                new() { Name = "Ayşe", Surname = "Demir", Tckn = "12345678902", Age = 29, Gender = "Kadın", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                new() { Name = "Mehmet", Surname = "Kara", Tckn = "12345678903", Age = 41, Gender = "Erkek", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                new() { Name = "Zeynep", Surname = "Aydın", Tckn = "12345678904", Age = 27, Gender = "Kadın", CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                new() { Name = "Emre", Surname = "Çelik", Tckn = "12345678905", Age = 36, Gender = "Erkek", CreatedBy = "System", CreatedAt = DateTime.UtcNow }
            };

            context.Person.AddRange(persons);
            context.SaveChanges();

            Log.Information("Database successfully seeded with initial person data.");
        }

        if (!context.Vehicles.Any())
        {
            var persons = context.Person.ToList();
            context.Vehicles.AddRange(new List<Filo.Domain.Entities.Vehicle>
            {
                new() { Brand = "BMW", Model = "320i", Year = 2022, PlateNumber = "34BMW123", Color = "Black", FuelType = "Gasoline", TransmissionType = "Automatic", PersonId = persons[0].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                new() { Brand = "Audi", Model = "A4", Year = 2021, PlateNumber = "34AUD456", Color = "White", FuelType = "Diesel", TransmissionType = "Automatic", PersonId = persons[1].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                new() { Brand = "Mercedes-Benz", Model = "C200", Year = 2023, PlateNumber = "34MER789", Color = "Gray", FuelType = "Gasoline", TransmissionType = "Automatic", PersonId = persons[2].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                new() { Brand = "Toyota", Model = "Corolla", Year = 2020, PlateNumber = "34TOY012", Color = "Blue", FuelType = "Hybrid", TransmissionType = "Automatic", PersonId = persons[3].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow },
                new() { Brand = "Volkswagen", Model = "Golf", Year = 2019, PlateNumber = "34VW345", Color = "Red", FuelType = "Gasoline", TransmissionType = "Manual", PersonId = persons[4].Id, CreatedBy = "System", CreatedAt = DateTime.UtcNow }
            });
            context.SaveChanges();
            Log.Information("Database successfully seeded with initial vehicle data.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapVehicleEndpoints();
app.MapPersonEndpoints();
app.MapGet("/", (HttpContext context) => Results.Redirect("/scalar/v1"));

try
{
    Log.Information("Filo API baslatiliyor...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API baslatilirken kritik hata olustu.");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
