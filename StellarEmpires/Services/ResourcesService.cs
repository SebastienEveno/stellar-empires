using Microsoft.Extensions.Caching.Memory;
using StellarEmpires.Domain.Models;
using StellarEmpires.Services;

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
        foreach(var cost in costs)
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