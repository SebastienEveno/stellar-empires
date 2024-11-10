using StellarEmpires.Events;
using System.Text.Json;

namespace StellarEmpires.Infrastructure.EventStore;

public class FileEventStore : IEventStore
{
	private readonly string BaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "EventStore");
	private readonly JsonSerializerOptions _jsonOptions;

	public FileEventStore()
	{
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
		await File.WriteAllTextAsync(filePath, json);
	}

	public async Task<IEnumerable<IDomainEvent>> GetEventsAsync<TEntity>(Guid entityId)
	{
		var filePath = GetFilePathForEntity<TEntity>();
		var allEvents = await LoadAllEventsAsync(filePath);

		return allEvents.FindAll(e => e.EntityId == entityId);
	}

	private async Task<List<IDomainEvent>> LoadAllEventsAsync(string filePath)
	{
		if (!Directory.Exists(BaseDirectory))
		{
			Directory.CreateDirectory(BaseDirectory);
		}

		if (!File.Exists(filePath))
		{
			return new List<IDomainEvent>();
		}

		var json = await File.ReadAllTextAsync(filePath);

		return JsonSerializer.Deserialize<List<IDomainEvent>>(json, _jsonOptions) ?? new List<IDomainEvent>();
	}

	private string GetFilePathForEntity<TEntity>()
	{
		var entityName = typeof(TEntity).Name.ToLower();

		var fileName = $"events-{entityName}.json";

		return Path.Combine(BaseDirectory, fileName);
	}
}
