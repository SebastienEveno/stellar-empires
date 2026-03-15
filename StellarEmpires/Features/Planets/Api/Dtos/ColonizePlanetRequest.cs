namespace StellarEmpires.Features.Planets.Api.Dtos;

public record ColonizePlanetRequest
{
    public required Guid PlayerId { get; init; }
}
