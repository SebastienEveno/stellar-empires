using StellarEmpires.Domain.Models;
using StellarEmpires.Events;
using StellarEmpires.Helpers;

namespace StellarEmpires.Tests.Domain;

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
        _planet = new Planet(_planetId, "APlanet", false, null, null);

        _utcNow = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTimeProvider.SetUtcNow(() => _utcNow);
    }

    [TearDown]
    public void Teardown()
    {
        DateTimeProvider.ResetUtcNow();
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
        Assert.That(_planet.DomainEvents.Count, Is.EqualTo(1), "One domain event should be raised.");
        Assert.That(_planet.DomainEvents.ToArray()[0], Is.InstanceOf<PlanetColonizedDomainEvent>(), "Raised event should be of type PlanetColonizedDomainEvent.");
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
		var planet = new Planet(Guid.NewGuid(), "Uncolonized Planet", false, null, null);
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
		var planet = new Planet(Guid.NewGuid(), "Colonized Planet", true, colonizerId, _utcNow);

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
		var planet = new Planet(planetId, "APlanet", true, colonizerId, _utcNow);
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
		var planet = new Planet(planetId, "APlanet", true, colonizerId, _utcNow);

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
		var planet = new Planet(planetId, "APlanet", true, colonizerId, _utcNow);

		// Act & Assert
		var ex = Assert.Throws<InvalidOperationException>(() => planet.Rename(string.Empty, colonizerId));
		Assert.That(ex.Message, Is.EqualTo("New name is either null or empty."));
	}

	[Test]
	public void Apply_ShouldUpdatePlanetName_WhenPlanetRenamedDomainEventApplied()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var planet = new Planet(_planetId, "APlanet", false, null, null);
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
}