using MiniAuth.Domain.Events;

namespace MiniAuth.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T domainEvent, CancellationToken ct = default) where T : DomainEvent;
}
