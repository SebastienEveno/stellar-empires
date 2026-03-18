using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Features.Mines.Events;
using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Helpers;

namespace StellarEmpires.Tests.Features.Mines.Domain;

[TestFixture]
public class MineProductionTests
{
    private Mine _metalMine;
    private Mine _crystalMine;
    private Mine _deuteriumMine;
    private Guid _planetId;
    private Guid _mineId;
    private DateTime _utcNow;

    [SetUp]
    public void Setup()
    {
        _planetId = Guid.NewGuid();
        _mineId = Guid.NewGuid();
        _metalMine = Mine.Create(_mineId, _planetId, ResourceType.Metal);
        _crystalMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Crystal);
        _deuteriumMine = Mine.Create(Guid.NewGuid(), _planetId, ResourceType.Deuterium);

        _utcNow = new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc);
        DateTimeProvider.SetUtcNow(() => _utcNow);
    }

    [TearDown]
    public void Teardown()
    {
        DateTimeProvider.ResetUtcNow();
    }

    [Test]
    public void CalculateProduction_ShouldThrowException_WhenHoursPassedIsZero()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _metalMine.CalculateProduction(0));
    }

    [Test]
    public void CalculateProduction_ShouldThrowException_WhenHoursPassedIsNegative()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _metalMine.CalculateProduction(-1));
    }

    [Test]
    public void CalculateProduction_ShouldReturnZero_WhenMineIsLevel0()
    {
        // Arrange
        Assert.That(_metalMine.Level, Is.EqualTo(0));

        // Act
        var production = _metalMine.CalculateProduction(1);

        // Assert
        Assert.That(production, Is.EqualTo(0));
    }

    [Test]
    public void CalculateProduction_ShouldCalculateCorrectly_ForMetalMine()
    {
        // Arrange - Upgrade mine to level 1
        _metalMine.Upgrade(new Dictionary<ResourceType, int>());

        // Act
        var productionPerHour = _metalMine.CalculateProduction(1);

        // Assert
        Assert.That(productionPerHour, Is.GreaterThan(0));
        Assert.That(productionPerHour, Is.EqualTo(_metalMine.ProductionRatePerHour));
    }

    [Test]
    public void CalculateProduction_ShouldScaleWithTime()
    {
        // Arrange
        _metalMine.Upgrade(new Dictionary<ResourceType, int>());
        var production1Hour = _metalMine.CalculateProduction(1);

        // Act
        var production2Hours = _metalMine.CalculateProduction(2);
        var production0_5Hours = _metalMine.CalculateProduction(0.5m);

        // Assert
        Assert.That(production2Hours, Is.EqualTo(production1Hour * 2));
        // 0.5 hours may have rounding, so we check it's close to half
        Assert.That(production0_5Hours, Is.LessThanOrEqualTo(production1Hour));
        Assert.That(production0_5Hours, Is.GreaterThanOrEqualTo(production1Hour / 2));
    }

    [Test]
    public void CalculateProduction_ShouldIncreaseWithLevel()
    {
        // Arrange
        _metalMine.Upgrade(new Dictionary<ResourceType, int>()); // Level 1
        var level1Production = _metalMine.CalculateProduction(1);

        // Act
        _metalMine.Upgrade(new Dictionary<ResourceType, int>()); // Level 2
        var level2Production = _metalMine.CalculateProduction(1);

        // Assert
        Assert.That(level2Production, Is.GreaterThan(level1Production));
    }

    [Test]
    public void ProduceResources_ShouldThrowException_WhenHoursPassedIsZero()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _metalMine.ProduceResources(0));
    }

    [Test]
    public void ProduceResources_ShouldThrowException_WhenHoursPassedIsNegative()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _metalMine.ProduceResources(-1));
    }

    [Test]
    public void ProduceResources_ShouldDoNothing_WhenMineIsLevel0()
    {
        // Arrange
        Assert.That(_metalMine.Level, Is.EqualTo(0));
        var initialEventCount = _metalMine.DomainEvents.Count;

        // Act
        _metalMine.ProduceResources(1);

        // Assert
        Assert.That(_metalMine.DomainEvents.Count, Is.EqualTo(initialEventCount));
    }

    [Test]
    public void ProduceResources_ShouldRaiseMineProductionDomainEvent()
    {
        // Arrange
        _metalMine.Upgrade(new Dictionary<ResourceType, int>());
        _metalMine.ClearDomainEvents();

        // Act
        _metalMine.ProduceResources(1);

        // Assert
        Assert.That(_metalMine.DomainEvents, Has.Count.GreaterThan(0));
        var productionEvent = _metalMine.DomainEvents.FirstOrDefault() as MineProductionDomainEvent;
        Assert.That(productionEvent, Is.Not.Null);
        Assert.That(productionEvent!.MineId, Is.EqualTo(_metalMine.Id));
        Assert.That(productionEvent.PlanetId, Is.EqualTo(_planetId));
        Assert.That(productionEvent.ResourceType, Is.EqualTo(ResourceType.Metal));
        Assert.That(productionEvent.AmountProduced, Is.GreaterThan(0));
        Assert.That(productionEvent.MineLevel, Is.EqualTo(1));
        Assert.That(productionEvent.HoursPassed, Is.EqualTo(1));
    }

    [Test]
    public void ProduceResources_ShouldIncludeCorrectProductionDetails()
    {
        // Arrange
        _metalMine.Upgrade(new Dictionary<ResourceType, int>());
        _metalMine.ClearDomainEvents();
        var hoursPassed = 2.5m;
        var expectedProduction = _metalMine.CalculateProduction(hoursPassed);

        // Act
        _metalMine.ProduceResources(hoursPassed);

        // Assert
        var productionEvent = _metalMine.DomainEvents.FirstOrDefault() as MineProductionDomainEvent;
        Assert.That(productionEvent, Is.Not.Null);
        Assert.That(productionEvent!.AmountProduced, Is.EqualTo((int)expectedProduction));
        Assert.That(productionEvent.ProductionRatePerHour, Is.EqualTo(_metalMine.ProductionRatePerHour));
        Assert.That(productionEvent.HoursPassed, Is.EqualTo(hoursPassed));
    }

    [Test]
    public void CalculateProduction_ForCrystalMine_ShouldHaveLowerRateThanMetal()
    {
        // Arrange
        _metalMine.Upgrade(new Dictionary<ResourceType, int>());
        _crystalMine.Upgrade(new Dictionary<ResourceType, int>());

        // Act
        var metalProduction = _metalMine.CalculateProduction(1);
        var crystalProduction = _crystalMine.CalculateProduction(1);

        // Assert
        Assert.That(crystalProduction, Is.LessThan(metalProduction));
    }

    [Test]
    public void CalculateProduction_ForDeuteriumMine_ShouldHaveLowerRateThanCrystal()
    {
        // Arrange
        _crystalMine.Upgrade(new Dictionary<ResourceType, int>());
        _deuteriumMine.Upgrade(new Dictionary<ResourceType, int>());

        // Act
        var crystalProduction = _crystalMine.CalculateProduction(1);
        var deuteriumProduction = _deuteriumMine.CalculateProduction(1);

        // Assert
        Assert.That(deuteriumProduction, Is.LessThan(crystalProduction));
    }

    [Test]
    public void ProduceResources_MultipleUpgrades_ShouldProduceMoreResources()
    {
        // Arrange
        _metalMine.Upgrade(new Dictionary<ResourceType, int>());
        var production1Level = _metalMine.CalculateProduction(1);

        _metalMine.Upgrade(new Dictionary<ResourceType, int>());
        var production2Level = _metalMine.CalculateProduction(1);

        _metalMine.Upgrade(new Dictionary<ResourceType, int>());
        var production3Level = _metalMine.CalculateProduction(1);

        // Assert
        Assert.That(production2Level, Is.GreaterThan(production1Level));
        Assert.That(production3Level, Is.GreaterThan(production2Level));
    }
}
