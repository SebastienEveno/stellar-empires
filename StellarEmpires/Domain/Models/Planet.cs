using StellarEmpires.Events;
using System.Text.Json.Serialization;

namespace StellarEmpires.Domain.Models;

public class Planet : Entity
{
    public string Name { get; private set; }
    public bool IsColonized { get; private set; }
    public Guid? ColonizedBy { get; private set; }
    public DateTime? ColonizedAt { get; private set; }
    public List<Mine> Mines { get; private set; }
    public Dictionary<ResourceType, int> Resources { get; private set; }

    private static readonly Dictionary<ResourceType, int> InitialResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Metal, 500 }, // Initial amount of Metal
            { ResourceType.Crystal, 200 }, // Initial amount of Crystal
            { ResourceType.Deuterium, 100 } // Initial amount of Deuterium
        };

    [JsonConstructor]
    private Planet(
        Guid id,
        string name,
        bool isColonized,
        Guid? colonizedBy,
        DateTime? colonizedAt,
        List<Mine> mines,
        Dictionary<ResourceType, int> resources) : base(id)
    {
        Name = name;
        IsColonized = isColonized;
        ColonizedBy = colonizedBy;
        ColonizedAt = colonizedAt;
        Mines = mines ?? new List<Mine>();
        Resources = resources ?? new Dictionary<ResourceType, int>();
    }

    public static Planet Create(Guid id, string name, bool isColonized, Guid? colonizedBy, DateTime? colonizedAt)
    {
        if (!isColonized && (colonizedBy != null || colonizedAt != null))
        {
            throw new InvalidOperationException("If the planet is not colonized, colonizedBy and colonizedAt must be null.");
        }

        if (isColonized && (colonizedBy == null || colonizedAt == null))
        {
            throw new InvalidOperationException("If the planet is colonized, colonizedBy and colonizedAt must not be null.");
        }

        var mines = new List<Mine>
            {
                Mine.Create(Guid.NewGuid(), id, ResourceType.Metal),
                Mine.Create(Guid.NewGuid(), id, ResourceType.Crystal),
                Mine.Create(Guid.NewGuid(), id, ResourceType.Deuterium)
            };

        var resources = new Dictionary<ResourceType, int>(InitialResources);

        var planet = new Planet(id, name, isColonized, colonizedBy, colonizedAt, mines, resources);
        var planetCreatedEvent = new PlanetCreatedDomainEvent
        {
            EntityId = planet.Id,
            PlanetName = planet.Name
        };

        planet.Apply(planetCreatedEvent);

        planet.AddDomainEvent(planetCreatedEvent);

        return planet;
    }

    public void UpgradeMine(ResourceType resourceType)
    {
        var mine = Mines.SingleOrDefault(m => m.ResourceType == resourceType);
        if (mine == null)
        {
            throw new InvalidOperationException($"No mine of type {resourceType} exists on this planet.");
        }

        var upgradeCost = mine.GetUpgradeCost(mine.Level + 1);
        foreach (var cost in upgradeCost)
        {
            if (!Resources.ContainsKey(cost.Key) || Resources[cost.Key] < cost.Value)
            {
                throw new InvalidOperationException($"Not enough {cost.Key} to upgrade the {resourceType} mine.");
            }
        }

        // Deduct resources for the upgrade
        foreach (var cost in upgradeCost)
        {
            Resources[cost.Key] -= cost.Value;
        }

        mine.Upgrade(upgradeCost);
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
