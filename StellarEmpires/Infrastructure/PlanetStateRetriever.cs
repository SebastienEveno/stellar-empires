using StellarEmpires.Domain.Models;
using StellarEmpires.Domain.Services;
using StellarEmpires.Infrastructure.EventStore;
using StellarEmpires.Infrastructure.PlanetStore;

namespace StellarEmpires.Infrastructure;

public class PlanetStateRetriever : IPlanetStateRetriever
{
	private readonly IPlanetStore _planetStore;
	private readonly IEventStore _eventStore;  // Event store to load events

	public PlanetStateRetriever(IPlanetStore planetStore, IEventStore eventStore)
	{
		_planetStore = planetStore;
		_eventStore = eventStore;
	}

	public async Task<Planet> GetCurrentStateAsync(Guid planetId)
	{
		var planet = await GetInitialStateAsync(planetId);

		var events = (await _eventStore.GetEventsAsync<Planet>(planet.Id))
			.OrderBy(e => e.OccurredOn)
			.ToList();

		foreach (var domainEvent in events)
		{
			planet.Apply(domainEvent);
		}

		return planet;
	}

	public async Task<Planet> GetInitialStateAsync(Guid planetId)
	{
		var planet = await _planetStore.GetPlanetByIdAsync(planetId);

		if (planet == null)
		{
			throw new InvalidOperationException("Planet not found.");
		}

		return planet;
	}
}
