using StellarEmpires.Events;

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
			PlayerId = playerId
		};

		Apply(colonizationEvent);

		AddDomainEvent(colonizationEvent);
	}

	public void Rename(string newName)
	{
		if (string.IsNullOrEmpty(newName))
		{
			throw new InvalidOperationException("New name is either null or empty.");
		}

		var renameEvent = new PlanetRenamedDomainEvent
		{
			EntityId = Id,
			PlanetName = newName
		};

		Apply(renameEvent);

		AddDomainEvent(renameEvent);
	}

	public override void Apply(IDomainEvent @event)
	{
		if (@event is PlanetColonizedDomainEvent planetColonizedDomainEvent)
		{
			IsColonized = true;
			ColonizedBy = planetColonizedDomainEvent.PlayerId;
			ColonizedAt = planetColonizedDomainEvent.OccurredOn;
		}

		if (@event is PlanetRenamedDomainEvent planetRenamedDomainEvent)
		{
			Name = planetRenamedDomainEvent.PlanetName;
		}
	}
}
