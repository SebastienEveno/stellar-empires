namespace StellarEmpires.Events;

public sealed record PlanetColonizedDomainEvent : IDomainEvent
{
	public Guid EntityId { get; init; }
	public Guid Id { get; init; }
	public DateTime OccurredOn { get; init; }
	public Guid PlayerId { get; init; }
	public DateTime ColonizedAt { get; init; }

	public PlanetColonizedDomainEvent(Guid planetId, Guid playerId, DateTime colonizedAt)
	{
		Id = Guid.NewGuid();
		OccurredOn = DateTime.UtcNow;
		EntityId = planetId;
		PlayerId = playerId;
		ColonizedAt = colonizedAt;
	}
}
