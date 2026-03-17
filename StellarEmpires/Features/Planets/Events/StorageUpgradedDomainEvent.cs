using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Shared.Events;

namespace StellarEmpires.Features.Planets.Events;

/// <summary>
/// Domain event raised when a planet's storage building is upgraded.
/// </summary>
public class StorageUpgradedDomainEvent : IDomainEvent
{
    /// <summary>Unique identifier for this event.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The ID of the planet.</summary>
    public required Guid EntityId { get; init; }

    /// <summary>The new level of the storage building.</summary>
    public required int NewLevel { get; init; }

    /// <summary>The total additional capacity provided by the storage building.</summary>
    public required int TotalAdditionalCapacity { get; init; }

    /// <summary>When the event occurred.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>The type name of this event.</summary>
    public string EventType => nameof(StorageUpgradedDomainEvent);
}

/// <summary>
/// Domain event raised when a planet's storage capacity is full and production is prevented.
/// </summary>
public class StorageFullDomainEvent : IDomainEvent
{
    /// <summary>Unique identifier for this event.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The ID of the planet.</summary>
    public required Guid EntityId { get; init; }

    /// <summary>The resource type that reached capacity.</summary>
    public required ResourceType ResourceType { get; init; }

    /// <summary>The current stored amount.</summary>
    public required int CurrentAmount { get; init; }

    /// <summary>The storage capacity.</summary>
    public required int Capacity { get; init; }

    /// <summary>When the event occurred.</summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>The type name of this event.</summary>
    public string EventType => nameof(StorageFullDomainEvent);
}
