using StellarEmpires.Domain;
using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Features.Planets.Events;
using StellarEmpires.Shared.Events;
using System.Text.Json.Serialization;

namespace StellarEmpires.Features.Planets.Domain;

public class Planet : Entity
{
    public string Name { get; private set; }
    public bool IsColonized { get; private set; }
    public Guid? ColonizedBy { get; private set; }
    public DateTime? ColonizedAt { get; private set; }
    public List<Mine> Mines { get; private set; }
    public Dictionary<ResourceType, int> Resources { get; private set; }

    // Coordinates
    public Galaxy Galaxy { get; private set; }
    public int System { get; private set; }
    public int Slot { get; private set; }

    // Storage
    public Storage? StorageBuilding { get; private set; }
    public Dictionary<ResourceType, StorageCapacity> StorageCapacities { get; private set; }

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
        Dictionary<ResourceType, int> resources,
        Galaxy galaxy,
        int system,
        int slot,
        Storage? storageBuilding,
        Dictionary<ResourceType, StorageCapacity>? storageCapacities) : base(id)
    {
        Name = name;
        IsColonized = isColonized;
        ColonizedBy = colonizedBy;
        ColonizedAt = colonizedAt;
        Mines = mines ?? new List<Mine>();
        Resources = resources ?? new Dictionary<ResourceType, int>();
        Galaxy = galaxy;
        System = system;
        Slot = slot;
        StorageBuilding = storageBuilding;
        StorageCapacities = storageCapacities ?? InitializeDefaultStorageCapacities();
    }

    public static Planet Create(Guid id, string name, bool isColonized, Guid? colonizedBy, DateTime? colonizedAt, Galaxy galaxy = Galaxy.Unknown, int system = 1, int slot = 1)
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
        var storageCapacities = InitializeDefaultStorageCapacities();

        var planet = new Planet(id, name, isColonized, colonizedBy, colonizedAt, mines, resources, galaxy, system, slot, null, storageCapacities);
        var planetCreatedEvent = new PlanetCreatedDomainEvent
        {
            EntityId = planet.Id,
            PlanetName = planet.Name
        };

        planet.Apply(planetCreatedEvent);

        planet.AddDomainEvent(planetCreatedEvent);

        return planet;
    }

    private static Dictionary<ResourceType, StorageCapacity> InitializeDefaultStorageCapacities()
    {
        return new Dictionary<ResourceType, StorageCapacity>
        {
            { ResourceType.Metal, StorageCapacity.CreateWithBaseCapacity(ResourceType.Metal) },
            { ResourceType.Crystal, StorageCapacity.CreateWithBaseCapacity(ResourceType.Crystal) },
            { ResourceType.Deuterium, StorageCapacity.CreateWithBaseCapacity(ResourceType.Deuterium) }
        };
    }

    /// <summary>
    /// Get the total storage capacity for a resource type (base + storage building upgrades).
    /// </summary>
    public int GetStorageCapacity(ResourceType resourceType)
    {
        if (!StorageCapacities.ContainsKey(resourceType))
        {
            throw new InvalidOperationException($"No storage capacity defined for {resourceType}.");
        }

        var baseCapacity = StorageCapacities[resourceType].Capacity;
        var additionalCapacity = StorageBuilding?.GetTotalAdditionalCapacity() ?? 0;

        return baseCapacity + additionalCapacity;
    }

    /// <summary>
    /// Get the remaining storage capacity for a resource type.
    /// </summary>
    public int GetRemainingStorageCapacity(ResourceType resourceType)
    {
        if (!StorageCapacities.ContainsKey(resourceType))
        {
            throw new InvalidOperationException($"No storage capacity defined for {resourceType}.");
        }

        var currentAmount = Resources.ContainsKey(resourceType) ? Resources[resourceType] : 0;
        var totalCapacity = GetStorageCapacity(resourceType);

        return totalCapacity - currentAmount;
    }

    /// <summary>
    /// Check if storage is full for a specific resource type.
    /// </summary>
    public bool IsStorageFull(ResourceType resourceType)
    {
        if (!StorageCapacities.ContainsKey(resourceType))
        {
            return false;
        }

        var currentAmount = Resources.ContainsKey(resourceType) ? Resources[resourceType] : 0;
        var totalCapacity = GetStorageCapacity(resourceType);

        return currentAmount >= totalCapacity;
    }

    /// <summary>
    /// Check if adding resources would exceed storage capacity.
    /// </summary>
    public bool WouldExceedStorage(ResourceType resourceType, int amount)
    {
        var remaining = GetRemainingStorageCapacity(resourceType);
        return amount > remaining;
    }

    public void UpgradeMine(ResourceType resourceType)
    {
        var mine = Mines.SingleOrDefault(m => m.ResourceType == resourceType);
        if (mine == null)
        {
            throw new InvalidOperationException($"No mine of type {resourceType} exists on this planet.");
        }

        // Check if storage is full before allowing upgrade
        if (IsStorageFull(resourceType))
        {
            var storageFull = new StorageFullDomainEvent
            {
                EntityId = Id,
                ResourceType = resourceType,
                CurrentAmount = Resources.ContainsKey(resourceType) ? Resources[resourceType] : 0,
                Capacity = GetStorageCapacity(resourceType)
            };
            AddDomainEvent(storageFull);

            throw new InvalidOperationException($"Storage for {resourceType} is full. Production is prevented. Upgrade your storage building or reduce resources.");
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

    /// <summary>
    /// Upgrade the storage building to the next level.
    /// </summary>
    public void UpgradeStorage()
    {
        if (StorageBuilding == null)
        {
            // Create storage building if it doesn't exist
            StorageBuilding = Storage.Create(Guid.NewGuid(), Id);
        }

        var nextLevel = StorageBuilding.Level + 1;
        var upgradeCost = StorageBuilding.GetUpgradeCost(nextLevel);

        // Check if we have enough resources
        foreach (var cost in upgradeCost)
        {
            if (!Resources.ContainsKey(cost.Key) || Resources[cost.Key] < cost.Value)
            {
                throw new InvalidOperationException($"Not enough {cost.Key} to upgrade storage. Required: {cost.Value}");
            }
        }

        // Deduct resources
        foreach (var cost in upgradeCost)
        {
            Resources[cost.Key] -= cost.Value;
        }

        // Upgrade the storage building
        StorageBuilding.Upgrade();

        // Raise domain event
        var storageUpgradedEvent = new StorageUpgradedDomainEvent
        {
            EntityId = Id,
            NewLevel = StorageBuilding.Level,
            TotalAdditionalCapacity = StorageBuilding.GetTotalAdditionalCapacity()
        };

        AddDomainEvent(storageUpgradedEvent);
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

    public void Rename(string? newName, Guid playerId)
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

        if (@event is StorageUpgradedDomainEvent storageUpgradedEvent)
        {
            // Event already updated the StorageBuilding state
            // This is here for event replay consistency
        }
    }
}
