namespace StellarEmpires.Features.Planets.Api.Dtos;

public record RenamePlanetRequest
{
    public required Guid PlayerId { get; init; }
    public required string? NewName { get; init; }
}
