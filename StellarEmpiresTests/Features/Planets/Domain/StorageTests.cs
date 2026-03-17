using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Helpers;

namespace StellarEmpires.Tests.Features.Planets.Domain;

[TestFixture]
public class StorageTests
{
    private DateTime _utcNow;

    [SetUp]
    public void Setup()
    {
        _utcNow = new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc);
        DateTimeProvider.SetUtcNow(() => _utcNow);
    }

    [TearDown]
    public void Teardown()
    {
        DateTimeProvider.ResetUtcNow();
    }

    [Test]
    public void Create_ShouldCreateStorageWithLevelOne()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var storageId = Guid.NewGuid();

        // Act
        var storage = Storage.Create(storageId, planetId);

        // Assert
        Assert.That(storage.Id, Is.EqualTo(storageId));
        Assert.That(storage.PlanetId, Is.EqualTo(planetId));
        Assert.That(storage.Level, Is.EqualTo(1));
    }

    [Test]
    public void GetTotalAdditionalCapacity_ShouldCalculateCorrectly()
    {
        // Arrange
        var storage = Storage.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        Assert.That(storage.GetTotalAdditionalCapacity(), Is.EqualTo(Storage.CapacityPerLevel)); // Level 1

        storage.Upgrade();
        Assert.That(storage.GetTotalAdditionalCapacity(), Is.EqualTo(Storage.CapacityPerLevel * 2)); // Level 2
    }

    [Test]
    public void Upgrade_ShouldIncrementLevel()
    {
        // Arrange
        var storage = Storage.Create(Guid.NewGuid(), Guid.NewGuid());
        Assert.That(storage.Level, Is.EqualTo(1));

        // Act
        storage.Upgrade();

        // Assert
        Assert.That(storage.Level, Is.EqualTo(2));
    }

    [Test]
    public void GetUpgradeCost_ShouldReturnCostForNextLevel()
    {
        // Arrange
        var storage = Storage.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var cost = storage.GetUpgradeCost(2);

        // Assert
        Assert.That(cost, Contains.Key(ResourceType.Metal));
        Assert.That(cost, Contains.Key(ResourceType.Crystal));
        Assert.That(cost, Contains.Key(ResourceType.Deuterium));

        // Total cost should be the base cost distributed
        var totalCost = cost[ResourceType.Metal] + cost[ResourceType.Crystal] + cost[ResourceType.Deuterium];
        Assert.That(totalCost, Is.GreaterThan(0));
    }

    [Test]
    public void GetUpgradeCost_ShouldThrowException_WhenNextLevelIsNotGreaterThanCurrent()
    {
        // Arrange
        var storage = Storage.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => storage.GetUpgradeCost(1));
        Assert.Throws<InvalidOperationException>(() => storage.GetUpgradeCost(0));
    }
}
