namespace StellarEmpires.Domain.Models;

public class Mine
{
    public ResourceType ResourceType { get; private set; }
    public int Level { get; set; }
    public int ProductionRatePerHour { get; set; }

    public Mine(ResourceType resourceType, int level, int productionRatePerHour)
    {
        ResourceType = resourceType;
        Level = level;
        ProductionRatePerHour = productionRatePerHour;
    }
}