using Microsoft.AspNetCore.Mvc;
using StellarEmpires.Application.Commands;
using StellarEmpires.Application.Queries;
using StellarEmpires.Domain.Models;
using StellarEmpires.Infrastructure.PlanetStore;
using StellarEmpires.WebApi.v1.Dtos;

namespace StellarEmpires.WebApi.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PlanetsController : ControllerBase
{
	private readonly IPlanetStore _planetStore;
	private readonly IColonizePlanetCommandHandler _colonizePlanetCommandHandler;
	private readonly IRenamePlanetCommandHandler _renamePlanetCommandHandler;
	private readonly IPlanetQueryHandler _planetQueryHandler;

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

	[HttpGet("initial", Name = nameof(GetAllInitialStates))]
	public async Task<IActionResult> GetAllInitialStates()
	{
		var allPlanetsInitialStates = await _planetStore.GetPlanetsAsync();

		return Ok(allPlanetsInitialStates.Select(ReadPlanetDto.FromPlanet));
	}

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
