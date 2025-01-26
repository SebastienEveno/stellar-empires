using StellarEmpires.Events;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellarEmpires.Infrastructure.EventStore;

public class DomainEventJsonConverter : JsonConverter<IDomainEvent>
{
	private readonly Dictionary<string, Type> _eventTypeMap = new()
	{
		{ nameof(MockDomainEvent), typeof(MockDomainEvent) },
		{ nameof(PlanetColonizedDomainEvent), typeof(PlanetColonizedDomainEvent) },
		{ nameof(PlanetRenamedDomainEvent), typeof(PlanetRenamedDomainEvent) },
		{ nameof(PlanetCreatedDomainEvent), typeof(PlanetCreatedDomainEvent) }
	};


	public override IDomainEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var document = JsonDocument.ParseValue(ref reader))
		{
			var rootElement = document.RootElement;

			// Use a property (e.g., EventType) to determine the concrete type
			if (rootElement.TryGetProperty("EventType", out JsonElement eventTypeElement))
			{
				var eventType = eventTypeElement.GetString();

				if (eventType != null)
				{
					if (_eventTypeMap.TryGetValue(eventType, out var targetType) is false)
					{
						throw new NotImplementedException($"Event type '{eventType}' is not supported");
					}

					return (IDomainEvent?)JsonSerializer.Deserialize(rootElement.GetRawText(), targetType, options) ?? throw new InvalidOperationException($"Could not deserialize event type \"{eventType}\" into target type \"{targetType}\"");
				}
			}

			throw new JsonException("Missing EventType property");
		}
	}

	public override void Write(Utf8JsonWriter writer, IDomainEvent value, JsonSerializerOptions options)
	{
		var eventType = value.GetType();

		JsonSerializer.Serialize(writer, value, eventType, options);
	}
}
