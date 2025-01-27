using Moq;
using StellarEmpires.Application.Queries;
using StellarEmpires.Domain.Models;
using StellarEmpires.Infrastructure.PlanetStore;

namespace StellarEmpires.Tests.Application.Queries;

[TestFixture]
public class PlanetQueryHandlerTests
{
	private Mock<IPlanetStore> _mockPlanetStore;
	private PlanetQueryHandler _handler;

	[SetUp]
	public void Setup()
	{
		_mockPlanetStore = new Mock<IPlanetStore>();
		_handler = new PlanetQueryHandler(_mockPlanetStore.Object);
	}

	[Test]
	public async Task Handle_ShouldReturnPlanetQueryModel_WhenPlanetExists()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var planet = Planet.Create(planetId, "Earth", false, null, null);

		_mockPlanetStore
			.Setup(store => store.GetPlanetByIdAsync(planetId))
			.ReturnsAsync(planet);

		// Act
		var result = await _handler.Handle(planetId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Id, Is.EqualTo(planetId));
		Assert.That(result.Name, Is.EqualTo("Earth"));
		Assert.That(result.IsColonized, Is.False);
		Assert.That(result.ColonizedBy, Is.Null);
		Assert.That(result.ColonizedAt, Is.Null);
	}

	[Test]
	public void Handle_ShouldThrowInvalidOperationException_WhenPlanetNotFound()
	{
		// Arrange
		var planetId = Guid.NewGuid();

		_mockPlanetStore
			.Setup(store => store.GetPlanetByIdAsync(planetId))
			.ReturnsAsync((Planet?)null);

		// Act & Assert
		var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.Handle(planetId));
		Assert.That(ex.Message, Is.EqualTo("Planet not found."));
	}
}


