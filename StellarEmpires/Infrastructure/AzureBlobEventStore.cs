using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using StellarEmpires.Events;
using System.Text.Json;

namespace StellarEmpires.Infrastructure;

public class AzureBlobEventStore : IEventStore
{
	private readonly BlobServiceClient _blobServiceClient;

	public AzureBlobEventStore(AzureBlobStorageConfig config)
	{
		_blobServiceClient = new BlobServiceClient(config.ConnectionString);
	}

	public async Task SaveEventAsync<TEntity>(IDomainEvent domainEvent)
	{
		var containerClient = GetContainerClient<TEntity>();
		var entityId = domainEvent.EntityId;
		var blobName = GetBlobNameForEntity(entityId);
		var blobClient = containerClient.GetBlobClient(blobName);

		List<IDomainEvent> existingEvents = await GetExistingEvents(blobClient);
		existingEvents.Add(domainEvent);

		var json = JsonSerializer.Serialize(existingEvents, new JsonSerializerOptions
		{
			WriteIndented = true
		});

		using var uploadStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
		await blobClient.UploadAsync(uploadStream, overwrite: true);
	}

	public async Task<IEnumerable<IDomainEvent>> GetEventsAsync<TEntity>(Guid entityId)
	{
		var containerClient = GetContainerClient<TEntity>();
		var blobName = GetBlobNameForEntity(entityId);
		var blobClient = containerClient.GetBlobClient(blobName);

		if (!await blobClient.ExistsAsync())
		{
			return new List<IDomainEvent>();
		}

		var downloadResult = await blobClient.DownloadAsync();
		using var streamReader = new StreamReader(downloadResult.Value.Content);
		var json = await streamReader.ReadToEndAsync();

		var events = JsonSerializer.Deserialize<List<IDomainEvent>>(json, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		});

		return events ?? new List<IDomainEvent>();
	}

	private BlobContainerClient GetContainerClient<TEntity>()
	{
		var containerName = $"events-{typeof(TEntity).Name.ToLower()}";
		var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
		containerClient.CreateIfNotExists(PublicAccessType.None);

		return containerClient;
	}

	private async Task<List<IDomainEvent>> GetExistingEvents(BlobClient blobClient)
	{
		if (await blobClient.ExistsAsync())
		{
			var downloadResult = await blobClient.DownloadAsync();
			using var streamReader = new StreamReader(downloadResult.Value.Content);
			var json = await streamReader.ReadToEndAsync();
			return JsonSerializer.Deserialize<List<IDomainEvent>>(json) ?? new List<IDomainEvent>();
		}

		return new List<IDomainEvent>();
	}

	private string GetBlobNameForEntity(Guid entityId)
	{
		return $"events-planets-{entityId}.json";
	}
}
