using StellarEmpires.Domain.Models;
using System.IO.Abstractions;
using System.Text.Json;

namespace StellarEmpires.Infrastructure.PlanetStore;

public class FilePlanetStore : IPlanetStore
{
	private readonly IFileSystem _fileSystem;
	private readonly string _planetConfigPath;
	private readonly JsonSerializerOptions _jsonOptions;

	public FilePlanetStore(IFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
		_planetConfigPath = _fileSystem.Path.Combine("Infrastructure", "PlanetStore", "planets.json");
		_jsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		};
	}

	public async Task SavePlanetAsync(Planet planet)
	{
		// Ensure directory exists
		var directoryPath = _fileSystem.Path.GetDirectoryName(_planetConfigPath);
		if (directoryPath != null && !_fileSystem.Directory.Exists(directoryPath))
		{
			_fileSystem.Directory.CreateDirectory(directoryPath);
		}

		var planets = await LoadPlanetsAsync();
		var existingPlanet = planets.FirstOrDefault(p => p.Id == planet.Id);

		if (existingPlanet != null)
		{
			planets.Remove(existingPlanet);
			//throw new InvalidOperationException("Planet already exists in the list.");
		}

		planets.Add(planet);
		await _fileSystem.File.WriteAllTextAsync(_planetConfigPath, JsonSerializer.Serialize(planets, _jsonOptions));
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
		if (!_fileSystem.File.Exists(_planetConfigPath))
		{
			return new List<Planet>();
		}

		var json = await _fileSystem.File.ReadAllTextAsync(_planetConfigPath);
		return JsonSerializer.Deserialize<List<Planet>>(json, _jsonOptions) ?? new List<Planet>();
	}
}
