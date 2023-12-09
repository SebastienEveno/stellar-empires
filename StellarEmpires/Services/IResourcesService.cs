using StellarEmpires.Domain.Models;

namespace StellarEmpires.Services;

public interface IResourcesService
{
    void IncreaseResources(IEnumerable<Mine> mines);
    bool HasEnoughResources(Dictionary<ResourceType, int> costs);
    void DeductResources(Dictionary<ResourceType, int> costs);
}