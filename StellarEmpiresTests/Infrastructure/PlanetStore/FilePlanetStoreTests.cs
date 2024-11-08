using StellarEmpires.Domain.Models;
using StellarEmpires.Infrastructure.PlanetStore;

namespace StellarEmpires.Tests.Infrastructure.PlanetStore;

[TestFixture]
public class FilePlanetStoreTests
{
    private FilePlanetStore _planetStore;
    private string _filePath;

    [SetUp]
    public void Setup()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "PlanetStore", "planets.json");
        _planetStore = new FilePlanetStore();

		// Ensure the file is removed before each test to avoid any side effects
		if (File.Exists(_filePath))
		{
			File.Delete(_filePath);
		}
	}

    [TearDown]
    public void Cleanup()
    {
		// Clean up the planets.json file after each test, if it exists
		if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }

		// Clean up the directory if it's empty
		var directoryPath = Path.GetDirectoryName(_filePath);
		if (directoryPath != null && Directory.Exists(directoryPath) && !Directory.EnumerateFileSystemEntries(directoryPath).Any())
		{
			Directory.Delete(directoryPath);
		}
	}

    [Test]
    public async Task SavePlanetAsync_ShouldSavePlanet()
    {
        // Arrange
        var planet = new Planet(Guid.NewGuid(), "Earth", false, null, null);

        // Act
        await _planetStore.SavePlanetAsync(planet);

        // Assert
        Assert.That(File.Exists(_filePath), Is.True, "The planet file should exist after saving a planet.");

        var planets = await _planetStore.GetPlanetsAsync();
        Assert.That(planets, Has.Count.EqualTo(1));
        Assert.That(planets[0].Name, Is.EqualTo("Earth"));
    }

	[Test]
	public async Task SavePlanetAsync_ShouldThrowException_WhenPlanetAlreadyExists()
	{
		// Arrange
		var planet = new Planet(Guid.NewGuid(), "Earth", false, null, null);
		await _planetStore.SavePlanetAsync(planet);

		// Act & Assert
		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
		{
			// Try to save the same planet again
			await _planetStore.SavePlanetAsync(planet);
		});

		Assert.That(exception.Message, Is.EqualTo("Planet already exists in the list."));
	}

	[Test]
    public async Task GetPlanetsAsync_ShouldReturnSavedPlanets()
    {
        // Arrange
        var planet1 = new Planet(Guid.NewGuid(), "Earth", false, null, null);
        var planet2 = new Planet(Guid.NewGuid(), "Mars", false, null, null);
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
        var planet1 = new Planet(Guid.NewGuid(), "Earth", false, null, null);
        var planet2 = new Planet(Guid.NewGuid(), "Mars", false, null, null);
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
