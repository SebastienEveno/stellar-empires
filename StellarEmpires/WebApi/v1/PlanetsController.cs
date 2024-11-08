using Microsoft.AspNetCore.Mvc;
using StellarEmpires.Domain.Models;
using StellarEmpires.Domain.Services;
using StellarEmpires.Infrastructure.PlanetStore;
using StellarEmpires.WebApi.v1.Dtos;

namespace StellarEmpires.WebApi.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PlanetsController : ControllerBase
{
	private readonly IPlanetStateRetriever _planetStateRetriever;
	private readonly IPlanetStore _planetStore;

	public PlanetsController(IPlanetStateRetriever planetStateRetriever, IPlanetStore planetStore)
	{
		_planetStateRetriever = planetStateRetriever;
		_planetStore = planetStore;
	}

	[HttpGet("{planetId}/initial", Name = nameof(GetInitialState))]
	public async Task<IActionResult> GetInitialState(Guid planetId)
	{
		try
		{
			var initialState = await _planetStateRetriever.GetInitialStateAsync(planetId);
			
			return Ok(ReadPlanetDto.FromPlanet(initialState));
		}
		catch (InvalidOperationException ex)
		{
			return NotFound(ex.Message);
		}
	}

	[HttpGet("{planetId}/current", Name = nameof(GetCurrentState))]
	public async Task<IActionResult> GetCurrentState(Guid planetId)
	{
		try
		{
			var currentState = await _planetStateRetriever.GetCurrentStateAsync(planetId);
			
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

		var newPlanet = new Planet(
			request.Id,
			request.Name,
			request.IsColonized,
			request.ColonizedBy,
			request.ColonizedAt
		);

		await _planetStore.SavePlanetAsync(newPlanet);

		return CreatedAtAction(nameof(GetCurrentState), new { planetId = newPlanet.Id }, ReadPlanetDto.FromPlanet(newPlanet));
	}
}
