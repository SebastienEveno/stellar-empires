using StellarEmpires.Domain.Models;
using System.Text.Json;

namespace StellarEmpires.Infrastructure;

public class PlanetRepository
{
	private readonly string _planetConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "planets.json");
	private readonly IEventStore _eventStore;

	public PlanetRepository(IEventStore eventStore)
	{
		_eventStore = eventStore;
	}

	public async Task<Planet> GetByIdAsync(Guid planetId)
	{
		var planets = LoadPlanetsFromJson();
		var planetInitialState = planets.FirstOrDefault(p => p.Id == planetId);
		if (planetInitialState == null)
		{
			throw new ArgumentException("Planet not found.");
		}

		var planet = new Planet(planetInitialState.Id, planetInitialState.Name, planetInitialState.IsColonized, planetInitialState.ColonizedBy, planetInitialState.ColonizedAt);

		var events = await _eventStore.GetEventsAsync<Planet>(planetId);

		foreach (var @event in events)
		{
			planet.Apply(@event);
		}

		return planet;
	}

	private List<Planet> LoadPlanetsFromJson()
	{
		var json = File.ReadAllText(_planetConfigPath);

		var planets = JsonSerializer.Deserialize<List<Planet>>(json);

		return planets ?? new List<Planet>();
	}
}
