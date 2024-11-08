using StellarEmpires.Domain.Models;

namespace StellarEmpires.Infrastructure.PlanetStore;

public interface IPlanetStore
{
	Task SavePlanetAsync(Planet planet);
	Task<List<Planet>> GetPlanetsAsync();
	Task<Planet?> GetPlanetByIdAsync(Guid planetId);
}
