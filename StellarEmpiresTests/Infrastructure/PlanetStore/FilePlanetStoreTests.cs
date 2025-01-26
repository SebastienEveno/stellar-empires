using StellarEmpires.Domain.Models;
using StellarEmpires.Infrastructure.PlanetStore;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;

namespace StellarEmpires.Tests.Infrastructure.PlanetStore;

[TestFixture]
public class FilePlanetStoreTests
{
	private MockFileSystem _fileSystem;
	private FilePlanetStore _planetStore;

	[SetUp]
	public void Setup()
	{
		_fileSystem = new MockFileSystem();
		_planetStore = new FilePlanetStore(_fileSystem);
	}

	[Test]
	public async Task SavePlanetAsync_ShouldCreateDirectory_WhenDirectoryDoesNotExist()
	{
		// Arrange
		var planet = Planet.Create(Guid.NewGuid(), "New Planet", false, null, null);

		// Act
		await _planetStore.SavePlanetAsync(planet);

		// Assert
		Assert.That(_fileSystem.Directory.Exists("Infrastructure/PlanetStore"), Is.True);
	}

	[Test]
	public async Task SavePlanetAsync_ShouldSavePlanetToFile()
	{
		// Arrange
		var planet = Planet.Create(Guid.NewGuid(), "Earth", false, null, null);

		// Act
		await _planetStore.SavePlanetAsync(planet);

		// Assert
		Assert.That(_fileSystem.File.Exists("Infrastructure/PlanetStore/planets.json"), Is.True, "The planet file should exist after saving a planet.");

		var fileContent = await _fileSystem.File.ReadAllTextAsync("Infrastructure/PlanetStore/planets.json");
		var planets = JsonSerializer.Deserialize<List<Planet>>(fileContent);
		Assert.That(planets, Has.Count.EqualTo(1));
		Assert.That(planets[0].Id, Is.EqualTo(planet.Id));
		Assert.That(planets[0].Name, Is.EqualTo("Earth"));
	}

	[Test]
	public async Task SavePlanetAsync_ShouldUpdatePlanetState_WhenPlanetAlreadyExists()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var planet = Planet.Create(planetId, "Earth", false, null, null);
		await _planetStore.SavePlanetAsync(planet);

		// Act
		var updatedPlanet = Planet.Create(planetId, "Mars", true, Guid.NewGuid(), DateTime.UtcNow);
		await _planetStore.SavePlanetAsync(updatedPlanet);

		// Assert
		var fileContent = await _fileSystem.File.ReadAllTextAsync("Infrastructure/PlanetStore/planets.json");
		var planets = JsonSerializer.Deserialize<List<Planet>>(fileContent);
		Assert.That(planets, Has.Count.EqualTo(1));
		Assert.That(planets[0].Id, Is.EqualTo(updatedPlanet.Id));
		Assert.That(planets[0].Name, Is.EqualTo("Mars"));
		Assert.That(planets[0].IsColonized, Is.True);
		Assert.That(planets[0].ColonizedBy, Is.EqualTo(updatedPlanet.ColonizedBy));
		Assert.That(planets[0].ColonizedAt, Is.EqualTo(updatedPlanet.ColonizedAt));
	}

	[Test]
	public async Task GetPlanetsAsync_ShouldReturnSavedPlanets()
	{
		// Arrange
		var planet1 = Planet.Create(Guid.NewGuid(), "Earth", false, null, null);
		var planet2 = Planet.Create(Guid.NewGuid(), "Mars", false, null, null);
		await _planetStore.SavePlanetAsync(planet1);
		await _planetStore.SavePlanetAsync(planet2);

		// Act
		var planets = await _planetStore.GetPlanetsAsync();

		// Assert
		Assert.That(planets, Has.Count.EqualTo(2));
		Assert.That(planets[0].Name, Is.EqualTo("Earth"));
		Assert.That(planets[1].Name, Is.EqualTo("Mars"));
	}

	[Test]
	public async Task GetPlanetsAsync_ShouldReturnEmptyList_WhenNoPlanetIsSaved()
	{
		// Act
		var planets = await _planetStore.GetPlanetsAsync();

		// Assert
		Assert.That(planets, Is.Empty);
	}

	[Test]
	public async Task GetPlanetByIdAsync_ShouldReturnCorrectPlanet()
	{
		// Arrange
		var planet1 = Planet.Create(Guid.NewGuid(), "Earth", false, null, null);
		var planet2 = Planet.Create(Guid.NewGuid(), "Mars", false, null, null);
		await _planetStore.SavePlanetAsync(planet1);
		await _planetStore.SavePlanetAsync(planet2);

		// Act
		var result = await _planetStore.GetPlanetByIdAsync(planet1.Id);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result?.Name, Is.EqualTo("Earth"));
	}

	[Test]
	public async Task GetPlanetByIdAsync_ShouldReturnNull_WhenPlanetNotFound()
	{
		// Arrange
		var nonExistentId = Guid.NewGuid();

		// Act
		var result = await _planetStore.GetPlanetByIdAsync(nonExistentId);

		// Assert
		Assert.That(result, Is.Null, "Should return null when the planet does not exist.");
	}
}
