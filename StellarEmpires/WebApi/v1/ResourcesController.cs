using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StellarEmpires.Domain.Models;

namespace StellarEmpires.WebApi.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ResourcesController : ControllerBase
{
    private readonly ILogger<ResourcesController> _logger;
    private readonly IMemoryCache _memoryCache;

    public ResourcesController(IMemoryCache memoryCache, ILogger<ResourcesController> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    [HttpGet("resources")]
    public IActionResult GetResources()
    {
        var metal = _memoryCache.TryGetValue(ResourceType.Metal.ToString(), out int currentMetal) ? currentMetal : 0;
        var crystal = _memoryCache.TryGetValue(ResourceType.Crystal.ToString(), out int currentCrystal) ? currentCrystal : 0;
        var deuterium = _memoryCache.TryGetValue(ResourceType.Deuterium.ToString(), out int currentDeuterium) ? currentDeuterium : 0;

        return Ok(new
        {
            Metal = metal,
            Crystal = crystal,
            Deuterium = deuterium
        });
    }
}