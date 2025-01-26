namespace StellarEmpires.Application.Commands;

public interface ICreatePlanetCommandHandler
{
	Task CreatePlanetAsync(CreatePlanetCommand command);
}
