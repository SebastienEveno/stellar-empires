using StellarEmpires.Helpers;

namespace StellarEmpires.Events;

public sealed record PlanetColonizedDomainEvent : IDomainEvent
{
	public string EventType => nameof(PlanetColonizedDomainEvent);

	public Guid Id => Guid.NewGuid();
	public DateTime OccurredOn => DateTimeProvider.UtcNow;

	public required Guid EntityId { get; init; }
	public required Guid PlayerId { get; init; }
	public required DateTime ColonizedAt { get; init; }
}
