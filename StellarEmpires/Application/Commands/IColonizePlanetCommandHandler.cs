namespace StellarEmpires.Application.Commands;

public interface IColonizePlanetCommandHandler
{
	Task ColonizePlanetAsync(ColonizePlanetCommand command);
}
