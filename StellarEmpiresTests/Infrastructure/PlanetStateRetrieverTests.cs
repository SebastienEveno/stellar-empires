using Moq;
using StellarEmpires.Domain.Models;
using StellarEmpires.Domain.Services;
using StellarEmpires.Events;
using StellarEmpires.Infrastructure.EventStore;
using StellarEmpires.Infrastructure.PlanetStore;
using StellarEmpires.Infrastructure;
using StellarEmpires.Helpers;

namespace StellarEmpires.Tests.Infrastructure;

public class PlanetStateRetrieverTests
{
	private IPlanetStateRetriever _planetStateRetriever;
	private Mock<IPlanetStore> _planetStore;
	private Mock<IEventStore> _eventStore;

	private DateTime _utcNow;

	[SetUp]
	public void Setup()
	{
		_planetStore = new Mock<IPlanetStore>();
		_eventStore = new Mock<IEventStore>();

		_planetStateRetriever = new PlanetStateRetriever(_planetStore.Object, _eventStore.Object);

		_utcNow = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => _utcNow);
	}

	[TearDown]
	public void Teardown()
	{
		DateTimeProvider.ResetUtcNow();
	}

	[Test]
	public void GetInitialStateAsync_PlanetNotFound_ThrowsInvalidOperationException()
	{
		// Arrange
		_planetStore
			.Setup(store => store.GetPlanetByIdAsync(It.IsAny<Guid>()))
			.ReturnsAsync((Planet)null);

		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await _planetStateRetriever.GetInitialStateAsync(Guid.NewGuid()));

		Assert.That(ex.Message, Is.EqualTo("Planet not found."));
	}

	[Test]
	public async Task GetInitialStateAsync_PlanetFound_ReturnsPlanet()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var mockPlanet = new Planet(planetId, "Earth", false, null, null);

		_planetStore
			.Setup(store => store.GetPlanetByIdAsync(planetId))
			.ReturnsAsync(mockPlanet);

		// Act
		var result = await _planetStateRetriever.GetInitialStateAsync(planetId);

		// Assert
		Assert.That(result, Is.EqualTo(mockPlanet));
	}

	[Test]
	public async Task GetCurrentStateAsync_AppliesEvents_ReturnsUpdatedPlanet()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var mockPlanet = new Planet(planetId, "Earth", false, null, null);

		_planetStore
			.Setup(store => store.GetPlanetByIdAsync(planetId))
			.ReturnsAsync(mockPlanet);

		var mockEvent = new PlanetColonizedDomainEvent
		{
			EntityId = planetId,
			PlayerId = Guid.NewGuid()
		};

		_eventStore
			.Setup(store => store.GetEventsAsync<Planet>(planetId))
			.ReturnsAsync(new List<IDomainEvent> { mockEvent });

		// Act
		var result = await _planetStateRetriever.GetCurrentStateAsync(planetId);

		// Assert
		Assert.That(result.IsColonized, Is.True);
		Assert.That(result.ColonizedBy, Is.EqualTo(mockEvent.PlayerId));
	}
}
