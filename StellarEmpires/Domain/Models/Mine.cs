using StellarEmpires.Events;
using System.Text.Json.Serialization;

namespace StellarEmpires.Domain.Models;

public class Mine : Entity
{
	public Guid PlanetId { get; private set; }
	public ResourceType ResourceType { get; private set; }
	public int Level { get; private set; }
	public int BaseProductionRatePerHour { get; private set; }
	public int ProductionRatePerHour { get; private set; }

	private readonly Dictionary<ResourceType, Dictionary<ResourceType, int>> InitialUpgradeCost = new()
	{
		{
			ResourceType.Metal, new Dictionary<ResourceType, int>
			{
				{ ResourceType.Metal, 60 },
				{ ResourceType.Crystal, 15 }
			}
		},
		{
			ResourceType.Crystal, new Dictionary<ResourceType, int>
			{
				{ ResourceType.Metal, 48 },
				{ ResourceType.Crystal, 24 }
			}
		},
		{
			ResourceType.Deuterium, new Dictionary<ResourceType, int>
			{
				{ ResourceType.Metal, 225 },
				{ ResourceType.Crystal, 75 }
			}
		}
	};

	[JsonConstructor]
	private Mine(
		Guid id,
		Guid planetId,
		ResourceType resourceType,
		int level,
		int baseProductionRatePerHour,
		int productionRatePerHour) : base(id)
	{
		PlanetId = planetId;
		ResourceType = resourceType;
		Level = level;
		BaseProductionRatePerHour = baseProductionRatePerHour;
		ProductionRatePerHour = productionRatePerHour;
	}

	public static Mine Create(Guid id, Guid planetId, ResourceType resourceType)
	{
		int level = 0;
		var mine = new Mine(
			id,
			planetId,
			resourceType,
			level,
			GetBaseProductionRatePerHour(resourceType),
			GetBaseProductionRatePerHour(resourceType));

		var mineCreatedEvent = new MineCreatedDomainEvent
		{
			EntityId = mine.Id,
			ResourceType = mine.ResourceType,
			BaseProductionRatePerHour = mine.BaseProductionRatePerHour
		};

		mine.Apply(mineCreatedEvent);
		mine.AddDomainEvent(mineCreatedEvent);

		return mine;
	}

	private static int GetBaseProductionRatePerHour(ResourceType resourceType)
	{
		return resourceType switch
		{
			ResourceType.Metal => 30,
			ResourceType.Crystal => 20,
			ResourceType.Deuterium => 10,
			_ => 0
		};
	}

	private int CalculateProductionRate(int level)
	{
		return (int)(BaseProductionRatePerHour * level * Math.Pow(1.1, level));
	}

	public Dictionary<ResourceType, int> GetUpgradeCost(int level)
	{
		var factor = Math.Pow(1.5, level);
		var newCost = new Dictionary<ResourceType, int>();
		var cost = InitialUpgradeCost[ResourceType];

		foreach (var costs in cost)
		{
			newCost[costs.Key] = (int)(costs.Value * factor);
		}

		return newCost;
	}

	public void Upgrade(Dictionary<ResourceType, int> consumedResources)
	{
		var previousLevel = Level;
		Level++;
		ProductionRatePerHour = CalculateProductionRate(Level);

		var mineUpgradedEvent = new MineUpgradedDomainEvent
		{
			EntityId = Id,
			PlanetId = PlanetId,
			ResourceType = ResourceType,
			PreviousLevel = previousLevel,
			NewLevel = Level,
			NewProductionRatePerHour = ProductionRatePerHour,
			ConsumedResources = consumedResources
		};

		Apply(mineUpgradedEvent);
		AddDomainEvent(mineUpgradedEvent);
	}

	public override void Apply(IDomainEvent domainEvent)
	{
		if (domainEvent is MineCreatedDomainEvent mineCreatedEvent)
		{
			ResourceType = mineCreatedEvent.ResourceType;
			BaseProductionRatePerHour = mineCreatedEvent.BaseProductionRatePerHour;
		}

		if (domainEvent is MineUpgradedDomainEvent mineUpgradedEvent)
		{
			Level = mineUpgradedEvent.NewLevel;
			ProductionRatePerHour = mineUpgradedEvent.NewProductionRatePerHour;
		}
	}
}
