using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Filo.Domain.Interfaces;

namespace Filo.Infrastructure.Persistence.Outbox;

public class ProcessOutboxMessagesJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(IServiceProvider serviceProvider, ILogger<ProcessOutboxMessagesJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Message Processing Job is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing outbox messages.");
            }

            // Wait 2 seconds before next poll
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Fetch top 20 unprocessed outbox messages that haven't failed too many times
        var messages = await context.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < 3)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(stoppingToken);

        if (!messages.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages...", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var type = GetTypeFromName(message.Type);
                if (type == null)
                {
                    _logger.LogError("Could not load type {TypeName} for outbox message {MessageId}.", message.Type, message.Id);
                    message.Error = $"Type {message.Type} could not be resolved.";
                    message.ProcessedOnUtc = DateTime.UtcNow; // Mark as processed to prevent getting stuck
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, type);
                if (domainEvent == null)
                {
                    _logger.LogError("Could not deserialize outbox message {MessageId} content to {TypeName}.", message.Id, message.Type);
                    message.Error = "Deserialization returned null.";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                // Dynamically invoke PublishAsync<T> on IEventBus
                var method = typeof(IEventBus)
                    .GetMethod(nameof(IEventBus.PublishAsync))
                    ?.MakeGenericMethod(type);

                if (method == null)
                {
                    _logger.LogError("Could not find or create generic PublishAsync method on IEventBus.");
                    continue;
                }

                await (Task)method.Invoke(eventBus, new object[] { type.Name, domainEvent })!;

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;

                Filo.Common.Telemetry.ApplicationTelemetry.OutboxMessagesProcessedCounter.Add(1, 
                    new System.Collections.Generic.KeyValuePair<string, object?>("type", type.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {MessageId}.", message.Id);
                message.Error = ex.ToString();
                message.RetryCount++;

                Filo.Common.Telemetry.ApplicationTelemetry.OutboxMessagesFailedCounter.Add(1, 
                    new System.Collections.Generic.KeyValuePair<string, object?>("type", message.Type));
            }
        }

        await context.SaveChangesAsync(stoppingToken);
    }

    private static Type? GetTypeFromName(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null) return type;

        // Extract clean type name (without assembly info) for general assembly search if needed
        var cleanTypeName = typeName.Split(',')[0].Trim();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName) ?? assembly.GetType(cleanTypeName);
            if (type != null) return type;
        }

        return null;
    }
}
