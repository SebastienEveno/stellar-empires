using StellarEmpires.Events;
using StellarEmpires.Helpers;

namespace StellarEmpires.Domain.Models;

public class Planet : Entity
{
	public string Name { get; private set; }
	public bool IsColonized { get; private set; }
	public Guid? ColonizedBy { get; private set; }
	public DateTime? ColonizedAt { get; private set; }

	public Planet(Guid id, string name, bool isColonized, Guid? colonizedBy, DateTime? colonizedAt) : base(id)
	{
		Name = name;
		IsColonized = isColonized;
		ColonizedBy = colonizedBy;
		ColonizedAt = colonizedAt;
	}

	public void Colonize(Guid playerId)
	{
		if (IsColonized)
		{
			throw new InvalidOperationException("Planet is already colonized.");
		}

		var colonizationEvent = new PlanetColonizedDomainEvent
		{
			EntityId = Id,
			PlayerId = playerId,
			ColonizedAt = DateTimeProvider.UtcNow
		};

		Apply(colonizationEvent);

		AddDomainEvent(colonizationEvent);
	}

	public override void Apply(IDomainEvent @event)
	{
		if (@event is PlanetColonizedDomainEvent planetColonizedDomainEvent)
		{
			IsColonized = true;
			ColonizedBy = planetColonizedDomainEvent.PlayerId;
			ColonizedAt = planetColonizedDomainEvent.ColonizedAt;
		}
	}
}
