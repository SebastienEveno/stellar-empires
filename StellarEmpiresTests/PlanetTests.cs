using StellarEmpires.Domain.Models;
using StellarEmpires.Events;
using StellarEmpires.Helpers;

namespace StellarEmpires.Tests;

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
		var colonizationTime = DateTimeProvider.UtcNow;
		var domainEvent = new PlanetColonizedDomainEvent
		{
			EntityId = _planetId,
			PlayerId = _playerId,
			ColonizedAt = colonizationTime
		};

		// Act
		_planet.Apply(domainEvent);

		// Assert
		Assert.That(_planet.IsColonized, Is.True, "Planet should be marked as colonized.");
		Assert.That(_planet.ColonizedBy, Is.EqualTo(_playerId), "PlayerId should match the ID of the colonizer.");
		Assert.That(_planet.ColonizedAt, Is.EqualTo(colonizationTime), "Colonization time should match the event's timestamp.");
	}
}