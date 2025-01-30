using StellarEmpires.Domain.Models;
using StellarEmpires.Infrastructure.PlanetStore;

namespace StellarEmpires.Application.Queries;

public class PlanetQueryHandler : IPlanetQueryHandler
{
	private readonly IPlanetStore _planetStore;

	public PlanetQueryHandler(IPlanetStore planetStore)
	{
		_planetStore = planetStore;
	}

	public async Task<Planet> Handle(Guid planetId)
	{
		var planet = await _planetStore.GetPlanetByIdAsync(planetId);

		if (planet == null)
		{
			throw new InvalidOperationException("Planet not found.");
		}

		return planet;
	}
}
