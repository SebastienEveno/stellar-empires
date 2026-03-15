using StellarEmpires.Helpers;

namespace StellarEmpires.Shared.Events;

public sealed record MockDomainEvent : IDomainEvent
{
    public string EventType => nameof(MockDomainEvent);

    public DateTime OccurredOn => DateTimeProvider.UtcNow;
    public Guid Id => Guid.NewGuid();
    public required Guid EntityId { get; init; }
}
