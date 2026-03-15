using Moq;
using StellarEmpires.Application.Commands;
using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Features.Planets.Services;
using StellarEmpires.Helpers;
using StellarEmpires.Infrastructure.EventStore;
using StellarEmpires.Shared.Events;

namespace StellarEmpires.Tests.Features.Planets.Commands;

[TestFixture]
public class ColonizePlanetCommandHandlerTests
{
    private ColonizePlanetCommandHandler _handler;

    private Mock<IPlanetStateRetriever> _planetStateRetrieverMock;
    private Mock<IEventStore> _eventStoreMock;

    private DateTime _utcNow;

    [SetUp]
    public void SetUp()
    {
        _planetStateRetrieverMock = new Mock<IPlanetStateRetriever>();
        _eventStoreMock = new Mock<IEventStore>();

        _handler = new ColonizePlanetCommandHandler(_planetStateRetrieverMock.Object, _eventStoreMock.Object);

        _utcNow = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTimeProvider.SetUtcNow(() => _utcNow);
    }

    [TearDown]
    public void Teardown()
    {
        DateTimeProvider.ResetUtcNow();
    }

    [Test]
    public async Task ColonizePlanetAsync_ShouldColonizePlanet_WhenPlanetExistsAndIsNotColonized()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "Test Planet", false, null, null);

        _planetStateRetrieverMock
            .Setup(r => r.GetCurrentStateAsync(planetId))
            .ReturnsAsync(planet);

        var command = new ColonizePlanetCommand { PlanetId = planetId, PlayerId = playerId };

        // Act
        await _handler.ColonizePlanetAsync(command);

        // Assert
        Assert.That(planet.IsColonized, Is.True);
        Assert.That(planet.ColonizedBy, Is.EqualTo(playerId));
        _eventStoreMock.Verify(e => e.SaveEventAsync<Planet>(It.IsAny<IDomainEvent>()), Times.Once);
    }

    [Test]
    public void ColonizePlanetAsync_ShouldThrowInvalidOperationException_WhenPlanetIsAlreadyColonized()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "Test Planet", true, playerId, _utcNow);

        _planetStateRetrieverMock
            .Setup(r => r.GetCurrentStateAsync(planetId))
            .ReturnsAsync(planet);

        var command = new ColonizePlanetCommand { PlanetId = planetId, PlayerId = playerId };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.ColonizePlanetAsync(command));
        Assert.That(ex.Message, Is.EqualTo("Planet is already colonized."));
        _eventStoreMock.Verify(e => e.SaveEventAsync<Planet>(It.IsAny<IDomainEvent>()), Times.Never);
    }
}
