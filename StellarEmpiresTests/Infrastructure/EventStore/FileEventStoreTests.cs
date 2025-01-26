using StellarEmpires.Domain.Models;
using StellarEmpires.Events;
using StellarEmpires.Helpers;
using StellarEmpires.Infrastructure.EventStore;
using StellarEmpires.Infrastructure.PlanetStore;
using StellarEmpires.Tests.Mocks;
using System.IO.Abstractions.TestingHelpers;

namespace StellarEmpires.Tests.Infrastructure.EventStore;

[TestFixture]
public class FileEventStoreTests
{
	private MockFileSystem _fileSystem;
	private FileEventStore _eventStore;
	private FilePlanetStore _planetStore;

	private DateTime _utcNow;

	[SetUp]
	public void Setup()
	{
		_fileSystem = new MockFileSystem();
		_planetStore = new FilePlanetStore(_fileSystem);
		_eventStore = new FileEventStore(_fileSystem, _planetStore);

		_utcNow = new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => _utcNow);
	}

	[TearDown]
	public void Teardown()
	{
		DateTimeProvider.ResetUtcNow();
	}

	[Test]
	public async Task SaveEventAsync_ShouldCreateDirectoryAndFile_WhenEventIsSaved()
	{
		// Arrange
		var domainEvent = new MockDomainEvent { EntityId = Guid.NewGuid() };

		// Act
		await _eventStore.SaveEventAsync<MockEntity>(domainEvent);

		// Assert
		var filePath = _fileSystem.Path.Combine("Infrastructure", "EventStore", "events-mockentity.json");
		Assert.That(_fileSystem.FileExists(filePath), Is.True);

		var fileContent = await _fileSystem.File.ReadAllTextAsync(filePath);
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
		var filePath = _fileSystem.Path.Combine("Infrastructure", "EventStore", "events-planet.json");
		Assert.That(_fileSystem.FileExists(filePath), Is.True);

		var fileContent = await _fileSystem.File.ReadAllTextAsync(filePath);

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
	public async Task GetEventsAsync_ShouldReturnEmptyList_WhenFileDoesNotExist()
	{
		// Act
		var events = await _eventStore.GetEventsAsync<Planet>(Guid.NewGuid());

		// Assert
		Assert.That(events, Is.Empty);
	}

	[Test]
	public async Task GetEventsAsync_ShouldCreateBaseDirectory_WhenNotExisting()
	{
		// Act
		await _eventStore.GetEventsAsync<Planet>(Guid.NewGuid());

		// Assert
		var baseDirPath = _fileSystem.Path.Combine("Infrastructure", "EventStore");
		Assert.That(_fileSystem.Directory.Exists(baseDirPath), Is.True);
	}

	[Test]
	public async Task SaveEventAsync_ShouldUpdatePlanetState_WhenEventIsSaved()
	{
		// Arrange
		var planetId = Guid.NewGuid();
		var planet = Planet.Create(planetId, "Earth", false, null, null);
		await _planetStore.SavePlanetAsync(planet);

		var domainEvent = new PlanetColonizedDomainEvent
		{
			EntityId = planetId,
			PlayerId = Guid.NewGuid()
		};

		// Act
		await _eventStore.SaveEventAsync<Planet>(domainEvent);

		// Assert
		var updatedPlanet = await _planetStore.GetPlanetByIdAsync(planetId);
		Assert.That(updatedPlanet, Is.Not.Null);
		Assert.That(updatedPlanet!.IsColonized, Is.True);
		Assert.That(updatedPlanet.ColonizedBy, Is.EqualTo(domainEvent.PlayerId));
		Assert.That(updatedPlanet.ColonizedAt, Is.EqualTo(_utcNow));
	}
}
