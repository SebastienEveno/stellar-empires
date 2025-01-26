using StellarEmpires.Helpers;

namespace StellarEmpires.Events;

public sealed record PlanetCreatedDomainEvent : IDomainEvent
{
	public string EventType => nameof(PlanetCreatedDomainEvent);

	public Guid Id => Guid.NewGuid();
	public DateTime OccurredOn { get; init; } = DateTimeProvider.UtcNow;

	public required Guid EntityId { get; init; }
	public required string PlanetName { get; init; }
}
