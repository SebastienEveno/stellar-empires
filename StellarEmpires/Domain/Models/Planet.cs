using StellarEmpires.Events;
using System.Text.Json.Serialization;

namespace StellarEmpires.Domain.Models;

public class Planet : Entity
{
	public string Name { get; private set; }
	public bool IsColonized { get; private set; }
	public Guid? ColonizedBy { get; private set; }
	public DateTime? ColonizedAt { get; private set; }

	[JsonConstructor]
	private Planet(Guid id, string name, bool isColonized, Guid? colonizedBy, DateTime? colonizedAt) : base(id)
	{
		Name = name;
		IsColonized = isColonized;
		ColonizedBy = colonizedBy;
		ColonizedAt = colonizedAt;
	}

	public static Planet Create(Guid id, string name, bool isColonized, Guid? colonizedBy, DateTime? colonizedAt)
	{
		var planet = new Planet(id, name, isColonized, colonizedBy, colonizedAt);
		if (!isColonized && (colonizedBy != null || colonizedAt != null))
		{
			throw new InvalidOperationException("If the planet is not colonized, colonizedBy and colonizedAt must be null.");
		}

		if (isColonized && (colonizedBy == null || colonizedAt == null))
		{
			throw new InvalidOperationException("If the planet is colonized, colonizedBy and colonizedAt must not be null.");
		}

		var planetCreatedEvent = new PlanetCreatedDomainEvent
		{
			EntityId = planet.Id,
			PlanetName = planet.Name
		};
		planet.Apply(planetCreatedEvent);
		planet.AddDomainEvent(planetCreatedEvent);
		return planet;
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

	public void Rename(string newName, Guid playerId)
	{
		if (!IsColonized || playerId != ColonizedBy)
		{
			throw new InvalidOperationException("Only the player who colonized the planet can rename it.");
		}

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
		if (@event is PlanetCreatedDomainEvent planetCreatedDomainEvent)
		{
			Name = planetCreatedDomainEvent.PlanetName;
		}

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
