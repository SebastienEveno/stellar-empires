using StellarEmpires.Domain.Models;

namespace StellarEmpires.Application.Queries;

public interface IPlanetQueryHandler
{
	Task<Planet> Handle(Guid planetId);
}
