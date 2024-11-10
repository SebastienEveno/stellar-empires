namespace StellarEmpires.WebApi.v1.Dtos;

public record RenamePlanetRequest
{
	public required string NewName { get; init; }
}
