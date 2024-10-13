using Moq;
using StellarEmpires.Domain.Models;
using StellarEmpires.Events;
using StellarEmpires.Helpers;
using StellarEmpires.Infrastructure;
using System.Reflection;
using System.Text.Json;

namespace StellarEmpires.Tests;

[TestFixture]
public class PlanetRepositoryTests
{
	private Mock<IEventStore> _eventStoreMock;
	private PlanetRepository _planetRepository;

	[SetUp]
	public void SetUp()
	{
		_eventStoreMock = new Mock<IEventStore>();
		_planetRepository = new PlanetRepository(_eventStoreMock.Object);
	}

	[Test]
	public async Task GetByIdAsync_ShouldReturnPlanet_WhenPlanetExists()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var planet = new Planet(planetId, "Earth", false, null, null);
		var planets = new List<Planet> { planet };
		var events = new List<IDomainEvent>();

		// Mock the IEventStore to return empty events
		_eventStoreMock
			.Setup(es => es.GetEventsAsync<Planet>(planetId))
			.ReturnsAsync(events);

		WritePlanetsToJson(planets);

		// Act
		var result = await _planetRepository.GetByIdAsync(planetId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Id, Is.EqualTo(planetId));
		Assert.That(result.Name, Is.EqualTo("Earth"));
		Assert.That(result.IsColonized, Is.False);
		Assert.That(result.ColonizedBy, Is.Null);
		Assert.That(result.ColonizedAt, Is.Null);
	}

	[Test]
	public void GetByIdAsync_ShouldThrowArgumentException_WhenPlanetDoesNotExist()
	{
		// Arrange
		var nonExistentPlanetId = Guid.NewGuid();
		var planets = new List<Planet>();

		WritePlanetsToJson(planets);

		// Act & Assert
		var ex = Assert.ThrowsAsync<ArgumentException>(() => _planetRepository.GetByIdAsync(nonExistentPlanetId));
		Assert.That(ex.Message, Is.EqualTo("Planet not found."));
	}

	[Test]
	public async Task GetByIdAsync_ShouldApplyEvents_WhenPlanetExists()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var playerId = Guid.NewGuid();
		var initialPlanet = new Planet(planetId, "Mars", false, null, null);
		var planets = new List<Planet> { initialPlanet };
		var overrideUtcNow = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => overrideUtcNow);
		var events = new List<IDomainEvent>
		{
			new PlanetColonizedDomainEvent(planetId, playerId, DateTimeProvider.UtcNow)
		};

		// Mock the IEventStore to return the defined events
		_eventStoreMock
			.Setup(es => es.GetEventsAsync<Planet>(planetId))
			.ReturnsAsync(events);

		WritePlanetsToJson(planets);

		// Act
		var result = await _planetRepository.GetByIdAsync(planetId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Id, Is.EqualTo(planetId));
		Assert.That(result.Name, Is.EqualTo("Mars"));
		Assert.That(result.IsColonized, Is.True);
		Assert.That(result.ColonizedBy, Is.EqualTo(playerId));
		Assert.That(result.ColonizedAt, Is.EqualTo(overrideUtcNow));
	}

	[TearDown]
	public void Teardown()
	{
		// Reset UtcNow after each test
		DateTimeProvider.ResetUtcNow();
	}

	private void WritePlanetsToJson(List<Planet> planets)
	{
		// Use reflection to set the private field _planetConfigPath for testing
		var fieldInfo = typeof(PlanetRepository).GetField("_planetConfigPath", BindingFlags.NonPublic | BindingFlags.Instance);
		fieldInfo.SetValue(_planetRepository, "path_to_your_planets.json");

		File.WriteAllText("path_to_your_planets.json", JsonSerializer.Serialize(planets));
	}
}
