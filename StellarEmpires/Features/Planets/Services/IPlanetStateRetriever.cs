using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Features.Planets.Services;

public interface IPlanetStateRetriever
{
    Task<Planet> GetInitialStateAsync(Guid planetId);
    Task<Planet> GetCurrentStateAsync(Guid planetId);
}
