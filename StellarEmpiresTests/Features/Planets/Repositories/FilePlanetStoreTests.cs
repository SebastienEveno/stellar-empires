using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Features.Planets.Repositories;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarEmpires.Tests.Features.Planets.Repositories;

[TestFixture]
public class FilePlanetStoreTests
{
    private MockFileSystem _fileSystem;
    private FilePlanetStore _planetStore;
    private static readonly JsonSerializerOptions TestJsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

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
        Assert.That(_fileSystem.Directory.Exists("Features/Planets/Repositories"), Is.True);
    }

    [Test]
    public async Task SavePlanetAsync_ShouldSavePlanetToFile()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Earth", false, null, null);

        // Act
        await _planetStore.SavePlanetAsync(planet);

        // Assert
        Assert.That(_fileSystem.File.Exists("Features/Planets/Repositories/planets.json"), Is.True, "The planet file should exist after saving a planet.");

        var fileContent = await _fileSystem.File.ReadAllTextAsync("Features/Planets/Repositories/planets.json");
        var planets = JsonSerializer.Deserialize<List<Planet>>(fileContent, TestJsonOptions);
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
        var fileContent = await _fileSystem.File.ReadAllTextAsync("Features/Planets/Repositories/planets.json");
        var planets = JsonSerializer.Deserialize<List<Planet>>(fileContent, TestJsonOptions);
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

    // Galaxy enum JSON serialization tests
    [Test]
    public async Task SavePlanetAsync_ShouldSerializeGalaxyEnumAsString_WithAndromeda()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "Andromeda Planet", false, null, null, Galaxy.Andromeda, 1, 1);

        // Act
        await _planetStore.SavePlanetAsync(planet);

        // Assert
        var fileContent = await _fileSystem.File.ReadAllTextAsync("Features/Planets/Repositories/planets.json");
        Assert.That(fileContent, Does.Contain("\"galaxy\": \"andromeda\""),
            "Galaxy enum should serialize as camelCase string 'andromeda'");
    }

    [Test]
    public async Task SavePlanetAsync_ShouldSerializeGalaxyEnumAsString_WithMilkyWay()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "MilkyWay Planet", false, null, null, Galaxy.MilkyWay, 2, 3);

        // Act
        await _planetStore.SavePlanetAsync(planet);

        // Assert
        var fileContent = await _fileSystem.File.ReadAllTextAsync("Features/Planets/Repositories/planets.json");
        Assert.That(fileContent, Does.Contain("\"galaxy\": \"milkyWay\""),
            "Galaxy enum should serialize as camelCase string 'milkyWay'");
    }

    [Test]
    public async Task SavePlanetAsync_ShouldSerializeGalaxyEnumAsString_WithUnknown()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "Unknown Planet", false, null, null, Galaxy.Unknown, 1, 1);

        // Act
        await _planetStore.SavePlanetAsync(planet);

        // Assert
        var fileContent = await _fileSystem.File.ReadAllTextAsync("Features/Planets/Repositories/planets.json");
        Assert.That(fileContent, Does.Contain("\"galaxy\": \"unknown\""),
            "Galaxy enum should serialize as camelCase string 'unknown'");
    }

    [Test]
    public async Task SavePlanetAsync_ShouldUsePropertyNamingPolicy_ForCoordinateProperties()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "Test Planet", false, null, null, Galaxy.MilkyWay, 5, 10);

        // Act
        await _planetStore.SavePlanetAsync(planet);

        // Assert
        var fileContent = await _fileSystem.File.ReadAllTextAsync("Features/Planets/Repositories/planets.json");
        // Properties should be in camelCase (system and slot not System and Slot)
        Assert.That(fileContent, Does.Contain("\"system\": 5"),
            "Property 'system' should use camelCase naming");
        Assert.That(fileContent, Does.Contain("\"slot\": 10"),
            "Property 'slot' should use camelCase naming");
    }

    [Test]
    public async Task GetPlanetsAsync_ShouldDeserializeGalaxyEnum_FromJsonString()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var originalPlanet = Planet.Create(planetId, "Test Planet", false, null, null, Galaxy.Andromeda, 3, 7);
        await _planetStore.SavePlanetAsync(originalPlanet);

        // Act
        var loadedPlanets = await _planetStore.GetPlanetsAsync();

        // Assert
        Assert.That(loadedPlanets, Has.Count.EqualTo(1));
        var loadedPlanet = loadedPlanets[0];
        Assert.That(loadedPlanet.Galaxy, Is.EqualTo(Galaxy.Andromeda),
            "Galaxy enum should correctly deserialize from JSON string 'andromeda'");
    }

    [Test]
    public async Task GetPlanetByIdAsync_ShouldRoundTripPlanetWithCoordinates_EnumValuesPreserved()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var originalPlanet = Planet.Create(planetId, "Round Trip Test", false, null, null, Galaxy.MilkyWay, 2, 5);

        // Act
        await _planetStore.SavePlanetAsync(originalPlanet);
        var loadedPlanet = await _planetStore.GetPlanetByIdAsync(planetId);

        // Assert
        Assert.That(loadedPlanet, Is.Not.Null);
        Assert.That(loadedPlanet!.Id, Is.EqualTo(originalPlanet.Id));
        Assert.That(loadedPlanet.Name, Is.EqualTo(originalPlanet.Name));
        Assert.That(loadedPlanet.Galaxy, Is.EqualTo(originalPlanet.Galaxy),
            "Galaxy enum should be preserved in round-trip serialization");
        Assert.That(loadedPlanet.System, Is.EqualTo(originalPlanet.System));
        Assert.That(loadedPlanet.Slot, Is.EqualTo(originalPlanet.Slot));
    }

    [Test]
    public async Task SavePlanetAsync_ShouldHandleMultiplePlanets_WithDifferentGalaxyValues()
    {
        // Arrange
        var planet1 = Planet.Create(Guid.NewGuid(), "Andromeda Planet", false, null, null, Galaxy.Andromeda, 1, 1);
        var planet2 = Planet.Create(Guid.NewGuid(), "MilkyWay Planet", false, null, null, Galaxy.MilkyWay, 2, 2);
        var planet3 = Planet.Create(Guid.NewGuid(), "Unknown Planet", false, null, null, Galaxy.Unknown, 3, 3);

        // Act
        await _planetStore.SavePlanetAsync(planet1);
        await _planetStore.SavePlanetAsync(planet2);
        await _planetStore.SavePlanetAsync(planet3);

        // Assert
        var loadedPlanets = await _planetStore.GetPlanetsAsync();
        Assert.That(loadedPlanets, Has.Count.EqualTo(3));

        var loadedPlanet1 = loadedPlanets.FirstOrDefault(p => p.Id == planet1.Id);
        var loadedPlanet2 = loadedPlanets.FirstOrDefault(p => p.Id == planet2.Id);
        var loadedPlanet3 = loadedPlanets.FirstOrDefault(p => p.Id == planet3.Id);

        Assert.That(loadedPlanet1!.Galaxy, Is.EqualTo(Galaxy.Andromeda));
        Assert.That(loadedPlanet2!.Galaxy, Is.EqualTo(Galaxy.MilkyWay));
        Assert.That(loadedPlanet3!.Galaxy, Is.EqualTo(Galaxy.Unknown));
    }

    [Test]
    public async Task SavePlanetAsync_ShouldPreserveCamelCasePropertyNames_OnRoundTrip()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var colonizerId = Guid.NewGuid();
        var colonizedAt = DateTime.UtcNow;
        var planet = Planet.Create(planetId, "Colonized Planet", true, colonizerId, colonizedAt, Galaxy.MilkyWay, 1, 5);

        // Act
        await _planetStore.SavePlanetAsync(planet);
        var fileContent = await _fileSystem.File.ReadAllTextAsync("Features/Planets/Repositories/planets.json");
        var loadedPlanets = await _planetStore.GetPlanetsAsync();

        // Assert
        // Verify JSON uses camelCase
        Assert.That(fileContent, Does.Contain("\"isColonized\""),
            "Property 'isColonized' should be in camelCase in JSON");
        Assert.That(fileContent, Does.Contain("\"colonizedBy\""),
            "Property 'colonizedBy' should be in camelCase in JSON");
        Assert.That(fileContent, Does.Contain("\"colonizedAt\""),
            "Property 'colonizedAt' should be in camelCase in JSON");

        // Verify deserialization works with camelCase
        Assert.That(loadedPlanets, Has.Count.EqualTo(1));
        var loadedPlanet = loadedPlanets[0];
        Assert.That(loadedPlanet.IsColonized, Is.True);
        Assert.That(loadedPlanet.ColonizedBy, Is.EqualTo(colonizerId));
    }

    [Test]
    public async Task SavePlanetAsync_ShouldUpdatePlanetWithCoordinates_WhenPlanetAlreadyExists()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var originalPlanet = Planet.Create(planetId, "Original", false, null, null, Galaxy.Andromeda, 1, 1);
        await _planetStore.SavePlanetAsync(originalPlanet);

        // Act
        var updatedPlanet = Planet.Create(planetId, "Updated", false, null, null, Galaxy.MilkyWay, 5, 10);
        await _planetStore.SavePlanetAsync(updatedPlanet);

        // Assert
        var loadedPlanet = await _planetStore.GetPlanetByIdAsync(planetId);
        Assert.That(loadedPlanet!.Name, Is.EqualTo("Updated"));
        Assert.That(loadedPlanet.Galaxy, Is.EqualTo(Galaxy.MilkyWay),
            "Galaxy enum should be updated when planet is re-saved");
        Assert.That(loadedPlanet.System, Is.EqualTo(5));
        Assert.That(loadedPlanet.Slot, Is.EqualTo(10));
    }
}
