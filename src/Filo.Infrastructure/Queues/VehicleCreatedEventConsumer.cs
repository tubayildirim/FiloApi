using Filo.Application.Features.Vehicles.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Filo.Infrastructure.Queues;

public class VehicleCreatedEventConsumer : IConsumer<VehicleCreatedEvent>
{
    private readonly ILogger<VehicleCreatedEventConsumer> _logger;

    public VehicleCreatedEventConsumer(ILogger<VehicleCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<VehicleCreatedEvent> context)
    {
        _logger.LogInformation("Araç oluşturuldu eventi yakalandı (MassTransit): Plaka: {PlateNumber}", context.Message.Vehicle.PlateNumber);
        return Task.CompletedTask;
    }
}
