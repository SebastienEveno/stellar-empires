namespace StellarEmpires.WebApi.v1.Dtos;

public record ColonizePlanetRequest
{
	public required Guid PlayerId { get; init; }
}
