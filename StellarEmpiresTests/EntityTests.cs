using StellarEmpires.Events;
using StellarEmpires.Helpers;
using StellarEmpires.Tests.Mocks;

namespace StellarEmpires.Tests;

[TestFixture]
public class EntityTests
{
	[Test]
	public void Entity_Should_Have_New_Guid_When_Created_Without_Id()
	{
		// Arrange & Act
		var entity = new MockEntity();

		// Assert
		Assert.That(entity.Id, Is.Not.EqualTo(Guid.Empty));
	}

	[Test]
	public void Entity_Should_Have_Specified_Guid_When_Created_With_Id()
	{
		// Arrange
		var guid = Guid.NewGuid();

		// Act
		var entity = new MockEntity(guid);

		// Assert
		Assert.That(entity.Id, Is.EqualTo(guid));
	}

	[Test]
	public void Entity_Should_Add_Domain_Event()
	{
		// Arrange
		var entity = new MockEntity();

		// Act
		entity.AddDomainEvent();

		// Assert
		Assert.That(entity.DomainEvents, Has.Count.EqualTo(1));
		Assert.That(entity.DomainEvents.First(), Is.TypeOf(typeof(MockDomainEvent)));
	}

	[Test]
	public void Entity_Should_Clear_Domain_Events()
	{
		// Arrange
		var entity = new MockEntity();
		entity.AddDomainEvent();

		// Act
		entity.ClearDomainEvents();

		// Assert
		Assert.That(entity.DomainEvents, Is.Empty);
	}

	[Test]
	public void Entity_Should_Load_And_Apply_Historical_Events()
	{
		// Arrange
		var entity = new MockEntity();
		var overrideUtcNow = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => overrideUtcNow);
		var domainEvent1 = new MockDomainEvent();

		overrideUtcNow = new DateTime(2023, 1, 1, 13, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => overrideUtcNow);
		var domainEvent2 = new MockDomainEvent();

		var historicalEvents = new List<IDomainEvent>
			{
				domainEvent1,
				domainEvent2
			};

		// Act
		entity.LoadFromHistory(historicalEvents);

		// Assert
		Assert.That(entity.LastEventOccurred, Is.EqualTo(overrideUtcNow));
		Assert.That(entity.LastEventOccurred, Is.EqualTo(historicalEvents.Last().OccurredOn));

		// Cleanup
		DateTimeProvider.ResetUtcNow();
	}

	[Test]
	public void Entity_Should_Set_OccurredOn_To_Overridden_UtcNow()
	{
		// Arrange
		var overrideUtcNow = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => overrideUtcNow);
		var entity = new MockEntity();

		// Act
		entity.AddDomainEvent();

		// Assert
		var domainEvent = (MockDomainEvent)entity.DomainEvents.First();
		Assert.That(domainEvent.OccurredOn, Is.EqualTo(overrideUtcNow));

		// Cleanup
		DateTimeProvider.ResetUtcNow();
	}

	[Test]
	public void Cloned_Entity_Should_Have_Same_Id()
	{
		// Arrange
		var entity = new MockEntity();

		// Act
		var clonedEntity = entity.Clone();

		// Assert
		Assert.That(clonedEntity.Id, Is.EqualTo(entity.Id));
	}

	[Test]
	public void Entities_With_Same_Id_Should_Be_Equal()
	{
		var guid = Guid.NewGuid();
		var entity1 = new MockEntity(guid);
		var entity2 = new MockEntity(guid);
		Assert.That(entity2, Is.EqualTo(entity1));
		Assert.That(entity1.Equals(entity2), Is.True);
	}

	[Test]
	public void Entities_With_Different_Ids_Should_Not_Be_Equal()
	{
		var entity1 = new MockEntity();
		var entity2 = new MockEntity();
		Assert.That(entity2, Is.Not.EqualTo(entity1));
		Assert.That(entity1.Equals(entity2), Is.False);
	}
}