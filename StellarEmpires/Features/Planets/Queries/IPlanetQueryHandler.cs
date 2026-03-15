using StellarEmpires.Features.Planets.Domain;

namespace StellarEmpires.Features.Planets.Queries;

public interface IPlanetQueryHandler
{
    Task<Planet> Handle(Guid planetId);
}
