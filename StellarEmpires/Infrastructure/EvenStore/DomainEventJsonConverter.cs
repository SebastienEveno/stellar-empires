using StellarEmpires.Events;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace StellarEmpires.Infrastructure.EvenStore;

public class DomainEventJsonConverter : JsonConverter<IDomainEvent>
{
	public override IDomainEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using (var document = JsonDocument.ParseValue(ref reader))
		{
			var rootElement = document.RootElement;

			// Use a property (e.g., EventType) to determine the concrete type
			if (rootElement.TryGetProperty("EventType", out JsonElement eventTypeElement))
			{
				var eventType = eventTypeElement.GetString();

				return eventType switch
				{
					nameof(MockDomainEvent) => JsonSerializer.Deserialize<MockDomainEvent>(rootElement.GetRawText(), options),
					nameof(PlanetColonizedDomainEvent) => JsonSerializer.Deserialize<PlanetColonizedDomainEvent>(rootElement.GetRawText(), options),
					// Add cases here for other event types in the future
					_ => throw new NotImplementedException($"Event type '{eventType}' is not supported")
				};
			}

			throw new JsonException("Missing EventType property");
		}
	}

	public override void Write(Utf8JsonWriter writer, IDomainEvent value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, (object)value, options);
	}
}
