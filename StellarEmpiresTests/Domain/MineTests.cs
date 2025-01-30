using StellarEmpires.Domain.Models;
using StellarEmpires.Events;

namespace StellarEmpires.Tests.Domain;

[TestFixture]
public class MineTests
{
	private Mine _mine;
	private Guid _mineId;
	private Guid _planetId;

	[SetUp]
	public void Setup()
	{
		_mineId = Guid.NewGuid();
		_planetId = Guid.NewGuid();
		_mine = Mine.Create(_mineId, _planetId, ResourceType.Metal);
	}

	[Test]
	public void Create_ShouldInitializeMineAndRaiseMineCreatedDomainEvent()
	{
		// Arrange
		var mineId = Guid.NewGuid();
		var planetId = Guid.NewGuid();
		var resourceType = ResourceType.Crystal;

		// Act
		var mine = Mine.Create(mineId, planetId, resourceType);

		// Assert
		Assert.That(mine.Id, Is.EqualTo(mineId));
		Assert.That(mine.PlanetId, Is.EqualTo(planetId));
		Assert.That(mine.ResourceType, Is.EqualTo(resourceType));
		Assert.That(mine.Level, Is.EqualTo(0));
		Assert.That(mine.BaseProductionRatePerHour, Is.EqualTo(20));
		Assert.That(mine.ProductionRatePerHour, Is.EqualTo(20));
		Assert.That(mine.DomainEvents.Count, Is.EqualTo(1), "One domain event should be raised.");
		Assert.That(mine.DomainEvents.First(), Is.InstanceOf<MineCreatedDomainEvent>(), "Raised event should be of type MineCreatedDomainEvent.");
	}

	[Test]
	public void Upgrade_ShouldIncreaseLevelAndProductionRateAndRaiseMineUpgradedDomainEvent()
	{
		// Arrange
		var initialLevel = _mine.Level;
		var initialProductionRate = _mine.ProductionRatePerHour;
		var upgradeCost = _mine.GetUpgradeCost(initialLevel);

		// Act
		_mine.Upgrade(upgradeCost);

		// Assert
		Assert.That(_mine.Level, Is.EqualTo(initialLevel + 1));
		Assert.That(_mine.ProductionRatePerHour, Is.GreaterThan(initialProductionRate));
		Assert.That(_mine.DomainEvents.Last(), Is.TypeOf<MineUpgradedDomainEvent>());
		var domainEvent = _mine.DomainEvents.Last() as MineUpgradedDomainEvent;
		Assert.That(domainEvent, Is.Not.Null);
		Assert.That(domainEvent.EntityId, Is.EqualTo(_mineId));
		Assert.That(domainEvent.NewLevel, Is.EqualTo(initialLevel + 1));
		Assert.That(domainEvent.NewProductionRatePerHour, Is.EqualTo(_mine.ProductionRatePerHour));
	}

	[Test]
	public void Apply_WhenMineCreatedDomainEvent_ShouldUpdateMineState()
	{
		// Arrange
		var domainEvent = new MineCreatedDomainEvent
		{
			EntityId = _mineId,
			ResourceType = ResourceType.Deuterium,
			BaseProductionRatePerHour = 10
		};

		// Act
		_mine.Apply(domainEvent);

		// Assert
		Assert.That(_mine.ResourceType, Is.EqualTo(ResourceType.Deuterium));
		Assert.That(_mine.BaseProductionRatePerHour, Is.EqualTo(10));
	}

	[Test]
	public void Apply_WhenMineUpgradedDomainEvent_ShouldUpdateMineState()
	{
		// Arrange
		var domainEvent = new MineUpgradedDomainEvent
		{
			EntityId = _mineId,
			PlanetId = _planetId,
			ResourceType = ResourceType.Metal,
			PreviousLevel = 0,
			NewLevel = 1,
			NewProductionRatePerHour = 33,
			ConsumedResources = new Dictionary<ResourceType, int>
			{
				{ ResourceType.Metal, 90 },
				{ ResourceType.Crystal, 22 }
			}
		};

		// Act
		_mine.Apply(domainEvent);

		// Assert
		Assert.That(_mine.Level, Is.EqualTo(1));
		Assert.That(_mine.ProductionRatePerHour, Is.EqualTo(33));
	}

	[Test]
	public void GetUpgradeCost_ShouldReturnCorrectCost()
	{
		// Arrange
		var level = 1;

		// Act
		var upgradeCost = _mine.GetUpgradeCost(level);

		// Assert
		Assert.That(upgradeCost[ResourceType.Metal], Is.EqualTo(90));
		Assert.That(upgradeCost[ResourceType.Crystal], Is.EqualTo(22));
	}
}
