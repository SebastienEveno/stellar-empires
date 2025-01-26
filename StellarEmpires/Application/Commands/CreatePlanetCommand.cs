namespace StellarEmpires.Application.Commands;

public record CreatePlanetCommand
{
	public required Guid Id { get; init; }
	public required string Name { get; init; }
	public required bool IsColonized { get; init; }
	public Guid? ColonizedBy { get; init; }
	public DateTime? ColonizedAt { get; init; }
}
