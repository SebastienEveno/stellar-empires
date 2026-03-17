using StellarEmpires.Domain;
using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Shared.Events;

namespace StellarEmpires.Features.Planets.Domain;

/// <summary>
/// Represents a Storage/Warehouse building on a planet.
/// Provides additional storage capacity for all resource types.
/// </summary>
public class Storage : Entity
{
    /// <summary>The ID of the planet this storage belongs to.</summary>
    public Guid PlanetId { get; private set; }

    /// <summary>The current level of the storage building.</summary>
    public int Level { get; private set; }

    /// <summary>Additional storage capacity per level (100 units per level).</summary>
    public const int CapacityPerLevel = 10000;

    private Storage(Guid id, Guid planetId, int level) : base(id)
    {
        PlanetId = planetId;
        Level = level;
    }

    /// <summary>
    /// Create a new Storage building for a planet.
    /// </summary>
    public static Storage Create(Guid id, Guid planetId)
    {
        return new Storage(id, planetId, 1);
    }

    /// <summary>
    /// Get the total additional capacity provided by this storage building.
    /// </summary>
    public int GetTotalAdditionalCapacity()
    {
        return Level * CapacityPerLevel;
    }

    /// <summary>
    /// Upgrade the storage building to the next level.
    /// </summary>
    public void Upgrade()
    {
        Level++;
    }

    /// <summary>
    /// Get the cost to upgrade the storage building to the next level.
    /// Returns a dictionary with resource type and amount needed.
    /// </summary>
    public Dictionary<ResourceType, int> GetUpgradeCost(int nextLevel)
    {
        if (nextLevel <= Level)
        {
            throw new InvalidOperationException("Next level must be greater than current level.");
        }

        // Base cost: 1000 per level (scales with level)
        var baseCost = (int)(1000 * Math.Pow(1.1, nextLevel - 1));

        return new Dictionary<ResourceType, int>
        {
            { ResourceType.Metal, (int)(baseCost * 0.6) },      // 60% Metal
            { ResourceType.Crystal, (int)(baseCost * 0.3) },    // 30% Crystal
            { ResourceType.Deuterium, (int)(baseCost * 0.1) }   // 10% Deuterium
        };
    }

    public override void Apply(IDomainEvent @event)
    {
        // Storage building events can be handled here
        // For now, events are applied at the Planet level
    }
}
