using StellarEmpires.Features.Mines.Services;
using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Features.Planets.Repositories;

namespace StellarEmpires.Features.Planets.Services;

/// <summary>
/// Service responsible for managing planet production cycles.
/// Handles triggering mine production for all planets and processing resulting resources.
/// </summary>
public class PlanetProductionService : IPlanetProductionService
{
    private readonly IPlanetStore _planetStore;
    private readonly IResourcesService _resourcesService;

    public PlanetProductionService(IPlanetStore planetStore, IResourcesService resourcesService)
    {
        _planetStore = planetStore;
        _resourcesService = resourcesService;
    }

    /// <summary>
    /// Triggers production for all planets in the game.
    /// </summary>
    /// <param name="hoursPassed">The number of hours of production to calculate for each mine.</param>
    public async Task ProduceForAllPlanetsAsync(decimal hoursPassed)
    {
        if (hoursPassed <= 0)
            throw new InvalidOperationException("Hours passed must be greater than zero.");

        var planets = await _planetStore.GetPlanetsAsync();

        foreach (var planet in planets)
        {
            ProduceForPlanet(planet, hoursPassed);
        }
    }

    /// <summary>
    /// Triggers production for a specific planet.
    /// </summary>
    /// <param name="planetId">The ID of the planet to produce for.</param>
    /// <param name="hoursPassed">The number of hours of production to calculate for each mine.</param>
    public async Task ProduceForPlanetAsync(Guid planetId, decimal hoursPassed)
    {
        if (hoursPassed <= 0)
            throw new InvalidOperationException("Hours passed must be greater than zero.");

        var planet = await _planetStore.GetPlanetByIdAsync(planetId);
        if (planet == null)
            throw new InvalidOperationException($"Planet with ID {planetId} not found.");

        ProduceForPlanet(planet, hoursPassed);
    }

    private void ProduceForPlanet(Planet planet, decimal hoursPassed)
    {
        // Trigger production on the planet
        planet.ProduceResources(hoursPassed);

        // Process the production events and update cached resources
        _resourcesService.ProcessPlanetProduction(planet);
    }
}
