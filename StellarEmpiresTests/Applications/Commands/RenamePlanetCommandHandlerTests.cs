using Moq;
using StellarEmpires.Application.Commands;
using StellarEmpires.Domain.Models;
using StellarEmpires.Domain.Services;
using StellarEmpires.Events;
using StellarEmpires.Helpers;
using StellarEmpires.Infrastructure.EventStore;

namespace StellarEmpires.Tests.Applications.Commands;

[TestFixture]
public class RenamePlanetCommandHandlerTests
{
	private RenamePlanetCommandHandler _handler;

	private Mock<IPlanetStateRetriever> _planetStateRetrieverMock;
	private Mock<IEventStore> _eventStoreMock;

	private DateTime _utcNow;

	[SetUp]
	public void SetUp()
	{
		_planetStateRetrieverMock = new Mock<IPlanetStateRetriever>();
		_eventStoreMock = new Mock<IEventStore>();

		_handler = new RenamePlanetCommandHandler(_planetStateRetrieverMock.Object, _eventStoreMock.Object);

		_utcNow = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => _utcNow);
	}

	[TearDown]
	public void Teardown()
	{
		DateTimeProvider.ResetUtcNow();
	}

	[Test]
	public async Task RenamePlanetAsync_ShouldRenamePlanetAndSaveEvent_WhenPlanetExists()
	{
		// Arrange
		var colonizerId = Guid.NewGuid();
		var planetId = Guid.NewGuid();
		var planet = Planet.Create(planetId, "Test Planet", true, colonizerId, _utcNow);

		_planetStateRetrieverMock
			.Setup(x => x.GetCurrentStateAsync(planetId))
			.ReturnsAsync(planet);

		var newPlanetName = "New Planet Name";
		var renameCommand = new RenamePlanetCommand
		{
			PlayerId = colonizerId,
			PlanetId = planetId,
			PlanetName = newPlanetName
		};

		// Act
		await _handler.RenamePlanetAsync(renameCommand);

		// Assert
		_planetStateRetrieverMock.Verify(x => x.GetCurrentStateAsync(planetId), Times.Once);
		Assert.That(planet.DomainEvents.Last(), Is.TypeOf<PlanetRenamedDomainEvent>());
		_eventStoreMock.Verify(x => x.SaveEventAsync<Planet>(It.Is<PlanetRenamedDomainEvent>(e =>
			e.EntityId == planetId &&
			e.PlanetName == newPlanetName
		)), Times.Once);
	}

	[Test]
	public void RenamePlanetAsync_ShouldThrowInvalidOperationException_WhenPlanetDoesNotExist()
	{
		// Arrange
		var colonizerId = Guid.NewGuid();
		var planetId = Guid.NewGuid();
		var renameCommand = new RenamePlanetCommand
		{
			PlayerId = colonizerId,
			PlanetId = planetId,
			PlanetName = "New Planet Name"
		};

		_planetStateRetrieverMock
			.Setup(x => x.GetCurrentStateAsync(planetId))
			.ReturnsAsync((Planet)null);

		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _handler.RenamePlanetAsync(renameCommand));
		Assert.That(ex.Message, Is.EqualTo("Planet not found."));
		_eventStoreMock.Verify(x => x.SaveEventAsync<Planet>(It.IsAny<IDomainEvent>()), Times.Never);
	}

	[Test]
	public void RenamePlanetAsync_ShouldThrowInvalidOperationException_WhenPlayerIsNotColonizer()
	{
		// Arrange
		var colonizerId = Guid.NewGuid();
		var playerId = Guid.NewGuid();
		var planetId = Guid.NewGuid();
		var renameCommand = new RenamePlanetCommand
		{
			PlayerId = playerId,
			PlanetId = planetId,
			PlanetName = "New Planet Name"
		};

		var planet = Planet.Create(planetId, "Test Planet", true, colonizerId, _utcNow);
		_planetStateRetrieverMock
			.Setup(x => x.GetCurrentStateAsync(planetId))
			.ReturnsAsync(planet);

		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _handler.RenamePlanetAsync(renameCommand));
		Assert.That(ex.Message, Is.EqualTo("Only the player who colonized the planet can rename it."));
		_eventStoreMock.Verify(x => x.SaveEventAsync<Planet>(It.IsAny<IDomainEvent>()), Times.Never);
	}
}
