using StellarEmpires.Domain.Models;

namespace StellarEmpires.Domain.Services;

public interface IPlanetStateRetriever
{
	Task<Planet> GetInitialStateAsync(Guid planetId);
	Task<Planet> GetCurrentStateAsync(Guid planetId);
}
