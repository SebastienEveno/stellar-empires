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

	[SetUp]
	public void Setup()
	{
		_baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "EventStore");
		Directory.CreateDirectory(_baseDirectory);

		_eventStore = new FileEventStore();

		DateTimeProvider.SetUtcNow(() => new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc));
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
		var filePath = Path.Combine(_baseDirectory, "events-mockentity.json");
		Assert.That(File.Exists(filePath), Is.True);

		var fileContent = await File.ReadAllTextAsync(filePath);
		Assert.That(fileContent, Does.Contain("OccurredOn"));
		Assert.That(fileContent, Does.Contain("2024-11-07T00:00:00Z"));
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
			Assert.That(((MockDomainEvent)retrievedEvent).OccurredOn, Is.EqualTo(new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc)));
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
		Assert.That(mockEntity.LastEventOccurred, Is.EqualTo(new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc)));
	}
}
