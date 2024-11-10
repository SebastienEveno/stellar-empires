namespace StellarEmpires.Application.Commands;

using StellarEmpires.Domain.Models;
using StellarEmpires.Domain.Services;
using StellarEmpires.Infrastructure.EventStore;


public class RenamePlanetCommandHandler : IRenamePlanetCommandHandler
{
	private readonly IPlanetStateRetriever _planetStateRetriever;
	private readonly IEventStore _eventStore;

	public RenamePlanetCommandHandler(IPlanetStateRetriever planetStateRetriever, IEventStore eventStore)
	{
		_planetStateRetriever = planetStateRetriever;
		_eventStore = eventStore;
	}

	public async Task RenamePlanetAsync(RenamePlanetCommand command)
	{
		var planet = await _planetStateRetriever.GetCurrentStateAsync(command.PlanetId);

		if (planet == null)
		{
			throw new InvalidOperationException("Planet not found.");
		}

		planet.Rename(command.PlanetName, command.PlayerId);

		await _eventStore.SaveEventAsync<Planet>(planet.DomainEvents.Last());
	}
}