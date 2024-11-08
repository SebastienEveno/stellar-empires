using StellarEmpires.Events;
using StellarEmpires.Helpers;
using StellarEmpires.Infrastructure.EventStore;
using System.Text.Json;

namespace StellarEmpires.Tests.Infrastructure.EventStore;

[TestFixture]
public class DomainEventJsonConverterTests
{
	private JsonSerializerOptions _options;

	[SetUp]
	public void Setup()
	{
		_options = new JsonSerializerOptions
		{
			Converters = { new DomainEventJsonConverter() },
			PropertyNameCaseInsensitive = true
		};
	}

	[Test]
	public void Serialize_ShouldIncludeEventType_ForMockDomainEvent()
	{
		// Arrange
		var entityId = Guid.NewGuid();
		var domainEvent = new MockDomainEvent { EntityId = entityId };

		// Act
		var json = JsonSerializer.Serialize<IDomainEvent>(domainEvent, _options);

		// Assert
		Assert.That(json, Does.Contain("\"EventType\":\"MockDomainEvent\""));
		Assert.That(json, Does.Contain("\"OccurredOn\""));
		Assert.That(json, Does.Contain($"\"EntityId\":\"{entityId}\""));
	}

	[Test]
	public void Deserialize_ShouldReturnMockDomainEvent_ForValidJson()
	{
		// Arrange
		var occurredOn = new DateTime(2024, 11, 7, 0, 0, 0, DateTimeKind.Utc);
		DateTimeProvider.SetUtcNow(() => occurredOn);

		var json = $@"
        {{
            ""EventType"": ""MockDomainEvent"",
            ""OccurredOn"": ""{occurredOn:O}"",
            ""Id"": ""{Guid.NewGuid()}"",
            ""EntityId"": ""{Guid.NewGuid()}""
        }}";

		// Act
		var domainEvent = JsonSerializer.Deserialize<IDomainEvent>(json, _options);

		// Assert
		Assert.That(domainEvent, Is.TypeOf<MockDomainEvent>());
		Assert.That(((MockDomainEvent)domainEvent).OccurredOn, Is.EqualTo(occurredOn));
	}

	[Test]
	public void Deserialize_ShouldThrowNotSupportedException_ForUnknownEventType()
	{
		// Arrange
		var json = @"
        {
            ""EventType"": ""UnknownEventType"",
            ""OccurredOn"": ""2024-11-07T00:00:00Z"",
            ""Id"": ""e8f50af2-8478-4b54-a245-e0026dcbb50c"",
            ""EntityId"": ""4a34c7b6-57d6-4b7a-b6a2-69e7d89edb44""
        }";

		// Act & Assert
		var ex = Assert.Throws<NotImplementedException>(() => JsonSerializer.Deserialize<IDomainEvent>(json, _options));
		Assert.That(ex.Message, Is.EqualTo("Event type 'UnknownEventType' is not supported"));
	}

	[Test]
	public void Deserialize_ShouldThrowJsonException_WhenEventTypeIsMissing()
	{
		// Arrange
		var json = @"
        {
            ""OccurredOn"": ""2024-11-07T00:00:00Z"",
            ""Id"": ""e8f50af2-8478-4b54-a245-e0026dcbb50c"",
            ""EntityId"": ""4a34c7b6-57d6-4b7a-b6a2-69e7d89edb44""
        }";

		// Act & Assert
		var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IDomainEvent>(json, _options));
		Assert.That(ex.Message, Is.EqualTo("Missing EventType property"));
	}
}
