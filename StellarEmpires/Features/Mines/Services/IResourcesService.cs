using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Features.Mines.Services;

public interface IResourcesService
{
    void IncreaseResources(IEnumerable<Mine> mines);

    /// <summary>
    /// Processes production events from a planet and updates cached resources accordingly.
    /// Handles both successful production and blocked production scenarios.
    /// </summary>
    void ProcessPlanetProduction(Planet planet);

    bool HasEnoughResources(Dictionary<ResourceType, int> costs);
    void DeductResources(Dictionary<ResourceType, int> costs);
}