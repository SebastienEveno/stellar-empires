namespace StellarEmpires.Application.Commands;

public interface IRenamePlanetCommandHandler
{
	Task RenamePlanetAsync(RenamePlanetCommand command);
}
