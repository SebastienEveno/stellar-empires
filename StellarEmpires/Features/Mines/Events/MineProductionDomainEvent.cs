using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Shared.Events;

namespace StellarEmpires.Features.Mines.Events;

/// <summary>
/// Domain event raised when a mine produces resources.
/// </summary>
public class MineProductionDomainEvent : IDomainEvent
{
    /// <summary>Unique identifier for this event.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The ID of the mine (used as EntityId for event tracking).</summary>
    public required Guid EntityId { get; init; }

    /// <summary>The ID of the mine.</summary>
    public required Guid MineId { get; init; }

    /// <summary>The ID of the planet where production occurred.</summary>
    public required Guid PlanetId { get; init; }

    /// <summary>The resource type being produced.</summary>
    public required ResourceType ResourceType { get; init; }

    /// <summary>The amount of resources produced.</summary>
    public required int AmountProduced { get; init; }

    /// <summary>The production rate per hour of the mine.</summary>
    public required int ProductionRatePerHour { get; init; }

    /// <summary>The level of the mine at time of production.</summary>
    public required int MineLevel { get; init; }

    /// <summary>The time period in hours for which production was calculated.</summary>
    public required decimal HoursPassed { get; init; }

    /// <summary>When the event occurred.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>The type name of this event.</summary>
    public string EventType => nameof(MineProductionDomainEvent);
}

/// <summary>
/// Domain event raised when mine production is blocked due to full storage.
/// </summary>
public class MineProductionBlockedDomainEvent : IDomainEvent
{
    /// <summary>Unique identifier for this event.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The ID of the planet (used as EntityId for event tracking).</summary>
    public required Guid EntityId { get; init; }

    /// <summary>The ID of the mine.</summary>
    public required Guid MineId { get; init; }

    /// <summary>The ID of the planet.</summary>
    public required Guid PlanetId { get; init; }

    /// <summary>The resource type that couldn't be produced.</summary>
    public required ResourceType ResourceType { get; init; }

    /// <summary>The amount that would have been produced.</summary>
    public required int ProductionBlocked { get; init; }

    /// <summary>The current storage amount.</summary>
    public required int CurrentStorageAmount { get; init; }

    /// <summary>The storage capacity limit.</summary>
    public required int StorageCapacityLimit { get; init; }

    /// <summary>When the event occurred.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>The type name of this event.</summary>
    public string EventType => nameof(MineProductionBlockedDomainEvent);
}
