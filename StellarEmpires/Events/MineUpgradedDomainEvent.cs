using StellarEmpires.Domain.Models;
using StellarEmpires.Helpers;

namespace StellarEmpires.Events;

public sealed record MineUpgradedDomainEvent : IDomainEvent
{
	public string EventType => nameof(MineUpgradedDomainEvent);

	public Guid Id => Guid.NewGuid();
	public DateTime OccurredOn { get; init; } = DateTimeProvider.UtcNow;

	public required Guid EntityId { get; init; } // MineId
	public required Guid PlanetId { get; init; }
	public required ResourceType ResourceType { get; init; }
	public required int PreviousLevel { get; init; }
	public required int NewLevel { get; init; }
	public required int NewProductionRatePerHour { get; init; }
	public required Dictionary<ResourceType, int> ConsumedResources { get; init; }
}
