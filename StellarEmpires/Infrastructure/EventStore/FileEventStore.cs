using StellarEmpires.Domain.Models;
using StellarEmpires.Events;
using StellarEmpires.Infrastructure.PlanetStore;
using System.IO.Abstractions;
using System.Text.Json;

namespace StellarEmpires.Infrastructure.EventStore;

public class FileEventStore : IEventStore
{
	private readonly IFileSystem _fileSystem;
	private readonly IPlanetStore _planetStore;
	private readonly string BaseDirectory;
	private readonly JsonSerializerOptions _jsonOptions;

	public FileEventStore(IFileSystem fileSystem, IPlanetStore planetStore)
	{
		_fileSystem = fileSystem;
		_planetStore = planetStore;
		BaseDirectory = _fileSystem.Path.Combine("Infrastructure", "EventStore");
		_jsonOptions = new JsonSerializerOptions
		{
			Converters = { new DomainEventJsonConverter() },
			WriteIndented = true,
			PropertyNameCaseInsensitive = true
		};
	}

	public async Task SaveEventAsync<TEntity>(IDomainEvent domainEvent)
	{
		var filePath = GetFilePathForEntity<TEntity>();
		var allEvents = await LoadAllEventsAsync(filePath);
		allEvents.Add(domainEvent);

		var json = JsonSerializer.Serialize(allEvents, _jsonOptions);
		await _fileSystem.File.WriteAllTextAsync(filePath, json);
		
		await SaveEntityState<TEntity>(domainEvent);
	}

	private async Task SaveEntityState<TEntity>(IDomainEvent domainEvent)
	{
		// TODO Add factory pattern if more entities to be supported
		
		// Save the latest state of the planet
		if (typeof(TEntity) == typeof(Planet))
		{
			var planet = await _planetStore.GetPlanetByIdAsync(domainEvent.EntityId)
				?? Planet.Create(domainEvent.EntityId, "New Planet", false, null, null);

			planet.Apply(domainEvent);

			await _planetStore.SavePlanetAsync(planet);
		}
	}

	public async Task<IEnumerable<IDomainEvent>> GetEventsAsync<TEntity>(Guid entityId)
	{
		var filePath = GetFilePathForEntity<TEntity>();
		var allEvents = await LoadAllEventsAsync(filePath);

		return allEvents.FindAll(e => e.EntityId == entityId);
	}

	private async Task<List<IDomainEvent>> LoadAllEventsAsync(string filePath)
	{
		if (!_fileSystem.Directory.Exists(BaseDirectory))
		{
			_fileSystem.Directory.CreateDirectory(BaseDirectory);
		}

		if (!_fileSystem.File.Exists(filePath))
		{
			return new List<IDomainEvent>();
		}

		var json = await _fileSystem.File.ReadAllTextAsync(filePath);

		return JsonSerializer.Deserialize<List<IDomainEvent>>(json, _jsonOptions) ?? new List<IDomainEvent>();
	}

	private string GetFilePathForEntity<TEntity>()
	{
		var entityName = typeof(TEntity).Name.ToLower();

		var fileName = $"events-{entityName}.json";

		return _fileSystem.Path.Combine(BaseDirectory, fileName);
	}
}
