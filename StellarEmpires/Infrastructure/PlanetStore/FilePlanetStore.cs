using StellarEmpires.Domain.Models;
using System.Text.Json;

namespace StellarEmpires.Infrastructure.PlanetStore;

public class FilePlanetStore : IPlanetStore
{
    private readonly string _planetConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "PlanetStore", "planets.json");
	private readonly JsonSerializerOptions _jsonOptions;

    public FilePlanetStore()
    {
		_jsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		};
    }

	public async Task SavePlanetAsync(Planet planet)
	{
		// Ensure directory exists
		var directoryPath = Path.GetDirectoryName(_planetConfigPath);
		if (directoryPath != null && !Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}

		var planets = await LoadPlanetsAsync();
		var existingPlanet = planets.FirstOrDefault(p => p.Id == planet.Id);

		if (existingPlanet != null)
		{
			throw new InvalidOperationException("Planet already exists in the list.");
		}

		planets.Add(planet);
		await File.WriteAllTextAsync(_planetConfigPath, JsonSerializer.Serialize(planets, _jsonOptions));
	}

	public Task<List<Planet>> GetPlanetsAsync()
	{
		return LoadPlanetsAsync();
	}

	public async Task<Planet?> GetPlanetByIdAsync(Guid planetId)
	{
		var planets = await LoadPlanetsAsync();
		return planets.FirstOrDefault(p => p.Id == planetId);
	}

	private async Task<List<Planet>> LoadPlanetsAsync()
	{
		if (!File.Exists(_planetConfigPath))
		{
			return new List<Planet>();
		}

		var json = await File.ReadAllTextAsync(_planetConfigPath);
		return JsonSerializer.Deserialize<List<Planet>>(json, _jsonOptions) ?? new List<Planet>();
	}
}
