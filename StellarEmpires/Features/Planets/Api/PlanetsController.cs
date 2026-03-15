using Microsoft.AspNetCore.Mvc;
using StellarEmpires.Application.Commands;
using StellarEmpires.Features.Planets.Api.Dtos;
using StellarEmpires.Features.Planets.Domain;
using StellarEmpires.Features.Planets.Queries;
using StellarEmpires.Features.Planets.Repositories;

namespace StellarEmpires.Features.Planets.Api;

/// <summary>
/// API controller for managing planet operations in Stellar Empires.
/// Provides endpoints for retrieving, creating, colonizing, and renaming planets.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PlanetsController : ControllerBase
{
    private readonly IPlanetStore _planetStore;
    private readonly IColonizePlanetCommandHandler _colonizePlanetCommandHandler;
    private readonly IRenamePlanetCommandHandler _renamePlanetCommandHandler;
    private readonly IPlanetQueryHandler _planetQueryHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanetsController"/> class.
    /// </summary>
    /// <param name="planetStore">The planet data store.</param>
    /// <param name="colonizePlanetCommandHandler">Handler for colonize planet commands.</param>
    /// <param name="renamePlanetCommandHandler">Handler for rename planet commands.</param>
    /// <param name="planetQueryHandler">Handler for planet queries.</param>
    public PlanetsController(
        IPlanetStore planetStore,
        IColonizePlanetCommandHandler colonizePlanetCommandHandler,
        IRenamePlanetCommandHandler renamePlanetCommandHandler,
        IPlanetQueryHandler planetQueryHandler)
    {
        _planetStore = planetStore;
        _colonizePlanetCommandHandler = colonizePlanetCommandHandler;
        _renamePlanetCommandHandler = renamePlanetCommandHandler;
        _planetQueryHandler = planetQueryHandler;
    }

    /// <summary>
    /// Retrieves the current state of a specific planet.
    /// </summary>
    /// <param name="planetId">The unique identifier of the planet.</param>
    /// <returns>
    /// 200 OK with the planet's current state as <see cref="ReadPlanetDto"/>,
    /// or 404 Not Found if the planet does not exist.
    /// </returns>
    [HttpGet("{planetId}/current", Name = nameof(GetCurrentState))]
    public async Task<IActionResult> GetCurrentState(Guid planetId)
    {
        try
        {
            var currentState = await _planetQueryHandler.Handle(planetId);

            return Ok(ReadPlanetDto.FromPlanet(currentState));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all planets in their initial states.
    /// </summary>
    /// <returns>200 OK with a collection of all planets as <see cref="ReadPlanetDto"/> objects.</returns>
    [HttpGet("initial", Name = nameof(GetAllInitialStates))]
    public async Task<IActionResult> GetAllInitialStates()
    {
        var allPlanetsInitialStates = await _planetStore.GetPlanetsAsync();

        return Ok(allPlanetsInitialStates.Select(ReadPlanetDto.FromPlanet));
    }

    /// <summary>
    /// Creates a new planet.
    /// </summary>
    /// <param name="request">The planet creation request containing name, colonization status, and optional colonization details.</param>
    /// <returns>
    /// 201 Created with the newly created planet's state as <see cref="ReadPlanetDto"/>,
    /// or 400 Bad Request if a planet with the same ID already exists.
    /// </returns>
    [HttpPost(Name = nameof(AddPlanet))]
    public async Task<IActionResult> AddPlanet([FromBody] CreatePlanetDto request)
    {
        var existingPlanet = await _planetStore.GetPlanetByIdAsync(request.Id);
        if (existingPlanet != null)
        {
            return BadRequest("Planet with the same ID already exists.");
        }

        var newPlanet = Planet.Create(
            request.Id,
            request.Name,
            request.IsColonized,
            request.ColonizedBy,
            request.ColonizedAt
        );

        await _planetStore.SavePlanetAsync(newPlanet);

        return CreatedAtAction(nameof(GetCurrentState), new { planetId = newPlanet.Id }, ReadPlanetDto.FromPlanet(newPlanet));
    }

    /// <summary>
    /// Colonizes a planet for a player.
    /// </summary>
    /// <param name="planetId">The unique identifier of the planet to colonize.</param>
    /// <param name="request">The colonization request containing the player ID.</param>
    /// <returns>
    /// 200 OK if colonization is successful,
    /// 404 Not Found if the planet does not exist,
    /// 409 Conflict if the planet is already colonized,
    /// or 500 Internal Server Error for unexpected errors.
    /// </returns>
    [HttpPost("{planetId}/colonize")]
    public async Task<IActionResult> ColonizePlanet(Guid planetId, [FromBody] ColonizePlanetRequest request)
    {
        try
        {
            var command = new ColonizePlanetCommand { PlanetId = planetId, PlayerId = request.PlayerId };

            await _colonizePlanetCommandHandler.ColonizePlanetAsync(command);

            return Ok("Planet successfully colonized.");
        }
        catch (InvalidOperationException ex) when (ex.Message == "Planet not found.")
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message == "Planet is already colonized.")
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Renames a planet for the player who colonized it.
    /// </summary>
    /// <param name="planetId">The unique identifier of the planet to rename.</param>
    /// <param name="request">The rename request containing the player ID and new planet name.</param>
    /// <returns>
    /// 200 OK if the rename is successful,
    /// 404 Not Found if the planet does not exist,
    /// 400 Bad Request if the new name is null/empty or if the player is not authorized to rename it,
    /// or 500 Internal Server Error for unexpected errors.
    /// </returns>
    [HttpPost("{planetId}/rename")]
    public async Task<IActionResult> RenamePlanet(Guid planetId, [FromBody] RenamePlanetRequest request)
    {
        try
        {
            var command = new RenamePlanetCommand
            {
                PlanetId = planetId,
                PlayerId = request.PlayerId,
                PlanetName = request.NewName
            };

            await _renamePlanetCommandHandler.RenamePlanetAsync(command);

            return Ok("Planet successfully renamed.");
        }
        catch (InvalidOperationException ex) when (ex.Message == "Planet not found.")
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message == "New name is either null or empty.")
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex) when (ex.Message == "Only the player who colonized the planet can rename it.")
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
