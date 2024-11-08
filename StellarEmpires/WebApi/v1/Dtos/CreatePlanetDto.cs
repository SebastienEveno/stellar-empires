namespace StellarEmpires.WebApi.v1.Dtos;

public sealed record CreatePlanetDto
{
	public Guid Id { get; init; } = Guid.NewGuid();
	public string Name { get; init; } = string.Empty;
	public bool IsColonized { get; init; } = false;
	public Guid? ColonizedBy { get; init; } = null;
	public DateTime? ColonizedAt { get; init; } = null;
}
