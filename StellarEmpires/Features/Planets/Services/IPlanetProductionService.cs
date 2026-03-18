namespace StellarEmpires.Features.Planets.Services;

/// <summary>
/// Service interface for managing planet production cycles.
/// </summary>
public interface IPlanetProductionService
{
    /// <summary>
    /// Triggers production for all planets in the game.
    /// </summary>
    /// <param name="hoursPassed">The number of hours of production to calculate for each mine.</param>
    Task ProduceForAllPlanetsAsync(decimal hoursPassed);

    /// <summary>
    /// Triggers production for a specific planet.
    /// </summary>
    /// <param name="planetId">The ID of the planet to produce for.</param>
    /// <param name="hoursPassed">The number of hours of production to calculate for each mine.</param>
    Task ProduceForPlanetAsync(Guid planetId, decimal hoursPassed);
}
