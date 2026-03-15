using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Features.Planets.Api.Dtos;

public sealed record CreatePlanetDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public bool IsColonized { get; init; } = false;
    public Guid? ColonizedBy { get; init; } = null;
    public DateTime? ColonizedAt { get; init; } = null;

    // Coordinates
    public Galaxy Galaxy { get; init; } = Galaxy.Unknown;
    public int System { get; init; } = 1;
    public int Slot { get; init; } = 1;
}
