using StellarEmpires.Domain.Models;

namespace StellarEmpires.Services;

public class MinesService : IMinesService
{
    private readonly List<Mine> _mines;
    private readonly int _initialMineLevel = 0;
    private readonly int _initialProductionRatePerHour = 0;
    private readonly ILogger<MinesService> _logger;
    private readonly IResourcesService _resourcesService;

    public MinesService(ILogger<MinesService> logger, IResourcesService resourcesService)
    {
        _mines = new List<Mine>
        {
            new Mine(ResourceType.Metal, _initialMineLevel, _initialProductionRatePerHour),
            new Mine(ResourceType.Crystal, _initialMineLevel, _initialProductionRatePerHour),
            new Mine(ResourceType.Deuterium, _initialMineLevel, _initialProductionRatePerHour)
        };
        _logger = logger;
        _resourcesService = resourcesService;
    }

    public IReadOnlyList<Mine> GetMines()
    {
        return _mines;
    }

    public void IncrementMineLevel(ResourceType resourceType)
    {
        var mine = _mines.FirstOrDefault(m => m.ResourceType == resourceType);
        
        if (mine != null)
        {
            var cost = GetMineLevelIncrementationCost(mine.Level);
            if (_resourcesService.HasEnoughResources(cost))
            {
                mine.Level++;
                mine.ProductionRatePerHour = GetProductionRatePerHour(mine.ResourceType, mine.Level); // 5 per second for level 1; 10 per second for level 2, etc.
                _resourcesService.DeductResources(cost);
            }
            else
            {
                _logger.LogInformation($"Not enough resources to increment mine level for {resourceType}");
            }
        }
        else
        {
            // Handle the case where the mine for the specified resource type is not found
            // You can throw an exception or log an error based on your requirements
            throw new InvalidOperationException($"Mine not found for resource type {resourceType}");
        }
    }

    private Dictionary<ResourceType, int> GetMineLevelIncrementationCost(int mineLevel)
    {
        var costs = new Dictionary<ResourceType, int>
        {
            { ResourceType.Metal, 30 * mineLevel * (int)Math.Pow(1.1, mineLevel) },
            { ResourceType.Crystal, 15 },
            { ResourceType.Deuterium, 10 }
        };

        return costs;
    }

    private int GetProductionRatePerHour(ResourceType resourceType, int mineLevel)
    {
        return resourceType switch
        {
            ResourceType.Metal => 30 * mineLevel * (int)Math.Pow(1.1, mineLevel),
            ResourceType.Crystal => 20 * mineLevel * (int)Math.Pow(1.1, mineLevel),
            ResourceType.Deuterium => 10 * mineLevel * (int)Math.Pow(1.1, mineLevel),
            _ => throw new NotImplementedException()
        };
    }
}