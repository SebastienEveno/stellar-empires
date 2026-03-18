using Microsoft.Extensions.Caching.Memory;
using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Features.Mines.Events;
using StellarEmpires.Features.Mines.Services;
using StellarEmpires.Features.Planets.Domain;

public class ResourcesService : IResourcesService
{
    private readonly IMemoryCache _memoryCache;
    // TODO: Consider resources configuration repository for this kind of info
    private readonly Dictionary<ResourceType, int> _initialResourceAmountPerResourceType = new()
    {
        { ResourceType.Metal, 100 },
        { ResourceType.Crystal, 50 },
        { ResourceType.Deuterium, 25 }
    };

    public ResourcesService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void IncreaseResources(IEnumerable<Mine> mines)
    {
        foreach (var mine in mines)
        {
            // Retrieve current value from the cache or set default value if not present
            var currentValue = GetCurrentResourceAmount(mine.ResourceType);

            // Increment resources based on the dynamic production rate of the mine (convert to production rate per second)
            currentValue += mine.ProductionRatePerHour / 3600;

            // Update value in the cache
            _memoryCache.Set(mine.ResourceType.ToString(), currentValue);
        }
    }

    /// <summary>
    /// Processes production from a planet's mines and updates cached resources.
    /// Handles both successful production and blocked production scenarios.
    /// </summary>
    public void ProcessPlanetProduction(Planet planet)
    {
        if (planet?.Mines == null || planet.Mines.Count == 0)
            return;

        foreach (var @event in planet.DomainEvents)
        {
            if (@event is MineProductionDomainEvent productionEvent)
            {
                // Add produced resources to cache
                var currentAmount = GetCurrentResourceAmount(productionEvent.ResourceType);
                _memoryCache.Set(productionEvent.ResourceType.ToString(), currentAmount + productionEvent.AmountProduced);
            }
            else if (@event is MineProductionBlockedDomainEvent blockedEvent)
            {
                // Log or handle blocked production (could notify player, etc.)
                // For now, we just track that production was blocked in the event
                // Production was not added to resources, so no cache update needed
            }
        }
    }

    private int GetCurrentResourceAmount(ResourceType resourceType)
    {
        return _memoryCache.TryGetValue(resourceType.ToString(), out int current)
                ? current
                : GetInitialResourceAmount(resourceType);
    }

    private int GetInitialResourceAmount(ResourceType resourceType)
    {
        return _initialResourceAmountPerResourceType.TryGetValue(resourceType, out int initialResourceAmount)
            ? initialResourceAmount
            : 0;
    }

    public bool HasEnoughResources(Dictionary<ResourceType, int> costs)
    {
        var hasEnoughResources = true;

        foreach (var cost in costs)
        {
            var resourceType = cost.Key;
            var currentValue = GetCurrentResourceAmount(resourceType);
            hasEnoughResources &= currentValue >= cost.Value;
        }

        return hasEnoughResources;
    }

    public void DeductResources(Dictionary<ResourceType, int> costs)
    {
        foreach (var cost in costs)
        {
            var resourceType = cost.Key;
            var currentValue = GetCurrentResourceAmount(resourceType);

            // Ensure we do not go below zero here
            currentValue -= Math.Min(currentValue, cost.Value);

            // Update value in the cache
            _memoryCache.Set(resourceType.ToString(), currentValue);
        }
    }
}