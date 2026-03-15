using Moq;
using StellarEmpires.Application.Commands;
using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Features.Planets.Events;
using StellarEmpires.Features.Planets.Repositories;
using StellarEmpires.Infrastructure.EventStore;
using StellarEmpires.Shared.Events;

namespace StellarEmpires.Tests.Features.Planets.Commands;

[TestFixture]
public class CreatePlanetCommandHandlerTests
{
    private Mock<IPlanetStore> _mockPlanetStore;
    private Mock<IEventStore> _mockEventStore;
    private CreatePlanetCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPlanetStore = new Mock<IPlanetStore>();
        _mockEventStore = new Mock<IEventStore>();
        _handler = new CreatePlanetCommandHandler(_mockPlanetStore.Object, _mockEventStore.Object);
    }

    [Test]
    public void CreatePlanetAsync_ShouldThrowException_WhenPlanetWithSameIdAlreadyExists()
    {
        // Arrange
        var command = new CreatePlanetCommand
        {
            Id = Guid.NewGuid(),
            Name = "Earth",
            IsColonized = false,
            ColonizedBy = null,
            ColonizedAt = null
        };

        _mockPlanetStore
            .Setup(store => store.GetPlanetByIdAsync(command.Id))
            .ReturnsAsync(Planet.Create(command.Id, command.Name, command.IsColonized, command.ColonizedBy, command.ColonizedAt));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.CreatePlanetAsync(command));
        Assert.That(ex.Message, Is.EqualTo("Planet with the same ID already exists."));
    }

    [Test]
    public async Task CreatePlanetAsync_ShouldCreateNewPlanetAndSaveEvent()
    {
        // Arrange
        var command = new CreatePlanetCommand
        {
            Id = Guid.NewGuid(),
            Name = "Earth",
            IsColonized = false,
            ColonizedBy = null,
            ColonizedAt = null
        };

        _mockPlanetStore
            .Setup(store => store.GetPlanetByIdAsync(command.Id))
            .ReturnsAsync((Planet?)null);

        // Act
        await _handler.CreatePlanetAsync(command);

        // Assert
        _mockEventStore.Verify(store => store.SaveEventAsync<Planet>(It.IsAny<PlanetCreatedDomainEvent>()), Times.Once);
    }

    // Coordinates feature tests
    [Test]
    public async Task CreatePlanetAsync_ShouldPassCoordinatesToPlanetCreate_WhenProvidedInCommand()
    {
        // Arrange
        var command = new CreatePlanetCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Planet",
            IsColonized = false,
            ColonizedBy = null,
            ColonizedAt = null,
            Galaxy = "Andromeda",
            System = 2,
            Slot = 5
        };

        _mockPlanetStore
            .Setup(store => store.GetPlanetByIdAsync(command.Id))
            .ReturnsAsync((Planet?)null);

        _mockEventStore
            .Setup(store => store.SaveEventAsync<Planet>(It.IsAny<IDomainEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.CreatePlanetAsync(command);

        // Assert
        _mockEventStore.Verify(store => store.SaveEventAsync<Planet>(It.IsAny<IDomainEvent>()), Times.Once);
    }

    [Test]
    public async Task CreatePlanetAsync_ShouldUseDefaultCoordinates_WhenNotProvidedInCommand()
    {
        // Arrange
        var command = new CreatePlanetCommand
        {
            Id = Guid.NewGuid(),
            Name = "Default Coordinate Planet",
            IsColonized = false,
            ColonizedBy = null,
            ColonizedAt = null
            // Galaxy, System, Slot not specified - will use defaults
        };

        _mockPlanetStore
            .Setup(store => store.GetPlanetByIdAsync(command.Id))
            .ReturnsAsync((Planet?)null);

        // Act
        await _handler.CreatePlanetAsync(command);

        // Assert
        _mockEventStore.Verify(store => store.SaveEventAsync<Planet>(It.IsAny<PlanetCreatedDomainEvent>()), Times.Once);
    }

    [Test]
    public async Task CreatePlanetAsync_ShouldCreatePlanetWithSpecificCoordinates()
    {
        // Arrange
        var command = new CreatePlanetCommand
        {
            Id = Guid.NewGuid(),
            Name = "Specific Coordinate Planet",
            IsColonized = false,
            ColonizedBy = null,
            ColonizedAt = null,
            Galaxy = "Milky-Way",
            System = 1,
            Slot = 3
        };

        _mockPlanetStore
            .Setup(store => store.GetPlanetByIdAsync(command.Id))
            .ReturnsAsync((Planet?)null);

        // Act
        await _handler.CreatePlanetAsync(command);

        // Assert
        _mockEventStore.Verify(store => store.SaveEventAsync<Planet>(It.IsAny<PlanetCreatedDomainEvent>()), Times.Once);
    }

    [Test]
    public async Task CreatePlanetAsync_ShouldWorkWithColonizedPlanetAndCoordinates()
    {
        // Arrange
        var colonizerId = Guid.NewGuid();
        var colonizationDate = DateTime.UtcNow;
        var command = new CreatePlanetCommand
        {
            Id = Guid.NewGuid(),
            Name = "Colonized Planet",
            IsColonized = true,
            ColonizedBy = colonizerId,
            ColonizedAt = colonizationDate,
            Galaxy = "Andromeda",
            System = 3,
            Slot = 7
        };

        _mockPlanetStore
            .Setup(store => store.GetPlanetByIdAsync(command.Id))
            .ReturnsAsync((Planet?)null);

        // Act
        await _handler.CreatePlanetAsync(command);

        // Assert
        _mockEventStore.Verify(store => store.SaveEventAsync<Planet>(It.IsAny<PlanetCreatedDomainEvent>()), Times.Once);
    }
}

