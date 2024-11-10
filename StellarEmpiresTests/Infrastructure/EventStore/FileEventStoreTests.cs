using StellarEmpires.Domain.Models;
using StellarEmpires.Events;
using StellarEmpires.Helpers;
using StellarEmpires.Infrastructure.EventStore;
using StellarEmpires.Tests.Mocks;

namespace StellarEmpires.Tests.Infrastructure.EventStore;

[TestFixture]
public class FileEventStoreTests
{
	private FileEventStore _eventStore;
	private string _baseDirectory;

	private DateTime _utcNow;

	[SetUp]
	public void Setup()
	{
		_baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "EventStore");

		_eventStore = new FileEventStore();

		_utcNow = new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => _utcNow);
	}

	[TearDown]
	public void Teardown()
	{
		DateTimeProvider.ResetUtcNow();

		var files = Directory.GetFiles(_baseDirectory, "events-*.json");

		foreach (var file in files)
		{
			File.Delete(file);
		}
	}

	[Test]
	public async Task SaveEventAsync_ShouldSaveEventToFile()
	{
		// Arrange
		var domainEvent = new MockDomainEvent{ EntityId = Guid.NewGuid() };

		// Act
		await _eventStore.SaveEventAsync<MockEntity>(domainEvent);

		// Assert
		Assert.That(Directory.Exists(_baseDirectory), Is.True);

		var filePath = Path.Combine(_baseDirectory, "events-mockentity.json");
		Assert.That(File.Exists(filePath), Is.True);

		var fileContent = await File.ReadAllTextAsync(filePath);
		Assert.That(fileContent, Does.Contain("OccurredOn"));
		Assert.That(fileContent, Does.Contain("2024-11-07T00:00:00Z"));
	}

	[Test]
	public async Task SaveEventAsync_ShouldSerializeAndSavePlanetColonizedDomainEventCorrectly()
	{
		// Arrange
		var domainEvent = new PlanetColonizedDomainEvent
		{
			EntityId = Guid.NewGuid(),
			PlayerId = Guid.NewGuid()
		};

		// Act
		await _eventStore.SaveEventAsync<Planet>(domainEvent);

		// Assert
		var filePath = Path.Combine(_baseDirectory, "events-planet.json");
		Assert.That(File.Exists(filePath), Is.True, "The event file should be created.");

		var fileContent = await File.ReadAllTextAsync(filePath);

		// Check serialization for specific properties
		var expectedEventType = nameof(PlanetColonizedDomainEvent);
		Assert.That(fileContent, Does.Contain($"\"EventType\": \"{expectedEventType}\""), "EventType should be serialized correctly.");
		Assert.That(fileContent, Does.Contain("\"OccurredOn\": \"2024-11-07T00:00:00Z\""), "OccurredOn should be serialized correctly.");
		Assert.That(fileContent, Does.Contain($"\"EntityId\": \"{domainEvent.EntityId}\""), "EntityId should be serialized correctly.");
		Assert.That(fileContent, Does.Contain($"\"PlayerId\": \"{domainEvent.PlayerId}\""), "PlayerId should be serialized correctly.");
	}

	[Test]
	public async Task GetEventsAsync_ShouldReturnSavedEvents()
	{
		// Arrange
		var entityId = Guid.NewGuid();
		var domainEvents = new List<IDomainEvent>
		{
			new MockDomainEvent { EntityId = entityId },
			new MockDomainEvent { EntityId = entityId }
		};

		foreach (var domainEvent in domainEvents)
		{
			await _eventStore.SaveEventAsync<MockEntity>(domainEvent);
		}

		// Act
		var retrievedEvents = await _eventStore.GetEventsAsync<MockEntity>(entityId);

		// Assert
		Assert.That(retrievedEvents.ToList(), Has.Count.EqualTo(2));
		foreach (var retrievedEvent in retrievedEvents)
		{
			Assert.That(retrievedEvent, Is.TypeOf<MockDomainEvent>());
			Assert.That(((MockDomainEvent)retrievedEvent).EntityId, Is.EqualTo(entityId));
			Assert.That(((MockDomainEvent)retrievedEvent).OccurredOn, Is.EqualTo(_utcNow));
		}
	}

	[Test]
	public async Task Apply_ShouldUpdateEntityLastEventOccurred()
	{
		// Arrange
		var mockEntity = new MockEntity();
		var domainEvent = new MockDomainEvent { EntityId =  mockEntity.Id };

		// Act
		await _eventStore.SaveEventAsync<MockEntity>(domainEvent);
		mockEntity.Apply(domainEvent);

		// Assert
		Assert.That(mockEntity.LastEventOccurred, Is.EqualTo(_utcNow));
	}
}
