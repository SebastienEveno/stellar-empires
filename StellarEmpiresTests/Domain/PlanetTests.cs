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
}
