using Microsoft.AspNetCore.Mvc;
using StellarEmpires.Domain.Models;
using StellarEmpires.Services;

namespace StellarEmpires.WebApi.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class MinesController : ControllerBase
{
    private readonly ILogger<MinesController> _logger;
    private readonly IMinesService _minesService;

    public MinesController(IMinesService minesService, ILogger<MinesController> logger)
    {
        _minesService = minesService;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult GetMines()
    {
        var mines = _minesService.GetMines();
        return Ok(mines);
    }

    [HttpPost("incrementMineLevel/{resourceType}")]
    public ActionResult IncrementMineLevel(ResourceType resourceType)
    {
        try
        {
            _minesService.IncrementMineLevel(resourceType);
            return Ok($"Mine level incremented for {resourceType}");
        }
        catch (InvalidOperationException ex)
        {
            // Handle the case where the mine for the specified resource type is not found
            // You can return a BadRequest or NotFound response based on your requirements
            return BadRequest(ex.Message);
        }
    }
}