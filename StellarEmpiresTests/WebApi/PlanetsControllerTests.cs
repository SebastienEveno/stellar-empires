using Microsoft.AspNetCore.Mvc;
using Moq;
using StellarEmpires.Application.Commands;
using StellarEmpires.Domain.Models;
using StellarEmpires.Domain.Services;
using StellarEmpires.Infrastructure.PlanetStore;
using StellarEmpires.WebApi.v1;
using StellarEmpires.WebApi.v1.Dtos;

namespace StellarEmpires.Tests.WebApi;

[TestFixture]
public class PlanetsControllerTests
{
	private Mock<IPlanetStateRetriever> _planetStateRetriever;
	private Mock<IPlanetStore> _planetStore;
	private Mock<IColonizePlanetCommandHandler> _colonizePlanetCommandHandler;
	private PlanetsController _controller;

	[SetUp]
	public void SetUp()
	{
		_planetStateRetriever = new Mock<IPlanetStateRetriever>();
		_planetStore = new Mock<IPlanetStore>();
		_colonizePlanetCommandHandler = new Mock<IColonizePlanetCommandHandler>();
		_controller = new PlanetsController(_planetStateRetriever.Object, _planetStore.Object, _colonizePlanetCommandHandler.Object);
	}

	[Test]
	public async Task GetInitialState_ShouldReturnInitialState_WhenPlanetExists()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var initialPlanet = new Planet(planetId, "Earth", false, null, null);
		_planetStateRetriever
			.Setup(x => x.GetInitialStateAsync(planetId))
			.ReturnsAsync(initialPlanet);

