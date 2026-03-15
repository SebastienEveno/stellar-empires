using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Helpers;
using StellarEmpires.Shared.Events;

namespace StellarEmpires.Features.Mines.Events;

public class MineCreatedDomainEvent : IDomainEvent
{
    public string EventType => nameof(MineCreatedDomainEvent);

    public Guid Id => Guid.NewGuid();
    public DateTime OccurredOn => DateTimeProvider.UtcNow;

    public Guid EntityId { get; init; }
    public required ResourceType ResourceType { get; init; }
    public required int BaseProductionRatePerHour { get; init; }
}
