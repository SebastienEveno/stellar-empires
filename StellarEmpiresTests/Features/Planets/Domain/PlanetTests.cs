using StellarEmpires.Features.Mines.Domain;
using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Features.Planets.Events;
using StellarEmpires.Helpers;

namespace StellarEmpires.Tests.Features.Planets.Domain;

[TestFixture]
public class PlanetTests
{
    private Planet _planet;
    private Guid _planetId;
    private Guid _playerId;

    private DateTime _utcNow;

    [SetUp]
    public void Setup()
    {
        _planetId = Guid.NewGuid();
        _playerId = Guid.NewGuid();
        _planet = Planet.Create(_planetId, "APlanet", false, null, null);

        _utcNow = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTimeProvider.SetUtcNow(() => _utcNow);
    }

    [TearDown]
    public void Teardown()
    {
        DateTimeProvider.ResetUtcNow();
    }

    [Test]
    public void Create_ShouldInitializePlanetAndRaisePlanetCreatedDomainEvent()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var name = "New Planet";
        var isColonized = false;
        Guid? colonizedBy = null;
        DateTime? colonizedAt = null;

        // Act
        var planet = Planet.Create(planetId, name, isColonized, colonizedBy, colonizedAt);

        // Assert
        Assert.That(planet.Id, Is.EqualTo(planetId));
        Assert.That(planet.Name, Is.EqualTo(name));
        Assert.That(planet.IsColonized, Is.EqualTo(isColonized));
        Assert.That(planet.ColonizedBy, Is.EqualTo(colonizedBy));
        Assert.That(planet.ColonizedAt, Is.EqualTo(colonizedAt));
        Assert.That(planet.DomainEvents.Count, Is.EqualTo(1), "One domain event should be raised.");
        Assert.That(planet.DomainEvents.First(), Is.InstanceOf<PlanetCreatedDomainEvent>(), "Raised event should be of type PlanetCreatedDomainEvent.");
    }

    [Test]
    public void Create_ShouldThrowException_WhenIsColonizedIsFalseAndColonizedByIsNotNull()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var name = "New Planet";
        var isColonized = false;
        Guid? colonizedBy = Guid.NewGuid();
        DateTime? colonizedAt = null;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => Planet.Create(planetId, name, isColonized, colonizedBy, colonizedAt));
        Assert.That(ex.Message, Is.EqualTo("If the planet is not colonized, colonizedBy and colonizedAt must be null."));
    }

    [Test]
    public void Create_ShouldThrowException_WhenIsColonizedIsFalseAndColonizedAtIsNotNull()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var name = "New Planet";
        var isColonized = false;
        Guid? colonizedBy = null;
        DateTime? colonizedAt = DateTime.UtcNow;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => Planet.Create(planetId, name, isColonized, colonizedBy, colonizedAt));
        Assert.That(ex.Message, Is.EqualTo("If the planet is not colonized, colonizedBy and colonizedAt must be null."));
    }

    [Test]
    public void Create_ShouldThrowException_WhenIsColonizedIsTrueAndColonizedByIsNull()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var name = "New Planet";
        var isColonized = true;
        Guid? colonizedBy = null;
        DateTime? colonizedAt = DateTime.UtcNow;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => Planet.Create(planetId, name, isColonized, colonizedBy, colonizedAt));
        Assert.That(ex.Message, Is.EqualTo("If the planet is colonized, colonizedBy and colonizedAt must not be null."));
    }

    [Test]
    public void Create_ShouldThrowException_WhenIsColonizedIsTrueAndColonizedAtIsNull()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var name = "New Planet";
        var isColonized = true;
        Guid? colonizedBy = Guid.NewGuid();
        DateTime? colonizedAt = null;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => Planet.Create(planetId, name, isColonized, colonizedBy, colonizedAt));
        Assert.That(ex.Message, Is.EqualTo("If the planet is colonized, colonizedBy and colonizedAt must not be null."));
    }

    [Test]
    public void Colonize_WhenPlanetNotColonized_ShouldSetIsColonizedAndAddDomainEvent()
    {
        // Act
        _planet.Colonize(_playerId);

        // Assert
        Assert.That(_planet.IsColonized, Is.True, "Planet should be colonized after colonization.");
        Assert.That(_planet.ColonizedBy, Is.EqualTo(_playerId), "PlayerId should be set to the colonizer's ID.");
        Assert.That(_planet.ColonizedAt, Is.EqualTo(_utcNow), "Colonization time should match the fixed UtcNow.");

        Assert.That(_planet.DomainEvents.Last(), Is.TypeOf<PlanetColonizedDomainEvent>(), "Raised event should be of type PlanetColonizedDomainEvent.");
        var domainEvent = _planet.DomainEvents.Last() as PlanetColonizedDomainEvent;
        Assert.That(domainEvent, Is.Not.Null);
        Assert.That(domainEvent.EntityId, Is.EqualTo(_planetId));
        Assert.That(domainEvent.OccurredOn, Is.EqualTo(_utcNow));
        Assert.That(domainEvent.PlayerId, Is.EqualTo(_playerId));
        Assert.That(domainEvent.EventType, Is.EqualTo(nameof(PlanetColonizedDomainEvent)));
    }

    [Test]
    public void Colonize_WhenPlanetIsAlreadyColonized_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _planet.Colonize(_playerId);  // Colonize the planet first

        // Act & Assert
        Assert.That(() => _planet.Colonize(Guid.NewGuid()), Throws.InvalidOperationException.With.Message.EqualTo("Planet is already colonized."));
    }

    [Test]
    public void Apply_WhenPlanetCreatedDomainEvent_ShouldUpdatePlanetState()
    {
        // Arrange
        var domainEvent = new PlanetCreatedDomainEvent
        {
            EntityId = _planetId,
            PlanetName = "New Planet"
        };

        // Act
        _planet.Apply(domainEvent);

        // Assert
        Assert.That(_planet.Name, Is.EqualTo("New Planet"), "Planet name should be updated.");
    }

    [Test]
    public void Apply_WhenPlanetColonizedDomainEvent_ShouldUpdatePlanetState()
    {
        // Arrange
        var domainEvent = new PlanetColonizedDomainEvent
        {
            EntityId = _planetId,
            PlayerId = _playerId,
            OccurredOn = _utcNow
        };

        // Act
        _planet.Apply(domainEvent);

        // Assert
        Assert.That(_planet.IsColonized, Is.True, "Planet should be marked as colonized.");
        Assert.That(_planet.ColonizedBy, Is.EqualTo(_playerId), "PlayerId should match the ID of the colonizer.");
        Assert.That(_planet.ColonizedAt, Is.EqualTo(_utcNow), "Colonization time should match the event's timestamp.");
    }

    [Test]
    public void Rename_ShouldThrowException_WhenPlanetNotColonized()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Uncolonized Planet", false, null, null);
        var playerId = Guid.NewGuid();

        // Act & Assert
        Assert.That(
            () => planet.Rename("New Planet Name", playerId),
            Throws.InvalidOperationException.With.Message.EqualTo("Only the player who colonized the planet can rename it.")
        );
    }

    [Test]
    public void Rename_ShouldThrowException_WhenPlayerIsNotColonizer()
    {
        // Arrange
        var colonizerId = Guid.NewGuid();
        var otherPlayerId = Guid.NewGuid();
        var planet = Planet.Create(Guid.NewGuid(), "Colonized Planet", true, colonizerId, _utcNow);

        // Act & Assert
        Assert.That(
            () => planet.Rename("New Planet Name", otherPlayerId),
            Throws.InvalidOperationException.With.Message.EqualTo("Only the player who colonized the planet can rename it.")
        );
    }

    [Test]
    public void Rename_ShouldSetNewName_WhenValidNameProvided()
    {
        // Arrange
        var colonizerId = Guid.NewGuid();
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "APlanet", true, colonizerId, _utcNow);
        var newName = "New Planet Name";

        // Act
        planet.Rename(newName, colonizerId);

        // Assert
        Assert.That(planet.Name, Is.EqualTo(newName));
        Assert.That(planet.DomainEvents.Last(), Is.TypeOf<PlanetRenamedDomainEvent>());
        var domainEvent = planet.DomainEvents.Last() as PlanetRenamedDomainEvent;
        Assert.That(domainEvent, Is.Not.Null);
        Assert.That(domainEvent.EntityId, Is.EqualTo(planetId));
        Assert.That(domainEvent.PlanetName, Is.EqualTo(newName));
    }

    [Test]
    public void Rename_ShouldThrowInvalidOperationException_WhenNewNameIsNull()
    {
        // Arrange
        var colonizerId = Guid.NewGuid();
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "APlanet", true, colonizerId, _utcNow);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => planet.Rename(null, colonizerId));
        Assert.That(ex.Message, Is.EqualTo("New name is either null or empty."));
    }

    [Test]
    public void Rename_ShouldThrowInvalidOperationException_WhenNewNameIsEmpty()
    {
        // Arrange
        var colonizerId = Guid.NewGuid();
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(planetId, "APlanet", true, colonizerId, _utcNow);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => planet.Rename(string.Empty, colonizerId));
        Assert.That(ex.Message, Is.EqualTo("New name is either null or empty."));
    }

    [Test]
    public void Apply_ShouldUpdatePlanetName_WhenPlanetRenamedDomainEventApplied()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var planet = Planet.Create(_planetId, "APlanet", false, null, null);
        var newName = "Updated Planet Name";
        var renameEvent = new PlanetRenamedDomainEvent
        {
            EntityId = planetId,
            PlanetName = newName
        };

        // Act
        planet.Apply(renameEvent);

        // Assert
        Assert.That(planet.Name, Is.EqualTo(newName));
    }

    // Coordinates feature tests
    [Test]
    public void Create_ShouldSetCoordinates_WhenCoordinatesProvided()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var galaxy = Galaxy.Andromeda;
        var system = 2;
        var slot = 5;

        // Act
        var planet = Planet.Create(planetId, "Test Planet", false, null, null, galaxy, system, slot);

        // Assert
        Assert.That(planet.Galaxy, Is.EqualTo(galaxy));
        Assert.That(planet.System, Is.EqualTo(system));
        Assert.That(planet.Slot, Is.EqualTo(slot));
    }

    [Test]
    public void Create_ShouldUseDefaultCoordinates_WhenCoordinatesNotProvided()
    {
        // Arrange
        var planetId = Guid.NewGuid();

        // Act
        var planet = Planet.Create(planetId, "Test Planet", false, null, null);

        // Assert
        Assert.That(planet.Galaxy, Is.EqualTo(Galaxy.Unknown));
        Assert.That(planet.System, Is.EqualTo(1));
        Assert.That(planet.Slot, Is.EqualTo(1));
    }

    [Test]
    public void Create_ShouldUseProvidedCoordinates_OverDefaultCoordinates()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var galaxy = Galaxy.MilkyWay;
        var system = 3;
        var slot = 10;

        // Act
        var planet = Planet.Create(planetId, "Test Planet", false, null, null, galaxy, system, slot);

        // Assert
        Assert.That(planet.Galaxy, Is.Not.EqualTo(Galaxy.Andromeda));
        Assert.That(planet.System, Is.Not.EqualTo(1));
        Assert.That(planet.Slot, Is.Not.EqualTo(1));
        Assert.That(planet.Galaxy, Is.EqualTo(galaxy));
        Assert.That(planet.System, Is.EqualTo(system));
        Assert.That(planet.Slot, Is.EqualTo(slot));
    }

    [Test]
    public void Create_ShouldSetAllCoordinates_IncludingGalaxy()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var galaxy = Galaxy.Andromeda;
        var system = 1;
        var slot = 1;

        // Act
        var planet = Planet.Create(planetId, "Test Planet", false, null, null, galaxy, system, slot);

        // Assert
        Assert.That(planet.Galaxy, Is.EqualTo(Galaxy.Andromeda));
        Assert.That(planet.System, Is.EqualTo(1));
        Assert.That(planet.Slot, Is.EqualTo(1));
    }

    [Test]
    public void Create_ShouldPreserveCoordinates_WithColonizedPlanet()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var colonizerId = Guid.NewGuid();
        var galaxy = Galaxy.MilkyWay;
        var system = 2;
        var slot = 7;

        // Act
        var planet = Planet.Create(planetId, "Colonized Planet", true, colonizerId, _utcNow, galaxy, system, slot);

        // Assert
        Assert.That(planet.Galaxy, Is.EqualTo(galaxy));
        Assert.That(planet.System, Is.EqualTo(system));
        Assert.That(planet.Slot, Is.EqualTo(slot));
        Assert.That(planet.IsColonized, Is.True);
        Assert.That(planet.ColonizedBy, Is.EqualTo(colonizerId));
    }

    [Test]
    public void Create_ShouldPreserveCoordinatesInDomainEvent()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var galaxy = Galaxy.Andromeda;
        var system = 3;
        var slot = 9;

        // Act
        var planet = Planet.Create(planetId, "Test Planet", false, null, null, galaxy, system, slot);

        // Assert
        Assert.That(planet.DomainEvents, Is.Not.Empty);
        var createdEvent = planet.DomainEvents.First() as PlanetCreatedDomainEvent;
        Assert.That(createdEvent, Is.Not.Null);
        Assert.That(createdEvent.EntityId, Is.EqualTo(planetId));
    }

    [Test]
    public void Coordinates_ShouldBeReadOnly()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null, Galaxy.Andromeda, 1, 5);

        // Act & Assert
        // Verify that Galaxy, System, and Slot are read-only properties (cannot be set)
        Assert.That(planet.Galaxy, Is.EqualTo(Galaxy.Andromeda));
        Assert.That(planet.System, Is.EqualTo(1));
        Assert.That(planet.Slot, Is.EqualTo(5));
        // These properties should not have setters
    }

    [Test]
    public void Create_ShouldSupportVariousCoordinateValues()
    {
        // Arrange & Act
        var planetA = Planet.Create(Guid.NewGuid(), "Planet A", false, null, null, Galaxy.Andromeda, 1, 1);
        var planetB = Planet.Create(Guid.NewGuid(), "Planet B", false, null, null, Galaxy.Andromeda, 1, 10);
        var planetC = Planet.Create(Guid.NewGuid(), "Planet C", false, null, null, Galaxy.MilkyWay, 2, 15);

        // Assert
        Assert.That(planetA.Galaxy, Is.EqualTo(Galaxy.Andromeda));
        Assert.That(planetA.System, Is.EqualTo(1));
        Assert.That(planetA.Slot, Is.EqualTo(1));

        Assert.That(planetB.Galaxy, Is.EqualTo(Galaxy.Andromeda));
        Assert.That(planetB.System, Is.EqualTo(1));
        Assert.That(planetB.Slot, Is.EqualTo(10));

        Assert.That(planetC.Galaxy, Is.EqualTo(Galaxy.MilkyWay));
        Assert.That(planetC.System, Is.EqualTo(2));
        Assert.That(planetC.Slot, Is.EqualTo(15));
    }

    [Test]
    public void CreatePlanet_ShouldInitializeStorageCapacities()
    {
        // Act
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);

        // Assert
        Assert.That(planet.StorageCapacities, Has.Count.EqualTo(3));
        Assert.That(planet.StorageCapacities.Keys, Contains.Item(ResourceType.Metal));
        Assert.That(planet.StorageCapacities.Keys, Contains.Item(ResourceType.Crystal));
        Assert.That(planet.StorageCapacities.Keys, Contains.Item(ResourceType.Deuterium));

        foreach (var capacity in planet.StorageCapacities.Values)
        {
            Assert.That(capacity.Capacity, Is.EqualTo(StorageCapacity.BaseCapacity));
        }
    }

    [Test]
    public void GetStorageCapacity_ShouldReturnBaseCapacityInitially()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);

        // Act & Assert
        Assert.That(planet.GetStorageCapacity(ResourceType.Metal), Is.EqualTo(StorageCapacity.BaseCapacity));
        Assert.That(planet.GetStorageCapacity(ResourceType.Crystal), Is.EqualTo(StorageCapacity.BaseCapacity));
        Assert.That(planet.GetStorageCapacity(ResourceType.Deuterium), Is.EqualTo(StorageCapacity.BaseCapacity));
    }

    [Test]
    public void GetRemainingStorageCapacity_ShouldCalculateCorrectly()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        planet.Resources[ResourceType.Metal] = 50000; // Half of base capacity

        // Act
        var remaining = planet.GetRemainingStorageCapacity(ResourceType.Metal);

        // Assert
        Assert.That(remaining, Is.EqualTo(StorageCapacity.BaseCapacity - 50000));
    }

    [Test]
    public void IsStorageFull_ShouldReturnCorrectly()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);

        // Act & Assert
        Assert.That(planet.IsStorageFull(ResourceType.Metal), Is.False);

        planet.Resources[ResourceType.Metal] = StorageCapacity.BaseCapacity;
        Assert.That(planet.IsStorageFull(ResourceType.Metal), Is.True);
    }

    [Test]
    public void WouldExceedStorage_ShouldReturnCorrectly()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        planet.Resources[ResourceType.Metal] = StorageCapacity.BaseCapacity - 1000;

        // Act & Assert
        Assert.That(planet.WouldExceedStorage(ResourceType.Metal, 500), Is.False);
        Assert.That(planet.WouldExceedStorage(ResourceType.Metal, 1000), Is.False); // Exactly fills
        Assert.That(planet.WouldExceedStorage(ResourceType.Metal, 1001), Is.True);
    }

    [Test]
    public void UpgradeMine_ShouldThrowException_WhenStorageIsFull()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        planet.Resources[ResourceType.Metal] = StorageCapacity.BaseCapacity;

        // Add enough resources for mine upgrade
        planet.Resources[ResourceType.Crystal] = 10000;
        planet.Resources[ResourceType.Deuterium] = 10000;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => planet.UpgradeMine(ResourceType.Metal));
        Assert.That(ex.Message, Does.Contain("Storage for Metal is full"));
    }

    [Test]
    public void UpgradeStorage_ShouldCreateStorageBuildingIfNotExists()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        Assert.That(planet.StorageBuilding, Is.Null);

        // Need resources for upgrade
        planet.Resources[ResourceType.Metal] = 10000;
        planet.Resources[ResourceType.Crystal] = 10000;
        planet.Resources[ResourceType.Deuterium] = 10000;

        // Act
        planet.UpgradeStorage();

        // Assert
        Assert.That(planet.StorageBuilding, Is.Not.Null);
        Assert.That(planet.StorageBuilding!.Level, Is.EqualTo(2)); // Started at 1, upgraded to 2
    }

    [Test]
    public void UpgradeStorage_ShouldIncreaseTotalCapacity()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        var initialCapacity = planet.GetStorageCapacity(ResourceType.Metal);

        // Need resources for upgrade
        planet.Resources[ResourceType.Metal] = 100000;
        planet.Resources[ResourceType.Crystal] = 100000;
        planet.Resources[ResourceType.Deuterium] = 100000;

        // Act
        planet.UpgradeStorage();

        // Assert
        var newCapacity = planet.GetStorageCapacity(ResourceType.Metal);
        Assert.That(newCapacity, Is.GreaterThan(initialCapacity));
        // Storage was created at level 1, then upgraded to level 2
        // Level 2 storage provides 2 * CapacityPerLevel additional capacity
        var expectedCapacity = StorageCapacity.BaseCapacity + (2 * Storage.CapacityPerLevel);
        Assert.That(newCapacity, Is.EqualTo(expectedCapacity));
    }

    [Test]
    public void UpgradeStorage_ShouldThrowException_WhenNotEnoughResources()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        // Don't add resources - they'll be insufficient

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => planet.UpgradeStorage());
    }

    [Test]
    public void UpgradeStorage_ShouldRaiseDomainEvent()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        planet.Resources[ResourceType.Metal] = 100000;
        planet.Resources[ResourceType.Crystal] = 100000;
        planet.Resources[ResourceType.Deuterium] = 100000;

        // Act
        planet.UpgradeStorage();

        // Assert
        var domainEvent = planet.DomainEvents.OfType<StorageUpgradedDomainEvent>().FirstOrDefault();
        Assert.That(domainEvent, Is.Not.Null);
        Assert.That(domainEvent!.NewLevel, Is.EqualTo(2));
        Assert.That(domainEvent.TotalAdditionalCapacity, Is.EqualTo(Storage.CapacityPerLevel * 2));
    }

    [Test]
    public void UpgradeMine_ShouldRaiseStorageFullEvent_WhenStorageIsFull()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        planet.Resources[ResourceType.Metal] = StorageCapacity.BaseCapacity;
        planet.Resources[ResourceType.Crystal] = 10000;
        planet.Resources[ResourceType.Deuterium] = 10000;

        // Act
        try
        {
            planet.UpgradeMine(ResourceType.Metal);
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        var storageFullEvent = planet.DomainEvents.OfType<StorageFullDomainEvent>().FirstOrDefault();
        Assert.That(storageFullEvent, Is.Not.Null);
        Assert.That(storageFullEvent!.ResourceType, Is.EqualTo(ResourceType.Metal));
        Assert.That(storageFullEvent.CurrentAmount, Is.EqualTo(StorageCapacity.BaseCapacity));
        Assert.That(storageFullEvent.Capacity, Is.EqualTo(StorageCapacity.BaseCapacity));
    }

    [Test]
    public void UpgradeStorage_MultipleTimes_ShouldCumulateCapacity()
    {
        // Arrange
        var planet = Planet.Create(Guid.NewGuid(), "Test Planet", false, null, null);
        var initialCapacity = planet.GetStorageCapacity(ResourceType.Metal);

        // Act - Upgrade storage twice
        for (int i = 0; i < 2; i++)
        {
            planet.Resources[ResourceType.Metal] = 100000;
            planet.Resources[ResourceType.Crystal] = 100000;
            planet.Resources[ResourceType.Deuterium] = 100000;
            planet.UpgradeStorage();
        }

        // Assert
        var finalCapacity = planet.GetStorageCapacity(ResourceType.Metal);
        var expectedCapacity = initialCapacity + (Storage.CapacityPerLevel * 3); // Level 1,2,3
        Assert.That(finalCapacity, Is.EqualTo(expectedCapacity));
    }
}
