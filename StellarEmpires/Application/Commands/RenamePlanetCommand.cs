namespace StellarEmpires.Application.Commands;

public record RenamePlanetCommand
{
	public required Guid PlanetId { get; init; }
	public required Guid PlayerId { get; init; }
	public required string PlanetName { get; init; }
}