		// Act
		var result = await _controller.GetInitialState(planetId) as OkObjectResult;

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.StatusCode, Is.EqualTo(200));
		Assert.That(result.Value, Is.EqualTo(ReadPlanetDto.FromPlanet(initialPlanet)));
	}

	[Test]
	public async Task GetInitialState_ShouldReturnNotFound_WhenPlanetDoesNotExist()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		_planetStateRetriever
			.Setup(x => x.GetInitialStateAsync(planetId))
			.ThrowsAsync(new InvalidOperationException("Planet not found."));

		// Act
		var result = await _controller.GetInitialState(planetId);

		// Assert
		Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

		var notFoundResult = result as NotFoundObjectResult;
		Assert.That(notFoundResult, Is.Not.Null);
		Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
		Assert.That(notFoundResult.Value, Is.EqualTo("Planet not found."));
	}

	[Test]
	public async Task GetCurrentState_ShouldReturnCurrentState_WhenPlanetExists()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var currentPlanet = new Planet(planetId, "Mars", true, Guid.NewGuid(), DateTime.UtcNow);
		_planetStateRetriever
			.Setup(x => x.GetCurrentStateAsync(planetId))
			.ReturnsAsync(currentPlanet);

		// Act
		var result = await _controller.GetCurrentState(planetId) as OkObjectResult;

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.StatusCode, Is.EqualTo(200));
		Assert.That(result.Value, Is.EqualTo(ReadPlanetDto.FromPlanet(currentPlanet)));
	}

	[Test]
	public async Task GetCurrentState_ShouldReturnNotFound_WhenPlanetDoesNotExist()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		_planetStateRetriever
			.Setup(x => x.GetCurrentStateAsync(planetId))
			.ThrowsAsync(new InvalidOperationException("Planet not found."));

		// Act
		var result = await _controller.GetCurrentState(planetId);

		// Assert
		Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

		var notFoundResult = result as NotFoundObjectResult;
		Assert.That(notFoundResult, Is.Not.Null);
		Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
		Assert.That(notFoundResult.Value, Is.EqualTo("Planet not found."));
	}

	[Test]
	public async Task GetAllInitialStates_ShouldReturnAllPlanetsInitialStates()
	{
		// Arrange
		var planets = new List<Planet>
		{
			new Planet(Guid.NewGuid(), "Mercury", false, null, null),
			new Planet(Guid.NewGuid(), "Venus", false, null, null)
		};
		_planetStore
			.Setup(x => x.GetPlanetsAsync())
			.ReturnsAsync(planets);

		// Act
		var result = await _controller.GetAllInitialStates() as OkObjectResult;

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.StatusCode, Is.EqualTo(200));
		Assert.That(result.Value, Is.EqualTo(planets.Select(ReadPlanetDto.FromPlanet)));
	}

	[Test]
	public async Task AddPlanet_ShouldReturnCreated_WhenPlanetDoesNotExist()
	{
		// Arrange
		var createPlanetDto = new CreatePlanetDto
		{
			Name = "New Planet",
			IsColonized = false
		};

		// Mock the _planetStore to return null for existing planet
		_planetStore
			.Setup(store => store.GetPlanetByIdAsync(createPlanetDto.Id))
			.ReturnsAsync((Planet?)null);

		// Act
		var result = await _controller.AddPlanet(createPlanetDto);

		// Assert
		var createdResult = result as CreatedAtActionResult;
		Assert.That(createdResult, Is.Not.Null);
		Assert.That(createdResult.StatusCode, Is.EqualTo(201));
		Assert.That(createdResult.Value, Is.InstanceOf<ReadPlanetDto>());
		Assert.That(((ReadPlanetDto)createdResult.Value).Id, Is.EqualTo(createPlanetDto.Id));
		Assert.That(createdResult.RouteValues["planetId"], Is.EqualTo(createPlanetDto.Id));
	}

	[Test]
	public async Task AddPlanet_ShouldReturnBadRequest_WhenPlanetWithSameIdExists()
	{
		// Arrange
		var existingPlanetId = Guid.NewGuid();
		var createPlanetDto = new CreatePlanetDto
		{
			Id = existingPlanetId,
			Name = "New Planet",
			IsColonized = false
		};

		var existingPlanet = new Planet(existingPlanetId, "Existing Planet", false, null, null);

		_planetStore
			.Setup(store => store.GetPlanetByIdAsync(createPlanetDto.Id))
			.ReturnsAsync(existingPlanet);

		// Act
		var result = await _controller.AddPlanet(createPlanetDto);

		// Assert
		var badRequestResult = result as BadRequestObjectResult;
		Assert.That(badRequestResult, Is.Not.Null);
		Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
		Assert.That(badRequestResult.Value, Is.EqualTo("Planet with the same ID already exists."));
	}

	[Test]
	public async Task AddPlanet_ShouldCallSavePlanetAsync_WhenPlanetIsNew()
	{
		// Arrange
		var createPlanetDto = new CreatePlanetDto
		{
			Name = "New Planet",
			IsColonized = false
		};

		_planetStore
			.Setup(store => store.GetPlanetByIdAsync(createPlanetDto.Id))
			.ReturnsAsync((Planet?)null);

		_planetStore
			.Setup(store => store.SavePlanetAsync(It.IsAny<Planet>()))
			.Returns(Task.CompletedTask);

		// Act
		await _controller.AddPlanet(createPlanetDto);

		// Assert
		_planetStore.Verify(store => store.SavePlanetAsync(It.IsAny<Planet>()), Times.Once);
	}

	[Test]
	public async Task AddPlanet_ShouldReturnCorrectLocationHeader()
	{
		// Arrange
		var createPlanetDto = new CreatePlanetDto
		{
			Name = "New Planet",
			IsColonized = false
		};

		_planetStore
			.Setup(store => store.GetPlanetByIdAsync(createPlanetDto.Id))
			.ReturnsAsync((Planet?)null);

		// Act
		var result = await _controller.AddPlanet(createPlanetDto);

		// Assert
		var createdResult = result as CreatedAtActionResult;
		Assert.That(createdResult, Is.Not.Null);
		Assert.That(createdResult.RouteValues["planetId"], Is.EqualTo(createPlanetDto.Id));
		Assert.That(createdResult.Value, Is.InstanceOf<ReadPlanetDto>());
	}

	[Test]
	public async Task ColonizePlanet_ShouldReturnOk_WhenPlanetIsSuccessfullyColonized()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var playerId = Guid.NewGuid();

		_colonizePlanetCommandHandler
			.Setup(h => h.ColonizePlanetAsync(It.IsAny<ColonizePlanetCommand>()))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _controller.ColonizePlanet(planetId, playerId) as OkObjectResult;

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.StatusCode, Is.EqualTo(200));
		Assert.That(result.Value, Is.EqualTo("Planet successfully colonized."));
	}

	[Test]
	public async Task ColonizePlanet_ShouldReturnNotFound_WhenPlanetIsNotFound()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var playerId = Guid.NewGuid();

		_colonizePlanetCommandHandler
			.Setup(h => h.ColonizePlanetAsync(It.IsAny<ColonizePlanetCommand>()))
			.ThrowsAsync(new InvalidOperationException("Planet not found."));

		// Act
		var result = await _controller.ColonizePlanet(planetId, playerId) as NotFoundObjectResult;

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.StatusCode, Is.EqualTo(404));
		Assert.That(result.Value, Is.EqualTo("Planet not found."));
	}

	[Test]
	public async Task ColonizePlanet_ShouldReturnConflict_WhenPlanetIsAlreadyColonized()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var playerId = Guid.NewGuid();

		_colonizePlanetCommandHandler
			.Setup(h => h.ColonizePlanetAsync(It.IsAny<ColonizePlanetCommand>()))
			.ThrowsAsync(new InvalidOperationException("Planet is already colonized."));

		// Act
		var result = await _controller.ColonizePlanet(planetId, playerId) as ConflictObjectResult;

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.StatusCode, Is.EqualTo(409));
		Assert.That(result.Value, Is.EqualTo("Planet is already colonized."));
	}

	[Test]
	public async Task ColonizePlanet_ShouldReturnStatusCode500_WhenAnUnexpectedErrorOccurs()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var playerId = Guid.NewGuid();

		_colonizePlanetCommandHandler
			.Setup(h => h.ColonizePlanetAsync(It.IsAny<ColonizePlanetCommand>()))
			.ThrowsAsync(new Exception("Unexpected error."));

		// Act
		var result = await _controller.ColonizePlanet(planetId, playerId) as ObjectResult;

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.StatusCode, Is.EqualTo(500));
		Assert.That(result.Value, Is.EqualTo("Unexpected error."));
	}
}
