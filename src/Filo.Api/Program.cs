using Filo.Api.Endpoints;
using Filo.Api.Middlewares;
using Scalar.AspNetCore;
using Filo.Application;
using Filo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

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
    Filo.Infrastructure.Persistence.AppDbContextSeed.SeedData(context);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseMiddleware<Filo.Api.Middleware.MockAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapVehicleEndpoints();
app.MapPersonEndpoints();
app.MapVehicleMatchPersonEndpoints();
app.MapVehicleFuelEndpoints();
app.MapVehicleMaintenanceEndpoints();
app.MapVehicleServiceEndpoints();
app.MapVehicleInsuranceEndpoints();
app.MapVehicleTrafficFineEndpoints();
app.MapVehicleTollEndpoints();
app.MapGet("/docs", (HttpContext context) => Results.Redirect("/scalar/v1"));

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
