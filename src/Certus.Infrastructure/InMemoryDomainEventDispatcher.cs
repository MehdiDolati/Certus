using Certus.Domain.SharedKernel;

namespace Certus.Infrastructure;

public class InMemoryDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
