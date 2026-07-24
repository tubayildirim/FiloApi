namespace Filo.Domain.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(string eventName, T message) where T : class;
}
