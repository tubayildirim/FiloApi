using Filo.Domain.Interfaces;
using Filo.Infrastructure.Caching;
using Filo.Infrastructure.Persistence;
using Filo.Infrastructure.Persistence.Repositories;
using Filo.Infrastructure.Queues;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        
        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, DistributedCacheService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<Filo.Infrastructure.Queues.VehicleCreatedEventConsumer>();
            
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventBus, Filo.Infrastructure.Queues.MassTransitEventBus>();

        services.AddHttpClient("DefaultClient")
            .AddPolicyHandler(Filo.Infrastructure.Resilience.ResilienceExtensions.CreateRetryPolicy());

        return services;
    }
}
