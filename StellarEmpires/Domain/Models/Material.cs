using StellarEmpires.Domain.Models;

public class Material
{
    public ResourceType Type { get; private set; }
    public MaterialType MaterialType { get; private set; }
    public Rarity Rarity { get; private set; }
    public Value Value { get; private set; }
}