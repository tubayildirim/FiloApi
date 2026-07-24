using Filo.Domain.Interfaces;
using MassTransit;

namespace Filo.Infrastructure.Queues;

public class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(string eventName, T message) where T : class
    {
        await _publishEndpoint.Publish(message);
    }
}
