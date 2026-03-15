using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Application.Commands;

public record CreatePlanetCommand
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required bool IsColonized { get; init; }
    public Guid? ColonizedBy { get; init; }
    public DateTime? ColonizedAt { get; init; }

    // Coordinates
    public Galaxy Galaxy { get; init; } = Galaxy.Unknown;
    public int System { get; init; } = 1;
    public int Slot { get; init; } = 1;
}
