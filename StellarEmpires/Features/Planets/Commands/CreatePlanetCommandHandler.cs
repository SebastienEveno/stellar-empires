using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Features.Planets.Repositories;
using StellarEmpires.Infrastructure.EventStore;

namespace StellarEmpires.Application.Commands;

public class CreatePlanetCommandHandler : ICreatePlanetCommandHandler
{
    private readonly IPlanetStore _planetStore;
    private readonly IEventStore _eventStore;

    public CreatePlanetCommandHandler(IPlanetStore planetStore, IEventStore eventStore)
    {
        _planetStore = planetStore;
        _eventStore = eventStore;
    }

    public async Task CreatePlanetAsync(CreatePlanetCommand command)
    {
        var existingPlanet = await _planetStore.GetPlanetByIdAsync(command.Id);
        if (existingPlanet != null)
        {
            throw new InvalidOperationException("Planet with the same ID already exists.");
        }

        var newPlanet = Planet.Create(
            command.Id,
            command.Name,
            command.IsColonized,
            command.ColonizedBy,
            command.ColonizedAt,
            command.Galaxy,
            command.System,
            command.Slot
        );

        await _eventStore.SaveEventAsync<Planet>(newPlanet.DomainEvents.Last());
    }
}
