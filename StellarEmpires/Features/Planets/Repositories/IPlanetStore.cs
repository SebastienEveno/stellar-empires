using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Features.Planets.Repositories;

public interface IPlanetStore
{
    Task SavePlanetAsync(Planet planet);
    Task<List<Planet>> GetPlanetsAsync();
    Task<Planet?> GetPlanetByIdAsync(Guid planetId);
}
