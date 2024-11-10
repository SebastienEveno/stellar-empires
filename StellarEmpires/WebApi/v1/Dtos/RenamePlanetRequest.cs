namespace StellarEmpires.WebApi.v1.Dtos;

public record RenamePlanetRequest
{
	public required Guid PlayerId { get; init; }
	public required string NewName { get; init; }
}
