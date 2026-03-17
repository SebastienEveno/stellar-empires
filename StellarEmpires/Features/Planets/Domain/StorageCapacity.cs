using StellarEmpires.Features.Mines.Domain;
using System.Text.Json.Serialization;

namespace StellarEmpires.Features.Planets.Domain;

/// <summary>
/// Represents the storage capacity for a specific resource type on a planet.
/// Tracks both the total capacity and current usage.
/// </summary>
public class StorageCapacity
{
    /// <summary>The type of resource this storage is for.</summary>
    public ResourceType ResourceType { get; private set; }

    /// <summary>The total storage capacity for this resource type.</summary>
    public int Capacity { get; private set; }

    /// <summary>Base storage capacity for all planets (can be upgraded with Storage building).</summary>
    public const int BaseCapacity = 100000;

    [JsonConstructor]
    private StorageCapacity(ResourceType resourceType, int capacity)
    {
        ResourceType = resourceType;
        Capacity = capacity;
    }

    /// <summary>
    /// Create a new storage capacity with the base capacity.
    /// </summary>
    public static StorageCapacity CreateWithBaseCapacity(ResourceType resourceType)
    {
        return new StorageCapacity(resourceType, BaseCapacity);
    }

    /// <summary>
    /// Create a storage capacity with a specified capacity.
    /// </summary>
    public static StorageCapacity Create(ResourceType resourceType, int capacity)
    {
        if (capacity <= 0)
        {
            throw new InvalidOperationException("Storage capacity must be greater than zero.");
        }

        return new StorageCapacity(resourceType, capacity);
    }

    /// <summary>
    /// Calculate the remaining storage space for a given resource amount.
    /// </summary>
    public int GetRemainingCapacity(int currentAmount)
    {
        if (currentAmount < 0)
        {
            throw new InvalidOperationException("Current amount cannot be negative.");
        }

        if (currentAmount > Capacity)
        {
            return 0; // Storage is over capacity
        }

        return Capacity - currentAmount;
    }

    /// <summary>
    /// Check if adding the specified amount would exceed storage capacity.
    /// </summary>
    public bool WouldExceedCapacity(int currentAmount, int amountToAdd)
    {
        if (currentAmount < 0 || amountToAdd < 0)
        {
            throw new InvalidOperationException("Amounts cannot be negative.");
        }

        return currentAmount + amountToAdd > Capacity;
    }

    /// <summary>
    /// Check if storage is full (current amount equals capacity).
    /// </summary>
    public bool IsFull(int currentAmount)
    {
        return currentAmount >= Capacity;
    }

    /// <summary>
    /// Upgrade the storage capacity by the specified amount.
    /// </summary>
    public void UpgradeCapacity(int additionalCapacity)
    {
        if (additionalCapacity <= 0)
        {
            throw new InvalidOperationException("Additional capacity must be greater than zero.");
        }

        Capacity += additionalCapacity;
    }

    /// <summary>
    /// Get the capacity utilization percentage (0-100).
    /// </summary>
    public int GetUtilizationPercentage(int currentAmount)
    {
        if (currentAmount < 0)
        {
            throw new InvalidOperationException("Current amount cannot be negative.");
        }

        if (Capacity == 0)
        {
            return 0;
        }

        return (int)((double)currentAmount / Capacity * 100);
    }
}
