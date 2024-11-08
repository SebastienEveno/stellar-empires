using StellarEmpires.Domain.Models;

namespace StellarEmpires.WebApi.v1.Dtos;

public sealed record ReadPlanetDto
{
	public Guid Id { get; init; }
	public string Name { get; init; }
	public bool IsColonized { get; init; }
	public Guid? ColonizedBy { get; init; }
	public DateTime? ColonizedAt { get; init; }

	public static ReadPlanetDto FromPlanet(Planet planet)
	{
		return new ReadPlanetDto
		{
			Id = planet.Id,
			Name = planet.Name,
			IsColonized = planet.IsColonized,
			ColonizedBy = planet.ColonizedBy,
			ColonizedAt = planet.ColonizedAt
		};
	}
}
