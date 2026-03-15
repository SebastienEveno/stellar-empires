namespace StellarEmpires.Application.Commands;

using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Features.Planets.Services;
using StellarEmpires.Infrastructure.EventStore;


public class ColonizePlanetCommandHandler : IColonizePlanetCommandHandler
{
    private readonly IPlanetStateRetriever _planetStateRetriever;
    private readonly IEventStore _eventStore;

    public ColonizePlanetCommandHandler(IPlanetStateRetriever planetStateRetriever, IEventStore eventStore)
    {
        _planetStateRetriever = planetStateRetriever;
        _eventStore = eventStore;
    }

    public async Task ColonizePlanetAsync(ColonizePlanetCommand command)
    {
        var planet = await _planetStateRetriever.GetCurrentStateAsync(command.PlanetId);

        planet.Colonize(command.PlayerId);

        await _eventStore.SaveEventAsync<Planet>(planet.DomainEvents.Last());
    }
}