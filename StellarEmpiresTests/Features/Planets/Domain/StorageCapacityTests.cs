using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Tests.Features.Planets.Domain;

[TestFixture]
public class StorageCapacityTests
{
    [Test]
    public void CreateWithBaseCapacity_ShouldSetBaseCapacity()
    {
        // Act
        var capacity = StorageCapacity.CreateWithBaseCapacity(ResourceType.Metal);

        // Assert
        Assert.That(capacity.ResourceType, Is.EqualTo(ResourceType.Metal));
        Assert.That(capacity.Capacity, Is.EqualTo(StorageCapacity.BaseCapacity));
    }

    [Test]
    public void Create_ShouldThrowException_WhenCapacityIsZeroOrNegative()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => StorageCapacity.Create(ResourceType.Metal, 0));
        Assert.Throws<InvalidOperationException>(() => StorageCapacity.Create(ResourceType.Metal, -100));
    }

    [Test]
    public void GetRemainingCapacity_ShouldCalculateCorrectly()
    {
        // Arrange
        var capacity = StorageCapacity.Create(ResourceType.Metal, 1000);

        // Act & Assert
        Assert.That(capacity.GetRemainingCapacity(0), Is.EqualTo(1000));
        Assert.That(capacity.GetRemainingCapacity(500), Is.EqualTo(500));
        Assert.That(capacity.GetRemainingCapacity(1000), Is.EqualTo(0));
    }

    [Test]
    public void WouldExceedCapacity_ShouldReturnCorrectly()
    {
        // Arrange
        var capacity = StorageCapacity.Create(ResourceType.Metal, 1000);

        // Act & Assert
        Assert.That(capacity.WouldExceedCapacity(800, 100), Is.False);  // 800 + 100 = 900 <= 1000
        Assert.That(capacity.WouldExceedCapacity(900, 101), Is.True);   // 900 + 101 = 1001 > 1000
        Assert.That(capacity.WouldExceedCapacity(1000, 1), Is.True);    // 1000 + 1 > 1000
    }

    [Test]
    public void IsFull_ShouldReturnCorrectly()
    {
        // Arrange
        var capacity = StorageCapacity.Create(ResourceType.Metal, 1000);

        // Act & Assert
        Assert.That(capacity.IsFull(500), Is.False);
        Assert.That(capacity.IsFull(1000), Is.True);
        Assert.That(capacity.IsFull(1001), Is.True); // Exceeding capacity still counts as full
    }

    [Test]
    public void UpgradeCapacity_ShouldIncreaseCapacity()
    {
        // Arrange
        var capacity = StorageCapacity.Create(ResourceType.Metal, 1000);

        // Act
        capacity.UpgradeCapacity(500);

        // Assert
        Assert.That(capacity.Capacity, Is.EqualTo(1500));
    }

    [Test]
    public void GetUtilizationPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var capacity = StorageCapacity.Create(ResourceType.Metal, 1000);

        // Act & Assert
        Assert.That(capacity.GetUtilizationPercentage(0), Is.EqualTo(0));
        Assert.That(capacity.GetUtilizationPercentage(500), Is.EqualTo(50));
        Assert.That(capacity.GetUtilizationPercentage(1000), Is.EqualTo(100));
    }
}
