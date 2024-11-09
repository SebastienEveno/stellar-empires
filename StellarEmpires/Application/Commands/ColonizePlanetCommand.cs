namespace StellarEmpires.Application.Commands;

public record ColonizePlanetCommand
{
	public required Guid PlanetId { get; init; }
	public required Guid PlayerId { get; init; }
}
