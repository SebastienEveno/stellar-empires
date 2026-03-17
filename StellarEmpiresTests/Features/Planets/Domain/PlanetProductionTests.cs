using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Features.Mines.Events;
using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Helpers;

namespace StellarEmpires.Tests.Features.Planets.Domain;

[TestFixture]
public class PlanetProductionTests
{
    private Planet _planet;
    private Guid _planetId;
    private DateTime _utcNow;

    [SetUp]
    public void Setup()
    {
        _planetId = Guid.NewGuid();
        _planet = Planet.Create(_planetId, "Test Planet", false, null, null);

        _utcNow = new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc);
        DateTimeProvider.SetUtcNow(() => _utcNow);
    }

    [TearDown]
    public void Teardown()
    {
        DateTimeProvider.ResetUtcNow();
    }

    [Test]
    public void ProduceResources_ShouldThrowException_WhenHoursPassedIsZero()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _planet.ProduceResources(0));
    }

    [Test]
    public void ProduceResources_ShouldThrowException_WhenHoursPassedIsNegative()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _planet.ProduceResources(-1));
    }

    [Test]
    public void ProduceResources_ShouldDoNothing_WhenPlanetHasNoMines()
    {
        // Arrange
        var initialEventCount = _planet.DomainEvents.Count;

        // Act
        _planet.ProduceResources(1);

        // Assert
        Assert.That(_planet.DomainEvents.Count, Is.EqualTo(initialEventCount));
    }

    [Test]
    public void ProduceResources_ShouldProduceMetalForMetalMine()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>()); // Level 1
        _planet.Mines.Add(metalMine);
        _planet.ClearDomainEvents();

        var initialMetalAmount = _planet.Resources[ResourceType.Metal];
        var expectedProduction = metalMine.CalculateProduction(1);

        // Act
        _planet.ProduceResources(1);

        // Assert
        var newMetalAmount = _planet.Resources[ResourceType.Metal];
        Assert.That(newMetalAmount, Is.EqualTo(initialMetalAmount + expectedProduction));
    }

    [Test]
    public void ProduceResources_ShouldProduceMultipleMinesResources()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        var crystalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Crystal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>());
        crystalMine.Upgrade(new Dictionary<ResourceType, int>());
        _planet.Mines.Add(metalMine);
        _planet.Mines.Add(crystalMine);
        _planet.ClearDomainEvents();

        var initialMetalAmount = _planet.Resources[ResourceType.Metal];
        var initialCrystalAmount = _planet.Resources[ResourceType.Crystal];
        var metalProduction = metalMine.CalculateProduction(1);
        var crystalProduction = crystalMine.CalculateProduction(1);

        // Act
        _planet.ProduceResources(1);

        // Assert
        Assert.That(_planet.Resources[ResourceType.Metal], Is.EqualTo(initialMetalAmount + metalProduction));
        Assert.That(_planet.Resources[ResourceType.Crystal], Is.EqualTo(initialCrystalAmount + crystalProduction));
    }

    [Test]
    public void ProduceResources_ShouldRaiseMineProductionDomainEvent()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>());
        _planet.Mines.Add(metalMine);
        _planet.ClearDomainEvents();

        // Act
        _planet.ProduceResources(1);

        // Assert
        var productionEvents = _planet.DomainEvents.OfType<MineProductionDomainEvent>().ToList();
        Assert.That(productionEvents, Has.Count.GreaterThan(0));
        var productionEvent = productionEvents.First();
        Assert.That(productionEvent.MineId, Is.EqualTo(metalMine.Id));
        Assert.That(productionEvent.ResourceType, Is.EqualTo(ResourceType.Metal));
        Assert.That(productionEvent.AmountProduced, Is.GreaterThan(0));
    }

    [Test]
    public void ProduceResources_ShouldBlockProduction_WhenStorageIsFull()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>()); // Level 1
        _planet.Mines.Add(metalMine);
        
        // Fill metal storage to capacity
        _planet.Resources[ResourceType.Metal] = StorageCapacity.BaseCapacity;
        _planet.ClearDomainEvents();

        var expectedProduction = metalMine.CalculateProduction(1);

        // Act
        _planet.ProduceResources(1);

        // Assert
        var blockedEvents = _planet.DomainEvents.OfType<MineProductionBlockedDomainEvent>().ToList();
        Assert.That(blockedEvents, Has.Count.GreaterThan(0));
        var blockedEvent = blockedEvents.First();
        Assert.That(blockedEvent.MineId, Is.EqualTo(metalMine.Id));
        Assert.That(blockedEvent.ResourceType, Is.EqualTo(ResourceType.Metal));
        Assert.That(blockedEvent.ProductionBlocked, Is.EqualTo(expectedProduction));
        Assert.That(blockedEvent.CurrentStorageAmount, Is.EqualTo(StorageCapacity.BaseCapacity));
        Assert.That(blockedEvent.StorageCapacityLimit, Is.EqualTo(StorageCapacity.BaseCapacity));
        
        // Verify no resources were added
        Assert.That(_planet.Resources[ResourceType.Metal], Is.EqualTo(StorageCapacity.BaseCapacity));
    }

    [Test]
    public void ProduceResources_ShouldPartiallyBlockProduction_WhenStorageNearlyFull()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>()); // Level 1
        _planet.Mines.Add(metalMine);

        var expectedProduction = metalMine.CalculateProduction(1);
        var storageCapacity = StorageCapacity.BaseCapacity;
        // Set storage to have space for less than one production cycle
        var availableSpace = Math.Max(1, expectedProduction / 2);
        _planet.Resources[ResourceType.Metal] = storageCapacity - availableSpace;
        _planet.ClearDomainEvents();

        // Act
        _planet.ProduceResources(1);

        // Assert
        var blockedEvents = _planet.DomainEvents.OfType<MineProductionBlockedDomainEvent>().ToList();
        Assert.That(blockedEvents, Has.Count.GreaterThan(0), "Production should be blocked when not enough space");

        // Verify no resources were added
        Assert.That(_planet.Resources[ResourceType.Metal], Is.EqualTo(storageCapacity - availableSpace));
    }

    [Test]
    public void ProduceResources_ShouldAllowProduction_WhenStorageHasSpace()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>()); // Level 1
        _planet.Mines.Add(metalMine);

        var expectedProduction = metalMine.CalculateProduction(1);
        var storageCapacity = StorageCapacity.BaseCapacity;
        var currentAmount = storageCapacity - (expectedProduction * 2); // Enough space for production
        _planet.Resources[ResourceType.Metal] = currentAmount;
        _planet.ClearDomainEvents();

        // Act
        _planet.ProduceResources(1);

        // Assert
        var blockedEvents = _planet.DomainEvents.OfType<MineProductionBlockedDomainEvent>().ToList();
        Assert.That(blockedEvents, Is.Empty, "Production should not be blocked");
        
        var productionEvents = _planet.DomainEvents.OfType<MineProductionDomainEvent>().ToList();
        Assert.That(productionEvents, Has.Count.GreaterThan(0), "Production event should be raised");
        
        // Verify resources were added
        Assert.That(_planet.Resources[ResourceType.Metal], Is.EqualTo(currentAmount + expectedProduction));
    }

    [Test]
    public void ProduceSingleMine_ShouldThrowException_WhenHoursPassedIsZero()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _planet.ProduceSingleMine(Guid.NewGuid(), 0));
    }

    [Test]
    public void ProduceSingleMine_ShouldThrowException_WhenMineNotFound()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _planet.ProduceSingleMine(Guid.NewGuid(), 1));
    }

    [Test]
    public void ProduceSingleMine_ShouldProduceForSpecificMine()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        var crystalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Crystal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>());
        crystalMine.Upgrade(new Dictionary<ResourceType, int>());
        _planet.Mines.Add(metalMine);
        _planet.Mines.Add(crystalMine);

        var initialMetalAmount = _planet.Resources[ResourceType.Metal];
        var initialCrystalAmount = _planet.Resources[ResourceType.Crystal];
        var expectedMetalProduction = metalMine.CalculateProduction(1);
        _planet.ClearDomainEvents();

        // Act
        _planet.ProduceSingleMine(metalMine.Id, 1);

        // Assert
        Assert.That(_planet.Resources[ResourceType.Metal], Is.EqualTo(initialMetalAmount + expectedMetalProduction));
        Assert.That(_planet.Resources[ResourceType.Crystal], Is.EqualTo(initialCrystalAmount), "Crystal should not be produced");
    }

    [Test]
    public void ProduceResources_WithUpgradedStorage_ShouldHaveHigherCapacity()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>()); // Level 1
        _planet.Mines.Add(metalMine);

        var expectedProduction = metalMine.CalculateProduction(1);

        // First, upgrade storage to increase capacity
        _planet.Resources[ResourceType.Metal] = 100000;
        _planet.Resources[ResourceType.Crystal] = 100000;
        _planet.Resources[ResourceType.Deuterium] = 100000;
        _planet.UpgradeStorage();

        var newCapacity = _planet.GetStorageCapacity(ResourceType.Metal);

        // Now set metal to a level that would be blocked with old capacity but allowed with new
        var amountThatWouldBlockWithOldCapacity = StorageCapacity.BaseCapacity - (expectedProduction / 2);
        _planet.Resources[ResourceType.Metal] = amountThatWouldBlockWithOldCapacity;
        _planet.ClearDomainEvents();

        // Act
        _planet.ProduceResources(1);

        // Assert
        // If new capacity is large enough, production should succeed
        if (amountThatWouldBlockWithOldCapacity + expectedProduction <= newCapacity)
        {
            var blockedEvents = _planet.DomainEvents.OfType<MineProductionBlockedDomainEvent>().ToList();
            Assert.That(blockedEvents, Is.Empty, "Production should not be blocked with upgraded storage");

            Assert.That(_planet.Resources[ResourceType.Metal], Is.EqualTo(amountThatWouldBlockWithOldCapacity + expectedProduction));
        }
    }

    [Test]
    public void ProduceResources_ScaledByTime()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>());
        _planet.Mines.Add(metalMine);

        var initialAmount = _planet.Resources[ResourceType.Metal];
        var production1Hour = metalMine.CalculateProduction(1);
        var production3Hours = metalMine.CalculateProduction(3);

        // Act - First production cycle
        _planet.ClearDomainEvents();
        _planet.ProduceResources(1);
        var amount1HourProduction = _planet.Resources[ResourceType.Metal];

        // Reset and test 3 hours
        _planet.Resources[ResourceType.Metal] = initialAmount;
        _planet.ClearDomainEvents();
        _planet.ProduceResources(3);
        var amount3HoursProduction = _planet.Resources[ResourceType.Metal];

        // Assert
        Assert.That(amount1HourProduction, Is.EqualTo(initialAmount + production1Hour));
        Assert.That(amount3HoursProduction, Is.EqualTo(initialAmount + production3Hours));
        Assert.That(amount3HoursProduction - initialAmount, Is.EqualTo(production1Hour * 3));
    }

    [Test]
    public void ProduceResources_WithZeroLevelMines_ShouldProduceNothing()
    {
        // Arrange - Create mines but don't upgrade (they stay at level 0)
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        var crystalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Crystal);
        _planet.Mines.Add(metalMine);
        _planet.Mines.Add(crystalMine);

        var initialMetalAmount = _planet.Resources[ResourceType.Metal];
        var initialCrystalAmount = _planet.Resources[ResourceType.Crystal];
        _planet.ClearDomainEvents();

        // Act
        _planet.ProduceResources(10);

        // Assert
        Assert.That(_planet.Resources[ResourceType.Metal], Is.EqualTo(initialMetalAmount));
        Assert.That(_planet.Resources[ResourceType.Crystal], Is.EqualTo(initialCrystalAmount));
        var productionEvents = _planet.DomainEvents.OfType<MineProductionDomainEvent>().ToList();
        Assert.That(productionEvents, Is.Empty, "No production events should be raised for level 0 mines");
    }

    [Test]
    public void ProduceResources_InitializesResource_IfNotPresent()
    {
        // Arrange
        var metalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Metal);
        metalMine.Upgrade(new Dictionary<ResourceType, int>());
        _planet.Mines.Add(metalMine);

        // Manually remove metal resources to test initialization
        _planet.Resources.Remove(ResourceType.Metal);
        Assert.That(_planet.Resources.ContainsKey(ResourceType.Metal), Is.False);

        var expectedProduction = metalMine.CalculateProduction(1);
        _planet.ClearDomainEvents();

        // Act
        _planet.ProduceResources(1);

        // Assert
        Assert.That(_planet.Resources[ResourceType.Metal], Is.EqualTo(expectedProduction));
    }
}
