using StellarEmpires.Events;
using System.Text.Json;

namespace StellarEmpires.Infrastructure.EvenStore;

public class FileEventStore : IEventStore
{
	private readonly string BaseDirectory = Path.Combine("Infrastructure", "EventStore");

	public async Task SaveEventAsync<TEntity>(IDomainEvent domainEvent)
	{
		var filePath = GetFilePathForEntity<TEntity>();
		var allEvents = await LoadAllEventsAsync(filePath);
		allEvents.Add(domainEvent);

		var json = JsonSerializer.Serialize(allEvents, new JsonSerializerOptions { WriteIndented = true });
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
		if (!File.Exists(filePath))
		{
			return new List<IDomainEvent>();
		}

		var json = await File.ReadAllTextAsync(filePath);

		var options = new JsonSerializerOptions
		{
			Converters = { new DomainEventJsonConverter() },
			PropertyNameCaseInsensitive = true
		};

		return JsonSerializer.Deserialize<List<IDomainEvent>>(json, options) ?? new List<IDomainEvent>();
	}

	private string GetFilePathForEntity<TEntity>()
	{
		var entityName = typeof(TEntity).Name.ToLower(); // e.g., "planet" or "building"

		var fileName = $"events-{entityName}.json";

		return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BaseDirectory, fileName);
	}
}
