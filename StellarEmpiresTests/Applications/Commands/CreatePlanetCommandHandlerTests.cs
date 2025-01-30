using Moq;
using StellarEmpires.Application.Commands;
using StellarEmpires.Domain.Models;
using StellarEmpires.Events;
using StellarEmpires.Infrastructure.EventStore;
using StellarEmpires.Infrastructure.PlanetStore;

namespace StellarEmpires.Tests.Application.Commands;

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
}

