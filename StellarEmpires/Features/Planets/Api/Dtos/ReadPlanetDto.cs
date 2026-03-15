using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Features.Planets.Api.Dtos;

public sealed record ReadPlanetDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public bool IsColonized { get; init; }
    public Guid? ColonizedBy { get; init; }
    public DateTime? ColonizedAt { get; init; }

    // Coordinates
    public required string Galaxy { get; init; }
    public int System { get; init; }
    public int Slot { get; init; }

    public static ReadPlanetDto FromPlanet(Planet planet)
    {
        return new ReadPlanetDto
        {
            Id = planet.Id,
            Name = planet.Name,
            IsColonized = planet.IsColonized,
            ColonizedBy = planet.ColonizedBy,
            ColonizedAt = planet.ColonizedAt,
            Galaxy = planet.Galaxy,
            System = planet.System,
            Slot = planet.Slot
        };
    }
}
